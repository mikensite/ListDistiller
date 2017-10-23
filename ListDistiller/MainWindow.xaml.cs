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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ListDistiller.Models;
using ListDistiller.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Threading;

namespace ListDistiller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ViewModels.ViewModel VM;

        public MainWindow()
        {
            InitializeComponent();

            this.VM = (ViewModels.ViewModel) this.Resources["VM"];
        }

        // Process Text box content into the object
        private void LoadText_Click(object sender, RoutedEventArgs e)
        {
            var txt = this.textBox.Text;

            // split lines
            var lst = txt.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            // System.Diagnostics.Debug.WriteLine(lst.Length);

            // to remeber original line numbers to support sorted or original order output
            int idSequence = 0;

            var z = lst.OrderBy( x=> x ).Select(x => new Advertiser { RawText = x.Trim(), Id = ++idSequence });

            // and we have our data model of each entry ready to be processed
            VM.Advertisers = new ObservableCollection<Advertiser>(z);

            VM.Message = "Loaded " + z.Count() + " entries loaded";

            // auto advance to next tab
            mainTab.SelectedIndex = 1;
        }

        // Process entries into entries
        private void processButton_Click(object sender, RoutedEventArgs e)
        {
            if (VM.Advertisers.Count == 0 ) {
                VM.Message = "Nothing to process, use LOAD on first tab.";
                mainTab.SelectedIndex = 0;
                return;
            }

            VM.Message = "Please wait. Processing ...";
            Dispatcher.Invoke((Action)(() => { }), DispatcherPriority.Render);

            // convert each raw entry to a scrubbed entry, based on user checkboxes
            foreach ( var item in VM.Advertisers )
                item.ScrubbedText = FilterThis(item.RawText);

            // now we just need to compare the scrubbed text for exact matches
            // since white spaces, puntucation and case diff are eliminated
            int matchCount = 0;

            foreach (var item in VM.Advertisers)
            {
                // create a list of other entries which match each entry
                item.Matches = VM.Advertisers
                    .Where(x => x.ScrubbedText.Equals(item.ScrubbedText) && item.Id != x.Id )
                    .Select(x => new MatchTag() { Confidence = 100, Target = x.Id }).ToList();

                matchCount += item.Matches.Count;
            }

            VM.Message = "Total:" + VM.Advertisers.Count +  " duplicates: " + matchCount;

            // auto advance ot next tab
            mainTab.SelectedIndex = 2;

        }

        // given a text, scrub it to stripped version for comparison 
        private string FilterThis( string txt)
        {
            string str = txt;

            // very simple rudementary entity title filtering
            if (VM.FilterAbbreviation)
            {
                str = Regex.Replace(str, "[ .,]LLC", "", RegexOptions.IgnoreCase );

                // make sure we take out CO as in company but not COM as in ".com"
                str = Regex.Replace(str, "[ .,]CO[^M]", "", RegexOptions.IgnoreCase);

                str = Regex.Replace(str, "[ .,]LTD", "", RegexOptions.IgnoreCase);

                str = Regex.Replace(str, "[ .,]INC", "", RegexOptions.IgnoreCase);

                // European AB and SA would be nice addition
            }

            // eliminate case differences
            if (VM.FilterIgnoreCase)
                str = str.ToLower();

            // remove punctuation
            if (VM.FilterIgnorePunctuation)
                str = Regex.Replace(str, "[,().:;\"'?{}]" + "*", "");

            // strip white spaces
            if (VM.FilterIgnoreSpaces)
                str = Regex.Replace(str, "[ \t]" + "*", "");                

            return str;
        }

        // potential extension to compare matches!
        //private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    var z = (Advertiser) ((FrameworkElement)sender).DataContext;

        //    foreach( var v in z.Matches )
        //        System.Diagnostics.Debug.WriteLine( v.Target + " : " + VM.Advertisers.Where(x=>x.Id == v.Target).First().RawText );
        //}

        // Save the scrubbed output to file
        private void SaveOutput_Click(object sender, RoutedEventArgs e)
        {
            VM.Message = "saving to desktop ...";
            Dispatcher.Invoke((Action)(() => { }), DispatcherPriority.Render);
            List<string> outputLines;

            // group the duplicates and pick the first one
            outputLines = VM.Advertisers
                .Where(x => x.Matches.Count > 0)
                .GroupBy(x => x.ScrubbedText, (key, x) => x.FirstOrDefault())
                .Select(x => x.RawText).ToList();

            // we asked to show all
            if (! VM.OnlyShowMatches)
            {
                // Append all entries which had no duplicates
                outputLines.AddRange(VM.Advertisers
                    .Where(x => x.Matches.Count == 0)
                    .Select(x => x.RawText).ToList());
            }

            outputLines.Sort();

            // rudely save to desktop!
            string strPath = Environment.GetFolderPath( System.Environment.SpecialFolder.DesktopDirectory);
            System.IO.File.WriteAllLines(strPath +  @"\DistillerOutput" + ".filtered.txt", outputLines);

            VM.Message = "Saved to desktop. file: DistillerOutput.filtered.txt";

        }
    }
}





