using BugHouse.Utils.Extensions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace BugHouse.Utils.Models
{

    [DataContract]
    public enum TipoStatusHealthCheck
    {
        [Display(Name = "Serviço Normal")]
        Normal,
        [Display(Name = "Serviço Aceitável")]
        Aceitavel,
        [Display(Name = "Serviço com Lentidão")]
        Lentidao,
        [Display(Name = "Serviço com Erro")]
        Erro
    }
    public class HealthCheck
    {
        public string Name { get; set; }

        public TipoStatusHealthCheck Status { get; set; }

        public string StatusService => Status.ToDisplayName();

        public TimeSpan Time { get; set; }

        public bool Essencial { get; set; }

        public string ErrorMensage { get; set; }

        public void SetStatus(TimeSpan aceitavel, TimeSpan lento, TimeSpan? erro = null)
        {
            if (Time < aceitavel)
            {
                Status = TipoStatusHealthCheck.Normal;
                return;
            }

            if (Time < lento)
            {
                Status = TipoStatusHealthCheck.Aceitavel;
                return;
            }

            if (erro.HasValue)
            {
                TimeSpan time = Time;
                TimeSpan? timeSpan = erro;
                if (time >= timeSpan)
                {
                    Status = TipoStatusHealthCheck.Erro;
                    return;
                }
            }

            Status = TipoStatusHealthCheck.Lentidao;
        }
    }
}
