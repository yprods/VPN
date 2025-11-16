using System;
using System.Windows;
using System.Windows.Threading;

namespace VPNApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Handle unhandled exceptions
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(
                $"An unhandled exception occurred:\n\n{e.Exception.Message}\n\n{e.Exception.StackTrace}",
                "Application Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            
            // Set handled to true to prevent app crash
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show(
                    $"A critical error occurred:\n\n{ex.Message}\n\n{ex.StackTrace}",
                    "Critical Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}

