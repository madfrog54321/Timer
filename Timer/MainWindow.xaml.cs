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
        private TrackTimer _timer;
        private List<Racer> _racers;
        private List<int> _currentRace;

        public MainWindow()
        {
            InitializeComponent();

            _currentRace = new List<int>();

            _racers = new List<Racer>();
            _racers.Add(new Racer("test car 1", "maker 1", "1"));
            _racers.Add(new Racer("test car 2", "maker 2", "2"));
            _racers.Add(new Racer("test car 3", "maker 3", "3"));
            _racers.Add(new Racer("test car 4", "maker 4", "4"));
            _racers.Add(new Racer("test car 5", "maker 5", "5"));
            _racers.Add(new Racer("test car 6", "maker 6", "6"));
            _racers.Add(new Racer("test car 7", "maker 7", "7"));
            _racers.Add(new Racer("test car 8", "maker 8", "8"));
            _racers.Add(new Racer("test car 9", "maker 9", "9"));
            _racers.Add(new Racer("test car 10", "maker 10", "10"));
            listBox.Items.Clear();
            foreach (Racer racer in _racers)
            {
                listBox.Items.Add(racer.Car.Name + ", " + racer.Maker.Name + ", " + racer.Barcode);
            }

            tbCommand.Focusable = true;

            clearRacers();

            updatePorts();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            _timer = new TrackTimer(cboPorts.Text);
            if (_timer.Connected)
            {
                btnConnect.IsEnabled = false;
                tbCommand.IsEnabled = true;
                _timer.OnGotRace += _timer_OnGotRace;
                _timer.getNextRace();
            }
        }

        private void _timer_OnGotRace(bool success, Race race)
        {
            if (Dispatcher.CheckAccess())
            {
                if (success)
                {
                    if (_currentRace.Count > 0)
                    {
                        Time[] times = race.Times;
                        for (int i = 0; i < times.Length; i++)
                        {
                            if (_currentRace[i] != -2)
                            {
                                _racers[_currentRace[i]].addTime(times[i]);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("A race has been recieved out of timing", "Main Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                clearRacers();
            }
            else
            {
                Dispatcher.Invoke(new Action<bool, Race>(_timer_OnGotRace), new object[] { success, race });
            }
        }

        private void updatePorts()
        {
            string[] ArrayComPortsNames = null;
            int index = -1;
            string ComPortName = null;

            ArrayComPortsNames = SerialPort.GetPortNames();
            do
            {
                index += 1;
                cboPorts.Items.Add(ArrayComPortsNames[index]);
            }
            while (!((ArrayComPortsNames[index] == ComPortName) || (index == ArrayComPortsNames.GetUpperBound(0))));

            Array.Sort(ArrayComPortsNames);

            //want to get first out
            if (index == ArrayComPortsNames.GetUpperBound(0))
            {
                ComPortName = ArrayComPortsNames[0];
            }
            cboPorts.Text = ArrayComPortsNames[0];
        }

        private void btnGetNext_Click(object sender, RoutedEventArgs e)
        {
            _timer.getNextRace();
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

        private void clearRacers()
        {
            if (_timer != null && _timer.Connected) { _timer.resetMask(); }
            listBox1.Items.Clear();
            _currentRace.Clear();
            label1.Content = "Waiting for race check in...";
        }

        private void addRacer(int index)
        {
            if (!_currentRace.Contains(index))
            {
                if (listBox1.Items.Count < 6)
                {
                    listBox1.Items.Add(_racers[index].Car.Name);
                    _currentRace.Add(index);
                    if (listBox1.Items.Count == 6)
                    {
                        label1.Content = "Waiting for race completion...";
                        _timer.getNextRace();
                    }
                }
                else
                {
                    MessageBox.Show("Cannot enter more than 6 racers into this race", "Main Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Racer " + _racers[index].Car.Name + " has allready been entered into this race", "Main Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void runBarcodeCommand(string barcode)
        {
            if (barcode == "reset")
            {
                clearRacers();
            }
            else if (barcode == "mask")
            {
                if(_currentRace.Count < 6)
                {
                    _timer.maskOffLane(_currentRace.Count + 1);
                    listBox1.Items.Add("Empty");
                    _currentRace.Add(-2);
                    if (listBox1.Items.Count == 6)
                    {
                        label1.Content = "Waiting for race completion...";
                        _timer.getNextRace();
                    }
                }
            }
            else
            {
                bool found = false;

                //for (int i = 1; i < 7 && !found; i++) {
                //    if(barcode == "mask" + i)
                //    {
                //        found = true;
                //        _timer.maskOffLane(i);
                //    }
                //}

                for (int i = 0; i < _racers.Count && !found; i++)
                {
                    if(_racers[i].Barcode == barcode)
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
            foreach (Time time in _racers[listBox.SelectedIndex].Times)
            {
                listBox2.Items.Add(time.Speed + "s, Lane: " + time.Lane + ", Place: " + time.Place);
            }
        }
    }
}
