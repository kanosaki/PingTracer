using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Reactive.Concurrency;
using System.Windows;
using Livet;
using System.Windows.Input;
using Livet.Commands;
using System.Windows.Media;

namespace PingTracer
{
    public class MainViewModel : ViewModel
    {
        const double VALUE_IGNORE_RATIO = 3;
        const double VALUE_IGNORE_THRESOULD = 100;
        public MainViewModel()
        {
            this.LoadDefaults();
            this.Init();
        }

        #region Initializers
        protected virtual void LoadDefaults()
        {
            this.StatisticsRange = TimeSpan.FromSeconds(60);
            this.PollInterval = TimeSpan.FromSeconds(2);
            this.RoundtripMaxItems = 30;
            this.Ttl = 30;
            this.TargetHost = "google.com";
        }

        public void Init()
        {
            _statisticsSamples = new List<PingResult>();
            this.StartTime = DateTime.Now; 
            this.Roundtrips = new ObservableCollection<ChartEntry>();
            this.SkippedResults = new ObservableCollection<PingResult>();
            this.TogglePollCommand = new ViewModelCommand(this.TogglePoll);
            this.InitTracer();
        }

        public void InitTracer()
        {
            if (_tracerToken != null)
            {
                _tracerToken.Dispose();
            }
            _tracer = new Tracer(this.TargetHost, this.Ttl, this.PollInterval);
            _tracerToken = _tracer.PingReplies.ObserveOnDispatcher().Subscribe(
                this.OnNextPingResult,
                (Exception ex) =>
                {
                });
        }
        #endregion

        #region Configuration Properties

        #region RoundtripMaxItems
        private int _RoundtripMaxItems;

        public int RoundtripMaxItems
        {
            get
            { return _RoundtripMaxItems; }
            set
            {
                if (_RoundtripMaxItems == value)
                    return;
                _RoundtripMaxItems = value;
                RaisePropertyChanged("RoundtripMaxItems");
            }
        }
        #endregion

        #region Ttl
        private int _Ttl;

        public int Ttl
        {
            get
            { return _Ttl; }
            set
            {
                if (_Ttl == value)
                    return;
                _Ttl = value;
                RaisePropertyChanged("Ttl");
            }
        }
        #endregion

        #region StatisticsRange
        private TimeSpan _StatisticsRange;

        public TimeSpan StatisticsRange
        {
            get
            { return _StatisticsRange; }
            set
            {
                if (_StatisticsRange == value)
                    return;
                _StatisticsRange = value;
                RaisePropertyChanged("StatisticsRange");
            }
        }
        #endregion

        #region PollInterval
        private TimeSpan _PollInterval;

        public TimeSpan PollInterval
        {
            get
            { return _PollInterval; }
            set
            {
                if (_PollInterval == value)
                    return;
                _PollInterval = value;
                RaisePropertyChanged("PollInterval");
            }
        }
        #endregion

        #region TargetHost
        private string _TargetHost;

        public string TargetHost
        {
            get
            { return _TargetHost; }
            set
            {
                if (_TargetHost == value)
                    return;
                _TargetHost = value;
                RaisePropertyChanged("TargetHost");
            }
        }
        #endregion

        #endregion

        #region Volatile Properties

        Tracer _tracer;

        IDisposable _tracerToken;

        List<PingResult> _statisticsSamples;

        #region StateColor
        private Brush _stateColor;

        public Brush StateColor
        {
            get
            { return _stateColor; }
            set
            {
                if (_stateColor == value)
                    return;
                _stateColor = value;
                RaisePropertyChanged("StateColor");
            }
        }
        #endregion

        #region ToggleButtonContent
        private string _ToggleButtonContent = "Start";

