using BugHouse.Utils.Exceptions.ThrowsExceptions;
using BugHouse.Utils.Extensions;
using BugHouse.Utils.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.Linq;

namespace BugHouse.Utils.Services.ValidateModelState
{
    public class ValidateModelStateAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Validação de Atributos
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState = context.ModelState;
            if (!modelState.IsValid)
            {

                List<Microsoft.AspNetCore.Mvc.ModelBinding.ModelErrorCollection> errors = modelState.Select(x => x.Value.Errors)
                                       .Where(y => y.Count > 0)
                                       .ToList();
                List<Validacao> validacoes = new();

                foreach (Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateEntry valueModel in modelState.Values)
                {
                    if (valueModel.Errors.IsNullOrEmpty())
                    {
                        continue;
                    }

                    foreach (Microsoft.AspNetCore.Mvc.ModelBinding.ModelError error in valueModel.Errors)
                    {
                        Validacao validacao = new()
                        {
                            Mensagem = error.ErrorMessage,
                            Propriedade = error.ErrorMessage
                        };
                        validacoes.Add(validacao);
                    }
                }

                throw new BadRequestException("O objeto foi enviado incorretamente", validacoes);
            }
        }
    }
}
