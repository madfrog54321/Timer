using System.Collections.Generic;

namespace Timer
{
    public class Racer
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
            set { _times = value; }
        }
        public void addTime(Time time)
        {
            _times.Add(time);
        }
        public void removeTime(int index)
        {
            _times.RemoveAt(index);
        }

        private string _class;
        public string Class
        {
            get { return _class; }
            set { _class = value; }
        }

        public Racer()
        {
            _car = new Car();
            _maker = new Maker();
            _barcode = string.Empty;
            _times = new List<Time>();
        }
        public Racer(Car car, Maker maker, string barcode, string rClass)
        {
            _car = car;
            _maker = maker;
            _barcode = barcode;
            _times = new List<Time>();
            _class = rClass;
        }
        public Racer(string carName, string makerName, string barcode, string rClass)
        {
            _car = new Car(carName);
            _maker = new Maker(makerName);
            _barcode = barcode;
            _times = new List<Time>();
            _class = rClass;
        }
        public Racer(string carName, string carImageUri, string makerName, string makerImageUri, string barcode, string rClass)
        {
            _car = new Car(carName, carImageUri);
            _maker = new Maker(makerName, makerImageUri);
            _barcode = barcode;
            _times = new List<Time>();
            _class = rClass;
        }
        public override bool Equals(object obj)
        {
            if(obj is Racer)
            {
                Racer test = obj as Racer;
                return test.Car.Name == Car.Name;
            }
            return false;
        }
    }
}
