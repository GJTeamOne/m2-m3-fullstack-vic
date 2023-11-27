using System.Net.Mime;
using Hellang.Middleware.ProblemDetails;
using Hellang.Middleware.ProblemDetails.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using WebApplication.Core;
using WebApplication.Core.Common.CustomProblemDetails;
using Microsoft.Extensions.Logging;
using WebApplication.Core.Behaviors;
using WebApplication.Middleware;
using FluentValidation;
using MediatR;
using WebApplication.Core.Common.Behaviours;

namespace WebApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        private IConfiguration Configuration { get; }
        private IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(
                        options =>
                        {
                            options.RespectBrowserAcceptHeader = true;
                            options.Filters.Add(new ProducesAttribute(MediaTypeNames.Application.Json, "application/problem+json"));
                            options.Filters.Add(new ConsumesAttribute(MediaTypeNames.Application.Json));

                            options.Filters.Add(new ProducesResponseTypeAttribute(typeof(BadRequestProblemDetails), StatusCodes.Status400BadRequest));
                            options.Filters.Add(new ProducesResponseTypeAttribute(typeof(NotFoundProblemDetails), StatusCodes.Status404NotFound));
                            options.Filters.Add(new ProducesResponseTypeAttribute(typeof(UnhandledExceptionProblemDetails), StatusCodes.Status500InternalServerError));
                        }
                    )
                    .AddNewtonsoftJson(
                        options =>
                        {
                            options.UseCamelCasing(true);
                            options.SerializerSettings.Converters.Add(new StringEnumConverter());
                        }
                    )
                    .AddProblemDetailsConventions();

            services.AddSwaggerGen(
                c =>
                {
                    c.SwaggerDoc(
                        "v1",
                        new OpenApiInfo
                        {
                            Title = "WebApplication",
                            Version = "v1"
                        }
                    );
                }
            );

            services.AddValidatorsFromAssemblyContaining<Startup>();

            // Register MediatR pipeline behavior
            services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehaviour<,>));

            services.AddCoreServices(Environment);

            

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApplication v1"));
            }

            // Register exception logging middleware
            app.UseMiddleware<ExceptionLoggingMiddleware>();

            app.UseHttpsRedirection();
            app.UseProblemDetails();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
