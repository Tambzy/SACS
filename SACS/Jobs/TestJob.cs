using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using SACS.Logging;
using Serilog;
using LogLevel = SACS.Logging.LogLevel;

namespace SACS.Jobs
{
    [DisallowConcurrentExecution]
    public class TestJob : IJob
    {
        public TestJob()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();
        }

        public Task Execute(IJobExecutionContext context)
        {
            Log.Information("Test");
            Log.CloseAndFlush();
            return Task.CompletedTask;
        }
    }
}