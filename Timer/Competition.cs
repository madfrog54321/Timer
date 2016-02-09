using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace Timer
{
    class Competition
    {
        private List<Racer> _racers = new List<Racer>();
        public List<Racer> Racers
        {
            get { return _racers; }
            set { _racers = value; }
        }

        public Competition()
        {
            //_racers = new List<Racer>();
            //_racers.Add(new Racer("car", "maker", "barcode"));
        }

        private const string DEFAULT_FILENAME = "competition.jsn";

        public void Save(string fileName = DEFAULT_FILENAME)
        {
            File.WriteAllText(fileName, (new JavaScriptSerializer()).Serialize(this));
        }

        public static Competition Load(string fileName = DEFAULT_FILENAME)
        {
            Competition t = new Competition();
            JavaScriptSerializer java = new JavaScriptSerializer();
            if (File.Exists(fileName))
            {
                t = java.Deserialize<Competition>(File.ReadAllText(fileName));
            }
            else
            {
                
            }
            return t;
        }
    }
}
