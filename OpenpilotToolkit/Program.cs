using System;
using System.IO;
using System.Windows.Forms;
using CefSharp.WinForms;
using FFMpegCore;
using LibVLCSharp.Shared;
using OpenpilotToolkit.Controls;
using OpenpilotToolkit.Properties;
using Serilog;

namespace OpenpilotToolkit
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        public static int Main(string[] args)
        {
            var exitCode = CefSharp.BrowserSubprocess.SelfHost.Main(args);

            if (exitCode >= 0)
            {
                return exitCode;
            }

            var settings = new CefSettings()
            {
                //By default CefSharp will use an in-memory cache, you need to specify a Cache Folder to persist data
                //CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache"),
                //BrowserSubprocessPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName
            };

            CefSharp.Cef.Initialize(settings);

            GlobalFFOptions.Configure(options => options.BinaryFolder = "./");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day, shared: true)
                .CreateLogger();

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            
            Application.ApplicationExit += (_, _) =>
            {
                Settings.Default.Save();
                Log.Information("Application Exiting.");
            };

            Log.Information("Application Starting.");

            Application.SetHighDpiMode(HighDpiMode.DpiUnaware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            
            var openpilotToolkitForm = new OpenpilotToolkitForm();

            Application.ThreadException += (sender, args) =>
            {
                Log.Error(args.Exception, "Unhandled Exception");
                ToolkitMessageDialog.ShowDialog(args.Exception.Message, openpilotToolkitForm);
            };
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Log.Error((Exception)args.ExceptionObject, "Unhandled Exception");
                ToolkitMessageDialog.ShowDialog(((Exception)args.ExceptionObject).Message, openpilotToolkitForm);
            };

            Core.Initialize();
            Application.Run(openpilotToolkitForm);
            return 0;
        }
    }
}
