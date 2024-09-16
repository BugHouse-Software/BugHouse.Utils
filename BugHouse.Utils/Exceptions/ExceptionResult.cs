using BugHouse.Utils.Extensions;
using BugHouse.Utils.Models;
using BugHouse.Utils.Translates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace BugHouse.Utils.Exceptions
{
    public static class ExceptionResult
    {
        public static void UseCustomErrors(this IApplicationBuilder app, bool includeDetails)
        {
            app.UseMiddleware<CustomErrorMiddleware>(includeDetails);
        }

        public class CustomErrorMiddleware
        {
            private readonly RequestDelegate _next;
            private readonly bool _includeDetails;

            public CustomErrorMiddleware(RequestDelegate next, bool includeDetails)
            {
                _next = next;
                _includeDetails = includeDetails;
            }

            public async Task InvokeAsync(HttpContext context, ITranslatesService translates)
            {
                try
                {
                    Exception exceptionBase = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                    if (!exceptionBase.IsNull())
                    {
                        await WriteResponse(context, exceptionBase, translates);
                    }
                    else
                        await _next(context); // Continua para o próximo middleware
                }
                catch (Exception ex)
                {
                    await WriteResponse(context, ex, translates);
                }
            }

            private async Task WriteResponse(HttpContext httpContext, Exception exceptionValue, ITranslatesService translates)
            {
                Exception exceptionBase = exceptionValue;
                if (exceptionBase == null)
                {
                    return;
                }

                #region Declaração problens Details
                ExceptionResponseBase problemDetails = new ExceptionResponseBase();
                httpContext.Response.ContentType = "application/problem+json";
                if (exceptionBase is ExceptionErrorBase ex)
                {
                    httpContext.Response.StatusCode = ex.CodeError;
                    problemDetails.Title = ex.Message;

                    #region Validações
                    if (!ex.Validations.IsNullOrEmpty())
                    {
                        problemDetails.Validations = ex.Validations;
                    }
                    #endregion

                    #region Detalhes
                    if (_includeDetails)
                    {
                        problemDetails.Detail = exceptionBase.ToString();
                    }
                    #endregion

                    #region UriRedirect
                    if (!ex.UriRedirect.IsNullOrWhiteSpace())
                    {
                        problemDetails.UrlRedirect = ex.UriRedirect;
                    }
                    #endregion
                }
                else
                {
                    if (exceptionBase is (DbUpdateException or SqlException or OracleException))
                    {

                        httpContext.Response.StatusCode = 422;

                        if (_includeDetails)
                            problemDetails.Detail = exceptionBase.ToString();

                        if (exceptionBase is DbUpdateException dbUpdate)
                        {
                            switch (dbUpdate.InnerException)
                            {
                                case OracleException oracleEx:
                                    switch (oracleEx.Number)
                                    {
                                        case 2291:
                                            problemDetails.Title = $"{translates.Get("O valor informado para a chave estrangeira")} '{GetConstraintNameFromMessage(oracleEx.Message, translates)}'";
                                            break;
                                        case 1:
                                            string duplicateKey = GetConstraintNameFromMessage(oracleEx.Message, translates);
                                            problemDetails.Title =$"{translates.Get("O registro com a chave")} '{duplicateKey}' {translates.Get("já foi inserido anteriormente")}.";
                                            break;

                                        case 12899:
                                            problemDetails.Title = $"{translates.Get("Valor muito grande para uma coluna.")}";
                                            break;

                                        case 1400:
                                            problemDetails.Title = $"{translates.Get("Tentativa de inserir um valor nulo em uma coluna não anulável.")}";
                                            break;

                                        default:
                                            problemDetails.Title =oracleEx.Message;
                                            break;
                                    }
                                    break;
                                case SqlException sqlEx:
                                    switch (sqlEx.Number)
                                    {
                                        case 547:
                                            string foreignKey = GetConstraintNameFromMessage(sqlEx.Message, translates);
                                            problemDetails.Title = $"{translates.Get("O valor informado para a chave estrangeira")} '{foreignKey}'";
                                            break;

                                        case 2627:
                                        case 2601:
                                            string uniqueKey = GetConstraintNameFromMessage(sqlEx.Message, translates);
                                            problemDetails.Title =$"{translates.Get("O registro com a chave")} '{uniqueKey}' {translates.Get("já foi inserido anteriormente")}.";
                                            break;
                                        case 1205:
                                            problemDetails.Title =translates.Get("Ocorreu um deadlock");
                                            break;

                                        default:
                                            problemDetails.Title =sqlEx.Message;
                                            break;
                                    }
                                    break;
                                default:
                                    problemDetails.Title =dbUpdate.Message;
                                    break;
                            }

                        }
                        else
                        {
                            problemDetails.Title =exceptionBase.Message;
                        }
                    }
                    else
                    {
                        httpContext.Response.StatusCode = 500;
                        problemDetails.Title = exceptionBase.Message ?? "Unidentified or unreported error.";

                        if (_includeDetails)
                        {
                            problemDetails.Detail = exceptionBase.ToString();
                        }
                    }
                }

                #endregion

                #region  Conatext Set Values
                if (Activity.Current?.Id != null)
                {
                    if (problemDetails.Extensions == null)
                    {
                        problemDetails.Extensions = new Dictionary<string, object>();
                    }

                    problemDetails.Extensions["traceIdActivity"] = Activity.Current?.Id;
                }

                if (httpContext?.TraceIdentifier != null)
                {
                    if (problemDetails.Extensions == null)
                    {
                        problemDetails.Extensions = new Dictionary<string, object>();
                    }

                    problemDetails.Extensions["traceIdContext"] = httpContext?.TraceIdentifier;
                }
                #endregion

                #region Configuration Serialize Json
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = true,
                    PropertyNamingPolicy = new LowercaseNamingPolicy()
                };

                #endregion
                await JsonSerializer.SerializeAsync(httpContext.Response.Body, problemDetails, options);


            }

            static string GetConstraintNameFromMessage(string message, ITranslatesService translates)
            {
                var parts = message.Split(new[] { "(", ")" }, StringSplitOptions.RemoveEmptyEntries);
                return parts.Length > 1 ? parts[1] : translates.Get("desconhecido");
            }

            private class LowercaseNamingPolicy : JsonNamingPolicy
            {
                public override string ConvertName(string name)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(name))
                        {
                            return name;
                        }

                        return char.ToLower(name[0]) + name.Substring(1);
                    }
                    catch (Exception ex)
                    {
                        var msg = ex.Message;
                        throw;
                    }
                }
            }



        }
    }
}
