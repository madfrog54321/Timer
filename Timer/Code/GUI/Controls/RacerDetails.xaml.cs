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
            
            _type = type;
        }
        
        private Dialog _dialogHost;
        private WindowType _type;
        private Racer _racer;
        
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

            MakeDialog(parent, creator, WindowType.Create);

        }

        public static void editOldRacer(Panel parent, Racer racer, racerUpdatedHandler returnHandler)
        {
            RacerDetails editor = new RacerDetails(WindowType.Display);
            editor.onUpdatedRacer += returnHandler;

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
            
            MakeDialog(parent, editor, WindowType.Display);
        }
        
        private static void MakeDialog(Panel parent, RacerDetails content, WindowType type)
        {
            content._dialogHost = new Dialog(parent, content, true, false, true, delegate ()
                {
                    if (content._type == WindowType.Display)//only allow saving after clicking of dialog, if not creating a new car
                    {
                        content.createSaveRacer();
                        return DialogButton.ReturnEvent.Close;
                    }
                    else
                    {
                        return DialogButton.ReturnEvent.DoNothing;
                    }
                }, (type == WindowType.Display ? new DialogButton("Delete", DialogButton.Alignment.Left, DialogButton.Style.Flat, delegate () {

                    DialogBox.showOptionBox(parent, "This will completly remove \"" + content.tbCarName.Text + "\" from the competition. All of its history will be deleted.", "Delete \"" + content.tbCarName.Text + "\"?", "Keep", "Delete", delegate (DialogBox.DialogResult result)
                    {
                        if (result == DialogBox.DialogResult.MainOption)
                        {
                            DataManager.Competition.Racers.Remove(content._racer);
                            content.triggerUpdatedRacer();
                            content._dialogHost.Close();//close this host dialog
                        }
                    });

                    return DialogButton.ReturnEvent.DoNothing;
                }) : null), new DialogButton((type == WindowType.Display ? "Revert" : "Forget"), DialogButton.Alignment.Right, DialogButton.Style.Flat, delegate () {
                    
                    //no nothing, this will act like a revert

                    return DialogButton.ReturnEvent.Close;
                }), new DialogButton("Save", DialogButton.Alignment.Right, DialogButton.Style.Normal, delegate () {

                    content.createSaveRacer();//save racer on save button click

                    return DialogButton.ReturnEvent.Close;
                }));
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
