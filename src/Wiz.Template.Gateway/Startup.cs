using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace Wiz.Template.Gateway
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            void options(IdentityServerAuthenticationOptions identityServer)
            {
                identityServer.Authority = Configuration.GetSection("IdentityServer:Authority").Value;
                identityServer.ApiName = Configuration.GetSection("IdentityServer:ApiName").Value;
                identityServer.ApiSecret = Configuration.GetSection("IdentityServer:ApiSecret").Value;
                identityServer.SupportedTokens = SupportedTokens.Both;
            }

            services.AddAuthentication().AddIdentityServerAuthentication(Configuration.GetSection("IdentityServer:ProviderKey").Value, options);

            services.AddCors(options =>
            {
                options.AddPolicy("GatewayCorsPolicy", builder =>
                {
                    builder.AllowAnyOrigin();
                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                });
            });

            services.AddOcelot(Configuration);
            services.AddApplicationInsightsTelemetry();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!env.IsProduction())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseCors("GatewayCorsPolicy");
            app.UseOcelot().Wait();
        }
    }
}
