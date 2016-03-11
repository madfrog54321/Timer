
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Timer
{
    /// <summary>
    /// Interaction logic for CarTile.xaml
    /// </summary>
    public partial class CarTile : UserControl
    {
        private CarTile()
        {
            InitializeComponent();
        }

        public delegate void addToRaceHandler();
        public event addToRaceHandler onAddToRace;
        private void triggerAddToRace()
        {
            addToRaceHandler handler = onAddToRace;
            if (handler != null)
            {
                handler();
            }
        }

        public static CarTile createTile(Racer racer, bool keepSize)
        {
            return createTile(racer, keepSize, false, null);
        }

        public static CarTile createTile(Racer racer, bool keepSize, addToRaceHandler addHandler)
        {
            return createTile(racer, keepSize, true, addHandler);
        }

        public void AnimateIn()
        {
            AnimateIn(0);
        }

        public void AnimateIn(int delay)
        {
            //do grow in animation
            ScaleTransform scale = new ScaleTransform(0, 0);
            MainGrid.RenderTransformOrigin = new Point(0.5, 0.5);
            MainGrid.RenderTransform = scale;

            DoubleAnimation growXAnimation = new DoubleAnimation();
            growXAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            growXAnimation.From = 0;
            growXAnimation.To = 1;
            growXAnimation.EasingFunction = new CubicEase
            {
                EasingMode = EasingMode.EaseOut
            };
            growXAnimation.BeginTime = new TimeSpan(delay * 1000);
            MainGrid.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, growXAnimation);

            DoubleAnimation growYAnimation = new DoubleAnimation();
            growYAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            growYAnimation.From = 0;
            growYAnimation.To = 1;
            growYAnimation.EasingFunction = new CubicEase
            {
                EasingMode = EasingMode.EaseOut
            };
            growYAnimation.Completed += delegate
            {
                
            };
            growYAnimation.BeginTime = new TimeSpan(delay * 1000);
            MainGrid.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, growYAnimation);
        }

        private static CarTile createTile(Racer racer, bool keepSize, bool haveAdd, addToRaceHandler addHandler)
        {
            CarTile tile = new CarTile();
            tile.tbCarName.Text = racer.Car.Name;
            tile.tbCreatorName.Text = racer.Maker.Name;
            tile.onAddToRace += addHandler;
            
            if (!racer.PassedInspection)
            {
                tile.disabled.Visibility = Visibility.Visible;
            }

            if (!haveAdd || !racer.PassedInspection)
            {
                tile.addHolder.Visibility = System.Windows.Visibility.Collapsed;
            }

            if (!keepSize)
            {
                //make unsized
                tile.MainGrid.Width = tile.MainGrid.Height = Double.NaN;
            }

            try
            {
                tile.imgCarImage.Source = DataManager.loadImage(DataManager.getAbsoluteUri(racer.Car.ImageUri));
            }
            catch (Exception ex1)
            {
                DataManager.MessageProvider.showError("Could Not Load Car's Picture. Falling Back to defalt.", ex1.Message);
                try
                {
                    tile.imgCarImage.Source = new BitmapImage(DataManager.getAbsoluteUri(DataManager.Settings.DefaltCarImageUri));
                }
                catch (Exception ex2)
                {
                    DataManager.MessageProvider.showError("Could Not Load Defalt Car's Picture.", ex2.Message);
                }
            }

            try
            {
                tile.imgCreatorPicture.Source = DataManager.loadImage(DataManager.getAbsoluteUri(racer.Maker.ImageUri));
            }
            catch (Exception ex1)
            {
                DataManager.MessageProvider.showError("Could Not Load Creator's Picture. Falling Back to defalt.", ex1.Message);
                try
                {
                    tile.imgCreatorPicture.Source = new BitmapImage(DataManager.getAbsoluteUri(DataManager.Settings.DefaltMakerImageUri));
                }
                catch (Exception ex2)
                {
                    DataManager.MessageProvider.showError("Could Not Load Defalt Creator's Picture", ex2.Message);
                }
            }

            return tile;
        }

        private void btnAddToRace_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            triggerAddToRace();
        }

        private void btnAddToRace_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            addOverlay.Visibility = System.Windows.Visibility.Visible;
        }

        private void btnAddToRace_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            addOverlay.Visibility = System.Windows.Visibility.Hidden;
        }

        private void addOverlay_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            addOverlay.Background = new SolidColorBrush(Color.FromArgb(0xaa, 0x00, 0x00, 0x00));
        }

        private void addOverlay_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            addOverlay.Background = new SolidColorBrush(Color.FromArgb(0x66, 0x00, 0x00, 0x00));
        }
    }
}
