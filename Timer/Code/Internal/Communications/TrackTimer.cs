﻿using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace Timer
{
    class TrackTimer
    {
        private const int BAUD_RATE = 9600;
        private const int DATA_BITS = 8;
        private const StopBits STOP_BITS = StopBits.One;
        private const Handshake HAND_SHAKE = Handshake.None;
        private const Parity PARITY = Parity.None;
        private const string MESSAGE_END = "\r";
        private const string RETURN_END = "\r\n";
        private const int DECIMAL_PLACES = 3;

        private SerialPort _serialPort;
        public bool Connected
        {
            get { return _serialPort.IsOpen; }
        }

        private String _currentResponse;
        private bool _waitingForRace;
        public delegate void gotRace(bool success, Race race);
        public event gotRace OnGotRace;
        private string lastComand = "";
        private void triggerGotRace(bool success, Race race)
        {
            gotRace handler = OnGotRace;
            if (handler != null)
            {
                handler(success, race);
            }
        }

        public void disconnect()
        {
            _serialPort.Close();
            _serialPort.Dispose();
        }

        public TrackTimer(string portName)
        {
            try
            {
                _currentResponse = "";
                _waitingForRace = false;

                _serialPort = new SerialPort();
                _serialPort.DataReceived += _serialPort_DataReceived;
                _serialPort.PortName = portName;
                _serialPort.BaudRate = BAUD_RATE;
                _serialPort.DataBits = DATA_BITS;
                _serialPort.StopBits = STOP_BITS;
                _serialPort.Handshake = HAND_SHAKE;
                _serialPort.Parity = PARITY;

                _serialPort.Open();

                setup();
            }
            catch (Exception ex)
            {
                DataManager.MessageProvider.showError("Could not connect to track timer", ex.Message);
            }
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            _currentResponse += _serialPort.ReadExisting();
            
            while (_currentResponse.Contains(RETURN_END))
            {
                string command = _currentResponse.Substring(0, _currentResponse.IndexOf(RETURN_END));
                processData(command);
                string infoLeft = _currentResponse.Substring(_currentResponse.IndexOf(RETURN_END) + RETURN_END.Length);
                _currentResponse = infoLeft;
            }
        }

        public void stopWaitingForRace()
        {
            _waitingForRace = false;
        }

        private void processData(string value)
        {
            //DataManager.MessageProvider.showError("Info", "\"" + value + "\"");

            if (value == "?")
            {
                DataManager.MessageProvider.showError("Track responded with \"?\"", "The track timer did not understand the command, or the command was invalid. Last command sent:" + Environment.NewLine + "\"" + lastComand + "\"");
            }
            else if (_waitingForRace && !String.IsNullOrEmpty(value) && value.Length > 5)
            {
                value = value.Replace("  ", "0 ");
                //if (value[value.Length - 1] == ' ')
                //{
                //    value = value.Substring(0, value.Length - 1) + '0';
                //}
                string[] splitValue = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                //form of: 1=9.9992
                List<Time> times = new List<Time>();
                bool success = true;
                for(int i = 0; i < splitValue.Length && success; i++)
                {
                    if (splitValue[i].Length == 5 + DECIMAL_PLACES)
                    {
                        //get the lane number
                        char laneC = splitValue[i][0];
                        int lane = 0;
                        if (int.TryParse(laneC.ToString(), out lane))
                        {
                            //get the time on that lane
                            string timeS = splitValue[i].Substring(2, splitValue[i].Length - 3);
                            float time = 0;
                            if(float.TryParse(timeS, out time))
                            {
                                //get what place it came in at
                                char placeC = splitValue[i][splitValue[i].Length - 1];
                                int place = 0;
                                if (int.TryParse(placeC.ToString(), out place))
                                {
                                    //add this lanes time
                                    times.Add(new Time(lane, time, place));
                                }
                                else
                                {
                                    DataManager.MessageProvider.showError("Failed to read timer", "Failed to convert [" + placeC + "] to a int");
                                    success = false;
                                }
                            }
                            else
                            {
                                DataManager.MessageProvider.showError("Failed to read timer", "Failed to convert [" + timeS + "] to a float");
                                success = false;
                            }
                        }
                        else
                        {
                            DataManager.MessageProvider.showError("Failed to read timer", "Failed to convert [" + laneC + "] to a int");
                            success = false;
                        }
                    }
                    else
                    {
                        DataManager.MessageProvider.showError("Failed to read timer", "The time code was not " + (5 + DECIMAL_PLACES).ToString() + " chars long. It was " + splitValue[i].Length + " chars");
                        success = false;
                    }
                }
                resetMask();
                _waitingForRace = false;
                triggerGotRace(success, new Race(times.ToArray()));
            }
        }

        public void getNextRace()
        {
            if (!_waitingForRace)
            {
                _waitingForRace = true;
                lastComand = "rg";
                sendCommand("rg");
            }
        }

        public void getRaceNow()
        {
            if (_waitingForRace)
            {
                lastComand = "ra";
                sendCommand("ra");
            }
        }

        public void getLastRace()
        {
            if (_waitingForRace)
            {
                lastComand = "rp";
                sendCommand("rp");
            }
        }

        public void maskOffLane(int lane)
        {
            lastComand = "om";
            sendCommand("om" + lane.ToString());
        }

        public void resetMask()
        {
            lastComand = "om0";
            sendCommand("om0");
        }

        public void reset()
        {
            lastComand = "r";
            sendCommand("r");
            _waitingForRace = false;//stop waiting for a race
        }

        public void setup()
        {
            lastComand = "ol1";
            sendCommand("ol1"); //set lane character
            lastComand = "op2";
            sendCommand("op2"); //set place character
            lastComand = "od3";
            sendCommand("od3"); //set decimals
        }

        public void setNumberLanes(int number)
        {
            lastComand = "on";
            sendCommand("on" + number.ToString());
        }

        private void sendCommand(string command)
        {
            _serialPort.Write(command + MESSAGE_END);
        }
    }
}
