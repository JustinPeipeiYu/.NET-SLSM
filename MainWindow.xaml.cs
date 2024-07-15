using System.Collections.Generic;
using System.ComponentModel;
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
        
        /*LISTS TO STORE DATASETS AND USER DATA*/
        List<DateTime> dates = new List<DateTime>();
        List<string> brands = new List<string>();
        List<float> standardPrices = new List<float>();
        List<float> largePrices = new List<float>();
        List<float> spendingAmount = new List<float>();
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
        }
        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            // Handle closing logic, set e.Cancel as needed
        }

        /*CUSTOM FUNCTIONS*/
        private void readPriceByBrand()
        {
            //NOTE: reading from text files requires a literal path instead of a variable path
            using (StreamReader sr = new StreamReader(System.IO.Path.Combine(path, "priceByBrand.txt"))) //read from file, capture last entry
            {
                string line;
                while ((line = sr.ReadLine()) != "" && line != null)
                {
                    string[] entries = line.Split(",");
                    brands.Add(entries[0].Trim());
                    standardPrices.Add(float.Parse(entries[1].Trim()));
                    largePrices.Add(float.Parse(entries[2].Trim()));
                }
                sr.Close();
            }
            populateInventory();
            return;
        }
        private void readDates()
        {
            //NOTE: reading from text files requires a literal path instead of a variable path
            using (StreamReader sr = new StreamReader(System.IO.Path.Combine(path, "dates.txt")))
            {
                string line;
                while ((line = sr.ReadLine()) != "" && line != null)
                {
                    string[] entries = line.Split(",");
                    dates.Add(DateTime.ParseExact(entries[0].Trim(),"d",culture));
                    entries = entries.Skip(1).ToArray();
                    foreach (string entry in entries)
                    {
                        spendingAmount.Add(float.Parse(entry.Trim()));
                    }
                }
                sr.Close();
            }
            return;
        }
        private void populateInventory()
        {
            foreach (var brand in brands){
                int i = brands.IndexOf(brand);
                inventory[brand] = new Tuple<float, float>(standardPrices[i], largePrices[i]);
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
                if (dates.Count > 0)
                {
                    //if the last date entry was today, do not write new date
                    if (dates.Last().Equals(DateTime.Today.Date))
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
        private void PhilipMorris_Selected(object sender, RoutedEventArgs e)
        {
            brandSelected = "Philip Morris";
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
