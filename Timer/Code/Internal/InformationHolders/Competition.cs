using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Web.Script.Serialization;

namespace Timer
{
    class Competition
    {
        private List<Racer> _racers;
        public List<Racer> Racers
        {
            get { return _racers; }
            set { _racers = value; }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public Competition()
        {
            _racers = new List<Racer>();
            _name = DateTime.Now.ToString("M-d-yyyy_h-mm-ss_tt");
        }

        private const string DEFAULT_FILENAME = "competition.jsn";
        public static string DefaltFileName
        {
            get { return DEFAULT_FILENAME; }
        }

        public void Save(string fileName = DEFAULT_FILENAME)
        {
            string file = DataManager.getAbsolutePath(fileName);
            try
            {
                File.WriteAllText(file, (new JavaScriptSerializer()).Serialize(this));
            }
            catch (Exception ex)
            {
                DataManager.MessageProvider.showError("Failed To Save Competition", "The competition could not be saved.");
            }
        }

        public static Competition Load(string fileName = DEFAULT_FILENAME)
        {
            string file = DataManager.getAbsolutePath(fileName);
            Competition t;
            if (File.Exists(file))
            {
                t = (new JavaScriptSerializer()).Deserialize<Competition>(File.ReadAllText(file));
            }
            else
            {
                t = new Competition();
            }
            return t;
        }

        public void export()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = Name;
            //dlg.DefaultExt = ".comp";
            //dlg.Filter = "Competition File (*.comp)|*.comp";
            dlg.DefaultExt = ".xlsx";
            dlg.Filter = "Excel File (*.xlsx)|*.xlsx";

            Nullable<bool> result = dlg.ShowDialog();
            
            if (result == true)
            {
                string filename = dlg.FileName;

                ExcelPackage export = new ExcelPackage();
                ExcelWorksheet overviewWS = export.Workbook.Worksheets.Add("Overview");

                overviewWS.InsertColumn(1, 13);
                overviewWS.InsertRow(1, Racers.Count + 1);

                overviewWS.Cells[1, 1].Value = "Car's Name";
                overviewWS.Cells[1, 2].Value = "Creator's Name";
                overviewWS.Cells[1, 3].Value = "Class";
                overviewWS.Cells[1, 4].Value = "Barcode";
                overviewWS.Cells[1, 5].Value = "Inspection";
                overviewWS.Cells[1, 6].Value = "Best Time";
                overviewWS.Cells[1, 7].Value = "Average Time";

                for (int i = 1; i <= DataManager.Settings.NumberOfLanes; i++)
                {
                    overviewWS.Cells[1, i + 7].Value = "Lane #" + i.ToString();
                }

                for (int i = 0; i < Racers.Count; i++)
                {
                    overviewWS.Cells[i + 2, 1].Value = Racers[i].Car.Name;
                    overviewWS.Cells[i + 2, 2].Value = Racers[i].Maker.Name;
                    overviewWS.Cells[i + 2, 3].Value = Racers[i].Class;
                    overviewWS.Cells[i + 2, 4].Value = Racers[i].Barcode;
                    overviewWS.Cells[i + 2, 5].Value = Racers[i].PassedInspection ? "Yes" : "No";
                    TimeInfo info = Racers[i].getTimeInfo();
                    if (info != null)
                    {
                        overviewWS.Cells[i + 2, 6].Value = info.BestTime;
                        overviewWS.Cells[i + 2, 7].Value = info.AverageTime;
                    }
                    else
                    {
                        overviewWS.Cells[i + 2, 6].Value = "N/A";
                        overviewWS.Cells[i + 2, 7].Value = "N/A";
                    }

                    for (int l = 0; l < DataManager.Settings.NumberOfLanes; l++)
                    {
                        List<double> times = new List<double>();
                        foreach (Time time in Racers[i].Times)
                        {
                            if (time.Lane == l + 1)
                            {
                                times.Add(time.Speed);
                            }
                        }

                        if (times.Count > 0)
                        {
                            times.Sort();
                            overviewWS.Cells[i + 2, l + 8].Value = times[0];
                        }
                        else
                        {
                            overviewWS.Cells[i + 2, l + 8].Value = "N/A";
                        }
                    }
                }

                overviewWS.Cells[overviewWS.Dimension.Address].AutoFitColumns();
                
                export.SaveAs(new FileInfo(filename));
            }
        }
    }
}
