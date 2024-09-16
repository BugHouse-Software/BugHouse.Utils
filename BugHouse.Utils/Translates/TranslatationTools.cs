using BugHouse.Utils.Extensions;
using BugHouse.Utils.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using static BugHouse.Utils.Translates.TranslatationTools;

namespace BugHouse.Utils.Translates
{
    internal static class TranslatationsTools
    {
        internal static void AddTranslatation(this IServiceCollection services, Func<ConfigureTranslate> value)
        {
            if (value.IsNull())
                return;

            var configureTranslation = value.Invoke();

            if (configureTranslation.TranslationsPath.IsNullOrWhiteSpace())
                throw new ApplicationException("The 'TranslationsPath' was not provided correctly.");

            if (configureTranslation.DefaultRequestCulture.IsNullOrWhiteSpace())
                throw new ApplicationException("The 'DefaultRequestCulture' was not provided correctly.");


            var arquivosLocalizados = Directory.GetFiles(configureTranslation.TranslationsPath, $"*{configureTranslation.ExtensionName}.json");

            List<string> linguagensLocalizadas = new List<string>();
            List<TranslateModel> translateModels = new List<TranslateModel>();

            foreach (var arquivo in arquivosLocalizados)
            {
                string text = File.ReadAllText(arquivo);

                var fileInfo = new FileInfo(arquivo);
                var name = fileInfo.Name.Replace($"{configureTranslation.ExtensionName}.json", "");

                var modelTranslate = new TranslateModel();
                modelTranslate.CultureLinguage =    name;
                modelTranslate.Translates = text.ToDeserialize<Dictionary<string, string>>();
                linguagensLocalizadas.Add(name);

                translateModels.Add(modelTranslate);
            }


            services.AddSingleton(translateModels);

            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(configureTranslation.DefaultRequestCulture);
                options.SupportedCultures = linguagensLocalizadas.Select(culture => new CultureInfo(culture)).ToList();
                options.SupportedUICultures = options.SupportedCultures;
            });

            services.AddScoped<ITranslatesService, TranslatesService>();
        }
    }
}
