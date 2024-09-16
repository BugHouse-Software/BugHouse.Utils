using System;
using BugHouse.Utils.Translates;
using BugHouse.Utils.Criptografia;
using Microsoft.Extensions.DependencyInjection;
using static BugHouse.Utils.Translates.TranslatationTools;

namespace BugHouse.Utils
{
    public static class ApiTools
    {
        public static void AddServicesTools(this IServiceCollection services, Func<ConfigureTranslate> value = null)
        {
            services.AddScoped<ICripitografarDataService, CripitografarDataService>();
            services.AddScoped<IJwtService, JwtService>();


            services.AddTranslatation(value);




        }
    }
}
