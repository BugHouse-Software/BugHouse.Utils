using BugHouse.Utils.Models;
using Microsoft.IdentityModel.Tokens;
using System;

namespace BugHouse.Utils.Criptografia
{
    public interface IJwtService
    {
        SessionDataEntity DeserializeJWT(string jwtToken);
        string GenerateTokenJwt(string userId,
                                       string usuario,
                                       DateTime expiresDate,
                                       string criptograpyData,
                                       int? tpLinguagem,
                                       object dadosUsuario = null,
                                       string type = "Bearer",
                                       string securityAlgorithms = SecurityAlgorithms.HmacSha256,
                                       string codeSecurity = "");
    }
}
