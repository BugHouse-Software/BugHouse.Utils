using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BugHouse.Utils.Models
{
    public class Validacao
    {
        public string Propriedade { get; set; }

        public string Mensagem { get; set; }

        [JsonIgnore]
        public List<string> Erros { get; set; }
    }
}
