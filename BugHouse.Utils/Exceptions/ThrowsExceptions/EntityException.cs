using System;

namespace BugHouse.Utils.Exceptions.ThrowsExceptions
{
    public class EntityException : ExceptionErrorBase
    {
        private const int _codigoError = 422;
        public EntityException(string mensagem) : base(mensagem, _codigoError)
        {
        }
        public EntityException(string mensagem, Exception ex) : base(mensagem, _codigoError, ex)
        {
        }
    }
}
