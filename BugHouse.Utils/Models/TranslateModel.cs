using System.Collections.Generic;

namespace BugHouse.Utils.Models
{
    internal class TranslateModel
    {
        public string CultureLinguage { get; set; }
        public Dictionary<string, string> Translates { get; set; }
    }
}
