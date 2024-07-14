using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Diagnostics.Metrics;
using System.Drawing;
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
using System.Xml.Linq;
using static System.Formats.Asn1.AsnWriter;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SLSM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //protected int index, hoursToBeat, sizeOfList, index2;
        string path = System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetEntryAssembly().Location);
        /// <summary>
        /*Lists of data*/
        /// </summary>
        List<DateTime> dates = new List<DateTime>();
        List<string> brands = new List<string>();
        List<float> standardPrices = new List<float>();
        List<float> largePrices = new List<float>();
        List<float> spendingAmount = new List<float>();
        Dictionary<string, Tuple<float, float>> inventory = new Dictionary<string, Tuple<float, float>>();
        //index to get price from inventory (index = 0 is small, index = 1 is large)
        int index = -1;
        //protected bool after;
        //Tuple<string, string> output;
        //Tuple<int, int> seeds;
        //CultureInfo provider;
        //int numOfPacks = 0;


        //public MainWindow() => InitializeComponent();

        /*
            path = System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetEntryAssembly().Location); ;//relative directory path
            */
        /*
         * !int.TryParse(Textbox1.Text, out parsedValue) //try to parse a textbox input
         */
        /*
         * Convert.ToInt32((date2 - date1).TotalHours) //convert date to int
         * */
        /*
         * using (StreamWriter sw = File.AppendText(path)) //write to text file
            {
                sw.WriteLine(entry1);
                sw.Close();
            }
         */
        /*
        protected List<string> separateEntry(string entries) //separating entries in string 
        {
            string sEntry="";
            provider = new CultureInfo("en-US");
            List<string> entryList = new List<string>();
            foreach (char c in entries)
            {
                if (c != '\n' && c!='\r')
                {
                    sEntry += c;
                } else if (c == '\n' && !sEntry.Equals(""))
                {
                    
                        entryList.Add(sEntry);
                    
                    sEntry = "";
                }
            }
            if (!sEntry.Equals(""))
            {
                entryList.Add(sEntry);
            }
                
            return entryList;
        }
        */
        /*                  
            string newWord="";                          //reverse a word
            for (int i=word.Length-1; i>=0; i--)
            {
                newWord += word.Substring(i, 1);
            }
            return newWord;
            */
        /*
         * using (StreamReader sr = new StreamReader(System.IO.Path.Combine(path, "seed.txt"))) //read from file, capture last entry
                {
                    first = sr.ReadLine();
                    whole=sr.ReadToEnd();
                    index = whole.LastIndexOf("\n");
                    last = whole.Substring(index+1);
                    sr.Close();
                }
         */

        /*
         * if (File.Exists(System.IO.Path.Combine(path, "dates.txt"))) //check if file exists, if not create
            {
                provider = new CultureInfo("en-US");
                using (StreamReader sr = new StreamReader(System.IO.Path.Combine(path, "dates.txt")))
            } else {
                write("", System.IO.Path.Combine(path, "dates.txt"));//update date
            }
         */
        /*
         * 
            days = Convert.ToInt32(hours / 24); //convert from one unit to another
         */
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //today's date

            //seeds = readSeed();
            //readDate(false);

            // Handle opening logic, NOTE: reading from text file requires a literal path instead of a variable path
            using (StreamReader sr = new StreamReader(System.IO.Path.Combine(path, "brandNames.txt"))) //read from file, capture last entry
            {
                string line;
                while ((line = sr.ReadLine()) != "" && line != null)
                {
                    brands.Add(line);
                }
                sr.Close();
            }
            using (StreamReader sr = new StreamReader(System.IO.Path.Combine(path, "spendingAmounts.txt")))
            {
                string line;
                while ((line = sr.ReadLine()) != "" && line != null)
                {
                    var price = float.Parse(line);
                    spendingAmount.Add(price);
                }
                sr.Close();
            }
            using (StreamReader sr = new StreamReader(System.IO.Path.Combine(path, "standardPrices.txt"))) 
            {
                string line;
                while ((line = sr.ReadLine()) != "" && line != null)
                {
                    var price = float.Parse(line);
                    standardPrices.Add(price);
                }
                sr.Close();
            }
            using (StreamReader sr = new StreamReader(System.IO.Path.Combine(path, "largePrices.txt"))) 
            {
                string line;
                while ((line = sr.ReadLine()) != "" && line != null)
                {
                    var price = float.Parse(line);
                    largePrices.Add(price);
                }
                sr.Close();
            }
            using (StreamReader sr = new StreamReader(System.IO.Path.Combine(path, "firstDate.txt"))) 
            {
                string line;
                while ((line = sr.ReadLine()) != "" && line != null)
                {
                    dates.Add(DateTime.Parse(line));
                }
                sr.Close();
            }
            populateInventory(inventory,brands,standardPrices,largePrices);
        }
        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            // Handle closing logic, set e.Cancel as needed
        }

        private Dictionary<string, Tuple<float, float>> populateInventory(Dictionary<string, Tuple<float, float>> inventory, List<string> brands,List<float> standardPrices,List<float> largePrices)
        {
            foreach (var brand in brands){
                int i = brands.IndexOf(brand);
                inventory[brand] = new Tuple<float, float>(standardPrices[i], largePrices[i]);
            }
            return inventory;
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


        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            /*If there entry, do not write to file*/
            if (dates.Count > 0)
            {
                return;
            }
            /*If no radio button selected, do not write to file*/
            if (index == -1)
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
                    /*NOTE: writing to text file requires literal path instead of variable path*/
                    using (StreamWriter sw = new StreamWriter(System.IO.Path.Combine(path, "firstDate.txt")))
                    {
                        sw.WriteLine(DateTime.Now.ToString("d/M/yyyy"));
                        sw.Close();
                    }
                    MessageBox.Show("Changes were saved.");
                    break;
                case MessageBoxResult.No:
                    break;
            }   
        }

        private void btnSubmit_Selected(object sender, RoutedEventArgs e, KeyEventArgs s)
        {
            if (s.Key == Key.Space)
            {
                switch (confirmSubmission())
                {
                    case MessageBoxResult.Yes:
                        /*NOTE: writing to text file requires literal path instead of variable path*/
                        using (StreamWriter sw = new StreamWriter(System.IO.Path.Combine(path, "firstDate.txt")))
                        {
                            sw.WriteLine(DateTime.Now.ToString("d/M/yyyy"));
                            sw.Close();
                        }
                        MessageBox.Show("Changes were saved.");
                        break;
                    case MessageBoxResult.No:
                        break;
                }
            }
        }

        private void rbRegular_Checked(object sender, RoutedEventArgs e)
        {
            index = 0;
        }

        private void rbKing_Checked(object sender, RoutedEventArgs e)
        {
            index = 1;
        }
    }//end class

}
