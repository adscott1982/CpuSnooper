using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CpuSnoop
{
    internal class MainWindowViewModel : BindableBase
    {
        private string logFileName = "processes.log";

        private List<ProcessInfo> processes;

        public MainWindowViewModel()
        {
            this.processes = new List<ProcessInfo>();
            this.AddAllProcesses();

            Task.Run(async () =>
            {
                while (true)
                {
                    foreach (var process in this.Processes)
                    {
                        process.Refresh();
                    }

                    await Task.Delay(1000);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var sortedProcesses = this.Processes.OrderByDescending(p => p.PeakUsage).ToList();
                        this.Processes = sortedProcesses;

                        if (File.Exists(logFileName))
                        {
                            File.Delete(logFileName);
                        }

                        File.WriteAllLines(logFileName, this.Processes.Select(p => p.ToString()));
                    });
                }
            });

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(10));

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        this.AddAllProcesses();
                    });
                }
            });
        }

        private void AddAllProcesses()
        {
            var processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                if (this.Processes.Any(p => p.Id == process.Id))
                {
                    continue;
                }

                this.Processes.Add(new ProcessInfo(process));
            }
        }

        public List<ProcessInfo> Processes
        {
            get => this.processes;
            set => this.SetProperty(ref this.processes, value);
        }
    }
}
