#if NET5_0
using Microsoft.AspNetCore.Hosting;
#else
using Microsoft.Extensions.Hosting;
#endif
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace MailjetApiClient
{
    public static class IServiceCollectionExtensions
    {
        private const string Identifier = "MailjetApi";
        
        #if NET5_0
        public static IServiceCollection AddMailjetApiClient(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            services.AddOptions();
            services.Configure<MailjetOptions>(configuration.GetSection(Identifier));
            services.TryAddTransient<IMailjetApiClient>((sp) =>
            {
                var options = sp.GetService<IOptions<MailjetOptions>>().Value;
                return new MailjetService(options, environment);
            });

            return services;
        }        
        #else
        public static IServiceCollection AddMailjetApiClient(this IServiceCollection services, IConfiguration configuration, IHostingEnvironment environment)
        {
            services.AddOptions();
            services.Configure<MailjetOptions>(configuration.GetSection(Identifier));
            services.TryAddTransient<IMailjetApiClient>((sp) =>
            {
                var options = sp.GetService<IOptions<MailjetOptions>>().Value;
                return new MailjetService(options, environment);
            });
            return services;
        }
        #endif
    }
}