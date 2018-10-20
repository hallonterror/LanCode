using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;

namespace LanMonitor
{
    /// <summary>
    /// Interaction logic for AddGameWindow.xaml
    /// </summary>
    public partial class AddGameWindow : Window, INotifyPropertyChanged
    {
        private string gameName, gameLink, gameProcess;
        public string GameName
        {
            get { return gameName; }
            set
            {
                if (value == gameName)
                    return;
                gameName = value;
                OnPropertyChanged("GameName");

                // Suggest a link to the game
                string igdb = "https://www.igdb.com/games/";
                string path = gameName.ToLower().Trim().Replace(' ', '-');
                GameLink = igdb + path;
            }
        }
        public string GameLink
        {
            get { return gameLink; }
            set
            {
                if (value == gameLink)
                    return;
                gameLink = value;
                OnPropertyChanged("GameLink");
            }
        }
        public string GameProcess
        {
            get { return gameProcess; }
            set
            {
                if (value == gameProcess)
                    return;
                gameProcess = value;
                OnPropertyChanged("GameProcess");
            }
        }

        public AddGameWindow(string process)
        {
            InitializeComponent();
            DataContext = this;
            GameProcess = process;
        }

        private void TestLink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(GameLink);
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
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
