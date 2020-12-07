using MvvmCross.IoC;
using Serilog;
using System.Windows;
using System.Windows.Threading;

namespace EmailManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Initialize();

            Log.Information("Application Started...");

            // Global exception handling  
            Current.DispatcherUnhandledException +=
                new DispatcherUnhandledExceptionEventHandler(AppDispatcherUnhandledException);


            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.CloseAndFlush();
            base.OnExit(e); 
        }

        protected static void Initialize()
        {
            MvxIoCProvider.Initialize();
            Setup.Initialize();
        }

        private void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowUnhandeledException(e);
        }

        private void ShowUnhandeledException(DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            Log.Error($" Exception:  {e.Exception.Message}");

            string errorMessage = $"An application error occurred. \n Do you want to continue?";

            if (MessageBox.Show(errorMessage, "Application Error", MessageBoxButton.YesNoCancel, MessageBoxImage.Error) ==
                MessageBoxResult.No && MessageBox.Show(
                    "WARNING: The application will close. Any changes will not be saved. Do you still want to close it ? ",
                    "Close the application", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) ==
                MessageBoxResult.Yes)
            {
                Current.Shutdown();
            }

        }
    }
}
