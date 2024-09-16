using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BugHouse.Utils.Extensions
{
    public static class HttpClientHelper
    {
        public static void AddAuthorization(this HttpClient client, string scheme, string value)
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(scheme, value);
        }
        public static void AddHeader(this HttpClient client, string key, string value)
        {
            client.DefaultRequestHeaders.Add(key, value);
        }

        public static async Task<IResponseHttpClient<response, error>> Get<response, error>(this HttpClient client, string endPoint, RequestConfiguration configuration = default(RequestConfiguration))
            where error : class
            where response : class
        {
            try
            {
                if (configuration.IsNull())
                    configuration = new();
                try
                {
                    client.AddHeader("Content-Type", configuration.ContentType);
                }
                catch { }

                var result = await client.GetAsync(endPoint);

                if (!result.IsSuccessStatusCode)
                {
                    var responseError = await GetResponse<error>(result);

                    return new ResponseHttpClient<response, error>()
                    {
                        IsSucess =false,
                        Error = responseError,
                        StatusCode = result.StatusCode
                    };
                }

                var responseMessage = await GetResponse<response>(result);

                return new ResponseHttpClient<response, error>()
                {
                    IsSucess =true,
                    Response = responseMessage,
                    StatusCode = result.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new ResponseHttpClient<response, error>()
                {
                    IsSucess =false,
                    Exception = ex,
                    StatusCode = HttpStatusCode.NotExtended
                };
            }
        }


        public static async Task<IResponseHttpClient<response, error>> Delete<response, error>(this HttpClient client, string endPoint, RequestConfiguration configuration = default(RequestConfiguration))
          where error : class
          where response : class
        {
            try
            {
                if (configuration.IsNull())
                    configuration = new();

                client.AddHeader("Content-Type", configuration.ContentType);
                var result = await client.DeleteAsync(endPoint);

                if (!result.IsSuccessStatusCode)
                {
                    var responseError = await GetResponse<error>(result);

                    return new ResponseHttpClient<response, error>()
                    {
                        IsSucess =false,
                        Error = responseError,
                        StatusCode = result.StatusCode
                    };
                }

                var responseMessage = await GetResponse<response>(result);

                return new ResponseHttpClient<response, error>()
                {
                    IsSucess =true,
                    Response = responseMessage,
                    StatusCode = result.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new ResponseHttpClient<response, error>()
                {
                    IsSucess =false,
                    Exception = ex,
                    StatusCode = HttpStatusCode.NotExtended
                };
            }
        }


        public static async Task<IResponseHttpClient<response, error>> Post<response, error>(this HttpClient client, string endPoint, object jsonContent, RequestConfiguration configuration = default(RequestConfiguration))
         where error : class
         where response : class
        {
            try
            {
                if (configuration.IsNull())
                    configuration = new();

                try
                {
                    client.AddHeader("Content-Type", configuration.ContentType);
                }
                catch { }
                var content = GenerateContent(jsonContent.ToSerialize(), configuration);
                var result = await client.PostAsync(endPoint, content);

                if (!result.IsSuccessStatusCode)
                {
                    var responseError = await GetResponse<error>(result);

                    return new ResponseHttpClient<response, error>()
                    {
                        IsSucess =false,
                        Error = responseError,
                        StatusCode = result.StatusCode
                    };
                }

                var responseMessage = await GetResponse<response>(result);

                return new ResponseHttpClient<response, error>()
                {
                    IsSucess =true,
                    Response = responseMessage,
                    StatusCode = result.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new ResponseHttpClient<response, error>()
                {
                    IsSucess =false,
                    Exception = ex,
                    StatusCode = HttpStatusCode.NotExtended
                };
            }
        }


        public static async Task<IResponseHttpClient<response, error>> Patch<response, error>(this HttpClient client, string jsonContent, string endPoint, RequestConfiguration configuration = default(RequestConfiguration))
       where error : class
       where response : class
        {
            try
            {
                if (configuration.IsNull())
                    configuration = new();

                client.AddHeader("Content-Type", configuration.ContentType);
                var content = GenerateContent(jsonContent, configuration);
                var result = await client.PatchAsync(endPoint, content);

                if (!result.IsSuccessStatusCode)
                {
                    var responseError = await GetResponse<error>(result);

                    return new ResponseHttpClient<response, error>()
                    {
                        IsSucess =false,
                        Error = responseError,
                        StatusCode = result.StatusCode
                    };
                }

                var responseMessage = await GetResponse<response>(result);

                return new ResponseHttpClient<response, error>()
                {
                    IsSucess =true,
                    Response = responseMessage,
                    StatusCode = result.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new ResponseHttpClient<response, error>()
                {
                    IsSucess =false,
                    Exception = ex,
                    StatusCode = HttpStatusCode.NotExtended
                };
            }
        }


        public static async Task<IResponseHttpClient<response, error>> Put<response, error>(this HttpClient client, string jsonContent, string endPoint, RequestConfiguration configuration = default(RequestConfiguration))
       where error : class
       where response : class
        {
            try
            {
                if (configuration.IsNull())
                    configuration = new();

                client.AddHeader("Content-Type", configuration.ContentType);
                var content = GenerateContent(jsonContent, configuration);
                var result = await client.PutAsync(endPoint, content);

                if (!result.IsSuccessStatusCode)
                {
                    var responseError = await GetResponse<error>(result);

                    return new ResponseHttpClient<response, error>()
                    {
                        IsSucess =false,
                        Error = responseError,
                        StatusCode = result.StatusCode
                    };
                }

                var responseMessage = await GetResponse<response>(result);

                return new ResponseHttpClient<response, error>()
                {
                    IsSucess =true,
                    Response = responseMessage,
                    StatusCode = result.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new ResponseHttpClient<response, error>()
                {
                    IsSucess =false,
                    Exception = ex,
                    StatusCode = HttpStatusCode.NotExtended
                };
            }
        }



        public static T ToDeserialize<T>(string value)
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private static async Task<T> GetResponse<T>(HttpResponseMessage value) where T : class
        {
            var responseMessage = await value.Content.ReadAsStringAsync();

            if (responseMessage.IsNullOrEmpty())
                return null;

            return ToDeserialize<T>(responseMessage);
        }
        private static HttpContent GenerateContent(string jsonContent, RequestConfiguration configuration)
        {
            var content = new StringContent(jsonContent, configuration.Encoding, configuration.ContentType);
            return content;
        }


        private class ResponseHttpClient<response, error> : IResponseHttpClient<response, error>
        {
            public bool IsSucess { get; set; }
            public response Response { get; set; }
            public error Error { get; set; }
            public HttpStatusCode StatusCode { get; set; }
            public Exception Exception { get; set; }
        }

    }


    public class RequestConfiguration
    {
        public string ContentType { get; set; } = "application/json";
        public Encoding Encoding { get; set; } = Encoding.UTF8;
    }

    public interface IResponseHttpClient<response, error>
    {
        public bool IsSucess { get; }
        public response Response { get; }
        public error Error { get; }
        public HttpStatusCode StatusCode { get; }
        public Exception Exception { get; }
    }
}
