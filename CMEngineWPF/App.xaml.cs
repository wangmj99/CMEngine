using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using log4net;


namespace CMEngineWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public App() : base()
        {
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        }

        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error("Unhandled exception occurred: \n" + e.Exception.Message);
            Log.Error("Sender: " + sender.ToString());
            Log.Error("Unhandled exception occurred: \n" + e.Exception.StackTrace);
            MessageBox.Show("Unhandled exception occurred: \n" + e.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            log4net.Config.XmlConfigurator.Configure();
            base.OnStartup(e);
        }

    }
}
