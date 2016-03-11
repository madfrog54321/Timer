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
    /// Interaction logic for Dialog.xaml
    /// </summary>
    public partial class Dialog : UserControl
    {
        public delegate DialogButton.ReturnEvent DialogClickOffCloseHandler();

        private Panel _parent;
        private bool _allowClickOffClose, _doingAnimation;
        public event DialogClickOffCloseHandler OnClickOffClose;

        new public Panel Parent
        {
            get { return _parent; }
        }

        public bool AllowClickOffClose
        {
            get { return _allowClickOffClose; }
            set { _allowClickOffClose = value; }
        }

        private DialogButton.ReturnEvent triggerOnClose()
        {
            DialogClickOffCloseHandler handler = OnClickOffClose;
            if (handler != null)
            {
                return handler();
            }
            return DialogButton.ReturnEvent.Empty;
        }

        public Dialog(Panel parent, UIElement content, bool fillHeight, bool fillWidth, bool allowClickOffClose, DialogClickOffCloseHandler onClickOffHandler, params DialogButton[] buttons)
        {
            InitializeComponent();

            Card.VerticalAlignment = (fillHeight ? VerticalAlignment.Stretch : VerticalAlignment.Center);
            Card.HorizontalAlignment = (fillWidth ? HorizontalAlignment.Stretch : HorizontalAlignment.Center);

            _allowClickOffClose = allowClickOffClose;

            if(content != null)
            {
                content.SetValue(Grid.RowProperty, 0);
                content.SetValue(Grid.ColumnProperty, 0);
                content.SetValue(Grid.RowSpanProperty, 1);
                content.SetValue(Grid.ColumnSpanProperty, 3);
                ContentHolder.Children.Add(content);
            }

            OnClickOffClose += onClickOffHandler;

            createButtons(buttons);
            show(parent);
        }

        public Dialog(Panel parent, UIElement content, bool fillHeight, bool fillWidth, int delay)
        {
            InitializeComponent();

            Card.VerticalAlignment = (fillHeight ? VerticalAlignment.Stretch : VerticalAlignment.Center);
            Card.HorizontalAlignment = (fillWidth ? HorizontalAlignment.Stretch : HorizontalAlignment.Center);

            _allowClickOffClose = false;

            if (content != null)
            {
                content.SetValue(Grid.RowProperty, 0);
                content.SetValue(Grid.ColumnProperty, 0);
                content.SetValue(Grid.RowSpanProperty, 1);
                content.SetValue(Grid.ColumnSpanProperty, 3);
                ContentHolder.Children.Add(content);
            }

            _parent = parent;
            _parent.Children.Add(this);
            startGrowInAnimation(true, delay);
        }

        private void show(Panel parent)
        {
            _parent = parent;
            _parent.Children.Add(this);
            startGrowInAnimation(false, 0);
        }

        public void Close()
        {
            startGrowOutAnimation(0);
        }

        private void remove()
        {
            _parent.Children.Remove(this);
        }

        private void createButtons(DialogButton[] buttons)
        {
            if (buttons != null && buttons.Length > 0)
            {
                foreach (DialogButton buttonInfo in buttons)
                {
                    if (buttonInfo != null)
                    {
                        Button button = new Button();
                        button.Content = buttonInfo.Text;
                        button.Click += delegate (object sender, RoutedEventArgs e)
                        {
                            if (buttonInfo.triggerOnClick() == DialogButton.ReturnEvent.Close)
                            {
                                Close();
                            }
                        };
                        button.Margin = new Thickness(8, 4, 8, 8);
                        button.SetValue(Grid.RowProperty, 1);
                        if (buttonInfo.Look == DialogButton.Style.Flat)
                        {
                            button.Style = FindResource("MaterialDesignFlatButton") as Style;
                        }
                        switch (buttonInfo.Position)
                        {
                            case DialogButton.Alignment.Left:
                                ButtonHolderLeft.Children.Add(button);
                                break;
                            case DialogButton.Alignment.Center:
                                ButtonHolderCenter.Children.Add(button);
                                break;
                            case DialogButton.Alignment.Right:
                                ButtonHolderRight.Children.Add(button);
                                break;
                        }
                    }
                }
            }
        }

        private void startGrowInAnimation(bool inAndOut, int delay)
        {
            if (!_doingAnimation)
            {
                _doingAnimation = true;
                Overlay.IsHitTestVisible = true;
                ContentHolder.Opacity = 0;

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
                growYAnimation.Completed += delegate
                {
                    _doingAnimation = false;
                    if (inAndOut)
                    {
                        startGrowOutAnimation(delay);
                    }
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
                ContentHolder.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            }
        }

        private void Overlay_Click(object sender, RoutedEventArgs e)
        {
            if (_allowClickOffClose)
            {
                if (triggerOnClose() != DialogButton.ReturnEvent.DoNothing)
                {
                    Close();
                }
            }
        }

        private void startGrowOutAnimation(int delay)
        {
            if (!_doingAnimation)
            {
                _doingAnimation = true;
                Overlay.IsHitTestVisible = false;

                //do fade in on overlay
                ColorAnimation overlayAnimation;
                overlayAnimation = new ColorAnimation();
                overlayAnimation.BeginTime = new TimeSpan(delay * 1000);
                overlayAnimation.From = Color.FromArgb(88, 0, 0, 0);
                overlayAnimation.To = Color.FromArgb(0, 0, 0, 0);
                overlayAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                Overlay.Background.BeginAnimation(SolidColorBrush.ColorProperty, overlayAnimation);

                //do grow in animation
                ScaleTransform scale = new ScaleTransform(1.0, 1.0);
                Card.RenderTransformOrigin = new Point(0.5, 0.5);
                Card.RenderTransform = scale;

                DoubleAnimation growXAnimation = new DoubleAnimation();
                growXAnimation.BeginTime = new TimeSpan(delay * 1000);
                growXAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                growXAnimation.From = 1;
                growXAnimation.To = 0;
                growXAnimation.EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseIn
                };
                Card.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, growXAnimation);

                DoubleAnimation growYAnimation = new DoubleAnimation();
                growYAnimation.BeginTime = new TimeSpan(delay * 1000);
                growYAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                growYAnimation.From = 1;
                growYAnimation.To = 0;
                growYAnimation.EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseIn
                };
                growYAnimation.Completed += delegate
                {
                    remove();
                    _doingAnimation = false;
                };
                Card.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, growYAnimation);

                //do fade out on content
                DoubleAnimation fadeOut = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromSeconds(0.3)),
                    BeginTime = new TimeSpan(delay * 1000)
                };
                ContentHolder.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            }
        }
    }
}
