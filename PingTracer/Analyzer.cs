using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingTracer
{
    public class PingResultAnalyzer : Livet.NotificationObject, IObserver<PingResult>
    {
        const double VALUE_IGNORE_RATIO = 3;
        const double VALUE_IGNORE_THRESOULD = 100;
        List<PingResult> _acceptedSamples;
        List<PingResult> _skippedSamples;

        public PingResultAnalyzer()
        {
            this.LoadDefaults();
            this.Init();
        }

        protected virtual void Init()
        {
            _acceptedSamples = new List<PingResult>();
            _skippedSamples = new List<PingResult>();
        }

        protected virtual void LoadDefaults()
        {
            this.TargetRange = TimeSpan.FromSeconds(60);
        }

        #region TargetRange
        private TimeSpan _TargetRange;

        public TimeSpan TargetRange
        {
            get
            { return _TargetRange; }
            set
            {
                if (_TargetRange == value)
                    return;
                _TargetRange = value;
                RaisePropertyChanged("TargetRange");
            }
        }
        #endregion

        #region RoundtripVariance
        private double _RoundtripVariance;

        public double RoundtripVariance
        {
            get
            { return _RoundtripVariance; }
            set
            {
                if (_RoundtripVariance == value)
                    return;
                _RoundtripVariance = value;
                RaisePropertyChanged("RoundtripVariance");
            }
        }
        #endregion

        #region RoundtripAverage
        private double _RoundtripAverage;

        public double RoundtripAverage
        {
            get
            { return _RoundtripAverage; }
            set
            {
                if (_RoundtripAverage == value)
                    return;
                _RoundtripAverage = value;
                RaisePropertyChanged("RoundtripAverage");
            }
        }
        #endregion

        #region RoundtripScore
        private double _RoundtripScore;

        public double RoundtripScore
        {
            get
            { return _RoundtripScore; }
            set
            {
                if (_RoundtripScore == value)
                    return;
                _RoundtripScore = value;
                RaisePropertyChanged("RoundtripScore");
            }
        }
        #endregion

        #region IgnoredValuesCount
        private int _IgnoredValuesCount;

        public int IgnoredValuesCount
        {
            get
            { return _IgnoredValuesCount; }
            set
            {
                if (_IgnoredValuesCount == value)
                    return;
                _IgnoredValuesCount = value;
                RaisePropertyChanged("IgnoredValuesCount");
            }
        }
        #endregion

        #region LastRoundtrip
        private double _LastRoundtrip;

        public double LastRoundtrip
        {
            get
            { return _LastRoundtrip; }
            set
            {
                if (_LastRoundtrip == value)
                    return;
                _LastRoundtrip = value;
                RaisePropertyChanged("LastRoundtrip");
            }
        }
        #endregion

        private bool ShouldIgnore(PingResult pr)
        {
            if (this.RoundtripAverage == default(double)) { return false; }
            return pr.RoundtripTime > this.RoundtripAverage * VALUE_IGNORE_RATIO && pr.RoundtripTime > VALUE_IGNORE_THRESOULD;
        }

        private void UpdateValues()
        {
            this.LastRoundtrip = _acceptedSamples.Last().RoundtripTime;
            this.IgnoredValuesCount = _skippedSamples.Count;
            var weightList = this.GetWeight(_acceptedSamples);
            var samples = _acceptedSamples.Select(pr => pr.RoundtripTime).ToList();
            this.RoundtripAverage = samples.WeightedAverage(weightList);
            this.RoundtripVariance = samples.WeightedStandardDiviation(weightList);
            this.RoundtripScore = this.GetScore();
        }

        private void CutOldSamples()
        {
            var limit = DateTime.Now - this.TargetRange;
            this.TruncateListByTime(_acceptedSamples, limit);
            this.TruncateListByTime(_skippedSamples, limit);
        }

        private void TruncateListByTime(List<PingResult> list, DateTime limit)
        {
            var cutIndex = list.FindIndex(pr => pr.TimeStamp > limit);
            if (cutIndex >= 0)
                list.RemoveRange(0, cutIndex);
        }

        protected virtual double GetScore()
        {
            return this.RoundtripAverage + 2 * this.RoundtripVariance;
        }

        private IList<double> GetWeight(ICollection<PingResult> source)
        {
            var now = DateTime.Now;
            return source.Select(pr => 1 - ((now - pr.TimeStamp).TotalMilliseconds / this.TargetRange.TotalMilliseconds)).ToList();
        }


        #region IObserver<PingResult> implementaiton
        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(PingResult pr)
        {
            if (this.ShouldIgnore(pr))
                _skippedSamples.Add(pr);
            else
                _acceptedSamples.Add(pr);
            this.CutOldSamples();
            this.UpdateValues();
        }
        #endregion
    }
}
