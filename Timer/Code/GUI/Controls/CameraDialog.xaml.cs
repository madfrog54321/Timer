using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Timer
{
    /// <summary>
    /// Interaction logic for CameraDialog.xaml
    /// </summary>
    public partial class CameraDialog : UserControl
    {
        private FilterInfoCollection _videoCaptureDevices;
        public VideoCaptureDevice _camera = new VideoCaptureDevice();
        private Dialog _hostDialog;

        public CameraDialog()
        {
            InitializeComponent();

            showCameras();
        }

        public static void Show(Panel parent, savedImageHandler onSavedImage)
        {
            CameraDialog cameraDialog = new CameraDialog();
            cameraDialog.onSavedImage += onSavedImage;

            cameraDialog._hostDialog = new Dialog(parent, cameraDialog, true, true, false, null,
                new DialogButton("Close Camera", DialogButton.Alignment.Right, DialogButton.Style.Flat, delegate ()
                {

                    cameraDialog.close();

                    return DialogButton.ReturnEvent.Close;
                }), new DialogButton("Import Image", DialogButton.Alignment.Left, DialogButton.Style.Flat, delegate ()
                {
                    Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                    dlg.DefaultExt = ".png";
                    dlg.Filter = "Image Files (*.jpeg,*.jpg,*.png,*.gif)|*.jpeg;*.jpg;*.png;*.gif";
                    Nullable<bool> result = dlg.ShowDialog();
                    if (result == true)
                    {
                        string filename = dlg.FileName;
                        cameraDialog.stopCamera();
                        CropDialog.Show(cameraDialog._hostDialog.Parent, new BitmapImage(new Uri(filename)), delegate (CropDialog.CloseEvent closeEvent, Uri file)
                        {
                            if (closeEvent == CropDialog.CloseEvent.Saved)
                            {
                                cameraDialog.triggerSavedImage(file);
                                cameraDialog.close();
                                cameraDialog._hostDialog.Close();
                            }
                            else
                            {
                                cameraDialog.startCamera();
                            }
                        });
                    }

                    return DialogButton.ReturnEvent.DoNothing;
                }));
        }

        private void close()
        {
            stopCamera();
        }

        public delegate void savedImageHandler(Uri file);
        public event savedImageHandler onSavedImage;
        private void triggerSavedImage(Uri file)
        {
            savedImageHandler handler = onSavedImage;
            if (handler != null)
            {
                handler(file);
            }
        }

        private void showCameras()
        {
            cmbCameras.Items.Clear();

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += delegate (object sender, DoWorkEventArgs e)
            {
                _videoCaptureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                Dispatcher.Invoke(delegate ()
                {
                    foreach (FilterInfo camera in _videoCaptureDevices)
                    {
                        cmbCameras.Items.Add(camera.Name);
                    }

                    if (cmbCameras.Items.Count > 0)
                    {
                        cmbCameras.SelectedIndex = 0;

                        //showResolutions();
                    }
                });
            };
            worker.RunWorkerAsync();
        }

        private void showResolutions()
        {
            cmbResolutions.Items.Clear();

            if (cmbCameras.SelectedIndex >= 0)
            {
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += delegate (object sender, DoWorkEventArgs e)
                {
                    VideoCaptureDevice currentCam = new VideoCaptureDevice(_videoCaptureDevices[Dispatcher.Invoke<int>(delegate()
                    {
                        return cmbCameras.SelectedIndex;
                    })].MonikerString);

                    Dispatcher.Invoke(delegate ()
                    {
                        foreach (VideoCapabilities camera in currentCam.VideoCapabilities)
                        {
                            string resolution = camera.FrameSize.Width + " X " + camera.FrameSize.Height;

                            cmbResolutions.Items.Add(resolution);
                        }

                        if (cmbResolutions.Items.Count > 0)
                        {
                            cmbResolutions.SelectedIndex = cmbResolutions.Items.Count - 1;

                            //createCamera();
                        }
                    });
                };
                worker.RunWorkerAsync();
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

        private void startCamera()
        {
            if (!_camera.IsRunning)
            {
                _camera.Start();
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

        private void btnTakePicture_Click(object sender, RoutedEventArgs e)
        {
            stopCamera();
            CropDialog.Show(_hostDialog.Parent, frameImage.Source, delegate (CropDialog.CloseEvent closeEvent, Uri file)
            {
                if (closeEvent == CropDialog.CloseEvent.Saved)
                {
                    triggerSavedImage(file);
                    close();
                    _hostDialog.Close();
                }
                else
                {
                    startCamera();
                }
            });
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            showCameras();
        }
    }
}
