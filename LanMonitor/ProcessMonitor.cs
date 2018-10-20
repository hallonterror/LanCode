using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace LanMonitor
{
    class GameInfo
    {
        public string Title { get; set; }
        public string Web{ get; set; }
        public TimeSpan Runtime { get; set; }
        public string RuntimeString
        {
            get {
                return Runtime.ToString(@"hh\:mm\:ss");
            }
        }
        public GameInfo(string name, string adress)
        {
            Title = name;
            Web = adress;
        }
    }

    class ProcessMonitor : INotifyCollectionChanged
    {
        private Dictionary<String, GameInfo> monitoredProceses = new Dictionary<String, GameInfo>();
        private HashSet<String> ignoredProcesses = new HashSet<String>();

        Dictionary<String, GameInfo> runningGames = new Dictionary<string, GameInfo>();
        HashSet<String> potentialGames = new HashSet<string>();

        public event NotifyCollectionChangedEventHandler CollectionChanged;

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
                        monitoredProceses[process] = new GameInfo(title, web);
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

            // Auto update at regular intervals
            var dueTime = TimeSpan.FromSeconds(10);
            var interval = TimeSpan.FromSeconds(30);
            RunPeriodicAsync(UpdateProcesses, dueTime, interval, CancellationToken.None);
        }

        public void UpdateProcesses()
        {
            Process[] processes = Process.GetProcesses();

            runningGames.Clear();
            potentialGames.Clear();
            foreach (Process proc in processes)
            {
                if (monitoredProceses.ContainsKey(proc.ProcessName))
                {
                    var game = monitoredProceses[proc.ProcessName];
                    game.Runtime = DateTime.Now - proc.StartTime;
                    runningGames[proc.ProcessName] = game;
                }
                else if (!ignoredProcesses.Contains(proc.ProcessName))
                {
                    potentialGames.Add(proc.ProcessName);
                }
            }

            CollectionChanged(this, )
        }

        public List<string> UnknownProcesses
        {
            get
            {
                return potentialGames.ToList();
            }
        }

        public Dictionary<string, GameInfo> RunningGames
        {
            get
            {
                return runningGames;
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


    }
}
