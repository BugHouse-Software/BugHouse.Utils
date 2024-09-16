using System;

namespace BugHouse.Utils.Exceptions.ThrowsExceptions
{
    public class AthorizationException : ExceptionErrorBase
    {
        private const int _codigoErro = 401;

        public AthorizationException() : base("User is not authorized.", _codigoErro)
        {
        }

        public AthorizationException(Exception ex) : base("User is not authorized.", _codigoErro, ex)
        {
        }

        public AthorizationException(string mensagem) : base(mensagem, _codigoErro)
        {
        }



    }
}
