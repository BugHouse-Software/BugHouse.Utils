using BugHouse.Utils.Models;
using BugHouse.Utils.Services.ServiceHealthCheck;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;


namespace BugHouse.Utils.Extensions
{
    public static class DefaultApiSetupExtension
    {

        private const string corsPolicyName = "AllowOrigin";
        public static void AddVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(delegate (ApiVersioningOptions p)
            {
                p.DefaultApiVersion = new ApiVersion(2, 0);
                p.ReportApiVersions = true;
                p.AssumeDefaultVersionWhenUnspecified = true;
            });
            services.AddVersionedApiExplorer(delegate (ApiExplorerOptions p)
            {
                p.GroupNameFormat = "'v'VVV";
                p.SubstituteApiVersionInUrl = true;
            });
        }
        public static void AddAllCors(this IServiceCollection services)
        {
            CorsPolicy cors = new CorsPolicy
            {
                IsOriginAllowed = (string s) => true
            };
            cors.Origins.Add("*");
            cors.Headers.Add("*");
            cors.ExposedHeaders.Add("*");
            cors.Methods.Add("*");
            services.AddCors(delegate (CorsOptions c)
            {
                c.AddPolicy(corsPolicyName, cors);
            });
        }
        public static void UseAllCors(this IApplicationBuilder builder)
        {
            builder.UseCors(corsPolicyName);
        }
        public static void UseSwagger(this IApplicationBuilder app, IApiVersionDescriptionProvider provider)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                foreach (var description in provider.ApiVersionDescriptions.ToList().OrderByDescending(x => x.ApiVersion))
                {
                    options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    description.GroupName.ToUpperInvariant() + (description.IsDeprecated ? " [DEPRECATED]" : ""));
                }
                options.DocExpansion(DocExpansion.List);
            });
        }
        public static void AddSwagger(this IServiceCollection services, bool hasJwt = true, bool hasDomain = true, string sufixTitle = "", string sufixDescription = "")
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            var currentAssembly = Assembly.GetCallingAssembly();

            services.AddSwaggerGen(options =>
            {
                options.CustomSchemaIds(type => type.ToString());
                options.EnableAnnotations();
                using (var serviceProvider = services.BuildServiceProvider())
                {
                    var provider = serviceProvider.GetRequiredService<IApiVersionDescriptionProvider>();
                    string assemblyDescription = sufixDescription;
                    var fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(currentAssembly.Location);
                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerDoc(description.GroupName, new OpenApiInfo()
                        {
                            Title = $"{currentAssembly.GetCustomAttribute<AssemblyProductAttribute>().Product}{sufixTitle}",
                            Version = "v" + description.ApiVersion.ToString(),
                            Description = $"{(description.IsDeprecated ? $"{assemblyDescription} - DEPRECATED" : $"{assemblyDescription}")} - {fvi.FileVersion}",
                            License = new OpenApiLicense
                            {
                                Name = "Brand Brasil",

                            }
                        });
                    }
                }
                options.OperationFilter<SwaggerOperationFilter>();
                options.SchemaFilter<EnumSerialization>();

                if (hasDomain)
                    options.OperationFilter<AddRequiredHeaderParameter>();

                if (hasJwt)
                {
                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = "Authorization via JWT",
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        In = ParameterLocation.Header,
                        Scheme = "bearer"
                    });
                }

                // integrate xml comments  
                var xmlDocs = currentAssembly.GetReferencedAssemblies()
                .Union(new AssemblyName[] { currentAssembly.GetName() })
                .Select(a => Path.Combine(Path.GetDirectoryName(currentAssembly.Location), $"{a.Name}.xml"))
                .Where(f => File.Exists(f)).ToArray();

                Array.ForEach(xmlDocs, (d) =>
                {
                    options.IncludeXmlComments(d);
                });
            });
        }

        public static IMvcBuilder AddControllers(this IServiceCollection services, bool policyAuthorization, bool healthChecksImplementation = false)
        {
            if (healthChecksImplementation)
            {
                IServiceProvider serviceProvider = null;
                services.AddHealthChecks().AddAsyncCheck("HealthCheck", (Func<Task<HealthCheckResult>>)async delegate
                {
                    List<HealthCheck> healthChecks = new List<HealthCheck>();
                    try
                    {
                        if (serviceProvider == null)
                        {
                            serviceProvider = services.BuildServiceProvider();
                        }

                        healthChecks.AddRange(await ServiceProviderServiceExtensions.GetRequiredService<IHealthCheckService>(serviceProvider).Execute());
                    }
                    catch (Exception ex)
                    {
                        HealthCheck item = new HealthCheck
                        {
                            Essencial = true,
                            Name = "HealthCheck",
                            ErrorMensage = ex.ToString(),
                            Status = TipoStatusHealthCheck.Erro,
                            Time = TimeSpan.FromMinutes(1.0)
                        };
                        healthChecks.Add(item);
                    }

                    Dictionary<string, object> dictionary = new Dictionary<string, object>();
                    HealthStatus healthStatus = HealthStatus.Healthy;
                    foreach (HealthCheck item2 in healthChecks)
                    {
                        dictionary.Add(item2.Name, item2);
                        if (healthStatus != HealthStatus.Degraded)
                        {
                            TipoStatusHealthCheck status = item2.Status;
                            bool flag = (uint)(status - 1) <= 1u;
                            if (flag || (item2.Status == TipoStatusHealthCheck.Erro && !item2.Essencial))
                            {
                                healthStatus = HealthStatus.Unhealthy;
                            }
                            else if (item2.Status == TipoStatusHealthCheck.Erro && item2.Essencial)
                            {
                                healthStatus = HealthStatus.Degraded;
                            }
                        }
                    }

                    ReadOnlyDictionary<string, object> data = new ReadOnlyDictionary<string, object>(dictionary);
                    return new HealthCheckResult(healthStatus, "Retorno com os Status do Serviço", null, data);
                }, (IEnumerable<string>?)null, (TimeSpan?)null);
            }
            else
            {
                services.AddHealthChecks();
            }

            return services.AddControllers(delegate (MvcOptions options)
            {
                options.Filters.Add(new ProducesResponseTypeAttribute(typeof(ExceptionResponseBase), 302));
                options.Filters.Add(new ProducesResponseTypeAttribute(typeof(ExceptionResponseBase), 400));
                options.Filters.Add(new ProducesResponseTypeAttribute(typeof(ExceptionResponseBase), 401));
                options.Filters.Add(new ProducesResponseTypeAttribute(typeof(ExceptionResponseBase), 403));
                options.Filters.Add(new ProducesResponseTypeAttribute(typeof(ExceptionResponseBase), 410));
                options.Filters.Add(new ProducesResponseTypeAttribute(typeof(ExceptionResponseBase), 412));
                options.Filters.Add(new ProducesResponseTypeAttribute(typeof(ExceptionResponseBase), 422));
                options.Filters.Add(new ProducesResponseTypeAttribute(typeof(ExceptionResponseBase), 423));
                options.Filters.Add(new ProducesResponseTypeAttribute(typeof(ExceptionResponseBase), 424));
                options.Filters.Add(new ProducesResponseTypeAttribute(typeof(ExceptionResponseBase), 500));
                if (policyAuthorization)
                {
                    AuthorizationPolicy policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                    options.Filters.Add(new AuthorizeFilter(policy));
                }
            }).AddNewtonsoftJson(delegate (MvcNewtonsoftJsonOptions options)
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });
        }

        public static void AddServices(this IServiceCollection services, Assembly assembly)
        {

            var serviceTypes = assembly.GetTypes()
                                        .Where(t => t.IsClass
                                                 && t.Name.EndsWith("Service"))
                                        .ToList();

            foreach (var serviceType in serviceTypes)
            {
                var interfaceType = serviceType.GetInterfaces()
                                               .FirstOrDefault(i => i.Name == $"I{serviceType.Name}");

                if (interfaceType != null)
                {
                    services.AddScoped(interfaceType, serviceType);
                }
                else
                {
                    services.AddScoped(serviceType);
                }
            }
        }

        public static void AddHandlers(this IServiceCollection services, Assembly assembly)
        {
            (from t in assembly.GetTypes()
             where t.Name.EndsWith("Handler", StringComparison.InvariantCultureIgnoreCase)
             where !t.IsAbstract && (from i in t.GetInterfaces()
                                     where i.Name.StartsWith("INotificationHandler", StringComparison.InvariantCultureIgnoreCase)
                                     select i).Count() == 1
             select t).ToList().ForEach(delegate (Type t)
             {
                 services.AddScoped((from i in t.GetInterfaces()
                                     where i.Name.StartsWith("INotificationHandler", StringComparison.InvariantCultureIgnoreCase)
                                     select i).First(), t);
             });
        }

        public static void AddBySuffix(this IServiceCollection services, Assembly assembly, string suffix)
        {
            (from t in assembly.GetTypes()
             where !t.IsAbstract && t.Name.EndsWith(suffix, StringComparison.InvariantCultureIgnoreCase) && (from i in t.GetInterfaces()
                                                                                                             where i.Name.Equals("I" + t.Name, StringComparison.InvariantCultureIgnoreCase)
                                                                                                             select i).Count() == 1
             select t).ToList().ForEach(delegate (Type t)
             {
                 services.AddScoped((from i in t.GetInterfaces()
                                     where i.Name.Equals("I" + t.Name, StringComparison.InvariantCultureIgnoreCase)
                                     select i).First(), t);
             });
        }

        public static IApplicationBuilder UseEndpoints(this IApplicationBuilder app, bool healthChecksImplementation = false)
        {
            return app.UseEndpoints(delegate (IEndpointRouteBuilder endpoints)
            {
                endpoints.MapControllers();
                if (healthChecksImplementation)
                {
                    app.UseEndpoints(delegate (IEndpointRouteBuilder endpoints)
                    {
                        endpoints.MapHealthChecks("/health", new HealthCheckOptions
                        {
                            ResponseWriter = async delegate (HttpContext context, HealthReport report)
                            {
                                HealthReportEntry value = report.Entries.FirstOrDefault().Value;
                                int num = 200;
                                HealthStatus status = value.Status;
                                if ((status == HealthStatus.Unhealthy || status == HealthStatus.Healthy) ? true : false)
                                {
                                    context.Response.ContentType = "application/json; charset=utf-8";
                                    context.Response.StatusCode = 200;
                                }
                                else
                                {
                                    context.Response.ContentType = "application/problem+json; charset=utf-8";
                                    num = 503;
                                }

                                context.Response.StatusCode = num;
                                string text = JsonConvert.SerializeObject(new
                                {
                                    Title = value.Description,
                                    Status = num,
                                    HealthyStatusName = value.Status.ToString(),
                                    HealthyStatus = value.Status,
                                    Data = value.Data
                                }, new JsonSerializerSettings
                                {
                                    NullValueHandling = NullValueHandling.Ignore,
                                    Formatting = Formatting.Indented
                                });
                                await context.Response.WriteAsync(text);
                            }
                        });
                    });
                }
                else
                {
                    endpoints.MapHealthChecks("/health");
                }
            });
        }




        private class EnumSerialization : ISchemaFilter
        {
            public void Apply(OpenApiSchema schema, SchemaFilterContext context)
            {
                if (!context.Type.IsEnum)
                {
                    return;
                }

                IList<IOpenApiAny> list = new List<IOpenApiAny>();
                MemberInfo[] members = context.Type.GetMembers();
                for (int i = 0; i < members.Length; i++)
                {
                    object obj = members[i].GetCustomAttributes(typeof(EnumMemberAttribute), inherit: false).FirstOrDefault();
                    if (obj != null)
                    {
                        EnumMemberAttribute enumMemberAttribute = (EnumMemberAttribute)obj;
                        list.Add(new OpenApiString(enumMemberAttribute.Value));
                    }
                }

                if (!list.IsNullOrEmpty())
                {
                    schema.Enum.Clear();
                    schema.Enum = list;
                }
            }
        }
        private class AddRequiredHeaderParameter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                if (operation.Parameters == null)
                {
                    operation.Parameters = new List<OpenApiParameter>();
                }


            }
        }
        private class SwaggerOperationFilter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                if (operation.Security == null)
                {
                    operation.Security = new List<OpenApiSecurityRequirement>();
                }

                OpenApiSecurityScheme openApiSecurityScheme = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };
                operation.Security.Add(new OpenApiSecurityRequirement { [openApiSecurityScheme] = new List<string>() });
            }
        }
    }

}
