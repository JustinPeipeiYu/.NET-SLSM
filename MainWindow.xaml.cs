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
        string path = System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetEntryAssembly().Location);
        static int numBrands = 11;
        static int numDays = 365;
        int numEntries;
        float totalSpending;
        /*ARRAYS TO STORE DATASETS AND USER DATA*/
        int[] days = new int[numDays];
        DateTime[] dates = new DateTime[numDays];
        string[] brands = new string[numBrands];
        float[] standardPrices = new float[numBrands];
        float[] largePrices = new float[numBrands];
        float[] spendingAmount = new float[numDays];
        float[] cumSpendingAmount = new float[numDays];
        Dictionary<string, Tuple<float, float>> inventory = new Dictionary<string, Tuple<float, float>>();
       
        //sizeIndex stores price from inventory (sizeIndex = 0 is small, sizeIndex = 1 is large)
        int sizeIndex = -1;
        string brandSelected = "";

        //datetime culture
        CultureInfo culture = new CultureInfo("en-US");
        
        /*MAIN PROGRAM*/
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Handle opening logic
            readDates();
            readPriceByBrand();
            initGraph();
        }
        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            // Handle closing logic, set e.Cancel as needed
        }

        /*CUSTOM FUNCTIONS*/
        private void initGraph()
        {
            /*VARIABLES FOR GRAPH*/
            const double margin = 10;
            double xmin = margin;
            double xmax = canGraph.Width - margin;
            double ymin = margin;
            double ymax = canGraph.Height - margin;
            const double vstep = 120/12;
            const double hstep = 365/12;
            // Make the X axis.
            GeometryGroup xaxis_geom = new GeometryGroup();
            xaxis_geom.Children.Add(new LineGeometry(
                new Point(xmin, ymax), new Point(xmax, ymax)));
            for (double x = xmin;x <= xmax; x += hstep)
            {
                xaxis_geom.Children.Add(new LineGeometry(
                    new Point(x, ymax - margin / 2),
                    new Point(x, ymax + margin / 2)));
            }
            System.Windows.Shapes.Path xaxis_path = new System.Windows.Shapes.Path();
            xaxis_path.StrokeThickness = 1;
            xaxis_path.Stroke = Brushes.Black;
            xaxis_path.Data = xaxis_geom;
            canGraph.Children.Add(xaxis_path);
            // Make the Y ayis.
            GeometryGroup yaxis_geom = new GeometryGroup();
            yaxis_geom.Children.Add(new LineGeometry(
                new Point(xmin, ymin), new Point(xmin, ymax)));
            for (double y = ymin; y <= ymax; y += vstep)
            {
                yaxis_geom.Children.Add(new LineGeometry(
                    new Point(xmin - margin / 2, y),
                    new Point(xmin + margin / 2, y)));
            }
            System.Windows.Shapes.Path yaxis_path = new System.Windows.Shapes.Path();
            yaxis_path.StrokeThickness = 1;
            yaxis_path.Stroke = Brushes.Black;
            yaxis_path.Data = yaxis_geom;
            canGraph.Children.Add(yaxis_path);
            //Draw the data set
            PointCollection points = new PointCollection();
            totalSpending = 0;
            for (int i = 0; i < numEntries; i++)
            {
                totalSpending += spendingAmount[i];
                points.Add(new Point(days[i] + xmin, ymax - vstep/100 * totalSpending));
                cumSpendingAmount[i] = totalSpending;
            }
            txtAmount.Text = totalSpending.ToString("C", new CultureInfo("en-US"));
            Brush brush1 = Brushes.Red;
            Polyline polyline = new Polyline();
            polyline.StrokeThickness = 2;
            polyline.Stroke = brush1;
            polyline.Points = points;
            canGraph.Children.Add(polyline);
        }
        private void readPriceByBrand()
        {
            using (StreamReader sr = new StreamReader(System.IO.Path.Combine(path, "priceByBrand.txt"))) //read from file, capture last entry
            {
                string line;
                for (int i = 0; i < numBrands; i++)
                {
                    line = sr.ReadLine();
                    string[] entries = line.Split(",");
                    brands[i]=entries[0].Trim();
                    standardPrices[i]=float.Parse(entries[1].Trim());
                    largePrices[i]=float.Parse(entries[2].Trim());
                }
                sr.Close();
            }
            populateInventory();
            return;
        }
        private void convertDatesToDays(DateTime[] dates)
        {
            days[0]=0;
            for (int i = 1; i < numEntries; i++)
            {
                days[i]=(dates[i].Date - dates[0].Date).Days;
            } 
        }
        private void readDates()
        {
            //reads the text file with dates and spending amounts 
            using (StreamReader sr = new StreamReader(System.IO.Path.Combine(path, "dates.txt")))
            {
                string line;
                int i = 0;
                while ((line = sr.ReadLine()) != "" && line != null)
                {
                    string[] entries = line.Split(",");
                    dates[i]=DateTime.ParseExact(entries[0].Trim(),"d",culture);
                    entries = entries.Skip(1).ToArray();
                    float sum = 0;
                    foreach (string entry in entries)
                    {
                        sum += float.Parse(entry.Trim());
                    }
                    spendingAmount[i] = sum;
                    i++;
                }
                numEntries = i;
                sr.Close();
            }
            convertDatesToDays(dates);
            return;
        }
        private void populateInventory()
        {
            for (int i = 0; i < numBrands; i++) {
                inventory[brands[i]] = new Tuple<float, float>(standardPrices[i], largePrices[i]);
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
                if (numEntries > 0)
                {
                    //if the last date entry was today, do not write new date
                    if (dates[numEntries].Equals(DateTime.Today.Date))
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
                    sw.Write(","+inventory[brandName].Item1);
                    sw.Close();
                }
            } else
            {
                using (StreamWriter sw = new StreamWriter(System.IO.Path.Combine(path, "dates.txt"), true))
                {
                    sw.Write(","+inventory[brandName].Item2);
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
                    //update program variables
                    readDates();
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
                        //update program variables
                        readDates();
                        break;
                    case MessageBoxResult.No:
                        break;
                }
            }
        }

    }//end class

}
