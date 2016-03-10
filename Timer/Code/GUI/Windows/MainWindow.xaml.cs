using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using AForge.Video.DirectShow;

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

            VideoCaptureDevice device = new VideoCaptureDevice();

            updateRacerList();

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
                updateTimeList();
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
                listBox1.Items.Add(DataManager.Competition.Racers[index].Car.Name);
            } //error handling
            else if (result == RaceManager.MakeNextReturn.CallBackUsed)
            {
                DataManager.MessageProvider.showMessage("Duplicate Racer", DataManager.Competition.Racers[index].Car.Name + " has allready been entered into this race");
            }
            else if (result == RaceManager.MakeNextReturn.RaceFull)
            {
                DataManager.MessageProvider.showMessage("Race is full", "Cannot enter more than " + DataManager.RaceManager.NumberOfLanes + " racers into a race");
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
                DataManager.MessageProvider.showMessage("Race is full", "Cannot enter more than " + DataManager.RaceManager.NumberOfLanes + " racers into a race");
            }
        }

        private void runBarcodeCommand(string barcode)
        {
            if (barcode == "reset")
            {
                DataManager.RaceManager.forgetRace();
                RaceManager_onReadyForNextRace();
            }
            else if (barcode == "mask")
            {
                addEmpty();
            }
            else
            {
                //try a car barcode
                bool found = false;
                
                for (int i = 0; i < DataManager.Competition.Racers.Count && !found; i++)
                {
                    if(DataManager.Competition.Racers[i].Barcode == barcode)
                    {
                        found = true;
                        addRacer(i);
                    }
                }

                if (!found)
                {
                    DataManager.MessageProvider.showError("Invalid Command", "[" + barcode + "] is not a valid command");
                }
            }
        }

        private void updateRacerList()
        {
            listBox.Items.Clear();
            stack.Children.Clear();
            foreach (Racer racer in DataManager.Competition.Racers)
            {
                listBox.Items.Add(racer.Car.Name + ", " + racer.Maker.Name + ", " + racer.Barcode);
                
                Image image = new Image();
                Uri uri = DataManager.getAbsoluteUri(racer.Car.ImageUri);
                try
                {
                    image.Source = new BitmapImage(uri);
                }
                catch (Exception ex)
                {
                    DataManager.MessageProvider.showError("Image Not Found", ex.Message);
                }
                image.Width = 50;
                image.Height = 50;
                
                Image image2 = new Image();
                Uri uri2 = DataManager.getAbsoluteUri(racer.Maker.ImageUri);
                try
                {
                    image2.Source = new BitmapImage(uri2);
                }
                catch(Exception ex)
                {
                    DataManager.MessageProvider.showError("Image Not Found", ex.Message);
                }
                image2.Width = 50;
                image2.Height = 50;

                StackPanel panel = new StackPanel();
                panel.Orientation = Orientation.Horizontal;
                panel.Children.Add(image);
                panel.Children.Add(image2);

                stack.Children.Add(panel);
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

        private void updateTimeList()
        {
            if (listBox.SelectedIndex >= 0 && listBox.SelectedIndex < DataManager.Competition.Racers.Count)
            {
                listBox2.Items.Clear();
                foreach (Time time in DataManager.Competition.Racers[listBox.SelectedIndex].Times)
                {
                    listBox2.Items.Add(time.Speed + "s, Lane: " + time.Lane + ", Place: " + time.Place);
                }
            }
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            updateTimeList();
        }

        private void btnAddCar_Click(object sender, RoutedEventArgs e)
        {
            //======================================================
            //check for another car containg any of this information
            //======================================================
            if (tbCarName.Text != string.Empty)
            {
                if (tbMakerName.Text != string.Empty)
                {
                    if (tbBarcode.Text != string.Empty)
                    {
                        DataManager.Competition.Racers.Add(new Racer(tbCarName.Text, DataManager.Settings.DefaltCarImageUri, tbMakerName.Text, DataManager.Settings.DefaltMakerImageUri, tbBarcode.Text, "", false));
                        tbCarName.Clear();
                        tbMakerName.Clear();
                        tbBarcode.Clear();
                        updateRacerList();
                    }
                    else
                    {
                        DataManager.MessageProvider.showMessage("Missing Car's Barcode", "The car's barcode must be entered");
                    }
                }
                else
                {
                    DataManager.MessageProvider.showMessage("Missing Maker's Name", "The maker's name must be entered");
                }
            }
            else
            {
                DataManager.MessageProvider.showMessage("Missing Car's Name", "The car's name must be entered");
            }
        }

        private void tbBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                btnAddCar_Click(sender, new RoutedEventArgs());
            }
        }

        private void tbMakerName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Keyboard.Focus(tbBarcode);
            }
        }

        private void tbCarName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Keyboard.Focus(tbMakerName);
            }
        }

        private void btnRemoveCar_Click(object sender, RoutedEventArgs e)
        {
            DataManager.Competition.Racers.RemoveAt(listBox.SelectedIndex);
            updateRacerList();
        }
    }
}
