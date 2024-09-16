using BugHouse.Utils.Extensions;
using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace BugHouse.Utils.Criptografia
{
    public class CripitografarDataService : ICripitografarDataService
    {

        public string Criptograr(object value, string usuario, DateTime expireDate)
        {
            string chave1 = GetKeyMac();
            string chave2 = usuario;
            string chave3 = expireDate.ToString("yyyy:HH:mm:dd:ss:MM");

            string valor = value.ToSerialize();

            // Concatenação das chaves para formar a chave final
            string chaveFinal = $"{chave1}{chave2}{chave3}";

            chaveFinal = chaveFinal.PadRight(32, '\0');

            // Limitando o tamanho da chave para 32 bytes para AES
            byte[] keyBytes = Encoding.UTF8.GetBytes(chaveFinal.Substring(0, 32));

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = keyBytes;
                aesAlg.GenerateIV();

                // Criando um encryptor para criptografar os dados
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Transformar os dados em bytes
                byte[] dadosBytes = Encoding.UTF8.GetBytes(valor);

                // Criptografar os dados
                byte[] dadosCriptografados = encryptor.TransformFinalBlock(dadosBytes, 0, dadosBytes.Length);

                // IV deve ser conhecido para descriptografar
                byte[] dadosCriptografadosComIV = new byte[aesAlg.IV.Length + dadosCriptografados.Length];
                Array.Copy(aesAlg.IV, 0, dadosCriptografadosComIV, 0, aesAlg.IV.Length);
                Array.Copy(dadosCriptografados, 0, dadosCriptografadosComIV, aesAlg.IV.Length, dadosCriptografados.Length);

                // Converter para base64 para ser um string válido
                return Convert.ToBase64String(dadosCriptografadosComIV);
            }

        }

        public Class Descriptografar<Class>(string valorCriptografado, string usuario, DateTime expireDate) where Class : class
        {
            string chave1 = GetKeyMac();
            string chave2 = usuario;
            string chave3 = expireDate.ToString("yyyy:HH:mm:dd:ss:MM");

            // Concatenação das chaves para formar a chave final
            string chaveFinal = $"{chave1}{chave2}{chave3}";
            chaveFinal = chaveFinal.PadRight(32, '\0');
            // Limitando o tamanho da chave para 32 bytes para AES
            byte[] keyBytes = Encoding.UTF8.GetBytes(chaveFinal.Substring(0, 32));

            // Convertendo o token criptografado de base64 para bytes
            byte[] dadosCriptografadosComIV = Convert.FromBase64String(valorCriptografado);

            // IV é o primeiro bloco de bytes
            byte[] iv = new byte[16];
            Array.Copy(dadosCriptografadosComIV, 0, iv, 0, 16);

            // Resto dos bytes são os dados criptografados
            byte[] dadosCriptografados = new byte[dadosCriptografadosComIV.Length - 16];
            Array.Copy(dadosCriptografadosComIV, 16, dadosCriptografados, 0, dadosCriptografadosComIV.Length - 16);

            // Configurando AES para decifrar os bytes
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = keyBytes;
                aesAlg.IV = iv;

                // Criar um descriptografador para os bytes
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                try
                {
                    // Criptografar e transpor os bytes criptografados
                    using (MemoryStream msDecrypt = new MemoryStream(dadosCriptografados))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                var valueString = srDecrypt.ReadToEnd();
                                return valueString.ToDeserialize<Class>();
                            }
                        }
                    }
                }
                catch (CryptographicException)
                {
                    throw new CryptographicException("Erro na descriptografia.");
                }
            }
        }


        private string GetKeyMac()
        {
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                // Verifica se a interface de rede é física e operacional
                if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                    networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    // Obtém o endereço MAC da interface de rede
                    PhysicalAddress macAddress = networkInterface.GetPhysicalAddress();
                    return macAddress.ToString();
                }
            }

            return "";
        }
    }
}
