using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListDistiller.Models
{
    public class Advertiser
    {
        public int Id { get; set; }
        public string RawText { get; set; }

        public string ScrubbedText { get; set; }

        public List<MatchTag> Matches { get; set; } = new List<MatchTag>();

    }


    public class MatchTag
    {
        public int Target { get; set; }

        public int Confidence { get; set; }


    }

}
