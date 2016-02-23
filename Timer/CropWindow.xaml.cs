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
            //stop the top left grip from extending past the image's top and left edge
            Point relativePoint = frameImage.TransformToVisual(Crop).Transform(new Point(0, 0));
            if (width < relativePoint.X)
            {
                width = relativePoint.X;
            }
            if (height < relativePoint.Y)
            {
                height = relativePoint.Y;
            }

            //make space for border of crop tool
            width -= 15.5;
            height -= 15.5;

            //check if new location will push the bottom right grip out of position
            Size cropMinSize = getMinSizeOfAllOtherCells(0);
            if (cropMinSize.Width + width > Crop.ActualWidth)
            {
                width = Crop.ActualWidth - cropMinSize.Width;
            }
            if (cropMinSize.Height + height > Crop.ActualHeight)
            {
                height = Crop.ActualHeight - cropMinSize.Height;
            }

            //check if the size has gone below 0, and if so set it to 0
            if (width < 0)
            {
                width = 0;
            }
            if (height < 0)
            {
                height = 0;
            }

            //set the top left most cell's size to the new calculated size
            Crop.ColumnDefinitions[0].Width = new GridLength(width);
            Crop.RowDefinitions[0].Height = new GridLength(height);
        }

        private Size getMinSizeOfAllOtherCells(int exludedDiagnalCell) 
        {
            Size size = new Size(0, 0);

            for(int i = 0; i < Crop.ColumnDefinitions.Count; i++)
            {
                if(i != exludedDiagnalCell)//dont add the exluded cell into the calculation
                {
                    if (Crop.ColumnDefinitions[i].Width.IsAbsolute)
                    {
                        size.Width += Crop.ColumnDefinitions[i].Width.Value;
                    }

                    if (Crop.RowDefinitions[i].Height.IsAbsolute)
                    {
                        size.Height += Crop.RowDefinitions[i].Height.Value;
                    }
                }
            }

            return size;
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


            width -= 15.5;
            height -= 15.5;


            if (width < 0)
            {
                width = 0;
            }

            if (height < 0)
            {
                height = 0;
            }

            Crop.ColumnDefinitions[13].Width = new GridLength(width);
            Crop.RowDefinitions[13].Height = new GridLength(height);
        }

        private enum MouseState
        {
            None, TopLeft, BottomRight
        }

        private MouseState _mouseState;

        private void BottomRightGrip_MouseDown(object sender, MouseButtonEventArgs e)
        {
            startMouseDragState(MouseState.BottomRight);
        }

        private void TopLeftGrip_MouseDown(object sender, MouseButtonEventArgs e)
        {
            startMouseDragState(MouseState.TopLeft);
        }

        private void startMouseDragState(MouseState newState)
        {
            CaptureMouse();
            TopLeftGrip.IsHitTestVisible = false;
            BottomRightGrip.IsHitTestVisible = false;
            Crop.IsHitTestVisible = false;
            MainGrid.IsHitTestVisible = false;
            btnTakePicture.IsHitTestVisible = false;

            _mouseState = newState;
        }

        private void stopMouseDragState()
        {
            ReleaseMouseCapture();
            TopLeftGrip.IsHitTestVisible = true;
            BottomRightGrip.IsHitTestVisible = true;
            Crop.IsHitTestVisible = true;
            MainGrid.IsHitTestVisible = true;
            btnTakePicture.IsHitTestVisible = true;

            _mouseState = MouseState.None;
        }

        private void frameImage_Loaded(object sender, RoutedEventArgs e)
        {
            setTopLeft(0, 0);
            setBottomRight(0, 0);
        }

        private void btnTakePicture_Click(object sender, RoutedEventArgs e)
        {
            Point topLeft_crop = getLocationOfCellOnCrop(3, 3);
            Point bottomRight_crop = getLocationOfCellOnCrop(11, 11);

            Point topLeft_image = Crop.TransformToVisual(frameImage).Transform(topLeft_crop);
            Point bottomRight_image = Crop.TransformToVisual(frameImage).Transform(bottomRight_crop);

            double maxWidth = frameImage.ActualWidth - Math.Max(topLeft_image.X, 0);
            double maxHeight = frameImage.ActualHeight - Math.Max(topLeft_image.Y, 0);

            int x = (int)Math.Max(topLeft_image.X, 0);
            int y = (int)Math.Max(topLeft_image.Y, 0);
            int width = (int)Math.Min(bottomRight_image.X - topLeft_image.X, maxWidth);
            int height = (int)Math.Min(bottomRight_image.Y - topLeft_image.Y, maxHeight);

            BitmapImage image = cropFromImageHolder(frameImage, new Int32Rect(x, y, width, height));

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

        private BitmapImage cropFromImageHolder(Image imageHolder, Int32Rect crop)
        {
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)imageHolder.ActualWidth, (int)imageHolder.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(imageHolder);

            return sourceToImage(new CroppedBitmap(bitmap, crop));
        }
        
        private BitmapImage sourceToImage(BitmapSource source)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                BitmapImage bImg = new BitmapImage();

                encoder.Frames.Add(BitmapFrame.Create(source));
                encoder.Save(memoryStream);

                bImg.BeginInit();
                bImg.StreamSource = new MemoryStream(memoryStream.ToArray());
                bImg.EndInit();

                bImg.Freeze();

                return bImg;
            }
        }

        private Point getLocationOfCellOnCrop(int column, int row)
        {
            Point point = new Point(0, 0);

            for (int i = 0; i < column; i++)
            {
                point.X += Crop.ColumnDefinitions[i].ActualWidth;
            }

            for (int i = 0; i < row; i++)
            {
                point.Y += Crop.RowDefinitions[i].ActualHeight;
            }

            return point;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
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
                stopMouseDragState();
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            stopMouseDragState();
        }
    }
}
