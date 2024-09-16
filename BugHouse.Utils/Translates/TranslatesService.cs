using BugHouse.Utils.Exceptions;
using BugHouse.Utils.Exceptions.ThrowsExceptions;
using BugHouse.Utils.Extensions;
using BugHouse.Utils.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BugHouse.Utils.Translates
{
    internal class TranslatesService : ITranslatesService
    {

        readonly private List<TranslateModel> _translates;
        private readonly IHttpContextAccessor _accessor;
        private readonly string _defaultValue;
        private readonly string _selectedLanguage;

        public TranslatesService(List<TranslateModel> translates, IHttpContextAccessor accessor, IOptions<RequestLocalizationOptions> options)
        {
            this._accessor=accessor;
            this._translates = translates;

            _defaultValue= options.Value.DefaultRequestCulture.Culture.Name;
            try
            {
                _selectedLanguage= _accessor.HttpContext.Request.Headers.AcceptLanguage.FirstOrDefault();
            }
            catch
            {
                _selectedLanguage="";
            }
        }

        public string Get(string key)
        {
            try
            {
                if (!_selectedLanguage.IsNullOrEmpty())
                {
                    var translateValues = _translates.FirstOrDefault(s => s.CultureLinguage == _selectedLanguage);

                    if (!translateValues.IsNull())
                    {
                        var result = translateValues.Translates.FirstOrDefault(s => s.Key.ToLower() == key.ToLower());
                        if (!result.IsNull())
                            return result.Value;
                    }
                }

                var defaultValues = _translates.FirstOrDefault(s => s.CultureLinguage == _defaultValue);

                if (defaultValues.IsNull())
                    throw new InternalServerErroException("An error occurred while locating the application's CultureLanguage.");


                var resultTranslate = defaultValues.Translates.FirstOrDefault(s => s.Key.ToLower() == key.ToLower());
                if (!resultTranslate.IsNull())
                    return resultTranslate.Value;
                else
                    return key;
            }
            catch (ExceptionErrorBase)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"An internal error occurred while trying to retrieve a translation for the value '{key}'.", ex);
            }
        }

        public string Get(string key, string cultureName)
        {
            try
            {
                if (!cultureName.IsNullOrEmpty())
                {
                    var translateValues = _translates.FirstOrDefault(s => s.CultureLinguage == cultureName);

                    if (!translateValues.IsNull())
                    {
                        var result = translateValues.Translates.FirstOrDefault(s => s.Key.ToLower() == key.ToLower());
                        if (!result.IsNull() && !result.Value.IsNullOrWhiteSpace())
                            return result.Value;
                    }
                }

                var defaultValues = _translates.FirstOrDefault(s => s.CultureLinguage == _defaultValue);

                if (defaultValues.IsNull())
                    throw new InternalServerErroException("An error occurred while locating the application's CultureLanguage.");


                var resultTranslate = defaultValues.Translates.FirstOrDefault(s => s.Key.ToLower() == key.ToLower());
                if (!resultTranslate.IsNull() && !resultTranslate.Value.IsNullOrWhiteSpace())
                    return resultTranslate.Value;
                else
                    return key;
            }
            catch (ExceptionErrorBase)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"An internal error occurred while trying to retrieve a translation for the value '{key}'.", ex);
            }
        }
    }
}