        public string ToggleButtonContent
        {
            get
            { return _ToggleButtonContent; }
            set
            {
                if (_ToggleButtonContent == value)
                    return;
                _ToggleButtonContent = value;
                RaisePropertyChanged("ToggleButtonContent");
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

        #region StartTime
        private DateTime _StartTime;

        public DateTime StartTime
        {
            get
            { return _StartTime; }
            set
            { 
                if (_StartTime == value)
                    return;
                _StartTime = value;
                RaisePropertyChanged("StartTime");
            }
        }
        #endregion

        #region ElapsedTime
        public TimeSpan ElapsedTime
        {
            get
            { return DateTime.Now - this.StartTime; }
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


        public ObservableCollection<PingResult> SkippedResults { get; private set; }
        public ObservableCollection<ChartEntry> Roundtrips { get; private set; }

        public ICommand TogglePollCommand { get; private set; }

        #endregion

        public void TogglePoll()
        {
            _tracer.Enabled = !_tracer.Enabled;
            if (_tracer.Enabled)
            {
                this.StartTime = DateTime.Now;
                this.ToggleButtonContent = "Stop";
            }
            else
            {
                this.ToggleButtonContent = "Start";
            }
        }

        protected void OnNextPingResult(PingResult pr)
        {
            if (this.ShouldIgnore(pr.RoundtripTime))
            {
                this.SkippedResults.Add(pr);
                this.TruncateSamples(this.SkippedResults);
                this.IgnoredValuesCount = this.SkippedResults.Count;
            }
            else
            {
                this.LastRoundtrip = pr.RoundtripTime;
                this.TruncateSamples(this.SkippedResults);
                this.IgnoredValuesCount = this.SkippedResults.Count;
                this.UpdateStatistics(pr);
                var label = pr.TimeStamp.ToString("mm:ss");
                var time = (int)pr.RoundtripTime;
                this.PushRoundtripItem(label, time);
                this.UpdateProgressBar(pr);
                this.RaisePropertyChanged(() => this.ElapsedTime);
            }
        }

        private bool ShouldIgnore(double roundtrip)
        {
            if (this.RoundtripAverage == default(double)) { return false; }
            return roundtrip > this.RoundtripAverage * VALUE_IGNORE_RATIO && roundtrip > VALUE_IGNORE_THRESOULD;
        }

        public void UpdateStatistics(PingResult result)
        {
            _statisticsSamples.Add(result);
            this.TruncateSamples(_statisticsSamples);
            var samples = _statisticsSamples.Select(pr => pr.RoundtripTime).ToList();
            var now = DateTime.Now;
            var maxTimeDelta = now - _statisticsSamples.First().TimeStamp;
            var weights = _statisticsSamples
                .Select(pr =>  1 - ((now - pr.TimeStamp).TotalMilliseconds / maxTimeDelta.TotalMilliseconds)).ToList();
            this.RoundtripAverage = samples.WeightedAverage(weights);
            this.RoundtripVariance = samples.WeightedStandardDiviation(weights);
            this.RoundtripScore = this.RoundtripAverage + this.RoundtripVariance;
        }

        private void TruncateSamples(IList<PingResult> target)
        {
            if (target.Count > 0 && target.First().TimeStamp < DateTime.Now - this.StatisticsRange)
                target.RemoveAt(0);
        }

        public void UpdateProgressBar(PingResult result)
        {
            this.StateColor = this.GetScoreColor(this.RoundtripScore);
        }

        protected Brush GetScoreColor(double score)
        {
            if (score > 50)
                return Brushes.Red;
            else if (score > 40)
                return Brushes.OrangeRed;
            else if (score > 30)
                return Brushes.Orange;
            else if (score > 20)
                return Brushes.Gold;
            else if (score > 10)
                return Brushes.Green;
            else
                return Brushes.Blue;
        }

        public void PushRoundtripItem(string tag, int value)
        {
            if (this.Roundtrips.Count > this.RoundtripMaxItems)
            {
                this.Roundtrips.RemoveAt(0);
            }
            this.Roundtrips.Add(new ChartEntry()
            {
                Label = tag,
                Current = value,
                Average = this.RoundtripAverage,
                Score = this.RoundtripScore
            });
        }
    }

    public class ChartEntry
    {
        public string Label { get; set; }
        public double Current { get; set; }
        public double Average { get; set; }
        public double Score { get; set; }
    }
}
