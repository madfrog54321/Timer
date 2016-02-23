using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
            using (System.Drawing.Image img = (Bitmap)eventArgs.Frame.Clone())//clone bitmap (stop threading error)
            using (MemoryStream ms = new MemoryStream())
            {
                //convert bitmap to bitmapimage
                img.Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.EndInit();

                bi.Freeze();//allow GC to collect the image
                Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    frameImage.Source = bi;
                }));
            }
            GC.Collect();//clear any images in memory
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
            CropWindow.cropPicture(frameImage.Source);
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            showCameras();
        }
    }
}
