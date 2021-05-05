using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;
using SACS.Jobs;
using Serilog;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Quartz.Spi;
using SACS.Logging.LogProviders;
using ILogProvider = Quartz.Logging.ILogProvider;

namespace SACS
{
    public class Program
    {
        private readonly IConfiguration _config;

        public Program()
        {
            var _builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(path: "appsettings.json");
            
            // build the configuration and assign to _config          
            _config = _builder.Build();
        }
        
        public static void Main(string[] args)
        {
            LogProvider.SetCurrentLogProvider(new ConsoleLogProvider());
            CreateHostBuilder(args).Build().Run();
            Log.CloseAndFlush();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    
                    Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Information()
                        .WriteTo.Console()
                        .CreateLogger();
                    
                    // Add the required Quartz.NET services
                    services.AddQuartz(q =>  
                    {
                        // Use a Scoped container to create jobs. I'll touch on this later
                        q.UseMicrosoftDependencyInjectionScopedJobFactory();
    
                        var jobKeyMain = new JobKey("WeeklyRemainder");
                        
                        q.AddJob<WeeklyRemainder>(opts => opts.WithIdentity(jobKeyMain));
                        q.AddTrigger(opts => opts
                            .ForJob(jobKeyMain) // link to the HelloWorldJob
                            .WithIdentity("WeeklyRemainder-Trigger") // give the trigger a unique name
                            .WithCronSchedule("0 0 0 ? * MON *")); // runs every Monday in Month at 00:00:00am
                        
                        Log.CloseAndFlush();
                    });

                    // Add the Quartz.NET hosted service

                    services.AddQuartzHostedService(
                        q => q.WaitForJobsToComplete = true);

                    // other config
                });

        
        private class ConsoleLogProvider : ILogProvider
        {
            public Logger GetLogger(string name)
            {
                return (level, func, exception, parameters) =>
                {
                    if (level >= LogLevel.Info && func != null)
                    {
                        Console.WriteLine("[" + DateTime.Now.ToLongTimeString() + "] [" + level + "] " + func(), parameters);
                    }
                    return true;
                };
            }

            public IDisposable OpenNestedContext(string message)
            {
                throw new NotImplementedException();
            }

            public IDisposable OpenMappedContext(string key, object value, bool destructure = false)
            {
                throw new NotImplementedException();
            }
        }
        
        private ServiceProvider ConfigureServices()
        {
            // this returns a ServiceProvider that is used later to call for those services
            // we can add types we have access to here, hence adding the new using statement:
            // using csharpi.Services;
            // the config we build is also added, which comes in handy for setting the command prefix!
            return new ServiceCollection()
                .AddSingleton(_config)
                .AddSingleton<TestJob>()
                .BuildServiceProvider();
        }

        // simple log provider to get something to the console
    }


}