
using System;
using System.Windows.Controls;
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

        public static CarTile createTile(Racer racer, bool keepSize)
        {
            CarTile tile = new CarTile();
            tile.tbCarName.Text = racer.Car.Name;
            tile.tbCreatorName.Text = racer.Maker.Name;

            if (!keepSize)
            {
                //make unsized
                tile.MainGrid.Width = tile.MainGrid.Height = Double.NaN;
            }

            try
            {
                tile.imgCarImage.Source = new BitmapImage(DataManager.getAbsoluteUri(racer.Car.ImageUri));
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
                tile.imgCreatorPicture.Source = new BitmapImage(DataManager.getAbsoluteUri(racer.Maker.ImageUri));
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
    }
}
