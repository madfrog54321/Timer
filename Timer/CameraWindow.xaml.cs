using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Drawing;
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
    /// Interaction logic for CameraWindow.xaml
    /// </summary>
    public partial class CameraWindow : Window
    {
        private FilterInfoCollection _videoCaptureDevices;
        public VideoCaptureDevice _camera = new VideoCaptureDevice();

        public CameraWindow()
        {
            InitializeComponent();

            showCameras();
        }

        private void showCameras()
        {
            _videoCaptureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            cmbCameras.Items.Clear();

            foreach (FilterInfo camera in _videoCaptureDevices)
            {
                cmbCameras.Items.Add(camera.Name);
            }

            if (cmbCameras.Items.Count > 0)
            {
                cmbCameras.SelectedIndex = 0;

                showResolutions();
            }
        }

        private void showResolutions()
        {
            cmbResolutions.Items.Clear();

            VideoCaptureDevice currentCam = new VideoCaptureDevice(_videoCaptureDevices[cmbCameras.SelectedIndex].MonikerString);

            foreach (VideoCapabilities camera in currentCam.VideoCapabilities)
            {
                string resolution = camera.FrameSize.Width + " X " + camera.FrameSize.Height;

                cmbResolutions.Items.Add(resolution);
            }

            if (cmbResolutions.Items.Count > 0)
            {
                cmbResolutions.SelectedIndex = cmbResolutions.Items.Count - 1;

                createCamera();
            }
        }

        private void createCamera()
        {
            if (cmbCameras.SelectedIndex >= 0 && cmbResolutions.SelectedIndex >= 0)
            {
                stopCamera();

                _camera = new VideoCaptureDevice(_videoCaptureDevices[cmbCameras.SelectedIndex].MonikerString);
                _camera.VideoResolution = _camera.VideoCapabilities[cmbResolutions.SelectedIndex];
                _camera.NewFrame += _camera_NewFrame;
                _camera.Start();
            }
        }

        private void stopCamera()
        {
            if (_camera.IsRunning)
            {
                _camera.SignalToStop();
                _camera.WaitForStop();
            }
        }

        private void _camera_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            Bitmap frame = (Bitmap)eventArgs.Frame.Clone();

            setFrame(frame);
        }

        private void setFrame(Bitmap frame)
        {
            if (Dispatcher.CheckAccess())
            {
                frameImage.Source = BitmapToImageSource(frame);
            }
            else
            {
                Dispatcher.BeginInvoke(new Action<Bitmap>(setFrame), new object[] { frame });
            }
        }

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        private void cmbCameras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            showResolutions();
        }

        private void cmbResolutions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            createCamera();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            stopCamera();
        }

        private void btnTakePicture_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource image = (BitmapSource)frameImage.Source;

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
        }
    }
}
