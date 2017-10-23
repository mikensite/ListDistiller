//================================================================================================
//
// ListDistillerCLI
//
// Command line application to take a textfile 
// and output a file without duplicates removed 
//  or output a file with duplicates and their respective line numbers
//
//
//
// Michael Nourani - 10-19-2017
//
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace ListDistillerCLI
{
    class Program
    {
        // actually should be called Model!
        private static ViewModel VM = new ViewModel();


        static int Main(string[] args)
        {
            Console.WriteLine("List Distiller");
            Console.WriteLine("--------------");

            foreach (string s in args)
            {
                if (s.StartsWith("-"))
                {
                    switch ((s + " ").ToLower().Substring(1, 1))
                    {
                        case "c":
                            VM.FilterIgnoreCase = !VM.FilterIgnoreCase;
                            break;
                        case "p":
                            VM.FilterIgnorePunctuation = !VM.FilterIgnorePunctuation;
                            break;
                        case "s":
                            VM.FilterIgnoreSpaces = !VM.FilterIgnoreSpaces;
                            break;
                        case "a":
                            VM.FilterAbbreviation = !VM.FilterAbbreviation;
                            break;
                        case "l":
                            VM.ListOutput = !VM.ListOutput;
                            break;
                        case "o":
                            VM.SortOutput = !VM.SortOutput;
                            break;
                        case "e":
                            VM.EntireOutput = !VM.EntireOutput;
                            break;
                        case "x":
                            VM.WriteOutfile = !VM.WriteOutfile;
                            break;
                        default:
                            if (!s.StartsWith("-h") && !s.StartsWith("-?"))
                                Console.WriteLine(" Error - invalid option " + s);
                            ShowHelp();
                            Environment.Exit(1);
                            break;
                    }
                }
                else
                {
                    if (System.IO.File.Exists(s))
                        VM.FileName = s;
                }
            }

            // if no filename, show help then exit
            if (string.IsNullOrEmpty(VM.FileName))
            {
                ShowHelp();
                Environment.Exit(1);
            }

            // load file into data model
            LoadFile();

            // process according to switches
            ProcessLines();


            Console.WriteLine("Total entries: " + VM.Advertisers.Count());
            Console.WriteLine("Duplicates: " + VM.DisplayAdvertisers.Count());

            // create a list or outputs
            List<string> outputLines;

            outputLines = VM.Advertisers
                .Where(x => x.Matches.Count > 0)
                .GroupBy(x => x.ScrubbedText, (key, x) => x.FirstOrDefault())
                .Select(x => x.RawText).ToList();

            // are we including non suplicates too?
            if (VM.EntireOutput)
            {
                outputLines.AddRange(VM.Advertisers
                    .Where(x => x.Matches.Count == 0)
                    .Select(x => x.RawText).ToList());
            }

            // sorted
            if (VM.SortOutput)
                outputLines.Sort();

            // if showing output on terminal 
            if (VM.ListOutput)
            {
                var lst = VM.DisplayAdvertisers.OrderBy(x => x.ScrubbedText).ToList();

                foreach (var v in lst)
                    Console.WriteLine("{0,8}. {1,-60}  Matches lines: {2}",   v.Id , v.RawText , string.Join(",", v.Matches.Select(x => x.Target.ToString())) );
            }

            // if generating output
            if (VM.WriteOutfile)
                System.IO.File.WriteAllLines(VM.FileName + ".filtered.txt", outputLines);


            return 0;
        }


        // list usage and switches
        static private void ShowHelp()
        {                        
            Console.WriteLine("usage: listdistillercli <filename>");
            Console.WriteLine("optional swicthes:");
            Console.WriteLine("-c  Case Sensitive - default case is ignored");
            Console.WriteLine("-a  Abbreviation are not removed (INC, LLC, CO, LTD) - default stripped for comparison");
            Console.WriteLine("-s  Compare with white spaces - default ignore white spaces");
            Console.WriteLine("-p  Compare honoring punctuations - default punctuations are ignored");
            Console.WriteLine("-o  Order alphabetically - default leave original order");
            Console.WriteLine("-l  List result is not displayed - default result is listed on screen");
            Console.WriteLine("-e  Entire file listed as output - default list only duplicates");
            Console.WriteLine("-x  Don't create output file - default generates output '<originalName>.FILTERED.TXT'");
        }
        

        // load file and create data model for each line
        private static void LoadFile()
        {
            string allText = System.IO.File.ReadAllText(VM.FileName);

            var lines = allText.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

            int idSequence = 0;

            // do sort before
            if (VM.SortOutput)
                lines = lines.OrderBy(x => x).ToList();

            // project each line into our model
            VM.Advertisers = lines.Select(x => new Advertiser { RawText = x, Id = ++idSequence }).ToList();

        }

        // process lines into scrubbed text and find matces
        private static void ProcessLines()
        {
            // get scrubbed version of raw text in ScrubbedText field
            foreach (var item in VM.Advertisers)
                item.ScrubbedText = FilterThis(item.RawText);

            // find matches, duplicates
            int matchCount = 0;

            // basically exact macth of scrubbed lines, since filtering does the srubbing
            foreach (var item in VM.Advertisers)
            {
                item.Matches = VM.Advertisers
                    .Where(x => x.ScrubbedText.Equals(item.ScrubbedText) && item.Id != x.Id)
                    .Select(x => new MatchTag() { Confidence = 100, Target = x.Id }).ToList();

                matchCount += item.Matches.Count;
            }
        }

        // apply each of the scrubs and filters based on flags
        // returns scrubbed text
        private static string FilterThis(string txt)
        {
            string str = txt;

            if (VM.FilterAbbreviation)
            {
                str = Regex.Replace(str, "[ .,]LLC", "", RegexOptions.IgnoreCase);
                str = Regex.Replace(str, "[ .,]CO[^M]", "", RegexOptions.IgnoreCase);
                str = Regex.Replace(str, "[ .,]LTD", "", RegexOptions.IgnoreCase);
                str = Regex.Replace(str, "[ .,]INC", "", RegexOptions.IgnoreCase);
            }

            if (VM.FilterIgnoreCase)
                str = str.ToLower();

            if (VM.FilterIgnorePunctuation)
                str = Regex.Replace(str, "[,().:;\"'?{}]" + "*", "");

            if (VM.FilterIgnoreSpaces)
                str = Regex.Replace(str, "[ \t]" + "*", "");

            return str;
        }
    }


    // Shoudl be called Model, all flags and processed lines
    public class ViewModel
    {
        public string Title { get; set; } = "List Distiller";

        // List of lines after processing
        private List<Advertiser> _Advertisers;
        public List<Advertiser> Advertisers
        {
            get { return _Advertisers; }
            set
            {
                _Advertisers = value;
            }
        }

        // List of entries, either complete or just duplicates based on the "OnlySHowMatches" flag
        // Filtered display to be as binding source WPF
        public IEnumerable<Advertiser> DisplayAdvertisers
        {
            get
            {
                return OnlyShowMatches ? _Advertisers.Where(x => x.Matches.Count > 0) : _Advertisers;
            }
        }

        // dummy function to enable code sharing with WPF binding
        private void OnPropertyChanged(string txt = "")
        {
        }

        // Filter flags
        // (OnPropertyChange calls are dummy tags and extraneous, lef over from WPF view model)

        private bool _filterIgnorePunctuation = true;
        public bool FilterIgnorePunctuation { get { return _filterIgnorePunctuation; } set { _filterIgnorePunctuation = value; this.OnPropertyChanged(); } }

        private bool _filterIgnoreCase = true;
        public bool FilterIgnoreCase { get { return _filterIgnoreCase; } set { _filterIgnoreCase = value; this.OnPropertyChanged(); } }

        private bool _filterIgnoreSpaces = true;
        public bool FilterIgnoreSpaces { get { return _filterIgnoreSpaces; } set { _filterIgnoreSpaces = value; this.OnPropertyChanged(); } }

        private bool _filterAbbreviation = true;
        public bool FilterAbbreviation { get { return _filterAbbreviation; } set { _filterAbbreviation = value; this.OnPropertyChanged(); } }

        private bool _listOutput = true;
        public bool ListOutput { get { return _listOutput; } set { _listOutput = value; this.OnPropertyChanged(); } }

        private bool _sortOutput = false;
        public bool SortOutput { get { return _sortOutput; } set { _sortOutput = value; this.OnPropertyChanged(); } }

        private bool _writeOutfile = true;
        public bool WriteOutfile { get { return _writeOutfile; } set { _writeOutfile = value; this.OnPropertyChanged(); } }

        private bool _entireOutput = true;
        public bool EntireOutput { get { return _entireOutput; } set { _entireOutput = value; this.OnPropertyChanged(); } }

        private string _fileName = string.Empty;
        public string FileName { get { return _fileName; } set { _fileName = value; this.OnPropertyChanged(); } }

        private bool _OnlyShowMatches = true;
        public bool OnlyShowMatches { get { return _OnlyShowMatches; } set { _OnlyShowMatches = value; this.OnPropertyChanged(); this.OnPropertyChanged("DisplayAdvertisers"); } }

    }



    // representing a line entry on the input file
    public class Advertiser
    {
        // just the line number
        public int Id { get; set; }
        public string RawText { get; set; }

        public string ScrubbedText { get; set; }

        // list of IDs this entry matches
        public List<MatchTag> Matches { get; set; } = new List<MatchTag>();
    }


    public class MatchTag
    {
        public int Target { get; set; }
        // not used, fuzzy matches would be next step
        public int Confidence { get; set; }
    }



}
