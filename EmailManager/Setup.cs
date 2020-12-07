using EmailManager.Contracts;
using EmailManager.Factory;
using MvvmCross;
using MvvmCross.IoC;
using Serilog;

namespace EmailManager
{
    public static class Setup
    {
        internal static void Initialize()
        {
            CreateLogger();
            RegisterIoc();            
        }

        private static void CreateLogger()
        {
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs\\EmailManagerLog.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        }

        private static void RegisterIoc()
        {
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IConnectionFactory, ConnectionFactory>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IEmailServiceFactory, EmailServiceFactory>();
        }
    }
}
