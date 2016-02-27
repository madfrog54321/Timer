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
            RacerDetails.createNewRacer(HostGrid, delegate()
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
