using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Timer
{
    class Maker
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private BitmapImage _image;
        public BitmapImage Image
        {
            get { return _image; }
            set { _image = value; }
        }

        public Maker()
        {
            _name = string.Empty;
            _image = null;
        }
        public Maker(string name)
        {
            _name = name;
            _image = null;
        }
        public Maker(string name, BitmapImage image)
        {
            _name = name;
            _image = image;
        }
    }
}
