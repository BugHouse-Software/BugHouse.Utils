using System;

namespace BugHouse.Utils.Models
{
    public class SessionDataEntity
    {
        public string Data { get; set; }
        public string Usuario { get; set; }
        public string UsuarioId { get; set; }
        public DateTime ExpiresDate { get; set; }
    }
}
