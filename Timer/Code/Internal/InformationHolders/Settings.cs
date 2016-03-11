using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace Timer
{
    class Settings
    {
        private int _numLanes;
        public int NumberOfLanes
        {
            get { return _numLanes; }
            set { _numLanes = value; }
        }

        private string _defaltCarImageUri;
        public string DefaltCarImageUri
        {
            get { return _defaltCarImageUri; }
            set { _defaltCarImageUri = value; }
        }

        private string _defaltMakerImageUri;
        public string DefaltMakerImageUri
        {
            get { return _defaltMakerImageUri; }
            set { _defaltMakerImageUri = value; }
        }

        private string _archiveDirectory;
        public string ArchiveDirectory
        {
            get { return _archiveDirectory; }
            set { _archiveDirectory = value; }
        }
        
        private string _imageDirectory;
        public string ImageDirectory
        {
            get { return _imageDirectory; }
            set { _imageDirectory = value; }
        }

        private List<string> _classes;
        public List<string> Classes
        {
            get { return _classes; }
            set { _classes = value; }
        }

        private string _emptyLaneBarcode;
        public string EmptyLaneBarcode
        {
            get { return _emptyLaneBarcode; }
            set { _emptyLaneBarcode = value; }
        }

        private string _resetBarcode;
        public string ResetBarcode
        {
            get { return _resetBarcode; }
            set { _resetBarcode = value; }
        }

        private double _raceDisplayHeight;
        public double RaceDisplayHeight
        {
            get { return _raceDisplayHeight; }
            set { _raceDisplayHeight = value; }
        }

        private double _standingsZoom;
        public double StandingsZoom
        {
            get { return _standingsZoom; }
            set { _standingsZoom = value; }
        }

        private double _tilesZoom;
        public double TilesZoom
        {
            get { return _tilesZoom; }
            set { _tilesZoom = value; }
        }

        private double _autoScrollSpeed;
        public double AutoScrollSpeed
        {
            get { return _autoScrollSpeed; }
            set { _autoScrollSpeed = value; }
        }

        public Settings()
        {
            _numLanes = 6;
            _defaltCarImageUri = "pack://application:,,,/Timer;component/Assets/Images/DefaltCarPicture.png";
            _defaltMakerImageUri = "pack://application:,,,/Timer;component/Assets/Images/DefaltCreatorImage.png";
            _archiveDirectory = "archives";
            _imageDirectory = "images";
            _classes = new List<string>();
            _emptyLaneBarcode = "empty";
            _resetBarcode = "reset";
            _raceDisplayHeight = 1;
            _standingsZoom = 1;
            _autoScrollSpeed = 50;
            _tilesZoom = 1;
        }

        private const string DEFAULT_FILENAME = "settings.jsn";

        public void Save(string fileName = DEFAULT_FILENAME)
        {
            string file = DataManager.getAbsolutePath(fileName);
            File.WriteAllText(file, (new JavaScriptSerializer()).Serialize(this));
        }

        public static Settings Load(string fileName = DEFAULT_FILENAME)
        {
            string file = DataManager.getAbsolutePath(fileName);
            Settings t = new Settings();//load defalt
            if (File.Exists(file))
            {
                t = (new JavaScriptSerializer()).Deserialize<Settings>(File.ReadAllText(file));
            }
            return t;
        }
    }
}
