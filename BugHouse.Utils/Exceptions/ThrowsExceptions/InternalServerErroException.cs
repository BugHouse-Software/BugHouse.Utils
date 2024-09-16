using BugHouse.Utils.Models;
using System;
using System.Collections.Generic;

namespace BugHouse.Utils.Exceptions.ThrowsExceptions
{
    public class InternalServerErroException : ExceptionErrorBase
    {
        private const int _codigoErro = 500;
        public InternalServerErroException(string mensagem) : base(mensagem, _codigoErro)
        {
        }

        public InternalServerErroException(string mensagem, List<Validacao> validacaos, Exception ex) : base(mensagem, _codigoErro, ex)
        {
        }


    }
}
