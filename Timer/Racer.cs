using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Timer
{
    class Racer
    {
        private Car _car;
        public Car Car
        {
            get { return _car; }
            set { _car = value; }
        }

        private Maker _maker;
        public Maker Maker
        {
            get { return _maker; }
            set { _maker = value; }
        }

        private string _barcode;
        public string Barcode
        {
            get { return _barcode; }
            set { _barcode = value; }
        }

        private List<Time> _times;
        public List<Time> Times
        {
            get { return _times; }
        }
        public void addTime(Time time)
        {
            _times.Add(time);
        }
        public void removeTime(int index)
        {
            _times.RemoveAt(index);
        }

        public Racer()
        {
            _car = new Car();
            _maker = new Maker();
            _barcode = string.Empty;
            _times = new List<Time>();
        }
        public Racer(Car car, Maker maker, string barcode)
        {
            _car = car;
            _maker = maker;
            _barcode = barcode;
            _times = new List<Time>();
        }
        public Racer(string carName, string makerName, string barcode)
        {
            _car = new Car(carName);
            _maker = new Maker(makerName);
            _barcode = barcode;
            _times = new List<Time>();
        }
        public Racer(string carName, BitmapImage carImage, string makerName, BitmapImage makerImage, string barcode)
        {
            _car = new Car(carName, carImage);
            _maker = new Maker(makerName, makerImage);
            _barcode = barcode;
            _times = new List<Time>();
        }
    }
}
