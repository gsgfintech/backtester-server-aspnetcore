using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Capital.GSG.FX.Backtest.MongoConnector;
using Capital.GSG.FX.Backtest.MongoConnector.Actioner;
using Backtester.Server.ControllerUtils;
using Capital.GSG.FX.Utils.Core.Logging;
using System.IO;

namespace Backtester.Server
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBacktestDBServer(Configuration.GetSection("BacktestDBServer"));

            services.AddControllerUtils(Configuration);

            // Add framework services.
            services.AddMvc();

            services.AddAuthentication(
                SharedOptions => SharedOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            GSGLoggerFactory.Instance.AddConsole(Configuration.GetSection("Logging"));
            GSGLoggerFactory.Instance.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseCookieAuthentication();

            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            {
                ClientId = Configuration["Authentication:AzureAd:ClientId"],
                Authority = Configuration["Authentication:AzureAd:AADInstance"] + Configuration["Authentication:AzureAd:TenantId"],
                CallbackPath = Configuration["Authentication:AzureAd:CallbackPath"]
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }

    internal static class ServiceExtensions
    {
        public static void AddBacktestDBServer(this IServiceCollection services, IConfigurationSection configSection)
        {
            string dbName = configSection.GetValue<string>("DBName");
            string host = configSection.GetValue<string>("Host");
            int port = configSection.GetValue<int>("Port");

            BacktestMongoDBServer backtestDbServer = new BacktestMongoDBServer(dbName, host, port);

            services.AddSingleton((serviceProvider) =>
            {
                return backtestDbServer.BacktestJobActioner;
            });

            services.AddSingleton((serviceProvider) =>
            {
                return backtestDbServer.BacktestJobGroupActioner;
            });

            services.AddSingleton((serviceProvider) =>
            {
                return backtestDbServer.BacktestWorkerActioner;
            });

            services.AddSingleton((serviceProvider) =>
            {
                return backtestDbServer.UnrealizedPnlSerieActioner;
            });
        }

        public static void AddControllerUtils(this IServiceCollection services, IConfigurationRoot config)
        {
            services.AddSingleton((serviceProvider) =>
            {
                var actioner = serviceProvider.GetService<BacktestJobActioner>();

                return new JobsControllerUtils(actioner);
            });

            services.AddSingleton((serviceProvider) =>
            {
                var actioner = serviceProvider.GetService<BacktestJobGroupActioner>();
                JobsControllerUtils jobsControllerUtils = serviceProvider.GetService<JobsControllerUtils>();

                return new JobGroupsControllerUtils(actioner, jobsControllerUtils);
            });

            services.AddSingleton((serviceProvider) =>
            {
                var actioner = serviceProvider.GetService<UnrealizedPnlSerieActioner>();

                return new UnrealizedPnlSeriesControllerUtils(actioner);
            });

            services.AddSingleton((serviceProvider) =>
            {
                string stratFilesUploadDirectory = config.GetValue<string>("StratFileDropDir") ?? Path.GetTempPath();
                JobsControllerUtils jobsControllerUtils = serviceProvider.GetService<JobsControllerUtils>();
                JobGroupsControllerUtils jobGroupsControllerUtils = serviceProvider.GetService<JobGroupsControllerUtils>();

                return new CreateJobControllerUtils(stratFilesUploadDirectory, jobsControllerUtils, jobGroupsControllerUtils);
            });

            services.AddSingleton((serviceProvider) =>
            {
                var actioner = serviceProvider.GetService<BacktestWorkerActioner>();
                JobsControllerUtils jobsControllerUtils = serviceProvider.GetService<JobsControllerUtils>();

                return new WorkersControllerUtils(actioner, jobsControllerUtils);
            });

            services.AddSingleton((serviceProvider) =>
            {
                JobsControllerUtils jobsControllerUtils = serviceProvider.GetService<JobsControllerUtils>();

                return new AlertsControllerUtils(jobsControllerUtils);
            });

            services.AddSingleton((serviceProvider) =>
            {
                JobsControllerUtils jobsControllerUtils = serviceProvider.GetService<JobsControllerUtils>();

                return new OrdersControllerUtils(jobsControllerUtils);
            });

            services.AddSingleton((serviceProvider) =>
            {
                JobsControllerUtils jobsControllerUtils = serviceProvider.GetService<JobsControllerUtils>();

                return new PositionsControllerUtils(jobsControllerUtils);
            });

            services.AddSingleton((serviceProvider) =>
            {
                JobsControllerUtils jobsControllerUtils = serviceProvider.GetService<JobsControllerUtils>();

                return new TradesControllerUtils(jobsControllerUtils);
            });

            services.AddSingleton((serviceProvider) =>
            {
                JobsControllerUtils jobsControllerUtils = serviceProvider.GetService<JobsControllerUtils>();

                return new StatusesControllerUtils(jobsControllerUtils);
            });
        }
    }
}
