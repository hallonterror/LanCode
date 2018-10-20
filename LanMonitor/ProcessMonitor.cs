using System;
using System.Collections.Generic;
using System.Linq;
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
        private Dictionary<String, GameInfo> monitoredProceses = new Dictionary<String, GameInfo>();
        private HashSet<String> ignoredProcesses = new HashSet<String>();

        public ProcessMonitor()
        {
            // Load monitored games
            using (var gameReader = new System.IO.StreamReader("config/monitored_games.xml"))
            {
                var doc = new System.Xml.XmlDocument();
                doc.Load(gameReader);
                foreach (System.Xml.XmlNode game in doc.LastChild.ChildNodes)
                {
                    String title = "", web = "", process = "";
                    foreach(System.Xml.XmlNode tag in game)
                    {
                        if (tag.Name.Equals("title")) { title = tag.InnerText; }
                        else if (tag.Name.Equals("web")) { web = tag.InnerText; }
                        else if (tag.Name.Equals("process")) { process = tag.InnerText; }
                    }
                    if (process != "" && web != "" && title != "")
                        monitoredProceses[process] = new GameInfo(title, web, process);
                }
            }

            // Load ignored processes (not games)
            using (var ignores = new System.IO.StreamReader("config/ignored_processes.txt"))
            {
                String line;
                while ((line = ignores.ReadLine()) != null)
                {
                    ignoredProcesses.Add(line.Trim());
                }
            }

#pragma warning disable 4014
            // Auto update at regular intervals
            var dueTime = TimeSpan.FromSeconds(10);
            var interval = TimeSpan.FromSeconds(30);
            RunPeriodicAsync(UpdateProcesses, dueTime, interval, CancellationToken.None);
#pragma warning restore 4014
        }

        public void UpdateProcesses()
        {
            Process[] processes = Process.GetProcesses();

            Dictionary<string, GameInfo> games = new Dictionary<string, GameInfo>();
            HashSet<string> unknowns = new HashSet<string>();

            foreach (Process proc in processes)
            {
                if (monitoredProceses.ContainsKey(proc.ProcessName))
                {
                    var game = monitoredProceses[proc.ProcessName];
                    game.Runtime = DateTime.Now - proc.StartTime;
                    games[proc.ProcessName] = game;
                }
                else if (!ignoredProcesses.Contains(proc.ProcessName))
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
