using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LanMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ProcessMonitor procMon = new ProcessMonitor();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = procMon;
            UpdateProcesses();
        }

        public void UpdateProcesses()
        {
            procMon.UpdateProcesses();

            foreach (String name in procMon.UnknownProcesses)
            {
                Console.WriteLine(name);
            }
        }

        private void RefreshManual(object sender, RoutedEventArgs e)
        {
            procMon.UpdateProcesses();
        }

        private string runningGamesUpdate()
        {
            ServerCommunication comm = new ServerCommunication();

            Task<string> getGameCount = comm.Get(@"/games/running/count");
            //return int.Parse(getGameCount.Result);

            Dictionary<string, string> payload = new Dictionary<string, string>();
            payload.Add("GameId", "csgo");
            payload.Add("UserId", "uid");
            payload.Add("RunTime", "1000");
            Task<string> post = comm.Post(@"/users/test_user", payload);
            string postStr = post.Result;

            return getGameCount.Result;
        }

        private void GetStatus(object sender, RoutedEventArgs e)
        {
            var gameCount = runningGamesUpdate();
            Console.WriteLine(gameCount);
        }

        private void add_game_Click(object sender, RoutedEventArgs e)
        {
            // Retrieve the selected list item
            if (unknown_list.SelectedValue != null)
            {
                AddGame(unknown_list.SelectedValue.ToString());
            }
        }
        private void AddGame(string processName)
        {
            var addGameWindow = new LanMonitor.AddGameWindow(processName);
            addGameWindow.ShowDialog();
            if (addGameWindow.DialogResult == true)
            {
                string fpath = "config/monitored_games.xml";
                XDocument doc = XDocument.Load(fpath);

                XElement newGame = new XElement("game");
                newGame.Add(new XElement("title", addGameWindow.GameName));
                newGame.Add(new XElement("web", addGameWindow.GameLink));
                newGame.Add(new XElement("process", addGameWindow.GameProcess));
                newGame.Add(new XElement("local", true));
                doc.Element("games").Add(newGame);
                doc.Save(fpath);

                procMon.MonitoredProceses.Add(
                    addGameWindow.GameProcess,
                    new GameInfo(
                        addGameWindow.GameName,
                        addGameWindow.GameLink,
                        addGameWindow.GameProcess));
                procMon.UpdateProcesses();
            }
        }

        private void remove_process_Click(object sender, RoutedEventArgs e)
        {
            if (unknown_list.SelectedValue != null)
            {
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    var processName = unknown_list.SelectedValue.ToString();
                    using (var sw = new System.IO.StreamWriter("config/ignored_processes.txt", true))
                    {
                        sw.WriteLine(processName);
                    }
                    procMon.IgnoredProcesses.Add(processName);
                    procMon.UpdateProcesses();
                }
            }
        }
    }
}
