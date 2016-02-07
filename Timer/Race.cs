using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timer
{
    class Race
    {
        private Time[] _times;
        public Time[] Times
        {
            get { return _times; }
        }

        public Race(Time[] times)
        {
            _times = times;
        }
    }
}
