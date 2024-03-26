using System.Diagnostics.Eventing.Reader;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using static System.Formats.Asn1.AsnWriter;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Smoke_Less_2024
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        protected int index, hoursToBeat, sizeOfList, index2;
        protected string path, first, whole, last, seed;
        protected bool after;
        Tuple<string, string> output;
        Tuple<int, int> seeds;
        CultureInfo provider;

        public MainWindow() => InitializeComponent();

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
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //today's date
            
            //seeds = readSeed();
            //readDate(false);
        }

        
    }//end class

}
