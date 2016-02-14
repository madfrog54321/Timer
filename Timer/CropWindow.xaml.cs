using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace Timer
{
    /// <summary>
    /// Interaction logic for CropWindow.xaml
    /// </summary>
    public partial class CropWindow : Window
    {
        private CropWindow()
        {
            InitializeComponent();

            _mouseState = MouseState.None;

        }

        static public void cropPicture(ImageSource picture)
        {
            CropWindow cropTool = new CropWindow();
            cropTool.frameImage.Source = picture;
            cropTool.ShowDialog();
        }

        private void setTopLeft(double width, double height)
        {
            Point relativePoint = frameImage.TransformToVisual(Crop).Transform(new Point(0, 0));
            if (width < relativePoint.X)
            {
                width = relativePoint.X;
            }

            if (height < relativePoint.Y)
            {
                height = relativePoint.Y;
            }

            width -= 12;
            height -= 12;

            if (width < 0)
            {
                width = 0;
            }

            if (height < 0)
            {
                height = 0;
            }

            Crop.ColumnDefinitions[0].Width = new GridLength(width);
            Crop.RowDefinitions[0].Height = new GridLength(height);
        }

        private void setBottomRight(double width, double height)
        {
            Point relativePoint = frameImage.TransformToVisual(Crop).Transform(new Point(frameImage.ActualWidth, frameImage.ActualHeight));
            
            if (Crop.ActualWidth - width > relativePoint.X)
            {
                width = Crop.ActualWidth - relativePoint.X;
            }

            if (Crop.ActualHeight - height > relativePoint.Y)
            {
                height = Crop.ActualHeight - relativePoint.Y;
            }


            width -= 12;
            height -= 12;


            if (width < 0)
            {
                width = 0;
            }

            if (height < 0)
            {
                height = 0;
            }

            Crop.ColumnDefinitions[6].Width = new GridLength(width);
            Crop.RowDefinitions[6].Height = new GridLength(height);
        }

        private enum MouseState
        {
            None, TopLeft, BottomRight
        }

        private MouseState _mouseState;

        private void Crop_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                if (_mouseState == MouseState.TopLeft)
                {
                    setTopLeft(e.GetPosition(Crop).X, e.GetPosition(Crop).Y);
                }
                else if (_mouseState == MouseState.BottomRight)
                {
                    setBottomRight(Crop.ActualWidth - e.GetPosition(Crop).X, Crop.ActualHeight - e.GetPosition(Crop).Y);
                }
            }
            else
            {
                _mouseState = MouseState.None;
            }
        }

        private void BottomRightGrip_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _mouseState = MouseState.BottomRight;
        }

        private void TopLeftGrip_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _mouseState = MouseState.TopLeft;
        }

        private void Crop_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _mouseState = MouseState.None;
        }

        private void frameImage_Loaded(object sender, RoutedEventArgs e)
        {
            setTopLeft(0, 0);
            setBottomRight(Crop.ActualWidth, Crop.ActualHeight);
        }

        private void btnTakePicture_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource rawImage = (BitmapSource)frameImage.Source;

            Point topLeftPoint = Crop.TransformToVisual(frameImage).Transform(new Point(Crop.ColumnDefinitions[0].Width.Value, Crop.RowDefinitions[0].Height.Value));
            Point bottomRightPoint = Crop.TransformToVisual(frameImage).Transform(new Point(Crop.ColumnDefinitions[6].Width.Value, Crop.RowDefinitions[6].Height.Value));
            bottomRightPoint.X = frameImage.ActualWidth - bottomRightPoint.X;
            bottomRightPoint.Y = frameImage.ActualHeight - bottomRightPoint.Y;

            CroppedBitmap image = new CroppedBitmap(rawImage, new Int32Rect((int)topLeftPoint.X, (int)topLeftPoint.Y, Math.Min((int)bottomRightPoint.X - (int)topLeftPoint.X, rawImage.PixelWidth), Math.Min((int)bottomRightPoint.Y - (int)topLeftPoint.Y, rawImage.PixelHeight)));

            string imageDirectory = DataManager.getAbsolutePath(DataManager.Settings.ImageDirectory);
            if (!Directory.Exists(imageDirectory))
            {
                Directory.CreateDirectory(imageDirectory);
            }

            string uniqueFileName;
            do
            {
                uniqueFileName = imageDirectory + "\\" + string.Format(@"{0}.png", Guid.NewGuid());
            }
            while (File.Exists(uniqueFileName));

            using (var fileStream = new FileStream(uniqueFileName, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(fileStream);
            }

            Close();
        }
    }
}
