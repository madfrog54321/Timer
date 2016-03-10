using System;
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

            int endIndex = _currentResponse.IndexOf(RETURN_END);
            if (endIndex != -1)
            {
                processData(_currentResponse.Substring(0, endIndex));
                _currentResponse = _currentResponse.Substring(endIndex + RETURN_END.Length);
            }

            if(_currentResponse.Length > 100)
            {
                DataManager.MessageProvider.showError("Invalid responce from track timer", "The responce buffer [size: 100 chars] has been filled without finding a valid response " + _currentResponse.Length);
                _currentResponse = "";//disgard responces
                _waitingForRace = false;//ready for new responce
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
                DataManager.MessageProvider.showError("Track responded with \"?\"", "The track timer did not understand the command, or the command was invalid");
            }
            else if (_waitingForRace && !String.IsNullOrEmpty(value))
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
                _waitingForRace = false;
                triggerGotRace(success, new Race(times.ToArray()));
            }
        }

        public void getNextRace()
        {
            if (!_waitingForRace)
            {
                _waitingForRace = true;
                sendCommand("rg");
            }
        }

        public void maskOffLane(int lane)
        {
            sendCommand("om" + lane.ToString());
        }

        public void resetMask()
        {
            sendCommand("om0");
        }

        public void reset()
        {
            sendCommand("r");
        }

        public void setup()
        {
            sendCommand("ol1"); //set lane character
            sendCommand("op2"); //set place character
            sendCommand("od3"); //set decimals
        }

        public void setNumberLanes(int number)
        {
            sendCommand("on" + number.ToString());
        }

        private void sendCommand(string command)
        {
            _serialPort.Write(command + MESSAGE_END);
        }
    }
}
