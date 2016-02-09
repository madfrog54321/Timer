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

        public Settings()
        {
            _numLanes = 6;
        }

        private const string DEFAULT_FILENAME = "settings.jsn";

        public void Save(string fileName = DEFAULT_FILENAME)
        {
            File.WriteAllText(fileName, (new JavaScriptSerializer()).Serialize(this));
        }

        public static Settings Load(string fileName = DEFAULT_FILENAME)
        {
            Settings t;
            if (File.Exists(fileName))
            {
                t = (new JavaScriptSerializer()).Deserialize<Settings>(File.ReadAllText(fileName));
            }
            else
            {
                t = new Settings();
            }
            return t;
        }
    }
}
