using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting.WindowsServices;

namespace Backtester.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (Debugger.IsAttached || args.Contains("--debug"))
            {
                var host = new WebHostBuilder()
                    .UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .UseApplicationInsights()
                    .Build();

                host.Run();
            }
            else
            {
                var cert = new X509Certificate2(args[1], args[2]);

                var host = new WebHostBuilder()
                    .UseKestrel(cfg => cfg.UseHttps(cert))
                    .UseUrls(args[0])
                    .UseContentRoot(args[3])
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .UseApplicationInsights()
                    .Build();

                host.RunAsService();
            }
        }
    }
}
