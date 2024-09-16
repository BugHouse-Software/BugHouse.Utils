using BugHouse.Utils.Models;
using System;
using System.Collections.Generic;

namespace BugHouse.Utils.Exceptions.ThrowsExceptions
{
    public class BadRequestException : ExceptionErrorBase
    {
        private const int _codigoErro = 400;
        public BadRequestException(string mensagem) : base(mensagem, _codigoErro)
        {
        }

        public BadRequestException(string mensagem, List<Validacao> validacaos) : base(mensagem, _codigoErro, ConverterValidacao(validacaos))
        {
        }

        public BadRequestException(string mensagem, List<Validacao> validacaos, Exception ex) : base(mensagem, _codigoErro, ConverterValidacao(validacaos), ex)
        {
        }


        private static Dictionary<string, List<string>> ConverterValidacao(List<Validacao> validacaos)
        {
            var @return = new Dictionary<string, List<string>>();

            foreach (var validacao in validacaos)
            {
                @return.Add(validacao.Propriedade, validacao.Erros);
            }

            return @return;
        }
    }
}
