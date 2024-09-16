using BugHouse.Utils.Exceptions.ThrowsExceptions;
using BugHouse.Utils.Extensions;
using BugHouse.Utils.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BugHouse.Utils.Criptografia
{
    internal class JwtService : IJwtService
    {
        internal void ValidateJwt(IServiceCollection services)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var hdKey = JwtHelperService.GetHardDriveSerial();
                var key = Encoding.UTF8.GetBytes(hdKey);


                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = "Dashboard",
                    ValidateAudience = true,
                    ValidAudience = "All",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine("Token inválido: " + context.Exception.Message);
                        return Task.CompletedTask;
                    }
                };
            });
            }
            catch (Exception ex)
            {
                throw new AthorizationException(ex);
            }

        }
        public string GenerateTokenJwt(string userId,
                                       string usuario,
                                       DateTime expiresDate,
                                       string criptograpyData,
                                       int? tpLinguagem,
                                       object dadosUsuario = null,
                                       string type = "Bearer",
                                       string securityAlgorithms = SecurityAlgorithms.HmacSha256Signature,
                                       string codeSecurity = "")
        {

            var codeSecurityValue = String.Empty;

            if (codeSecurity.IsNullOrWhiteSpace())
                codeSecurityValue = JwtHelperService.GetHardDriveSerial();
            else
                codeSecurityValue = codeSecurity;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(codeSecurityValue));
            var credentials = new SigningCredentials(securityKey, securityAlgorithms);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim("date", expiresDate.ToString()),
                new Claim("type", type),


                new Claim(JwtRegisteredClaimNames.PreferredUsername, usuario),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

            };

            if (!tpLinguagem.IsNull())
                claims.Add(new Claim("linguagem", tpLinguagem.ToString()));

            if (!criptograpyData.IsNullOrWhiteSpace())
                claims.Add(new Claim(JwtRegisteredClaimNames.CHash, criptograpyData));

            if (!dadosUsuario.IsNull())
                claims.Add(new Claim(JwtRegisteredClaimNames.Profile, dadosUsuario.ToSerialize()));

            var token = new JwtSecurityToken(
                claims: claims,
                expires: expiresDate,
                signingCredentials: credentials
            );
            var valueToken = new JwtSecurityTokenHandler().WriteToken(token);
            return valueToken;
        }

        public SessionDataEntity DeserializeJWT(string jwtToken)
        {
            try
            {

                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                JwtSecurityToken parsedToken = tokenHandler.ReadJwtToken(jwtToken);

                var @return = new SessionDataEntity();
                @return.UsuarioId= parsedToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
                String xx = parsedToken.Claims.FirstOrDefault(c => c.Type == "date").Value.ToString();
                @return.ExpiresDate = xx.ToDateTime();


                @return.Usuario= parsedToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.PreferredUsername)?.Value;
                @return.Data= parsedToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.CHash)?.Value;

                return @return;
            }
            catch (Exception ex)
            {
                throw new AthorizationException(ex);
            }

        }


    }

    public struct JwtHelperService
    {
        public static string GetHardDriveSerial()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return GetHardDriveSerialWindows();
            else
                return GetHardDriveSerialLinux();
        }


        private static string GetHardDriveSerialLinux()
        {


            try
            {
                string devicePath = "/dev/sda";
                string serialFilePath = $"/sys/block/{Path.GetFileName(devicePath)}/serial";

                if (File.Exists(serialFilePath))
                {
                    string serialNumber = File.ReadAllText(serialFilePath).Trim();
                    return serialNumber;
                }
                else
                {

                    string diskSerialNumber = Environment.GetEnvironmentVariable("ASPNET_VERSION");

                    if (!diskSerialNumber.IsNullOrWhiteSpace())
                        return diskSerialNumber;

                }
            }
            catch (Exception ex)
            {
                throw new Exception("Hard drive serial number not found.", ex);
            }

            throw new Exception("Hard drive serial number not found.");
        }

        private static string GetHardDriveSerialWindows()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_DiskDrive");
            ManagementObjectCollection collection = searcher.Get();

            foreach (ManagementObject obj in collection)
            {
                return obj["SerialNumber"].ToString().Trim();
            }

            throw new Exception("Hard drive serial number not found.");
        }

    }

}



