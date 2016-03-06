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

        public static CarList createListItem(Racer racer, int place)
        {
            CarList item = new CarList();

            List<int> lanesDone = new List<int>();
            double totalTime = 0;
            double bestTime = 10;
            foreach (Time time in racer.Times)
            {
                totalTime += time.Speed;
                if(bestTime > time.Speed)
                {
                    bestTime = time.Speed;
                }
                if (!lanesDone.Contains(time.Lane))
                {
                    lanesDone.Add(time.Lane);
                }
            }

            item.place.Content = place;

            item.timeAverage.Text = String.Format("{0:0.000}", (totalTime / racer.Times.Count)) + "s";
            item.timeBest.Text = String.Format("{0:0.000}", bestTime) + "s";

            item.Lanes.Children.Clear();
            for (int i = 0; i < DataManager.Settings.NumberOfLanes; i++)
            {
                Label lane = new Label();
                lane.Content = (i + 1).ToString();
                lane.HorizontalAlignment = HorizontalAlignment.Center;
                lane.VerticalAlignment = VerticalAlignment.Center;
                lane.FontSize = 30;

                if (lanesDone.Contains(i + 1))
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
