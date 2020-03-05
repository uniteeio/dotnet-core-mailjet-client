using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace MailjetApiClient
{
    public static class IServiceCollectionExtensions
    {
        private const string Identifier = "MailjetApi";
        
        public static IServiceCollection AddMailjetApiClient(this IServiceCollection services,
            IConfiguration configuration, IHostingEnvironment environment)
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
    }
}