using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timer
{
    public class TimeInfo
    {
        private double _averageTime;
        public double AverageTime
        {
            get { return _averageTime; }
            set { _averageTime = value; }
        }

        private double _bestTime;
        public double BestTime
        {
            get { return _bestTime; }
            set { _bestTime = value; }
        }

        private List<int> _lanesDone;
        public List<int> LanesDone
        {
            get { return _lanesDone; }
            set { _lanesDone = value; }
        }
        
        public bool HasAllLanesDone()
        {
            bool missed = false;
            for (int i = 1; i <= DataManager.Settings.NumberOfLanes && !missed; i++)
            {
                if (!_lanesDone.Contains(i))
                {
                    missed = true;
                }
            }
            return !missed;
        }

        public TimeInfo(double averageTime, double bestTime, List<int> lanesDone)
        {
            _averageTime = averageTime;
            _bestTime = bestTime;
            _lanesDone = lanesDone;
        }
    }
}
