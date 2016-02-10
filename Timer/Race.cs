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
