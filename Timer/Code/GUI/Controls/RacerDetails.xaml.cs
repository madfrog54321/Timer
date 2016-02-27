using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Timer
{
    /// <summary>
    /// Interaction logic for RacerDetails.xaml
    /// </summary>
    public partial class RacerDetails : UserControl
    {
        public delegate void racerUpdatedHandler();
        public event racerUpdatedHandler onUpdatedRacer;
        private void triggerUpdatedRacer()
        {
            racerUpdatedHandler handler = onUpdatedRacer;
            if (handler != null)
            {
                handler();
            }
        }

        private enum WindowType
        {
            Create, Display
        }

        private RacerDetails(WindowType type)
        {
            InitializeComponent();

            _doingAnimation = false;
            _type = type;
            if(_type == WindowType.Create)
            {
                btnDelete.Visibility = Visibility.Collapsed;
                btnRevert.Content = "Forget";
            }
        }
        
        private Panel _parent;
        private bool _doingAnimation;
        private WindowType _type;
        private Racer _racer;

        private void startGrowInAnimation()
        {
            if (!_doingAnimation)
            {
                _doingAnimation = true;
                Overlay.IsHitTestVisible = true;
                Info.Opacity = 0;

                //do fade in on overlay
                ColorAnimation overlayAnimation;
                overlayAnimation = new ColorAnimation();
                overlayAnimation.From = Color.FromArgb(0, 0, 0, 0);
                overlayAnimation.To = Color.FromArgb(88, 0, 0, 0);
                overlayAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                Overlay.Background.BeginAnimation(SolidColorBrush.ColorProperty, overlayAnimation);

                //do grow in animation
                ScaleTransform scale = new ScaleTransform(1.0, 1.0);
                Card.RenderTransformOrigin = new Point(0.5, 0.5);
                Card.RenderTransform = scale;

                DoubleAnimation growXAnimation = new DoubleAnimation();
                growXAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                growXAnimation.From = 0;
                growXAnimation.To = 1;
                growXAnimation.EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseOut
                };
                Card.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, growXAnimation);
                
                DoubleAnimation growYAnimation = new DoubleAnimation();
                growYAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                growYAnimation.From = 0;
                growYAnimation.To = 1;
                growYAnimation.EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseOut
                };
                growYAnimation.Completed += delegate {

                    _doingAnimation = false;

                };
                Card.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, growYAnimation);
                
                //do fade in on content
                DoubleAnimation fadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    BeginTime = TimeSpan.FromSeconds(0.2),
                    Duration = new Duration(TimeSpan.FromSeconds(0.3))
                };
                Info.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            }
        }

        private void startGrowOutAnimation()
        {
            if (!_doingAnimation)
            {
                _doingAnimation = true;
                Overlay.IsHitTestVisible = false;

                //do fade in on overlay
                ColorAnimation overlayAnimation;
                overlayAnimation = new ColorAnimation();
                overlayAnimation.From = Color.FromArgb(88, 0, 0, 0);
                overlayAnimation.To = Color.FromArgb(0, 0, 0, 0);
                overlayAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                Overlay.Background.BeginAnimation(SolidColorBrush.ColorProperty, overlayAnimation);

                //do grow in animation
                ScaleTransform scale = new ScaleTransform(1.0, 1.0);
                Card.RenderTransformOrigin = new Point(0.5, 0.5);
                Card.RenderTransform = scale;

                DoubleAnimation growXAnimation = new DoubleAnimation();
                growXAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                growXAnimation.From = 1;
                growXAnimation.To = 0;
                growXAnimation.EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseIn
                };
                Card.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, growXAnimation);

                DoubleAnimation growYAnimation = new DoubleAnimation();
                growYAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                growYAnimation.From = 1;
                growYAnimation.To = 0;
                growYAnimation.EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseIn
                };
                growYAnimation.Completed += delegate {

                    _parent.Children.Remove(this);//remove after animation
                    _doingAnimation = false;
                };
                Card.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, growYAnimation);
                
                //do fade out on content
                DoubleAnimation fadeOut = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromSeconds(0.3))
                };
                Info.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            }
        }

        public static void createNewRacer(Panel parent, racerUpdatedHandler returnHandler)
        {
            RacerDetails creator = new RacerDetails(WindowType.Create);
            creator.onUpdatedRacer += returnHandler;

            try
            {
                creator.imgCarPicture.Source = new BitmapImage(DataManager.getAbsoluteUri(DataManager.Settings.DefaltCarImageUri));
            }
            catch (Exception ex2)
            {
                DataManager.MessageProvider.showError("Could Not Load Defalt Car's Picture.", ex2.Message);
            }

            try
            {
                creator.imgCreatorPicture.Source = new BitmapImage(DataManager.getAbsoluteUri(DataManager.Settings.DefaltMakerImageUri));
            }
            catch (Exception ex2)
            {
                DataManager.MessageProvider.showError("Could Not Load Defalt Creator's Picture", ex2.Message);
            }

            creator._parent = parent;
            creator._parent.Children.Add(creator);
            creator.startGrowInAnimation();
        }

        public static void editOldRacer(Panel parent, Racer racer, racerUpdatedHandler returnHandler)
        {
            RacerDetails editor = new RacerDetails(WindowType.Display);
            editor.onUpdatedRacer += returnHandler;
            editor._parent = parent;
            editor._parent.Children.Add(editor);
            editor.startGrowInAnimation();

            editor._racer = racer;

            try
            {
                editor.imgCarPicture.Source = new BitmapImage(DataManager.getAbsoluteUri(racer.Car.ImageUri));
            }
            catch (Exception ex1)
            {
                DataManager.MessageProvider.showError("Could Not Load Car's Picture. Falling Back to defalt.", ex1.Message);
                try
                {
                    editor.imgCarPicture.Source = new BitmapImage(DataManager.getAbsoluteUri(DataManager.Settings.DefaltCarImageUri));
                }
                catch (Exception ex2)
                {
                    DataManager.MessageProvider.showError("Could Not Load Defalt Car's Picture.", ex2.Message);
                }
            }

            try
            {
                editor.imgCreatorPicture.Source = new BitmapImage(DataManager.getAbsoluteUri(racer.Maker.ImageUri));
            }
            catch (Exception ex1)
            {
                DataManager.MessageProvider.showError("Could Not Load Creator's Picture. Falling Back to defalt.", ex1.Message);
                try
                {
                    editor.imgCreatorPicture.Source = new BitmapImage(DataManager.getAbsoluteUri(DataManager.Settings.DefaltMakerImageUri));
                }
                catch (Exception ex2)
                {
                    DataManager.MessageProvider.showError("Could Not Load Defalt Creator's Picture", ex2.Message);
                }
            }

            editor.tbCarName.Text = racer.Car.Name;
            editor.tbCreatorName.Text = racer.Maker.Name;
            editor.tbBarcode.Text = racer.Barcode;
        }

        private void btnSave_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            createSaveRacer();
        }

        private void Overlay_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(_type == WindowType.Display)//only allow saving after clicking of dialog, if not creating a new car
            {
                createSaveRacer();
            }
        }

        private void createSaveRacer()
        {
            //========= create/update racer ========

            //create racer
            if (_type == WindowType.Create)
            {
                DataManager.Competition.Racers.Add(new Racer(tbCarName.Text, DataManager.getRelativePath(imgCarPicture.Source.ToString()),
                    tbCreatorName.Text, DataManager.getRelativePath(imgCreatorPicture.Source.ToString()), tbBarcode.Text));
            }
            else if(_type == WindowType.Display)
            {
                _racer.Car.Name = tbCarName.Text;
                _racer.Maker.Name = tbCreatorName.Text;
                _racer.Barcode = tbBarcode.Text;

                try
                {
                    _racer.Car.ImageUri = DataManager.getRelativePath(imgCarPicture.Source.ToString());
                }
                catch(Exception ex)
                {
                    DataManager.MessageProvider.showError("Could Not Apply Car's Image", ex.Message);
                }

                try
                {
                    _racer.Maker.ImageUri = DataManager.getRelativePath(imgCreatorPicture.Source.ToString());
                }
                catch (Exception ex)
                {
                    DataManager.MessageProvider.showError("Could Not Apply Creator's Image", ex.Message);
                }
            }

            //========save changes to file=========
            DataManager.saveSettings();

            triggerUpdatedRacer();
            startGrowOutAnimation();
        }

        private void btnRevert_Click(object sender, RoutedEventArgs e)
        {
            //do not save the changes made
            startGrowOutAnimation();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DialogBox.showOptionBox(_parent, "This will completly remove \"" + tbCarName.Text + "\" from the competition. All of its history will be deleted.", "Delete \"" + tbCarName.Text + "\"?", "Keep", "Delete", delegate (DialogBox.DialogResult result)
            {
                if(result == DialogBox.DialogResult.MainOption)
                {
                    DataManager.Competition.Racers.Remove(_racer);
                    triggerUpdatedRacer();
                    startGrowOutAnimation();
                }
            });
        }

        private void btnCarPicture_Click(object sender, RoutedEventArgs e)
        {
            CameraWindow camera = new CameraWindow();
            camera.onSavedImage += delegate (Uri file)
            {
                imgCarPicture.Source = new BitmapImage(file);
            };
            camera.ShowDialog();
        }

        private void btnCreatorPicture_Click(object sender, RoutedEventArgs e)
        {
            CameraWindow camera = new CameraWindow();
            camera.onSavedImage += delegate (Uri file)
            {
                imgCreatorPicture.Source = new BitmapImage(file);
            };
            camera.ShowDialog();
        }
    }
}
