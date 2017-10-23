using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ListDistiller.Models;


namespace ListDistiller.ViewModels
{
    // utility base class to allow short hand for Propperty Changes 
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // the view model (since a simple utility, includes kitchen sink!)
    public class ViewModel : ViewModelBase
    {

        public string Title { get; set; } = "List Distiller";

        private ObservableCollection<Advertiser> _Advertisers;
        public ObservableCollection<Advertiser> Advertisers
        {
            get { return _Advertisers; }
            set
            {
                _Advertisers = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged("DisplayAdvertisers");
            }
        }

        // filetred view based on "Show Dupllicates only" or else show all
        public IEnumerable<Advertiser> DisplayAdvertisers
        {
            get
            {
                return OnlyShowMatches ? _Advertisers.Where(x => x.Matches.Count > 0) : _Advertisers;
            }
        }

        // checkboxes and filenames

        private bool _filterIgnorePunctuation = true;
        public bool FilterIgnorePunctuation { get { return _filterIgnorePunctuation; } set { _filterIgnorePunctuation = value; this.OnPropertyChanged(); } }

        private bool _filterIgnoreCase = true;
        public bool FilterIgnoreCase { get { return _filterIgnoreCase; } set { _filterIgnoreCase = value; this.OnPropertyChanged(); } }

        private bool _filterIgnoreSpaces = true;
        public bool FilterIgnoreSpaces { get { return _filterIgnoreSpaces; } set { _filterIgnoreSpaces = value; this.OnPropertyChanged(); } }

        private bool _filterAbbreviation = true;
        public bool FilterAbbreviation { get { return _filterAbbreviation; } set { _filterAbbreviation = value; this.OnPropertyChanged(); } }

        private bool _listOutput = false;
        private bool ListOutput { get { return _listOutput; } set { _listOutput = value; this.OnPropertyChanged(); } }

        private bool _sortOutput = false;
        private bool SortOutput { get { return _sortOutput; } set { _sortOutput = value; this.OnPropertyChanged(); } }

        private bool _writeOutfile = true;
        private bool WriteOutfile { get { return _writeOutfile; } set { _writeOutfile = value; this.OnPropertyChanged(); } }

        private bool _entireOutput = true;
        private bool EntireOutput { get { return _entireOutput; } set { _entireOutput = value; this.OnPropertyChanged(); } }

        private string _fileName = string.Empty;
        private string FileName { get { return _fileName; } set { _fileName = value; this.OnPropertyChanged(); } }

        private bool _OnlyShowMatches = false;
        public bool OnlyShowMatches { get { return _OnlyShowMatches; } set { _OnlyShowMatches = value; this.OnPropertyChanged(); this.OnPropertyChanged("DisplayAdvertisers"); } }

        private string _Message = string.Empty; 
        public string  Message { get { return _Message; } set { _Message = value; this.OnPropertyChanged();  } }
    }

}
