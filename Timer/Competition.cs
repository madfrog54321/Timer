using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace Timer
{
    class Competition
    {
        private List<Racer> _racers;
        public List<Racer> Racers
        {
            get { return _racers; }
            set { _racers = value; }
        }

        public Competition()
        {
            _racers = new List<Racer>();
        }

        private const string DEFAULT_FILENAME = "competition.jsn";

        public void Save(string fileName = DEFAULT_FILENAME)
        {
            File.WriteAllText(fileName, (new JavaScriptSerializer()).Serialize(this));
        }

        public static Competition Load(string fileName = DEFAULT_FILENAME)
        {
            Competition t;
            if (File.Exists(fileName))
            {
                t = (new JavaScriptSerializer()).Deserialize<Competition>(File.ReadAllText(fileName));
            }
            else
            {
                t = new Competition();
            }
            return t;
        }
    }
}
