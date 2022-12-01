using Prism.Mvvm;
using System;
using System.Diagnostics;

namespace CpuSnoop
{
    internal class ProcessInfo : BindableBase
    {
        private Process process;
        private DateTime? lastTime;
        private TimeSpan? lastTotalProcessorTime;

        public ProcessInfo(Process process)
        {
            this.process = process;

            try
            {
                this.lastTime = process.StartTime;
                this.lastTotalProcessorTime = process.TotalProcessorTime;
                this.Path = process.MainModule?.FileName;
            }
            catch 
            { 
            }
            
        }

        public int Id => this.process.Id;

        public string Name => this.process.ProcessName;

        public double ProcessorUsage { get; private set; }

        public double PeakUsage { get; private set; }

        public string Path { get; private set; }

        public bool Denied { get; private set; }

        public void Refresh()
        {
            if (this.Denied)
            {
                return;
            }

            try
            {
                var curTime = DateTime.Now;
                var curTotalProcessorTime = this.process.TotalProcessorTime;

                if (this.lastTime.HasValue)
                {
                    var timeElapsed = (curTime - this.lastTime).Value.TotalMilliseconds;
                    var additionalCpuTime = (curTotalProcessorTime - this.lastTotalProcessorTime).Value.TotalMilliseconds;

                    var scaledCpuPercentage = (additionalCpuTime / timeElapsed) / (double)Environment.ProcessorCount;

                    this.ProcessorUsage = scaledCpuPercentage * 100;

                    if (this.ProcessorUsage > this.PeakUsage)
                    {
                        this.PeakUsage = this.ProcessorUsage;
                    }
                }

                lastTime = curTime;
                lastTotalProcessorTime = curTotalProcessorTime;

                this.RaisePropertyChanged(nameof(this.ProcessorUsage));
            }
            catch(Exception e)
            {
                this.Denied= true;
            }
        }

        public override string ToString()
        {
            return $"{Name} | {this.PeakUsage:F2} | {this.Path}";
        }
    }
}
