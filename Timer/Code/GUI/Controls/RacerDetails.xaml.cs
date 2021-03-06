﻿using System;
using System.Collections.Generic;
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

        public delegate void racerDeletedHandler();
        public event racerDeletedHandler onDeletedRacer;
        private void triggerDeletedRacer()
        {
            racerDeletedHandler handler = onDeletedRacer;
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

            foreach(string rClass in DataManager.Settings.Classes)
            {
                cboClass.Items.Add(rClass);
            }
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

            creator.raceData.Children.Clear();
            creator.raceData.ColumnDefinitions.Clear();

            MakeDialog(parent, creator, WindowType.Create);

        }

        public static void editOldRacer(Panel parent, Racer racer, racerUpdatedHandler returnHandler, racerDeletedHandler deleteHandler)
        {
            RacerDetails editor = new RacerDetails(WindowType.Display);
            editor.onUpdatedRacer += returnHandler;
            editor.onDeletedRacer += deleteHandler;

            editor._racer = racer;

            try
            {
                editor.imgCarPicture.Source = DataManager.loadImage(DataManager.getAbsoluteUri(racer.Car.ImageUri));
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
                editor.imgCreatorPicture.Source = DataManager.loadImage(DataManager.getAbsoluteUri(racer.Maker.ImageUri));
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
            editor.cboClass.Text = racer.Class;
            editor.passedInspect.IsChecked = racer.PassedInspection;
            
            editor.raceData.Children.Clear();
            editor.raceData.ColumnDefinitions.Clear();
            if (racer.Times.Count > 0)
            {
                editor.scores.Visibility = Visibility.Visible;
                double totalTime = 0;
                int totalAmount = 0;
                double bestTime = 10;

                editor.lbNoRaceData.Visibility = Visibility.Collapsed;
                for (int i = 0; i < DataManager.Settings.NumberOfLanes; i++)
                {
                    editor.raceData.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                    //<Label Grid.Column="5" Content="6" FontSize="30" HorizontalAlignment="Center" VerticalAlignment="Center"/>
                    Label lbLaneHeader = new Label();
                    lbLaneHeader.Content = (i + 1).ToString();
                    lbLaneHeader.FontSize = 30;
                    lbLaneHeader.HorizontalAlignment = HorizontalAlignment.Center;
                    lbLaneHeader.VerticalAlignment = VerticalAlignment.Center;
                    lbLaneHeader.SetValue(Grid.RowProperty, 0);
                    lbLaneHeader.SetValue(Grid.ColumnProperty, i);
                    editor.raceData.Children.Add(lbLaneHeader);

                    //<Border Grid.Column="6" Grid.Row="1" Grid.RowSpan="2" BorderThickness="0.5, 0, 1, 0" BorderBrush="{DynamicResource MaterialDesignDivider}" Margin="0, 16, 0, 8"/>
                    Border border = new Border();
                    border.HorizontalAlignment = HorizontalAlignment.Stretch;
                    border.VerticalAlignment = VerticalAlignment.Stretch;
                    border.Margin = new Thickness(0, 8, 0, 8);
                    border.BorderBrush = editor.FindResource("MaterialDesignDivider") as Brush;
                    border.BorderThickness = new Thickness((i == 0 ? 0 : 0.5), 0, (i == DataManager.Settings.NumberOfLanes - 1 ? 0 : 0.5), 0);
                    border.SetValue(Grid.RowProperty, 0);
                    border.SetValue(Grid.RowSpanProperty, 2);
                    border.SetValue(Grid.ColumnProperty, i);
                    editor.raceData.Children.Add(border);

                    StackPanel stp = new StackPanel();
                    stp.HorizontalAlignment = HorizontalAlignment.Center;
                    stp.VerticalAlignment = VerticalAlignment.Top;
                    stp.Orientation = Orientation.Vertical;
                    stp.SetValue(Grid.RowProperty, 1);
                    stp.SetValue(Grid.ColumnProperty, i);

                    List<double> times = new List<double>();
                    foreach (Time time in racer.Times)
                    {
                        if (time.Lane == i + 1)
                        {
                            times.Add(time.Speed);
                        }
                    }
                    times.Sort();

                    for (int t = 0; t < times.Count; t++)
                    {
                        Label lbTime = new Label();
                        lbTime.Content = String.Format("{0:0.000}", times[t]) + "s";
                        lbTime.FontSize = 18;
                        if (t == 0)
                        {
                            totalTime += times[t];
                            totalAmount++;
                            if (bestTime > times[t])
                            {
                                bestTime = times[t];
                            }
                        }
                        else
                        {
                            lbTime.Opacity = 0.5;
                        }
                        lbTime.HorizontalAlignment = HorizontalAlignment.Center;
                        lbTime.VerticalAlignment = VerticalAlignment.Top;
                        lbTime.SetValue(Grid.RowProperty, 1);
                        lbTime.SetValue(Grid.ColumnProperty, i);
                        stp.Children.Add(lbTime);
                    }
                    
                    editor.timeAverage.Text = String.Format("{0:0.000}", (totalTime / totalAmount)) + "s";
                    editor.timeBest.Text = String.Format("{0:0.000}", bestTime) + "s";

                    editor.raceData.Children.Add(stp);
                }
            }

            MakeDialog(parent, editor, WindowType.Display);
        }
        
        private static void MakeDialog(Panel parent, RacerDetails content, WindowType type)
        {
            content._dialogHost = new Dialog(parent, content, false, false, true, delegate ()
                {
                    if (content._type == WindowType.Display)//only allow saving after clicking of dialog, if not creating a new car
                    {
                        if (content.createSaveRacer())
                        {
                            return DialogButton.ReturnEvent.Close;
                        }
                        else
                        {
                            return DialogButton.ReturnEvent.DoNothing;
                        }
                    }
                    else
                    {
                        return DialogButton.ReturnEvent.DoNothing;
                    }
                }, (type == WindowType.Display ? new DialogButton("Delete", DialogButton.Alignment.Left, DialogButton.Style.Flat, delegate () {

                    DialogBox.showOptionBox(parent, "This will completly remove \"" + content.tbCarName.Text + "\" from the competition. All of its history will be deleted. The current race will also be reset.", "Delete \"" + content.tbCarName.Text + "\"?", "Keep", "Delete", delegate (DialogBox.DialogResult result)
                    {
                        if (result == DialogBox.DialogResult.MainOption)
                        {
                            DataManager.Competition.Racers.Remove(content._racer);
                            content.triggerUpdatedRacer();
                            content.triggerDeletedRacer();
                            content._dialogHost.Close();//close this host dialog
                        }
                    });

                    return DialogButton.ReturnEvent.DoNothing;
                }) : null), new DialogButton((type == WindowType.Display ? "Revert" : "Forget"), DialogButton.Alignment.Right, DialogButton.Style.Flat, delegate () {

                    if(type == WindowType.Display)
                    {
                        string changes = "";
                        
                        if (content._racer.Car.Name != content.tbCarName.Text)
                        {
                            changes += Environment.NewLine + "Car's Name: " + content._racer.Car.Name + " -> " + content.tbCarName.Text;
                        }
                        if (content._racer.Car.ImageUri != DataManager.getRelativePath(content.imgCarPicture.Source.ToString()))
                        {
                            changes += Environment.NewLine + "Car's Image";
                        }
                        if (content._racer.Maker.Name != content.tbCreatorName.Text)
                        {
                            changes += Environment.NewLine + "Creator's Name: " + content._racer.Maker.Name + " -> " + content.tbCreatorName.Text;
                        }
                        if (content._racer.Maker.ImageUri != DataManager.getRelativePath(content.imgCreatorPicture.Source.ToString()))
                        {
                            changes += Environment.NewLine + "Creator's Name";
                        }
                        if (content._racer.Barcode != content.tbBarcode.Text)
                        {
                            changes += Environment.NewLine + "Barcode: " + content._racer.Barcode + " -> " + content.tbBarcode.Text;
                        }
                        if (content._racer.Class != content.cboClass.Text)
                        {
                            changes += Environment.NewLine + "Class: " + content._racer.Class + " -> " + content.cboClass.Text;
                        }
                        if (content._racer.PassedInspection != (content.passedInspect.IsChecked == true))
                        {
                            changes += Environment.NewLine + "Passed Inspection: " + (content._racer.PassedInspection ? "Yes" : "No") + " -> " + ((content.passedInspect.IsChecked == true)?"Yes":"No");
                        }

                        //no nothing, this will act like a revert
                        if (changes.Length > 0)
                        {
                            DialogBox.showOptionBox(parent, "This will revert the following changes" + changes, "Revert Changes Made To \"" + content.tbCarName.Text + "\"?", "Keep", "Revert", delegate (DialogBox.DialogResult result)
                            {
                                if (result == DialogBox.DialogResult.MainOption)
                                {
                                    content._dialogHost.Close();//close this host dialog
                                }
                            });
                        }
                        else
                        {
                            return DialogButton.ReturnEvent.Close;
                        }
                    }
                    else
                    {
                        bool changed = false;
                        if ("" != content.tbCarName.Text)
                        {
                            changed = true;
                        }
                        if (DataManager.Settings.DefaltCarImageUri != DataManager.getRelativePath(content.imgCarPicture.Source.ToString()))
                        {
                            changed = true;
                        }
                        if ("" != content.tbCreatorName.Text)
                        {
                            changed = true;
                        }
                        if (DataManager.Settings.DefaltMakerImageUri != DataManager.getRelativePath(content.imgCreatorPicture.Source.ToString()))
                        {
                            changed = true;
                        }
                        if ("" != content.tbBarcode.Text)
                        {
                            changed = true;
                        }
                        if ("" != content.cboClass.Text)
                        {
                            changed = true;
                        }
                        if (false != (content.passedInspect.IsChecked == true))
                        {
                            changed = true;
                        }

                        if (changed)
                        {
                            DialogBox.showOptionBox(parent, "This will erase all data entered for " + content.tbCarName.Text + ".", "Forget \"" + content.tbCarName.Text + "\"?", "Keep", "Forget", delegate (DialogBox.DialogResult result)
                            {
                                if (result == DialogBox.DialogResult.MainOption)
                                {
                                    content._dialogHost.Close();//close this host dialog
                                }
                            });
                        }
                        else
                        {
                            return DialogButton.ReturnEvent.Close;
                        }
                    }

                    return DialogButton.ReturnEvent.DoNothing;
                }), new DialogButton("Save", DialogButton.Alignment.Right, DialogButton.Style.Normal, delegate () {

                    if (content.createSaveRacer())//save racer on save button click
                    {
                        return DialogButton.ReturnEvent.Close;
                    }
                    else
                    {
                        return DialogButton.ReturnEvent.DoNothing;
                    }
                }));
        }

        private bool createSaveRacer()
        {
            //========= create/update racer ========

            

            //create racer
            if (_type == WindowType.Create)
            {
                int count = DataManager.Competition.Racers.Count;
                bool noMatch = true;
                int i = 0;
                for (; i < count && noMatch; i++)
                {
                    if (DataManager.Competition.Racers[i].Barcode == tbBarcode.Text)
                    {
                        noMatch = false;
                    }
                }

                if (noMatch)
                {
                    DataManager.Competition.Racers.Add(new Racer(tbCarName.Text, DataManager.getRelativePath(imgCarPicture.Source.ToString()),
                        tbCreatorName.Text, DataManager.getRelativePath(imgCreatorPicture.Source.ToString()), tbBarcode.Text, cboClass.Text, passedInspect.IsChecked == true));

                    //========save changes to file=========
                    DataManager.saveSettings();

                    triggerUpdatedRacer();
                }
                else
                {
                    DataManager.MessageProvider.showMessage("Another Racer Has The Same Barcode", DataManager.Competition.Racers[i].Car.Name + " allready has the barcode [" + tbBarcode.Text + "]. The barcode for this racer must be changed.");
                    return false;
                }
            }
            else if (_type == WindowType.Display)
            {
                bool changed = false;
                bool checkBarcode = false;

                if (_racer.Car.Name != tbCarName.Text)
                {
                    changed = true;
                }
                if (_racer.Maker.Name != tbCreatorName.Text)
                {
                    changed = true;
                }
                if (_racer.Barcode != tbBarcode.Text)
                {
                    changed = true;
                    checkBarcode = true;
                }
                if (_racer.Class != cboClass.Text)
                {
                    changed = true;
                }
                if (_racer.PassedInspection != (passedInspect.IsChecked == true))
                {
                    changed = true;
                }
                if (_racer.Car.ImageUri != DataManager.getRelativePath(imgCarPicture.Source.ToString()))
                {
                    changed = true;
                }
                if (_racer.Maker.ImageUri != DataManager.getRelativePath(imgCreatorPicture.Source.ToString()))
                {
                    changed = true;
                }

                if (changed)
                {
                    if (checkBarcode)
                    {
                        int count = DataManager.Competition.Racers.Count;
                        bool noMatch = true;
                        int i = 0;
                        for (; i < count && noMatch; i++)
                        {
                            if (DataManager.Competition.Racers[i].Barcode == tbBarcode.Text)
                            {
                                noMatch = false;
                                i--;
                            }
                        }

                        if (noMatch)
                        {
                            _racer.Car.Name = tbCarName.Text;
                            _racer.Maker.Name = tbCreatorName.Text;
                            _racer.Barcode = tbBarcode.Text;
                            _racer.Class = cboClass.Text;
                            _racer.PassedInspection = passedInspect.IsChecked == true;
                            try
                            {
                                _racer.Car.ImageUri = DataManager.getRelativePath(imgCarPicture.Source.ToString());
                            }
                            catch (Exception ex)
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


                            //========save changes to file=========
                            DataManager.saveSettings();

                            triggerUpdatedRacer();
                        }
                        else
                        {
                            DataManager.MessageProvider.showMessage("Another Racer Has The Same Barcode", DataManager.Competition.Racers[i].Car.Name + " allready has the barcode [" + tbBarcode.Text + "]. The barcode for this racer must be changed.");
                            return false;
                        }
                    }
                    else
                    {
                        _racer.Car.Name = tbCarName.Text;
                        _racer.Maker.Name = tbCreatorName.Text;
                        _racer.Barcode = tbBarcode.Text;
                        _racer.Class = cboClass.Text;
                        _racer.PassedInspection = passedInspect.IsChecked == true;
                        try
                        {
                            if (_racer.Car.ImageUri != DataManager.getRelativePath(imgCarPicture.Source.ToString()))
                            {
                                _racer.Car.ImageUri = DataManager.getRelativePath(imgCarPicture.Source.ToString());
                                changed = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            DataManager.MessageProvider.showError("Could Not Apply Car's Image", ex.Message);
                        }

                        try
                        {
                            if (_racer.Maker.ImageUri != DataManager.getRelativePath(imgCreatorPicture.Source.ToString()))
                            {
                                _racer.Maker.ImageUri = DataManager.getRelativePath(imgCreatorPicture.Source.ToString());
                                changed = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            DataManager.MessageProvider.showError("Could Not Apply Creator's Image", ex.Message);
                        }


                        //========save changes to file=========
                        DataManager.saveSettings();

                        triggerUpdatedRacer();
                    }
                }
            }
            return true;
        }

        private void btnCarPicture_Click(object sender, RoutedEventArgs e)
        {
            CameraDialog.Show(_dialogHost.Parent, delegate (Uri file)
            {
                imgCarPicture.Source = DataManager.loadImage(file);
            });
        }

        private void btnCreatorPicture_Click(object sender, RoutedEventArgs e)
        {
            CameraDialog.Show(_dialogHost.Parent, delegate (Uri file)
            {
                imgCreatorPicture.Source = DataManager.loadImage(file);
            });
        }

        private void Grid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            btnCreatorPicture.Visibility = creatorOverLay.Visibility = Visibility.Visible;
        }

        private void Grid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            btnCreatorPicture.Visibility = creatorOverLay.Visibility = Visibility.Collapsed;
        }

        private void Grid_MouseEnter_1(object sender, System.Windows.Input.MouseEventArgs e)
        {
            carOverlay.Visibility = Visibility.Visible;
        }

        private void Grid_MouseLeave_1(object sender, System.Windows.Input.MouseEventArgs e)
        {
            carOverlay.Visibility = Visibility.Collapsed;
        }
    }
}
