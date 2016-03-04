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

            updateRacerList();

            //addEmpty();
        }


        //private void addRacer(int index)
        //{
        //    //RaceManager.MakeNextReturn result = DataManager.RaceManager.makeNext_CarLane(index);
        //    RaceManager.MakeNextReturn result = RaceManager.MakeNextReturn.Added;

        //    if (result == RaceManager.MakeNextReturn.Added)
        //    {
        //        //listBox1.Items.Add(DataManager.Competition.Racers[index].Car.Name);

        //        CarTile tile = CarTile.createTile(DataManager.Competition.Racers[index], false);
        //        tile.SetValue(Grid.RowProperty, 2);//change index
        //        tile.Margin = new Thickness(8, 8, 8, 0);
        //        tile.SetValue(Grid.ColumnProperty, 2);
        //        MainGrid.Children.Add(tile);

        //    } //error handling
        //    else if (result == RaceManager.MakeNextReturn.CallBackUsed)
        //    {
        //        DataManager.MessageProvider.showMessage("Duplicate Racer", DataManager.Competition.Racers[index].Car.Name + " has allready been entered into this race");
        //    }
        //    else if (result == RaceManager.MakeNextReturn.RaceFull)
        //    {
        //        DataManager.MessageProvider.showMessage("Race is full", "Cannot enter more than " + DataManager.RaceManager.NumberOfLanes + " racers into a race");
        //    }
        //}

        //private void addEmpty()
        //{
        //    //RaceManager.MakeNextReturn result = DataManager.RaceManager.makeNext_EmptyLane();
        //    RaceManager.MakeNextReturn result = RaceManager.MakeNextReturn.Added;

        //    if (result == RaceManager.MakeNextReturn.Added)
        //    {
        //        //listBox1.Items.Add("Empty");


        //        //< TextBlock Grid.Column = "2" Opacity = "0.7" Grid.Row = "2" TextAlignment = "Left" Margin = "0, 4, 0, 0" HorizontalAlignment = "Center" VerticalAlignment = "Center" FontSize = "18" >
        //        //                   Empty
        //        //           </ TextBlock >

        //        TextBlock textBlock = new TextBlock();

        //        textBlock.SetValue(Grid.ColumnProperty, DataManager.RaceManager.nextOpenLane);

        //        textBlock.Margin = new Thickness(0, 0, 0, 0);
        //        textBlock.TextAlignment = TextAlignment.Center;
        //        textBlock.FontSize = 18;
        //        textBlock.Text = "Empty";
        //        textBlock.Opacity = 0.7;
        //        textBlock.VerticalAlignment = VerticalAlignment.Center;
        //        RaceList.Children.Add(textBlock);

        //    } //error handling
        //    else if (result == RaceManager.MakeNextReturn.RaceFull)
        //    {
        //        DataManager.MessageProvider.showMessage("Race is full", "Cannot enter more than " + DataManager.RaceManager.NumberOfLanes + " racers into a race");
        //    }
        //}

        //private void runBarcodeCommand(string barcode)
        //{
        //    if (barcode == "reset")
        //    {
        //        DataManager.RaceManager.forgetRace();
        //        //===reset race===
        //    }
        //    else if (barcode == "mask")
        //    {
        //        addEmpty();
        //    }
        //    else
        //    {
        //        //try a car barcode
        //        bool found = false;

        //        for (int i = 0; i < DataManager.Competition.Racers.Count && !found; i++)
        //        {
        //            if (DataManager.Competition.Racers[i].Barcode == barcode)
        //            {
        //                found = true;
        //                addRacer(i);
        //            }
        //        }

        //        if (!found)
        //        {
        //            DataManager.MessageProvider.showError("Invalid Command", "[" + barcode + "] is not a valid command");
        //        }
        //    }
        //}


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
            listHolder.Children.Clear();
            foreach (Racer racer in DataManager.Competition.Racers)
            {
                CarTile tile = CarTile.createTile(racer, true);
                tile.MouseUp += delegate
                {
                    RacerDetails.editOldRacer(HostGrid, racer, delegate ()
                    {
                        updateRacerList();
                    });
                };
                tile.Margin = new Thickness(8, 8, 0, 0);
                listHolder.Children.Add(tile);
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
                    Drawer.Children.Clear();//clear the drawer of the information
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

        private void Overlay_MouseUp(object sender, MouseButtonEventArgs e)
        {
            flyoutAnimation(ANIMATION_TIME, OVERLAY_OPACTIY);
        }
    }
}
