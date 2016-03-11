using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.IO.Ports;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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

        public static BitmapImage loadImage(Uri uri)
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = uri;
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.EndInit();
            return bi;
        }

        public static bool archiveCompetition()
        {
            string archiveDirectory = DataManager.getAbsolutePath(DataManager.Settings.ArchiveDirectory);
            if (!Directory.Exists(archiveDirectory))
            {
                Directory.CreateDirectory(archiveDirectory);
            }

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.InitialDirectory = archiveDirectory;
            dlg.FileName = Competition.Name;
            dlg.DefaultExt = ".comp";
            dlg.Filter = "Competition File (*.comp)|*.comp";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                FileInfo saveFile = new FileInfo(dlg.FileName);
                string fileName = saveFile.Name.Substring(0, saveFile.Name.IndexOf(saveFile.Extension));

                Competition oldComp = _competition;
                oldComp.Name = fileName;
                oldComp.Save();
                _competition = new Competition();
                triggerCompetitionChanged();

                string tempFolder = saveFile.Directory + @"\" + fileName;
                if (Directory.Exists(tempFolder))
                {
                    Directory.Delete(tempFolder, true);
                }
                Directory.CreateDirectory(tempFolder);

                string imagesDirectory = DataManager.getAbsolutePath(DataManager.Settings.ImageDirectory);
                Directory.Move(imagesDirectory, tempFolder + @"\" + DataManager.Settings.ImageDirectory);//move images folder to zip folder
                
                File.Move(getAbsolutePath(Competition.DefaltFileName), tempFolder + @"\" + Competition.DefaltFileName);//move competition file to zip folder

                //string startPath = @"c:\example\start";
                //string zipPath = @"c:\example\result.zip";
                //string extractPath = @"c:\example\extract";

                ZipFile.CreateFromDirectory(tempFolder, saveFile.FullName);

                Directory.Delete(tempFolder, true);

                _competition = Competition.Load();
                triggerCompetitionChanged();
                //ZipFile.ExtractToDirectory(zipPath, extractPath);
            }

            return result == true;
        }

        public static void unarchiveCompetition(Panel parent)
        {
            string archiveDirectory = DataManager.getAbsolutePath(DataManager.Settings.ArchiveDirectory);
            if (!Directory.Exists(archiveDirectory))
            {
                Directory.CreateDirectory(archiveDirectory);
            }

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = archiveDirectory;
            dlg.DefaultExt = ".comp";
            dlg.Filter = "Competition File (*.comp)|*.comp";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                DialogBox.showOptionBox(parent, "Do you want to archive the current competition? If you do not archive the current competition and all of its data/images it will be deleted.", "Archive The Current Competition?", "Delete", "Archive", delegate (DialogBox.DialogResult res)
                {
                    bool done = false;
                    if (res == DialogBox.DialogResult.MainOption)
                    {
                        if (!archiveCompetition())
                        {
                            done = true;
                        }
                    }
                    
                    if(!done)
                    {
                        FileInfo saveFile = new FileInfo(dlg.FileName);
                        string fileName = saveFile.Name.Substring(0, saveFile.Name.IndexOf(saveFile.Extension));

                        string tempFolder = saveFile.Directory + @"\" + saveFile.Name.Substring(0, saveFile.Name.IndexOf(saveFile.Extension));
                        if (Directory.Exists(tempFolder))
                        {
                            Directory.Delete(tempFolder, true);
                        }
                        Directory.CreateDirectory(tempFolder);

                        ZipFile.ExtractToDirectory(saveFile.FullName, tempFolder);

                        string imagesDirectory = DataManager.getAbsolutePath(DataManager.Settings.ImageDirectory);


                        if (Directory.Exists(imagesDirectory))
                        {
                            Directory.Delete(imagesDirectory, true);
                        }
                        if (File.Exists(getAbsolutePath(Competition.DefaltFileName)))
                        {
                            File.Delete(getAbsolutePath(Competition.DefaltFileName));
                        }

                        Directory.Move(tempFolder + @"\" + DataManager.Settings.ImageDirectory, imagesDirectory);//move images folder to normal folder

                        File.Move(tempFolder + @"\" + Competition.DefaltFileName, getAbsolutePath(Competition.DefaltFileName));//move images folder to normal folder

                        Directory.Delete(tempFolder, true);
                        File.Delete(saveFile.FullName);

                        _competition = Competition.Load();
                        _competition.Name = fileName;
                        triggerCompetitionChanged();
                    }
                });
            }
        }

        public delegate void competitionChangeHandler();
        public static event competitionChangeHandler competitionChanged;
        private static void triggerCompetitionChanged()
        {
            competitionChangeHandler handler = competitionChanged;
            if (handler != null)
            {
                handler();
            }
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
