using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Diagnostics.Metrics;
using System.DirectoryServices;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Channels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Documents.DocumentStructures;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using static System.Formats.Asn1.AsnWriter;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SLSM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //path to text files
        string path = Directory.GetCurrentDirectory();
        //variables for size of array
        static int numDays = 365;
        //variable for number of line entries in text file
        int entryNumber;
        //variable for total spending
        float totalSpending;
        /*ARRAYS TO STORE DATASETS AND USER DATA*/
        int[] days = new int[numDays];
        DateTime[] dates = new DateTime[numDays];
        float[] spendingAmount = new float[numDays];
        float[] cumSpendingAmount = new float[numDays];
        
        //Dictionary of cigarette prices by brand
        Dictionary<string, double[]> inventory = new Dictionary<string, double[]>{{ "Benson and Hedges", [17.5, 21.5]},{ "Canadian Classics", [15.75, 18.75] }, { "Du Maurier", [18.6, 22.6] }, { "Export A", [17.5, 21.5] }, { "John Player Special", [14.75, 17.75] }, { "LD", [14.5, 17.5] }, { "Macdonald Select", [14.25, 18.25] }, { "Marlboro" , [14.4, 17.91] }, { "Matinee" , [14.5, 18.5] }, { "NEXT", [14.5, 18.5] }, { "Number 7" , [14.25, 18.25] }, { "Pall Mall", [14.5, 17.5] } };

        /*VARIABLES FOR GRAPH*/
        const double margin = 10;
        double xmin = margin;
        double xmax;
        double ymin = margin;
        double ymax;
        double hstep;
        double vstep;
        double totalDays;

        //variable for slider value
        double sldDay;

        //sizeIndex to choose price from inventory (sizeIndex = 0 is small price, sizeIndex = 1 is large price)
        int sizeIndex = -1;
        string brandSelected = "";

        //datetime constant
        static CultureInfo culture = new CultureInfo("en-US");

        /*MAIN PROGRAM*/
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Handle opening logic
            readDates();
            updateSpendingAmounts();
            initGraph();
            //takes integer parameter sldDay
            updateTextBoxes(0);
        }

        /*CUSTOM FUNCTIONS*/

        //find spending amount
        private double findSpendingAmountPerDay(double sliderValue)
        {
            int nearestDay = (int)Math.Round(sliderValue);
            if (nearestDay == 0 || entryNumber == 0)
            {
                return 0;
            } else if (nearestDay > days[entryNumber - 1])
            {
                return cumSpendingAmount[entryNumber - 1] / nearestDay;
            } else {
                int i = 0;
                while (days[i] < nearestDay)
                {
                    i++;
                }
                if (days[i] != nearestDay)
                {
                    return cumSpendingAmount[i - 1] / nearestDay;
                }
                return cumSpendingAmount[i] / nearestDay;
            }
        }
        void updateSpendingAmounts()
        {
            //recalculate total spending
            totalSpending = 0;
            for (int i = 0; i < entryNumber; i++)
            {
                totalSpending += spendingAmount[i];
                cumSpendingAmount[i] = totalSpending;
            }
        }
        private void initGraph()
        {
            //calculate dataset y values
            if (entryNumber != 0 && DateTime.Today.Date != dates[entryNumber - 1])
            {
                totalDays = (DateTime.Today.Date - dates[0]).Days + 1;
            } else if (entryNumber != 0)
            {
                totalDays = days[entryNumber - 1];
            } else
            {
                totalDays = 0;
            }
            //define dimensions
            ymax = canGraph.Height - margin;
            xmax = canGraph.Width + margin;
            hstep = (xmax - xmin) / totalDays;
            vstep = (ymax - ymin )/ totalSpending;
            // Make the X axis.
            GeometryGroup xaxis_geom = new GeometryGroup();
            xaxis_geom.Children.Add(new LineGeometry(
                new Point(xmin, ymax), new Point(xmax, ymax)));
            System.Windows.Shapes.Path xaxis_path = new System.Windows.Shapes.Path();
            xaxis_path.StrokeThickness = 1;
            xaxis_path.Stroke = Brushes.Black;
            xaxis_path.Data = xaxis_geom;
            canGraph.Children.Add(xaxis_path);
            // Make the Y ayis.
            GeometryGroup yaxis_geom = new GeometryGroup();
            yaxis_geom.Children.Add(new LineGeometry(
                new Point(xmin, 0), new Point(xmin, ymax)));
            System.Windows.Shapes.Path yaxis_path = new System.Windows.Shapes.Path();
            yaxis_path.StrokeThickness = 1;
            yaxis_path.Stroke = Brushes.Black;
            yaxis_path.Data = yaxis_geom;
            canGraph.Children.Add(yaxis_path);
            //Draw the data set
            PointCollection points = new PointCollection();
            points.Add(new Point(xmin, ymax));
            for (int i = 0; i < entryNumber; i++)
            {
                points.Add(new Point(xmin + days[i] * hstep, ymax - cumSpendingAmount[i] * vstep));
            }
            if (entryNumber != 0 && totalDays != days[entryNumber - 1]) {
                points.Add(new Point(xmin + totalDays * hstep, ymax - cumSpendingAmount[entryNumber - 1] * vstep));
            }
            Brush brush1 = Brushes.Red;
            Polyline polyline = new Polyline();
            polyline.StrokeThickness = 2;
            polyline.Stroke = brush1;
            polyline.Points = points;
            canGraph.Children.Add(polyline);
        }
        private void drawVertLine(double x)
        {
            slrRate.Minimum = 0;
            slrRate.Maximum = totalDays;
            GeometryGroup slider_geom = new GeometryGroup();
            slider_geom.Children.Add(new LineGeometry(
                new Point(xmin + x * hstep, 0), new Point(xmin + x * hstep, ymax)));
            System.Windows.Shapes.Path slider_path = new System.Windows.Shapes.Path();
            slider_path.StrokeThickness = 1;
            slider_path.Stroke = Brushes.DarkGray;
            slider_path.Data = slider_geom;
            canGraph.Children.Add(slider_path);
        }
        private void updateTextBoxes(double day)
        {
            txtDay.Text = slrRate.Value.ToString();
            txtRate.Text = findSpendingAmountPerDay(day).ToString("C", new CultureInfo("en-US"));
        }
        private void convertDatesToDays(DateTime[] dates)
        {
            days[0]=1;
            for (int i = 1; i < entryNumber; i++)
            {
                days[i]=(dates[i].Date - dates[0].Date).Days + 1;
            } 
        }
        private void readDates()
        {
            entryNumber = 0;
            if (File.Exists(System.IO.Path.Combine(path, "dates.txt")))
            {//reads the text file with dates and spending amounts 
                using (StreamReader sr = new StreamReader(System.IO.Path.Combine(path, "dates.txt")))
                {
                    string line;
                    while ((line = sr.ReadLine()) != "" && line != null)
                    {
                        string[] entries = line.Split(",");
                        dates[entryNumber] = DateTime.ParseExact(entries[0].Trim(), "d", culture);
                        entries = entries.Skip(1).ToArray();
                        float sum = 0;
                        foreach (string entry in entries)
                        {
                            sum += float.Parse(entry.Trim());
                        }
                        spendingAmount[entryNumber] = sum;
                        entryNumber++;
                    }
                    sr.Close();
                }
                convertDatesToDays(dates);
            }
            else//create file
            {
                using (StreamWriter sw = new StreamWriter(System.IO.Path.Combine(path, "dates.txt")))
                {
                    sw.Write("");
                    sw.Close();
                }
            }
        }

        private MessageBoxResult confirmSubmission()
        {
            string messageBoxText = "Do you want to save changes?";
            string caption = "SLSM";
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBoxResult result;
            result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
            return result;
        }

        private void writeDate()
        {
            /*NOTE: writing to text file requires literal path instead of variable path*/
            using (StreamWriter sw = new StreamWriter(System.IO.Path.Combine(path, "dates.txt"), true))
            {
                //if the file is not blank, write date on new line
                if (entryNumber > 0)
                {
                    if (dates[entryNumber-1].Equals(DateTime.Today.Date))
                    {
                        sw.Close();
                        return;
                    }
                    sw.WriteLine("");
                    sw.Write(DateTime.Now.ToString("d", culture));

                } else //otherwise write date on first line
                {
                    sw.Write(DateTime.Now.ToString("d", culture));
                }
                sw.Close();
            }
            return;
        }

        private void writePrice(string brandName, int sizeIndex)
        {
            if (sizeIndex==0)
            {
                /*NOTE: writing to text file requires literal path instead of variable path*/
                using (StreamWriter sw = new StreamWriter(System.IO.Path.Combine(path, "dates.txt"),true))
                {
                    sw.Write("," + inventory[brandName][0]);
                    sw.Close();
                }
            } else
            {
                using (StreamWriter sw = new StreamWriter(System.IO.Path.Combine(path, "dates.txt"), true))
                {
                    sw.Write("," + inventory[brandName][1]);
                    sw.Close();
                }
            }
            return;
        }
        
        /*DYNAMIC METHODS*/
        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            /*If no radio button selected, do not write to file*/
            if (sizeIndex == -1)
            {
                return;
            }
            /*If no brand selected, do not write to file*/
            if (lbBrand.SelectedItem == null) 
            {
                return;
            }
            //submission procedure when submit button is clicked
            switch (confirmSubmission())
            {
                case MessageBoxResult.Yes:
                    //save changes
                    writeDate();
                    writePrice(brandSelected, sizeIndex);
                    MessageBox.Show("Changes were saved.");
                    //read new data into program
                    readDates();
                    //update program variables
                    sldDay = double.Parse(slrRate.Value.ToString());
                    updateSpendingAmounts();
                    //update display
                    canGraph.Children.Clear();
                    initGraph();
                    drawVertLine(sldDay);
                    updateTextBoxes(sldDay);
                    break;
                case MessageBoxResult.No:
                    break;
            }   
        }

        private void rbRegular_Checked(object sender, RoutedEventArgs e)
        {
            sizeIndex = 0;
        }

        private void rbKing_Checked(object sender, RoutedEventArgs e)
        {
            sizeIndex = 1;
        }

        private void BensonAndHedges_Selected(object sender, RoutedEventArgs e)
        {
            brandSelected = "Benson and Hedges";
        }

        private void CanadianClassics_Selected(object sender, RoutedEventArgs e)
        {
            brandSelected = "Canadian Classics";
        }

        private void DuMaurier_Selected(object sender, RoutedEventArgs e)
        {
            brandSelected = "Du Maurier";
        }

        private void ExportA_Selected(object sender, RoutedEventArgs e)
        {
            brandSelected = "Export A";
        }
        private void JohnPlayerSpecial_Selected(object sender, RoutedEventArgs e)
        {
            brandSelected = "John Player Special";
        }
        private void LD_Selected(object sender, RoutedEventArgs e)
        {
            brandSelected = "LD";
        }

        private void MacdonaldSelect_Selected(object sender, RoutedEventArgs e)
        {
            brandSelected = "Macdonald Select";
        }
        private void Matinee_Selected(object sender, RoutedEventArgs e)
        {
            brandSelected = "Matinee";
        }
        private void NEXT_Selected(object sender, RoutedEventArgs e)
        {
            brandSelected = "NEXT";
        }

        private void Number7_Selected(object sender, RoutedEventArgs e)
        {
            brandSelected = "Number 7";
        }

        private void PallMall_Selected(object sender, RoutedEventArgs e)
        {
            brandSelected = "Pall Mall";
        }
        private void Marlboro_Selected(object sender, RoutedEventArgs e)
        {
            brandSelected = "Marlboro";
        }

        private void btnSubmit_KeyDown(object sender, KeyEventArgs e)
        {
            //submission procedure when spacebar is pressed
            if (e.Key == Key.Space)
            {
                /*If no radio button selected, do not write to file*/
                if (sizeIndex == -1)
                {
                    return;
                }
                /*If no brand selected, do not write to file*/
                if (lbBrand.SelectedItem == null)
                {
                    return;
                }
                switch (confirmSubmission())
                {
                    case MessageBoxResult.Yes:
                        //save changes
                        writeDate();
                        writePrice(brandSelected, sizeIndex);
                        MessageBox.Show("Changes were saved.");
                        //read new data into program
                        readDates();
                        //update program variables
                        sldDay = double.Parse(slrRate.Value.ToString());
                        updateSpendingAmounts();
                        //update display
                        canGraph.Children.Clear();
                        initGraph();
                        drawVertLine(sldDay);
                        updateTextBoxes(sldDay);
                        break;
                    case MessageBoxResult.No:
                        break;
                }
            }
        }

        private void slrRate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            sldDay = double.Parse(slrRate.Value.ToString());
            canGraph.Children.Clear();
            initGraph();
            drawVertLine(sldDay);
            updateTextBoxes(sldDay);
        }
    }//end class

}
