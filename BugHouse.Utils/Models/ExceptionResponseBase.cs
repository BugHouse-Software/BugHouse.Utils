using Newtonsoft.Json;
using System.Collections.Generic;

namespace BugHouse.Utils.Models
{
    public class ExceptionResponseBase
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("status")]
        public int? Status { get; set; }

        [JsonProperty("detail")]
        public string Detail { get; set; }

        [JsonProperty("instance")]
        public string Instance { get; set; }
        [JsonProperty("urlRedirect")]
        public string UrlRedirect { get; set; }
        [JsonProperty("extensions")]
        public IDictionary<string, object> Extensions { get; set; }
        [JsonProperty("validations")]
        public IDictionary<string, List<string>> Validations { get; set; }

    }
}
