using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Timer
{
    /// <summary>
    /// Interaction logic for test.xaml
    /// </summary>
    public partial class test : Window
    {
        private double ANIMATION_TIME = 0.4, OVERLAY_OPACTIY = 0.4; 

        public test()
        {
            InitializeComponent();

            DataManager.MessageProvider = new DialogMessageProvider(MessageGrid, delegate
            {
                DataManager.MessageProvider = new BasicMessageProvider();
            });

            updateRacerList();
            
            updatePorts();

            updateSettings();

            startAutoScroll();

            //addEmpty();
        }
        
        private DateTime lastTime;
        private enum ScrollEffect { Down, Up, PauseBeforeUp, PauseBeforeDown}
        private ScrollEffect scrollDirection = ScrollEffect.PauseBeforeDown;
        private void startAutoScroll()
        {
            lastTime = DateTime.Now;
            CompositionTarget.Rendering += delegate (object send, EventArgs args)
            {
                DateTime now = DateTime.Now;
                double elapsed = (now - lastTime).TotalSeconds;
                if (btnScrollDown.IsChecked.HasValue && btnScrollDown.IsChecked.Value)
                {
                    if (scrollDirection == ScrollEffect.Down || scrollDirection == ScrollEffect.Up)
                    {
                        lastTime = now;


                        if (scrollDirection == ScrollEffect.Down)
                        {
                            if (listScrollbar.ScrollableHeight <= listScrollbar.VerticalOffset)
                            {
                                scrollDirection = ScrollEffect.PauseBeforeUp;
                            }
                            else
                            {
                                listScrollbar.ScrollToVerticalOffset(listScrollbar.VerticalOffset + (elapsed * DataManager.Settings.AutoScrollSpeed));
                            }
                        }
                        else
                        {
                            if (0 >= listScrollbar.VerticalOffset)
                            {
                                scrollDirection = ScrollEffect.PauseBeforeDown;
                            }
                            else
                            {
                                listScrollbar.ScrollToVerticalOffset(listScrollbar.VerticalOffset - (elapsed * 1000));
                            }
                        }
                    }
                    else
                    {
                        if (elapsed > 300 / DataManager.Settings.AutoScrollSpeed)
                        {
                            if (scrollDirection == ScrollEffect.PauseBeforeUp)
                            {
                                scrollDirection = ScrollEffect.Up;
                                lastTime = now;
                            }
                            else
                            {
                                scrollDirection = ScrollEffect.Down;
                                lastTime = now;
                            }
                        }
                    }
                }
            };
        }

        private void updatePorts()
        {
            cboPorts.Items.Clear();
            string[] ports = DataManager.getPorts();
            foreach (string port in ports)
            {
                cboPorts.Items.Add(port);
            }
            cboPorts.SelectedIndex = 0;
        }

        private void addRacer(int index)
        {
            if (DataManager.readyForRace)
            {
                RaceManager.MakeNextReturn result = DataManager.RaceManager.makeNext_CarLane(index);

                if (result == RaceManager.MakeNextReturn.Added)
                {
                    //listBox1.Items.Add(DataManager.Competition.Racers[index].Car.Name);

                    try
                    {
                        CarTile tile = CarTile.createTile(DataManager.Competition.Racers[index], false);
                        tile.SetValue(Grid.ColumnProperty, DataManager.RaceManager.nextOpenLane - 2);
                        tile.Margin = new Thickness(8, 8, 8, 0);
                        RaceList.Children.Add(tile);
                    }
                    catch (Exception ex)
                    {
                        DataManager.MessageProvider.showError("Could Not Add Racer", ex.Message);
                    }

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
            else
            {
                DataManager.MessageProvider.showMessage("Not Ready To Add Racer", "The track timer does not seem to be connected. Please try to reconnect to the track timer.");
            }
        }

        private void addEmpty()
        {
            if (DataManager.readyForRace)
            {
                RaceManager.MakeNextReturn result = DataManager.RaceManager.makeNext_EmptyLane();

                if (result == RaceManager.MakeNextReturn.Added)
                {
                    //listBox1.Items.Add("Empty");


                    //< TextBlock Grid.Column = "2" Opacity = "0.7" Grid.Row = "2" TextAlignment = "Left" Margin = "0, 4, 0, 0" HorizontalAlignment = "Center" VerticalAlignment = "Center" FontSize = "18" >
                    //                   Empty
                    //           </ TextBlock >

                    TextBlock textBlock = new TextBlock();

                    textBlock.SetValue(Grid.ColumnProperty, DataManager.RaceManager.nextOpenLane - 2);

                    textBlock.Margin = new Thickness(0, 0, 0, 0);
                    textBlock.TextAlignment = TextAlignment.Center;
                    textBlock.FontSize = 18;
                    textBlock.Text = "Empty";
                    textBlock.Opacity = 0.7;
                    textBlock.VerticalAlignment = VerticalAlignment.Center;
                    RaceList.Children.Add(textBlock);

                } //error handling
                else if (result == RaceManager.MakeNextReturn.RaceFull)
                {
                    DataManager.MessageProvider.showMessage("Race is full", "Cannot enter more than " + DataManager.RaceManager.NumberOfLanes + " racers into a race");
                }
            }
            else
            {
                DataManager.MessageProvider.showMessage("Not Ready To Add Racer", "The track timer does not seem to be connected. Please try to reconnect to the track timer.");
            }
        }

        private void tbCommand_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (btnLockKeyboard.IsChecked.HasValue && btnLockKeyboard.IsChecked.Value)
            {
                if(e.NewFocus != btnLockKeyboard)
                {
                    Storyboard sb = this.FindResource("blinkLabel") as Storyboard;
                    Storyboard.SetTarget(sb, btnLockKeyboard);
                    sb.Begin();
                    //Keyboard.Focus(tbCommand);
                    e.Handled = true;
                }
            }
        }

        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            if (btnLockKeyboard.IsChecked.HasValue && btnLockKeyboard.IsChecked.Value)
            {
                Keyboard.Focus(tbCommand);
            }
        }

        private void showTimerConnectMessage()
        {
            DataManager.MessageProvider.showMessage("Could Not Talk To Timer", "The track timer does not seem to be connected. Please try to reconnect to the track timer.");
        }

        private void runBarcodeCommand(string barcode)
        {
            if (barcode == DataManager.Settings.ResetBarcode)
            {
                if (DataManager.readyForRace)
                {
                    try
                    {
                        DataManager.RaceManager.forgetRace();
                        RaceList.Children.Clear();
                    }
                    catch (Exception ex)
                    {
                        showTimerConnectMessage();
                    }
                }
                else
                {
                    showTimerConnectMessage();
                }
                //===reset race===
            }
            else if (barcode == DataManager.Settings.EmptyLaneBarcode)
            {
                addEmpty();
            }
            else
            {
                //try a car barcode
                bool found = false;

                for (int i = 0; i < DataManager.Competition.Racers.Count && !found; i++)
                {
                    if (DataManager.Competition.Racers[i].Barcode == barcode)
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


        private void button_Click(object sender, RoutedEventArgs e)
        {
            flyinAnimation(ANIMATION_TIME, OVERLAY_OPACTIY);
        }

        private double DrawerWidth
        {
            get { return Drawer.Width; }
            set {
                Drawer.Width = value;
                Drawer.Margin = new Thickness(0, 0, -Drawer.Width - 10, 0);
            }
        }

        private bool _doingAnimation = false;

        private void flyinAnimation(double time, double opacity)
        {
            if (!_doingAnimation)
            {
                _doingAnimation = true;
                //get ready
                Overlay.IsHitTestVisible = true;
                Overlay.Opacity = 0;
                Overlay.Visibility = Visibility.Visible;
                Drawer.Margin = new Thickness(0, 0, -Drawer.Width - 10, 0);
                Drawer.Visibility = Visibility.Visible;

                //do fade in overlay animation
                DoubleAnimation fadeIn = new DoubleAnimation
                {
                    To = opacity,
                    From = 0,
                    BeginTime = TimeSpan.FromSeconds(0),
                    Duration = TimeSpan.FromSeconds(time),
                    FillBehavior = FillBehavior.Stop
                };
                fadeIn.Completed += (s, a) => Overlay.Opacity = opacity;
                Overlay.BeginAnimation(UIElement.OpacityProperty, fadeIn);

                //do fly in drawer animation
                ThicknessAnimation flyIn = new ThicknessAnimation
                {
                    To = new Thickness(0, 0, 0, 0),
                    From = Drawer.Margin,
                    BeginTime = TimeSpan.FromSeconds(0),
                    Duration = TimeSpan.FromSeconds(time),
                    EasingFunction = new CubicEase
                    {
                        EasingMode = EasingMode.EaseOut
                    },
                    FillBehavior = FillBehavior.Stop
                };
                flyIn.Completed += (s, a) => { Drawer.Margin = new Thickness(0, 0, 0, 0); _doingAnimation = false; };
                Drawer.BeginAnimation(FrameworkElement.MarginProperty, flyIn);
            }
        }

        private void updateRacerList()
        {
            tileHolder.Children.Clear();
            listHolder.Children.Clear();
            if (btnTileMode.IsChecked.HasValue && btnTileMode.IsChecked.Value)
            {
                if (btnShowClasses.IsChecked.HasValue && btnShowClasses.IsChecked.Value)
                {
                    Dictionary<string, List<Racer>> standings = new Dictionary<string, List<Racer>>();
                    foreach (Racer racer in DataManager.Competition.Racers)
                    {
                        if (!standings.ContainsKey(racer.Class))
                        {
                            standings.Add(racer.Class, new List<Racer>());
                        }
                        standings[racer.Class].Add(racer);
                    }
                    List<string> classes = standings.Keys.ToList();

                    for (int c = 0; c < classes.Count; c++)
                    {
                        listGroup group = new listGroup();
                        group.Title.Content = classes[c];

                        foreach (Racer racer in standings[classes[c]])
                        {
                            CarTile tile = CarTile.createTile(racer, true, delegate ()
                            {
                                addRacer(DataManager.Competition.Racers.IndexOf(racer));
                            });
                            tile.MouseUp += delegate
                            {
                                RacerDetails.editOldRacer(HostGrid, racer, delegate ()
                                {
                                    updateRacerList();
                                });
                            };
                            tile.Cursor = Cursors.Hand;
                            tile.Margin = new Thickness(8, 8, 0, 0);
                            group.tileHolder.Children.Add(tile);
                        }

                        tileHolder.Children.Add(group);
                    }
                }
                else
                {
                    foreach (Racer racer in DataManager.Competition.Racers)
                    {
                        CarTile tile = CarTile.createTile(racer, true, delegate ()
                        {
                            addRacer(DataManager.Competition.Racers.IndexOf(racer));
                        });
                        tile.MouseUp += delegate
                        {
                            RacerDetails.editOldRacer(HostGrid, racer, delegate ()
                            {
                                updateRacerList();
                            });
                        };
                        tile.Cursor = Cursors.Hand;
                        tile.Margin = new Thickness(8, 8, 0, 0);
                        tileHolder.Children.Add(tile);
                    }
                }
            }
            else
            {
                if(btnShowClasses.IsChecked.HasValue && btnShowClasses.IsChecked.Value)
                {
                    Dictionary<string, List<KeyValuePair<double, Racer>>> standings = new Dictionary<string, List<KeyValuePair<double, Racer>>>();
                    foreach (Racer racer in DataManager.Competition.Racers)
                    {
                        double totalTime = 0;
                        foreach (Time time in racer.Times)
                        {
                            totalTime += time.Speed;
                        }
                        if (!standings.ContainsKey(racer.Class))
                        {
                            standings.Add(racer.Class, new List<KeyValuePair<double, Racer>>());
                        }
                        standings[racer.Class].Add(new KeyValuePair<double, Racer>(totalTime / racer.Times.Count, racer));
                    }

                    List<string> classes = standings.Keys.ToList();
                    for (int c = 0; c < classes.Count; c++)
                    {
                        standings[classes[c]].Sort(
                            delegate (KeyValuePair<double, Racer> firstPair,
                            KeyValuePair<double, Racer> nextPair)
                            {
                                return firstPair.Key.CompareTo(nextPair.Key);
                            }
                        );

                        listGroup group = new listGroup();
                        group.Title.Content = classes[c];

                        int i = 0;
                        foreach (KeyValuePair<double, Racer> racer in standings[classes[c]])
                        {
                            i++;
                            CarList listItem = CarList.createListItem(racer.Value, i);
                            listItem.MouseUp += delegate
                            {
                                RacerDetails.editOldRacer(HostGrid, racer.Value, delegate ()
                                {
                                    updateRacerList();
                                });
                            };
                            listItem.Margin = new Thickness(0, 0, 0, 8);
                            group.listHolder.Children.Add(listItem);
                        }

                        listHolder.Children.Add(group);
                    }
                }
                else
                {
                    List<KeyValuePair<double, Racer>> standings = new List<KeyValuePair<double, Racer>>();

                    foreach (Racer racer in DataManager.Competition.Racers)
                    {
                        double totalTime = 0;
                        foreach (Time time in racer.Times)
                        {
                            totalTime += time.Speed;
                        }
                        standings.Add(new KeyValuePair<double, Racer>(totalTime / racer.Times.Count, racer));
                    }

                    standings.Sort(
                        delegate (KeyValuePair<double, Racer> firstPair,
                        KeyValuePair<double, Racer> nextPair)
                        {
                            return firstPair.Key.CompareTo(nextPair.Key);
                        }
                    );
                    
                    int i = 0;
                    foreach (KeyValuePair<double, Racer> racer in standings)
                    {
                        i++;
                        CarList listItem = CarList.createListItem(racer.Value, i);
                        listItem.MouseUp += delegate
                        {
                            RacerDetails.editOldRacer(HostGrid, racer.Value, delegate ()
                            {
                                updateRacerList();
                            });
                        };
                        listItem.Margin = new Thickness(0, 0, 0, 8);
                        listHolder.Children.Add(listItem);
                    }
                }
            }
        }

        private void flyoutAnimation(double time, double opactity)
        {
            if (!_doingAnimation)
            {
                _doingAnimation = true;
                //get ready
                Overlay.IsHitTestVisible = false;
                Overlay.Opacity = opactity;
                Overlay.Visibility = Visibility.Visible;
                Drawer.Margin = new Thickness(0, 0, 0, 0);
                Drawer.Visibility = Visibility.Visible;

                //do fade out overlay animation
                DoubleAnimation fadeOut = new DoubleAnimation
                {
                    To = 0,
                    From = opactity,
                    BeginTime = TimeSpan.FromSeconds(0),
                    Duration = TimeSpan.FromSeconds(time),
                    FillBehavior = FillBehavior.Stop
                };
                fadeOut.Completed += (s, a) => { Overlay.Opacity = 0; Overlay.Visibility = Visibility.Collapsed; };
                Overlay.BeginAnimation(UIElement.OpacityProperty, fadeOut);

                //do fly out drawer animation
                ThicknessAnimation flyOut = new ThicknessAnimation
                {
                    To = new Thickness(0, 0, -Drawer.Width - 10, 0),
                    From = Drawer.Margin,
                    BeginTime = TimeSpan.FromSeconds(0),
                    Duration = TimeSpan.FromSeconds(time),
                    EasingFunction = new CubicEase
                    {
                        EasingMode = EasingMode.EaseIn
                    },
                    FillBehavior = FillBehavior.Stop
                };
                flyOut.Completed += (s, a) => {
                    Drawer.Margin = new Thickness(0, 0, -Drawer.Width - 10, 0);
                    Drawer.Visibility = Visibility.Collapsed;
                    _doingAnimation = false;
                };
                Drawer.BeginAnimation(FrameworkElement.MarginProperty, flyOut);
            }
        }

        private void loadDetails()
        {
            //RacerDetails racerDetails = new RacerDetails();
            //Drawer.Children.Add(racerDetails);
            //flyinAnimation(ANIMATION_TIME, OVERLAY_OPACTIY);
        }

        private void PopupBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            flyinAnimation(ANIMATION_TIME, OVERLAY_OPACTIY);
        }

        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            loadDetails();
        }

        private void btnCreateRacer_Click(object sender, RoutedEventArgs e)
        {
            RacerDetails.createNewRacer(HostGrid, delegate ()
            {
                updateRacerList();
            });
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (btnConnect.Content.Equals("Connect"))
            {
                if (DataManager.tryConnectTimer(cboPorts.Text))
                {
                    //btnConnect.IsEnabled = false;
                    btnConnect.Content = "Disconnect";
                    cboPorts.IsEnabled = false;
                    DataManager.RaceManager.onReadyForNextRace += RaceManager_onReadyForNextRace;
                    DataManager.RaceManager.onRaceIsFull += RaceManager_onRaceIsFull;
                }
            }
            else
            {
                DataManager.disconnectTimer();
                cboPorts.IsEnabled = true;
                btnConnect.Content = "Connect";
            }
        }

        private void RaceManager_onReadyForNextRace()
        {
            if (Dispatcher.CheckAccess())
            {
                //reset race list
                RaceList.Children.Clear();
                updateRacerList();
            }
            else
            {
                Dispatcher.Invoke(new Action(RaceManager_onReadyForNextRace));
            }
        }

        private void RaceManager_onRaceIsFull()
        {
            if (Dispatcher.CheckAccess())
            {
                //label1.Content = "Waiting for race completion...";
                //wait for race
            }
            else
            {
                Dispatcher.Invoke(new Action(RaceManager_onRaceIsFull));
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            updatePorts();
        }

        private void tbCommand_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnLockKeyboard.IsChecked = true;
                runBarcodeCommand(tbCommand.Text);
                tbCommand.Text = string.Empty;
            }
            else if (e.Key == Key.Escape)
            {
                btnLockKeyboard.IsChecked = false;
            }
        }

        private void updateClassList()
        {
            lbClasses.Items.Clear();
            foreach (string rClass in DataManager.Settings.Classes)
            {
                lbClasses.Items.Add(rClass);
            }
        }

        private void btnClassAdd_Click(object sender, RoutedEventArgs e)
        {
            InputDialog.show(HostGrid, "Class' Name", "Create", delegate(string input)
            {
                DataManager.Settings.Classes.Add(input);
                DataManager.Settings.Save();
                updateClassList();
            });
        }

        private void btnClassDelete_Click(object sender, RoutedEventArgs e)
        {
            if(lbClasses.SelectedIndex >= 0)
            {
                DialogBox.showOptionBox(HostGrid, "This will remove \"" + lbClasses.SelectedItem as string + "\" from generic class list. This will not effect any of the racers' classes.", "Delete \"" + lbClasses.SelectedItem as string + "\"?", "Keep", "Delete", delegate (DialogBox.DialogResult result)
                {
                    if (result == DialogBox.DialogResult.MainOption)
                    {
                        DataManager.Settings.Classes.Remove(lbClasses.SelectedItem as string);
                        DataManager.Settings.Save();
                        updateClassList();
                    }
                });
            }
        }

        private bool _loadingSettings = true;
        private void updateSettings()
        {
            _loadingSettings = true;
            tbEmptyBarcode.Text = DataManager.Settings.EmptyLaneBarcode;
            tbResetBarcode.Text = DataManager.Settings.ResetBarcode;

            heightSlider.Value = DataManager.Settings.RaceDisplayHeight;
            MainGrid.RowDefinitions[2].Height = new GridLength(DataManager.Settings.RaceDisplayHeight, GridUnitType.Star);

            uiScaleSlider.Value = DataManager.Settings.StandingsZoom;
            tilesSlider.Value = DataManager.Settings.TilesZoom;
            autoScrollSlider.Value = DataManager.Settings.AutoScrollSpeed;

            updateClassList();
            _loadingSettings = false;
        }

        private void tbEmptyBarcode_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(!_loadingSettings)
            {
                DataManager.Settings.EmptyLaneBarcode = tbEmptyBarcode.Text;
                DataManager.Settings.Save();
            }
        }

        private void tbResetBarcode_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_loadingSettings)
            {
                DataManager.Settings.ResetBarcode = tbResetBarcode.Text;
                DataManager.Settings.Save();
            }
        }

        private void heightSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_loadingSettings)
            {
                DataManager.Settings.RaceDisplayHeight = heightSlider.Value;
                MainGrid.RowDefinitions[2].Height = new GridLength(DataManager.Settings.RaceDisplayHeight, GridUnitType.Star);
                DataManager.Settings.Save();
            }
        }

        private void btnLockKeyboard_Checked(object sender, RoutedEventArgs e)
        {
            btnLockKeyboard.Content = new PackIcon() { Kind = PackIconKind.Lock, Width = 24, Height = 24 };
            Keyboard.Focus(tbCommand);
        }

        private void btnLockKeyboard_Unchecked(object sender, RoutedEventArgs e)
        {
            btnLockKeyboard.Content = new PackIcon() { Kind = PackIconKind.LockOpen, Width = 24, Height = 24 };
        }
        
        private void btnListMode_Checked(object sender, RoutedEventArgs e)
        {
            updateRacerList();
            listHolder.Visibility = Visibility.Visible;
            tileHolder.Visibility = Visibility.Collapsed;
        }

        private void btnTileMode_Checked(object sender, RoutedEventArgs e)
        {
            updateRacerList();
            listHolder.Visibility = Visibility.Collapsed;
            tileHolder.Visibility = Visibility.Visible;
        }

        private void uiScaleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_loadingSettings)
            {
                DataManager.Settings.StandingsZoom = uiScaleSlider.Value;
                DataManager.Settings.Save();
            }
        }

        private void listScrollbar_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            btnScrollDown.IsChecked = false;
        }

        private void autoScrollSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_loadingSettings)
            {
                DataManager.Settings.AutoScrollSpeed = autoScrollSlider.Value;
                DataManager.Settings.Save();
            }
        }

        private void btnShowClasses_Checked(object sender, RoutedEventArgs e)
        {
            updateRacerList();
        }

        private void btnShowClasses_Unchecked(object sender, RoutedEventArgs e)
        {
            updateRacerList();
        }

        private void btnScrollDown_Checked(object sender, RoutedEventArgs e)
        {
            lastTime = DateTime.Now;
            scrollDirection = ScrollEffect.Up;
        }

        private void tilesSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_loadingSettings)
            {
                DataManager.Settings.TilesZoom = tilesSlider.Value;
                DataManager.Settings.Save();
            }
        }

        private void Overlay_MouseUp(object sender, MouseButtonEventArgs e)
        {
            flyoutAnimation(ANIMATION_TIME, OVERLAY_OPACTIY);
        }
    }
}
