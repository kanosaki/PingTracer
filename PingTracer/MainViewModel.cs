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
using Livet.EventListeners;
using System.Windows.Input;
using Livet.Commands;
using System.Windows.Media;

namespace PingTracer
{
    public class MainViewModel : ViewModel
    {

        public MainViewModel()
        {
            this.LoadDefaults();
            this.Init();
        }

        #region Initializers
        protected virtual void LoadDefaults()
        {
            this.PollInterval = TimeSpan.FromSeconds(2);
            this.RoundtripMaxItems = 30;
            this.Ttl = 30;
            this.TargetHost = "google.com";
        }

        public void Init()
        {
            _analyzer = new PingResultAnalyzer();
            _analyzerAdapter = new PropertyChangedEventListener(_analyzer, (s, e) => this.RaisePropertyChanged(e.PropertyName));
            this.StartTime = DateTime.Now;
            this.Roundtrips = new ObservableCollection<ChartEntry>();
            this.TogglePollCommand = new ViewModelCommand(this.TogglePoll);
            this.InitTracer();
        }

        public void InitTracer()
        {
            _tracer = new Tracer(this.TargetHost, this.Ttl, this.PollInterval);
            _tracer.PingReplies.ObserveOnDispatcher().Subscribe(
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

        PingResultAnalyzer _analyzer;
        PropertyChangedEventListener _analyzerAdapter;

        #region Analyzer adapting properties
        public TimeSpan AnalyzerTargetRange { get { return _analyzer.TargetRange; } }
        public double RoundtripAverage { get { return _analyzer.RoundtripAverage; } }
        public double RoundtripVariance { get { return _analyzer.RoundtripVariance; } }
        public double RoundtripScore { get { return _analyzer.RoundtripScore; } }
        public double LastRoundtrip { get { return _analyzer.LastRoundtrip; } }
        public int IgnoredValuesCount { get { return _analyzer.IgnoredValuesCount; } }


        #endregion

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

        #region SignalLevel変更通知プロパティ
        private int _SignalLevel;

        public int SignalLevel
        {
            get
            { return _SignalLevel; }
            set
            {
                if (_SignalLevel == value)
                    return;
                _SignalLevel = value;
                RaisePropertyChanged("SignalLevel");
            }
        }
        #endregion


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
            _analyzer.OnNext(pr);
            var label = pr.TimeStamp.ToString("mm:ss");
            var time = (int)pr.RoundtripTime;
            this.PushRoundtripItem(label, time);
            this.UpdateVisualizers(pr);
            this.RaisePropertyChanged(() => this.ElapsedTime);
        }

        public void UpdateVisualizers(PingResult result)
        {
            this.StateColor = this.GetScoreColor(this.RoundtripScore);
            this.SignalLevel = Math.Max(5 - (int)Math.Floor(this.RoundtripScore / 10), 0);
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
                return Brushes.RoyalBlue;
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
