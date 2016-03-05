﻿using System;
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
            set { _messageProvider = value; }
        }

        static private RaceManager _raceManager;
        static public RaceManager RaceManager
        {
            get {
                if(_raceManager == null)
                {
                    MessageProvider.showError("Race Manager Missing", "The race manager was called, but it does not exist. Some unexpected resalts may occur. It is recommended to close the application immediately.");
                    return new RaceManager(Settings.NumberOfLanes);
                }
                else
                {
                    return _raceManager;
                }
            }
        }

        public static bool readyForRace
        {
            get { return _trackTimer != null && _raceManager != null && _trackTimer.Connected; }
        }

        static public string[] getPorts()
        {
            return SerialPort.GetPortNames();
        }

        static private TrackTimer _trackTimer;
        static public TrackTimer TrackTimer
        {
            get
            {
                if (_trackTimer == null)
                {
                    MessageProvider.showError("Track Timer Missing", "The track timer was called, but it does not exist. Some unexpected resalts may occur. It is recommended to close the application immediately.");
                    return new TrackTimer("");
                }
                else
                {
                    return _trackTimer;
                }
            }
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

        static public void disconnectTimer()
        {
            _trackTimer.disconnect();
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

        public static Uri getRelativeUri(string absolutePath)
        {
            return getAssemblyUri().MakeRelativeUri(new Uri(absolutePath));
        }

        public static Uri getRelativeUri(Uri absoluteUri)
        {
            return getAssemblyUri().MakeRelativeUri(absoluteUri);
        }

        public static string getRelativePath(string absolutePath)
        {
            return getRelativeUri(absolutePath).ToString();
        }

        public static string getRelativePath(Uri absoluteUri)
        {
            return getRelativeUri(absoluteUri).ToString();
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
