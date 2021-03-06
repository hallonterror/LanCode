﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.ObjectModel;
//using System.Collections.Specialized;

namespace LanMonitor
{
    class GameInfo
    {
        public GameInfo(string name, string adress, string process)
        {
            Title = name;
            Web = adress;
            Process = process;
        }

        public string Title { get; set; }
        public string Web{ get; set; }
        public string Process{ get; set; }
        public TimeSpan Runtime { get; set; }
        public string RuntimeString
        {
            get {
                return Runtime.ToString(@"hh\:mm\:ss");
            }
        }
    }

    class ProcessMonitor : INotifyPropertyChanged
    {
        // Configuration variables
        public HashSet<string> IgnoredProcesses { get; set; }
        public Dictionary<string, GameInfo> MonitoredProceses { get; set; }

        public ProcessMonitor()
        {
            // Load monitored games
            MonitoredProceses = new Dictionary<string, GameInfo>();
            var doc = XDocument.Load("config/monitored_games.xml");
            foreach (XElement game in doc.Element("games").Elements())
            {
                string title = game.Element("title").Value;
                string web = game.Element("web").Value;
                string process = game.Element("process").Value;
                MonitoredProceses[process] = new GameInfo(title, web, process);
            }

            // Load ignored processes (not games)
            IgnoredProcesses = new HashSet<string>();
            using (var ignores = new System.IO.StreamReader("config/ignored_processes.txt"))
            {
                String line;
                while ((line = ignores.ReadLine()) != null)
                {
                    IgnoredProcesses.Add(line.Trim());
                }
            }

#pragma warning disable 4014
            // Auto update at regular intervals
            RefreshTime = 30;
            var dueTime = TimeSpan.FromSeconds(10);
            var interval = TimeSpan.FromSeconds(RefreshTime);
            RefreshTask = RunPeriodicAsync(UpdateProcesses, dueTime, interval, CancellationToken.None);
#pragma warning restore 4014
        }

        public void UpdateProcesses()
        {
            Process[] processes = Process.GetProcesses();

            Dictionary<string, GameInfo> games = new Dictionary<string, GameInfo>();
            HashSet<string> unknowns = new HashSet<string>();

            foreach (Process proc in processes)
            {
                if (MonitoredProceses.ContainsKey(proc.ProcessName))
                {
                    var game = MonitoredProceses[proc.ProcessName];
                    games[proc.ProcessName] = game;
                    try
                    {
                        game.Runtime = DateTime.Now - proc.StartTime;
                    }
                    catch
                    {
                        game.Runtime = new TimeSpan(0);
                    }
                }
                else if (!IgnoredProcesses.Contains(proc.ProcessName))
                {
                    unknowns.Add(proc.ProcessName);
                }
            }

            // Update observables
            RunningGames.Clear();
            UnknownProcesses.Clear();
            foreach(KeyValuePair<string, GameInfo> game in games)
            {
                RunningGames.Add(game.Value);
            }
            foreach(string proc in unknowns)
            {
                UnknownProcesses.Add(proc);
            }
        }

        public ObservableCollection<GameInfo> RunningGames { get; } = new ObservableCollection<GameInfo>();
        public ObservableCollection<string> UnknownProcesses { get; } = new ObservableCollection<string>();

        static Task RefreshTask { get; set; }

        private int refreshTime;
        public int RefreshTime {
            get
            {
                return refreshTime;
            }
            set
            {
                if (value != refreshTime)
                {
                    refreshTime = value;
                    OnPropertyChanged("RefreshTime");
                }
            }
        }

        private static async Task RunPeriodicAsync(Action onTick,
                                           TimeSpan dueTime,
                                           TimeSpan interval,
                                           CancellationToken token)
        {
            // Initial wait time before we begin the periodic loop.
            if (dueTime > TimeSpan.Zero)
                await Task.Delay(dueTime, token);

            // Repeat this loop until cancelled.
            while (!token.IsCancellationRequested)
            {
                // Call our onTick function.
                onTick?.Invoke();

                // Wait to repeat again.
                if (interval > TimeSpan.Zero)
                    await Task.Delay(interval, token);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
