using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Reflection;

namespace Timer
{
    static class DataManager
    {
        private static Competition _competition;
        public static Competition Competition
        {
            get { return _competition; }
        }

        private static Settings _settings;
        public static Settings Settings
        {
            get { return _settings; }
        }

        static private MessageProvider _messageProvider;
        static public MessageProvider MessageProvider
        {
            get { return _messageProvider; }
        }

        static private RaceManager _raceManager;
        static public RaceManager RaceManager
        {
            get { return _raceManager; }
        }

        static public string[] getPorts()
        {
            return SerialPort.GetPortNames();
        }

        static private TrackTimer _trackTimer;
        static public TrackTimer TrackTimer
        {
            get { return _trackTimer; }
        }

        static public bool tryConnectTimer(string port)
        {
            _trackTimer = new TrackTimer(port);
            if (_trackTimer.Connected)
            {
                //if connected setup race manager, because it uses the track timer
                _raceManager = new RaceManager(Settings.NumberOfLanes);
                RaceManager.onGotRace += RaceManager_onGotRace;
            }
            return _trackTimer.Connected;
        }
        
        private static void RaceManager_onGotRace(Dictionary<int, Time> results)
        {
            foreach (KeyValuePair<int, Time> lane in results)
            {
                Competition.Racers[lane.Key].addTime(lane.Value);
            }
            saveSettings();
        }

        private static Uri getAssemblyUri()
        {
            return new Uri(Assembly.GetExecutingAssembly().Location);
        }

        public static string getAbsolutePath(string relativePath)
        {
            return getAbsoluteUri(relativePath).LocalPath;
        }

        public static Uri getAbsoluteUri(string relativePath)
        {
            return new Uri(getAssemblyUri(), relativePath);
        }

        [STAThread]
        public static void Main()
        {
            _messageProvider = new BasicMessageProvider();//enable basic message provider

            //open settings
            _settings = Settings.Load();
            _competition = Competition.Load();

            //start wpf application thread
            var application = new App();
            application.Exit += Application_Exit;
            application.InitializeComponent();
            application.Run();
        }

        public static void saveSettings()
        {
            _settings.Save();
            _competition.Save();
        }

        private static void Application_Exit(object sender, System.Windows.ExitEventArgs e)
        {
            saveSettings();
        }
    }
}
