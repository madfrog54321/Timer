using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timer
{
    static class DataManager
    {
        private const int NUMBER_OF_LANES = 6;//use settings file

        //serve out info objects
        static private TrackTimer _trackTimer;
        public static TrackTimer TrackTimer
        {
            get { return _trackTimer; }
        }

        static private List<Racer> _racers;
        public static List<Racer> Racers
        {
            get { return _racers; }
        }

        static private RaceManager _raceManager;
        public static RaceManager RaceManager
        {
            get { return _raceManager; }
        }

        static private MessageProvider _messageProvider;
        public static MessageProvider MessageProvider
        {
            get { return _messageProvider; }
        }
        //======================

        public static bool tryConnectTimer(string port)
        {
            _trackTimer = new TrackTimer(port);
            if (_trackTimer.Connected)
            {
                //if connected setup race manager, because it uses the track timer
                _raceManager = new RaceManager(NUMBER_OF_LANES);
                RaceManager.onGotRace += RaceManager_onGotRace;
            }
            return _trackTimer.Connected;
        }

        public static string[] getPorts()
        {
            return SerialPort.GetPortNames();
        }

        [STAThread]
        public static void Main()
        {
            //get and manage info for application
            _racers = new List<Racer>();

            _messageProvider = new BasicMessageProvider();//enable basic message provider

            //replace with settings
            Racers.Add(new Racer("test car 1", "maker 1", "1"));
            Racers.Add(new Racer("test car 2", "maker 2", "2"));
            Racers.Add(new Racer("test car 3", "maker 3", "3"));
            Racers.Add(new Racer("test car 4", "maker 4", "4"));
            Racers.Add(new Racer("test car 5", "maker 5", "5"));
            Racers.Add(new Racer("test car 6", "maker 6", "6"));
            Racers.Add(new Racer("test car 7", "maker 7", "7"));
            Racers.Add(new Racer("test car 8", "maker 8", "8"));
            Racers.Add(new Racer("test car 9", "maker 9", "9"));
            Racers.Add(new Racer("test car 10", "maker 10", "10"));
            //===========================

            //start wpf application thread
            var application = new App();
            application.InitializeComponent();
            application.Run();
        }

        private static void RaceManager_onGotRace(Dictionary<int, Time> results)
        {
            foreach (KeyValuePair<int, Time> lane in results)
            {
                Racers[lane.Key].addTime(lane.Value);
            }
        }
    }
}
