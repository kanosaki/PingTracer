using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Threading;
using System.Reactive;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Reactive.Concurrency;
using System.Net;
using System.Diagnostics;

namespace PingTracer
{
    public class Tracer
    {
        public IObservable<PingResult> PingReplies { get; private set; }
        public TimeSpan Interval { get; set; }
        public string TargetHost { get; set; }
        public int Ttl { get; set; }
        public bool Enabled { get; set; }

        /// <summary>
        /// ms
        /// </summary>
        public int PingTimeout { get; set; }

        Ping _ping;
        PingOptions _pingOptions;
        byte[] _data = BitConverter.GetBytes(1);
        IPAddress _target;

        public Tracer(string target, int ttl, TimeSpan interval)
        {
            this.TargetHost = target;
            this.Ttl = ttl;
            this.Interval = interval;
            this.PingTimeout = 1000;
            this.Init();
            this.InitWorker();
        }

        private void InitWorker()
        {
            this.PingReplies =
               Observable.Interval(this.Interval, TaskPoolScheduler.Default)
                   .StartWith(0)
                   .Where(_ => this.Enabled)
                   .Select(this.ExecPing);
        }

        private void Init()
        {
            _ping = new Ping();
            _pingOptions = new PingOptions(this.Ttl, true);
            _target = Dns.GetHostEntry(this.TargetHost).AddressList.First();
        }

        protected PingResult ExecPing(long ping_count)
        {
            var sw = new Stopwatch();
            sw.Start();
            var result = new PingResult(_ping.Send(_target, this.PingTimeout, _data, _pingOptions));
            sw.Stop();
            result.InjectRoudTripTime(sw.Elapsed);
            return result;
        }

        public void Start()
        {
            this.Enabled = true;
        }
        public void Stop()
        {
            this.Enabled = false;
        }
    }

    /// <summary>
    /// Proxy class for PingReply
    /// </summary>
    public class PingResult
    {

        public PingResult(PingReply wrapped)
        {
            _wraped = wrapped;
            this.TimeStamp = DateTime.Now;
        }

        PingReply _wraped;
        TimeSpan _injectedTime;
        public double RoundtripTime
        {
            get { return this.CalcRoundTripTime(); }
        }
        public IPAddress Address { get { return _wraped.Address; } }
        public byte[] Buffer { get { return _wraped.Buffer; } }
        public IPStatus Status { get { return _wraped.Status; } }
        public PingOptions Options { get { return _wraped.Options; } }
        public DateTime TimeStamp { get; private set; }

        public void InjectRoudTripTime(TimeSpan time)
        {
            _injectedTime = time;
        }

        protected virtual double CalcRoundTripTime()
        {
            if (_wraped.Status == IPStatus.Success)
            {
                return _wraped.RoundtripTime;

            }
            else if (_wraped.Status == IPStatus.TtlExpired)
            {
                return _injectedTime.TotalMilliseconds;
            }
            else
            {
                if (_wraped.RoundtripTime == 0)
                {
                    return _injectedTime.TotalMilliseconds;
                }
                else
                {
                    return _wraped.RoundtripTime;
                }
            }
        }


    }
}
