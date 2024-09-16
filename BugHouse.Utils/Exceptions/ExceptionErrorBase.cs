using System;
using System.Collections.Generic;

namespace BugHouse.Utils.Exceptions
{
    public abstract class ExceptionErrorBase : Exception
    {
        public int CodeError { get; }
        public string UriRedirect { get; }
        public Dictionary<string, List<string>> Validations { get; }

        public ExceptionErrorBase(string mensagem, int codigoErro) : base(mensagem)
        {
            CodeError = codigoErro;
        }
        public ExceptionErrorBase(string mensagem, int codigoErro, Exception ex) : base(mensagem, ex)
        {
            CodeError = codigoErro;
        }


        #region Validations
        public ExceptionErrorBase(string mensagem, int codigoErro, Dictionary<string, List<string>> validations) : base(mensagem)
        {
            this.CodeError = codigoErro;
            this.Validations = validations;
        }
        public ExceptionErrorBase(string mensagem, int codigoErro, Dictionary<string, List<string>> validations, Exception ex) : base(mensagem, ex)
        {
            this.CodeError = codigoErro;
            this.Validations = validations;
        }
        #endregion


        #region UriRedirect
        public ExceptionErrorBase(string mensagem, int codigoErro, string uriRedirect) : base(mensagem)
        {
            this.CodeError = codigoErro;
            this.UriRedirect = uriRedirect;
        }
        public ExceptionErrorBase(string mensagem, int codigoErro, string uriRedirect, Exception ex) : base(mensagem, ex)
        {
            this.CodeError = codigoErro;
            this.UriRedirect = uriRedirect;
        }
        #endregion

    }
}
