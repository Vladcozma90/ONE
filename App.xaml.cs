using System;
using System.Windows;
using System.Windows.Threading;

namespace ONE
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Catch all unhandled exceptions and prevent crashes
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Unhandled Exception: {e.Exception.Message}\n\n{e.Exception.StackTrace}",
                            "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;  // Prevents the app from crashing
        }
    }
}
