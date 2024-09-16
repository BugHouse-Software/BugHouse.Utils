using BugHouse.Utils.Exceptions.ThrowsExceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Threading.Tasks;

namespace BugHouse.Utils.Exceptions
{
    public class EFExceptionHandlerMiddleware
    {
        private readonly RequestDelegate next;

        public EFExceptionHandlerMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (ExceptionErrorBase)
            {
                await next(context);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is OracleException oracleEx)
            {
                switch (oracleEx.Number)
                {
                    case 2291:
                        string invalidForeignKey = GetConstraintNameFromMessage(oracleEx.Message);
                        throw new EntityException($"O valor informado para a chave estrangeira '{invalidForeignKey}' é inválido.", oracleEx.InnerException);

                    case 1:
                        string duplicateKey = GetConstraintNameFromMessage(oracleEx.Message);
                        throw new EntityException($"O registro com a chave '{duplicateKey}' já foi inserido anteriormente.", oracleEx.InnerException);

                    default:
                        throw new EntityException(oracleEx.Message, oracleEx.InnerException);
                }
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx)
            {
                switch (sqlEx.Number)
                {
                    case 547: // Foreign key violation
                        string foreignKey = GetConstraintNameFromMessage(sqlEx.Message);
                        throw new EntityException($"O valor informado para a chave estrangeira '{foreignKey}' é inválido.", sqlEx.InnerException);

                    case 2627: // Unique constraint violation
                    case 2601: // Duplicated key row error
                        string uniqueKey = GetConstraintNameFromMessage(sqlEx.Message);
                        throw new EntityException($"O registro com a chave '{uniqueKey}' já foi inserido anteriormente.", sqlEx.InnerException);

                    default:
                        throw new EntityException(sqlEx.Message, sqlEx.InnerException);
                }
            }
            catch (DbUpdateException ex)
            {
                throw new EntityException(ex.Message, ex.InnerException);
            }
            catch (Exception)
            {
                throw;
            }

            string GetConstraintNameFromMessage(string message)
            {
                var parts = message.Split(new[] { "(", ")" }, StringSplitOptions.RemoveEmptyEntries);
                return parts.Length > 1 ? parts[1] : "desconhecido";
            }
        }

    }
}

