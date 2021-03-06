﻿using System.Collections.Generic;

namespace Timer
{
    class RaceManager
    {
        private enum SpecialCallBack
        {
            EmptyLane = -111
        }

        public delegate void raceResultsHandler(Dictionary<int, Time> results);
        public event raceResultsHandler onGotRace;
        private void triggerGotRace(Dictionary<int, Time> results)
        {
            raceResultsHandler handler = onGotRace;
            if (handler != null)
            {
                handler(results);
            }
        }
        
        public delegate void raceFullHandler();
        public event raceFullHandler onRaceIsFull;
        private void triggerRaceIsFull()
        {
            raceFullHandler handler = onRaceIsFull;
            if (handler != null)
            {
                handler();
            }
        }

        public delegate void readyForNextRaceHandler();
        public event readyForNextRaceHandler onReadyForNextRace;
        private void triggerReadyForNextRace()
        {
            readyForNextRaceHandler handler = onReadyForNextRace;
            if (handler != null)
            {
                handler();
            }
        }

        private int _numLanes;
        private List<int> _raceCallBack;//hold index of car in another list

        public RaceManager(int numLanes)
        {
            DataManager.TrackTimer.OnGotRace += _timer_OnGotRace;
            _raceCallBack = new List<int>();
            _numLanes = numLanes;
        }

        private void _timer_OnGotRace(bool success, Race race)
        {
            if (success)//got times from tack
            {
                if (race.Times.Length == _numLanes)//check for correct lane count
                {
                    Dictionary<int, Time> _results = new Dictionary<int, Time>();
                    for (int i = 0; i < _numLanes; i++)
                    {
                        if(_raceCallBack[i] != (int)SpecialCallBack.EmptyLane)
                        {
                            _results.Add(_raceCallBack[i], race.Times[i]);//get to time of the lane, and link it with the callback number
                        }
                    }
                    forgetRace();//the race was finished, so remove callback
                    triggerGotRace(_results);//score race
                    triggerReadyForNextRace();//signal ready for next race
                }
                else
                {
                    DataManager.MessageProvider.showError("Error mapping times", "The Track Timer returned more lanes than the Race Manager was told about");
                }
            }
            else
            {
                DataManager.MessageProvider.showError("Error getting times", "The Track Timer failed to get the times");
            }
        }

        public int NumberOfLanes
        {
            get { return _numLanes; }
        }

        public void forgetRace()
        {
            DataManager.TrackTimer.stopWaitingForRace();
            DataManager.TrackTimer.reset();
            DataManager.TrackTimer.resetMask();//clear mask on phisical timer
            _raceCallBack.Clear();
        }

        private void notifyTimer()
        {
            DataManager.TrackTimer.getNextRace();
        }

        public enum MakeNextReturn { CallBackUsed, RaceFull, Added, NotPassed }

        public MakeNextReturn makeNext_CarLane(int callBack)
        {
            if (DataManager.Competition.Racers[callBack].PassedInspection)
            {
                if (!_raceCallBack.Contains(callBack))
                {
                    if (!raceIsFull)
                    {
                        _raceCallBack.Add(callBack);
                        if (raceIsFull)
                        {
                            notifyTimer();
                            triggerRaceIsFull();
                        }
                        return MakeNextReturn.Added;
                    }
                    else
                    {
                        return MakeNextReturn.RaceFull;
                    }
                }
                else
                {
                    return MakeNextReturn.CallBackUsed;
                }
            }
            else
            {
                return MakeNextReturn.NotPassed;
            }
        }

        public MakeNextReturn makeNext_EmptyLane()
        {
            if (!raceIsFull)
            {
                DataManager.TrackTimer.maskOffLane(nextOpenLane);
                _raceCallBack.Add((int)SpecialCallBack.EmptyLane);
                if (raceIsFull)
                {
                    notifyTimer();
                    triggerRaceIsFull();
                }
                return MakeNextReturn.Added;
            }
            else
            {
                return MakeNextReturn.RaceFull;
            }
        }

        public int nextOpenLane
        {
            get { return _raceCallBack.Count + 1; }
        }

        public bool raceIsFull
        {
            get { return nextOpenLane > _numLanes; }
        }
    }
}
