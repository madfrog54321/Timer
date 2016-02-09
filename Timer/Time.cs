using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Timer
{
    class Time
    {
        private float _speed;
        public float Speed
        {
            get { return _speed; }
            set { _speed = value; }
        }
        private int _lane;
        public int Lane
        {
            get { return _lane; }
            set { _lane = value; }
        }
        private int _place;
        public int Place
        {
            get { return _place; }
            set { _place = value; }
        }

        public Time(int lane, float speed, int place)
        {
            _lane = lane;
            _speed = speed;
            _place = place;
        }

        public Time()
        {
            //for json deserialize
        }
    }
}
