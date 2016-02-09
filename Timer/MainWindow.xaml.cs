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
using System.Windows.Navigation;
using System.IO.Ports;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace Timer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            listBox.Items.Clear();
            foreach (Racer racer in DataManager.Racers)
            {
                listBox.Items.Add(racer.Car.Name + ", " + racer.Maker.Name + ", " + racer.Barcode);
            }

            tbCommand.Focusable = true;

            label1.Content = "Waiting for race check in...";

            updatePorts();
        }

        private void RaceManager_onReadyForNextRace()
        {
            if (Dispatcher.CheckAccess())
            {
                label1.Content = "Waiting for race check in...";
                listBox1.Items.Clear();
            }
            else
            {
                Dispatcher.Invoke(new Action(RaceManager_onReadyForNextRace));
            }
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (DataManager.tryConnectTimer(cboPorts.Text))
            {
                btnConnect.IsEnabled = false;
                tbCommand.IsEnabled = true;
                DataManager.RaceManager.onReadyForNextRace += RaceManager_onReadyForNextRace;
                DataManager.RaceManager.onRaceIsFull += RaceManager_onRaceIsFull;
            }
        }

        private void RaceManager_onRaceIsFull()
        {
            if (Dispatcher.CheckAccess())
            {
                label1.Content = "Waiting for race completion...";
            }
            else
            {
                Dispatcher.Invoke(new Action(RaceManager_onRaceIsFull));
            }
        }

        private void updatePorts()
        {
            string[] ports = DataManager.getPorts();
            foreach (string port in ports)
            {
                cboPorts.Items.Add(port);
            }
            cboPorts.SelectedIndex = 0;
        }

        private void tbCommand_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                if (btnLockKeyboard.Content.ToString() == "Lock")
                {
                    btnLockKeyboard.Content = "Unlock";
                }
                runBarcodeCommand(tbCommand.Text);
                tbCommand.Text = string.Empty;
            }
            else if (e.Key == Key.Escape)
            {
                if (btnLockKeyboard.Content.ToString() == "Unlock")
                {
                    btnLockKeyboard.Content = "Lock";
                }
            }
        }

        private void addRacer(int index)
        {
            RaceManager.MakeNextReturn result = DataManager.RaceManager.makeNext_CarLane(index);

            if (result == RaceManager.MakeNextReturn.Added)
            {
                listBox1.Items.Add(DataManager.Racers[index].Car.Name);
            } //error handling
            else if (result == RaceManager.MakeNextReturn.CallBackUsed)
            {
                MessageBox.Show("Racer " + DataManager.Racers[index].Car.Name + " has allready been entered into this race", "Main Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (result == RaceManager.MakeNextReturn.RaceFull)
            {
                MessageBox.Show("Cannot enter more than 6 racers into this race", "Main Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void addEmpty()
        {
            RaceManager.MakeNextReturn result = DataManager.RaceManager.makeNext_EmptyLane();

            if (result == RaceManager.MakeNextReturn.Added)
            {
                listBox1.Items.Add("Empty");
            } //error handling
            else if (result == RaceManager.MakeNextReturn.RaceFull)
            {
                MessageBox.Show("Cannot enter more than 6 racers into this race", "Main Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void runBarcodeCommand(string barcode)
        {
            if (barcode == "reset")
            {
                DataManager.RaceManager.forgetRace();
            }
            else if (barcode == "mask")
            {
                addEmpty();
            }
            else
            {
                //try a car barcode
                bool found = false;
                
                for (int i = 0; i < DataManager.Racers.Count && !found; i++)
                {
                    if(DataManager.Racers[i].Barcode == barcode)
                    {
                        found = true;
                        addRacer(i);
                    }
                }

                if (!found)
                {
                    MessageBox.Show("The command code [" + barcode + "] does not belong to a command", "Main Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void btnLockKeyboard_Click(object sender, RoutedEventArgs e)
        {
            if(btnLockKeyboard.Content.ToString() == "Lock")
            {
                btnLockKeyboard.Content = "Unlock";
                Keyboard.Focus(tbCommand);
            } 
            else
            {
                btnLockKeyboard.Content = "Lock";
            }
        }

        private void tbCommand_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (btnLockKeyboard.Content.ToString() == "Unlock")
            {
                Storyboard sb = this.FindResource("blinkLabel") as Storyboard;
                Storyboard.SetTarget(sb, btnLockKeyboard);
                sb.Begin();
                //Keyboard.Focus(tbCommand);
                e.Handled = true;
                if(e.NewFocus == listBox)
                {

                }
            }
        }

        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            if (btnLockKeyboard.Content.ToString() == "Unlock")
            {
                Keyboard.Focus(tbCommand);
            }
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            listBox2.Items.Clear();
            foreach (Time time in DataManager.Racers[listBox.SelectedIndex].Times)
            {
                listBox2.Items.Add(time.Speed + "s, Lane: " + time.Lane + ", Place: " + time.Place);
            }
        }
    }
}
