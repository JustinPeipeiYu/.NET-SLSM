using Microsoft.VisualBasic;
using Microsoft.Win32;
using System;
using System.Diagnostics.Metrics;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Smoke_Less_2024
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int counter, index, streak, longestStreak;
        private bool show;
        string path, fullPath, last;
        DateTime today,lastDate;

        public MainWindow() => InitializeComponent();

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //variable to store entire content of date.txt
            string whole; 
            //today's date
            today= DateTime.Today;
            //statistics textblocks are hidden upon start up
            show = false;
            path = System.IO.Path.GetDirectoryName
                (System.Reflection.Assembly.GetExecutingAssembly().Location);//relative directory path
            fullPath = System.IO.Path.Combine(path, "dates.txt");//relative path for dates.txt
            if (File.Exists(fullPath))//first, read latest date entry if text file exists
            {
                using (StreamReader sr = new StreamReader(System.IO.Path.Combine(path, "dates.txt")))
                {
                    whole = await sr.ReadToEndAsync();//read entire text file
                    index = whole.LastIndexOf("\n");//obtain the index of the last date entry in text file
                    last = whole.Substring(index + 1);//obtain the last date entry in text file
                }
                //convert date string to date time
                lastDate = DateTime.ParseExact(last, "d", CultureInfo.InvariantCulture); 
                streak = daysBetween(lastDate, today);//calculate days since last date entry
            } else
            {
                //file does not exists, so create and write today's date into file
                write("", today.ToString("MM/dd/yyyy"), System.IO.Path.Combine(path, "dates.txt"));
            }
            fullPath = System.IO.Path.Combine(path, "streaks.txt");//relative path for streaks.txt
            if (File.Exists(fullPath))//Second, read latest streak entry if it exists
            {
                using (StreamReader sr = new StreamReader(System.IO.Path.Combine(path, "streaks.txt")))
                {
                    whole = await sr.ReadToEndAsync();//read entire text file
                    index = whole.LastIndexOf("\n");//obtain the index of the last streak entry of text file
                    longestStreak =Int32.Parse(whole.Substring(index + 1));//obtain the last streak entry of text file
                }
            } else
            {
                longestStreak = 0;
                //file does not exist so create and write longest streak into file
                write("", longestStreak.ToString(), System.IO.Path.Combine(path, "streaks.txt"));
            }
            //relative path for daily count.txt
            fullPath = System.IO.Path.Combine(path, "daily count.txt");
            if (File.Exists(fullPath))//Second, read daily count entry if it exists
            {
                using (StreamReader sr = new StreamReader(System.IO.Path.Combine(path, "daily count.txt")))
                {
                    whole = await sr.ReadToEndAsync();//read entire text file
                    index = whole.LastIndexOf("\n");//obtain the index of the last count entry of text file
                    counter = Int32.Parse(whole.Substring(index + 1));//obtain the last count entry of text file
                    
                }
            } 
            else
            {
                counter = 0;
                //file does not exist so create and write count into file
                write("", counter.ToString(), System.IO.Path.Combine(path, "daily count.txt"));
            }
        }

        private int daysBetween(DateTime date1, DateTime date2)
        {
            return (date2 - date1).Days; 
        }

        private void write(string entry1, string entry2, string path)
        {
            using (StreamWriter sw = File.AppendText(path))
            {
                if (entry1.Equals("")) { sw.Write(entry2); }
                else { sw.Write(Environment.NewLine + entry2); }
            }
        }

        
        private void add_Click(object sender, RoutedEventArgs e)
        {
            //end streak and write to streak.txt if it a new high score and it is not the same day
            if ((streak - 1) != -1)
            {
                write(last, today.ToString("MM/dd/yyyy"), System.IO.Path.Combine(path, "dates.txt"));
                if ((streak - 1) > longestStreak)
                {
                    write(longestStreak.ToString(), (streak - 1).ToString(), System.IO.Path.Combine(path, "streaks.txt"));
                }
                //reset streak to 0
                streak = 0;
                ResultBlock2.Text = "You have not smoked for " + streak + " days";
            }
            else if ((streak - 1) == 0)//reset the counter if it's a new day
            {
                counter = 0;
            }
            //increase the daily count
            counter++;
            //write to text boxes
            if (counter == 1) { ResultBlock.Text = "You smoked 1 time today"; }
            else { ResultBlock.Text = "You smoked " + counter + " times today"; }
            write(counter.ToString(), counter.ToString(), System.IO.Path.Combine(path, "daily count.txt"));
        }


        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //toggle the visibility of the statistics
            if (show) { ResultBlock.Visibility = Visibility.Hidden; 
                ResultBlock2.Visibility = Visibility.Hidden;
                ResultBlock3.Visibility = Visibility.Hidden;
                show = false; }
            else{ ResultBlock.Visibility = Visibility.Visible; 
                ResultBlock2.Visibility = Visibility.Visible; 
                ResultBlock3.Visibility = Visibility.Visible;
                show = true; }
            //write daily count to first text block
            if (counter == 1) { ResultBlock.Text = "You smoked 1 time today"; }
            else { ResultBlock.Text = "You smoked " + counter + " times today"; }
            //write the current streak to the second text block
            if ((streak-1) == 1) { ResultBlock2.Text = "You have not smoked for 1 day"; }
            else if ((streak-1)==-1){ ResultBlock2.Text = "You have not smoked for " + streak + " days"; }
            else{ ResultBlock2.Text = "You have not smoked for " + (streak-1) + " days"; }
            //write the longgest streak to the third text block
            if ((streak-1)> longestStreak)
            {
                if ((streak-1) == 1) { ResultBlock3.Text = "Your longest break was 1 day"; }
                else { ResultBlock3.Text = "Your longest break was " + (streak-1) + " days"; }
            } else
            {
                if (longestStreak == 1) { ResultBlock3.Text = "Your longest break was 1 day"; }
                else { ResultBlock3.Text = "Your longest break was " + longestStreak + " days"; }
            }         
        }
    }
}
