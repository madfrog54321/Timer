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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Timer
{
    /// <summary>
    /// Interaction logic for carList.xaml
    /// </summary>
    public partial class CarList : UserControl
    {
        public CarList()
        {
            InitializeComponent();
        }

        public static CarList createListItem(Racer racer, int place, TimeInfo timeInfo)
        {
            CarList item = new CarList();
            
            item.place.Content = place;
            
            item.timeAverage.Text = String.Format("{0:0.000}", timeInfo.AverageTime) + "s";
            item.timeBest.Text = String.Format("{0:0.000}", timeInfo.BestTime) + "s";

            if (timeInfo.HasAllLanesDone())
            {
                item.timeAverageHolder.Opacity = 1;
                item.timeBestHolder.Opacity = 1;
            }

            if (!racer.PassedInspection)
            {
                item.disabled.Visibility = Visibility.Visible;
            }

            item.Lanes.Children.Clear();
            for (int i = 0; i < DataManager.Settings.NumberOfLanes; i++)
            {
                Label lane = new Label();
                lane.Content = (i + 1).ToString();
                lane.HorizontalAlignment = HorizontalAlignment.Center;
                lane.VerticalAlignment = VerticalAlignment.Center;
                lane.FontSize = 30;

                if (timeInfo.LanesDone.Contains(i + 1))
                {
                    lane.IsEnabled = false;
                    lane.FontWeight = FontWeights.UltraLight;
                    lane.Opacity = 0.5;
                }

                item.Lanes.Children.Add(lane);
            }

            item.tbCarName.Text = racer.Car.Name;
            item.tbCreatorName.Text = racer.Maker.Name;

            try
            {
                item.imgCarImage.Source = new BitmapImage(DataManager.getAbsoluteUri(racer.Car.ImageUri));
            }
            catch (Exception ex1)
            {
                DataManager.MessageProvider.showError("Could Not Load Car's Picture. Falling Back to defalt.", ex1.Message);
                try
                {
                    item.imgCarImage.Source = new BitmapImage(DataManager.getAbsoluteUri(DataManager.Settings.DefaltCarImageUri));
                }
                catch (Exception ex2)
                {
                    DataManager.MessageProvider.showError("Could Not Load Defalt Car's Picture.", ex2.Message);
                }
            }

            try
            {
                item.imgCreatorPicture.Source = new BitmapImage(DataManager.getAbsoluteUri(racer.Maker.ImageUri));
            }
            catch (Exception ex1)
            {
                DataManager.MessageProvider.showError("Could Not Load Creator's Picture. Falling Back to defalt.", ex1.Message);
                try
                {
                    item.imgCreatorPicture.Source = new BitmapImage(DataManager.getAbsoluteUri(DataManager.Settings.DefaltMakerImageUri));
                }
                catch (Exception ex2)
                {
                    DataManager.MessageProvider.showError("Could Not Load Defalt Creator's Picture", ex2.Message);
                }
            }

            return item;
        }

        public static CarList createListItem(Racer racer)
        {
            CarList item = new CarList();

            item.place.Content = "";
            
            item.timeAverage.Text = "-";
            item.timeBest.Text = "-";
            
            if (!racer.PassedInspection)
            {
                item.disabled.Visibility = Visibility.Visible;
            }

            item.Lanes.Children.Clear();
            for (int i = 0; i < DataManager.Settings.NumberOfLanes; i++)
            {
                Label lane = new Label();
                lane.Content = (i + 1).ToString();
                lane.HorizontalAlignment = HorizontalAlignment.Center;
                lane.VerticalAlignment = VerticalAlignment.Center;
                lane.FontSize = 30;

                item.Lanes.Children.Add(lane);
            }

            item.tbCarName.Text = racer.Car.Name;
            item.tbCreatorName.Text = racer.Maker.Name;

            try
            {
                item.imgCarImage.Source = new BitmapImage(DataManager.getAbsoluteUri(racer.Car.ImageUri));
            }
            catch (Exception ex1)
            {
                DataManager.MessageProvider.showError("Could Not Load Car's Picture. Falling Back to defalt.", ex1.Message);
                try
                {
                    item.imgCarImage.Source = new BitmapImage(DataManager.getAbsoluteUri(DataManager.Settings.DefaltCarImageUri));
                }
                catch (Exception ex2)
                {
                    DataManager.MessageProvider.showError("Could Not Load Defalt Car's Picture.", ex2.Message);
                }
            }

            try
            {
                item.imgCreatorPicture.Source = new BitmapImage(DataManager.getAbsoluteUri(racer.Maker.ImageUri));
            }
            catch (Exception ex1)
            {
                DataManager.MessageProvider.showError("Could Not Load Creator's Picture. Falling Back to defalt.", ex1.Message);
                try
                {
                    item.imgCreatorPicture.Source = new BitmapImage(DataManager.getAbsoluteUri(DataManager.Settings.DefaltMakerImageUri));
                }
                catch (Exception ex2)
                {
                    DataManager.MessageProvider.showError("Could Not Load Defalt Creator's Picture", ex2.Message);
                }
            }

            return item;
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

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            overlay.Opacity = 0.6;
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            overlay.Opacity = 0.3;
        }
    }
}
