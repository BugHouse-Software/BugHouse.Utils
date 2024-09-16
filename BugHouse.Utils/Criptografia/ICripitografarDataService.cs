using System;

namespace BugHouse.Utils.Criptografia
{
    public interface ICripitografarDataService
    {
        string Criptograr(object value, string usuario, DateTime expireDate);
        Class Descriptografar<Class>(string valorCriptografado, string usuario, DateTime expireDate) where Class : class;
    }
}
