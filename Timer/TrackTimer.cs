using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Timer
{
    class TrackTimer
    {
        private const int BAUD_RATE = 9600;
        private const int DATA_BITS = 8;
        private const StopBits STOP_BITS = StopBits.One;
        private const Handshake HAND_SHAKE = Handshake.None;
        private const Parity PARITY = Parity.None;
        private const string GET_NEXT_RACE = "rg";
        private const string MESSAGE_END = "\r";
        private const string RETURN_END = "\r\n";
        private const int NUM_TRACKS = 6;
        private const string ERROR_HEADER = "Track Error";
        private const string MASK_LANE = "om";
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

        public TrackTimer(string portName)
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
            try
            {
                _serialPort.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ERROR_HEADER);
            }
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            _currentResponse += _serialPort.ReadExisting();

            int endIndex = _currentResponse.IndexOf(RETURN_END);
            if (endIndex != -1)
            {
                processData(_currentResponse.Substring(0, endIndex));
                _currentResponse = _currentResponse.Substring(endIndex + RETURN_END.Length + 1);
            }

            if(_currentResponse.Length > 50)
            {
                MessageBox.Show("The responce buffer [size: 50 chars] has been filled without finding a valid response");
            }
        }

        private void processData(string value)
        {
            if (_waitingForRace)
            {
                value = value.Replace("  ", "0 ");
                if (value[value.Length - 1] == ' ')
                {
                    value = value.Substring(0, value.Length - 1) + '0';
                }
                string[] splitValue = value.Split(' ');
                if (splitValue.Length == NUM_TRACKS)
                {
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
                                        MessageBox.Show("Failed to convert: " + placeC + " to a int", ERROR_HEADER, MessageBoxButton.OK, MessageBoxImage.Error);
                                        success = false;
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Failed to convert " + timeS + " to a float", ERROR_HEADER, MessageBoxButton.OK, MessageBoxImage.Error);
                                    success = false;
                                }
                            }
                            else
                            {
                                MessageBox.Show("Failed to convert: " + laneC + " to a int", ERROR_HEADER, MessageBoxButton.OK, MessageBoxImage.Error);
                                success = false;
                            }
                        }
                        else
                        {

                            MessageBox.Show("The time code was not " + (5 + DECIMAL_PLACES).ToString() + " chars long, it was " + splitValue[i].Length, ERROR_HEADER, MessageBoxButton.OK, MessageBoxImage.Error);
                            success = false;
                        }
                    }
                    _waitingForRace = false;
                    triggerGotRace(success, new Race(times.ToArray()));
                }
                else
                {
                    MessageBox.Show(splitValue.Length + " lanes were return instead of the expected " + NUM_TRACKS, ERROR_HEADER);
                }
            }
        }

        public void getNextRace()
        {
            if (!_waitingForRace)
            {
                _waitingForRace = true;
                _serialPort.Write(GET_NEXT_RACE + MESSAGE_END);
            }
        }

        public void maskOffLane(int lane)
        {
            _serialPort.Write(MASK_LANE + lane + MESSAGE_END);
        }

        public void resetMask()
        {
            _serialPort.Write(MASK_LANE + 0 + MESSAGE_END);
        }

        public void reset()
        {
            //this is not writen
        }
    }
}
