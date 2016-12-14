using log4net;
using SerialMediaRemoteControl.Objects;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SerialMediaRemoteControl
{
    class Program
    {
        [DllImport("kernel32.dll")]
        //static extern bool AttachConsole(int dwProcessId);
        //private const int ATTACH_PARENT_PROCESS = -1;
        private static extern int FreeConsole();

        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(Main));

        static void Main(string[] args)
        {
            if (!AppDomain.CurrentDomain.FriendlyName.EndsWith("vshost.exe")) //pokud delam debug, neni toto treba
            {
                Application.ThreadException += Application_ThreadException; ;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException; ;
            }

            var cfg = Config.Load();
            if (cfg == null)
                cfg = new Config();

            var arguments = new Helpers.Arguments(args);
            if (arguments["ged"] != null)
            {
                Helpers.log4netHelper.AddConsoleAppender(true);
                cfg.GenerateExampleData();
                log.Info("Config file created");
                Environment.Exit(0);
            }
            if (arguments["GetCommands"] != null || arguments["gc"] != null)
            {
                Helpers.log4netHelper.AddConsoleAppender(true);
                log.Info("Here are supported commands:");
                Enum.GetNames(typeof(WindowsInput.Native.VirtualKeyCode)).ToList().ForEach(item => Console.WriteLine(item.ToString()));
                Console.WriteLine("VOLUME_SET:90");
                Environment.Exit(0);
            }
            if (arguments["help"] != null || arguments["h"] != null)
            {
                printHelp();
                Environment.Exit(0);
            }
            if (arguments["console"] != null || arguments["c"] != null)
            {
                Helpers.log4netHelper.AddConsoleAppender(true);
            }
            else
                FreeConsole();

            //Run main application
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main(args));           
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            log.Fatal("Fatal error:", (Exception)e.ExceptionObject);
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            log.Fatal("Fatal error:", (Exception)e.Exception);
        }

        static void printHelp()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("SMRC - easy way to control media via serial communication");
            sb.AppendLine();
            sb.AppendLine("help | h             - show help");
            sb.AppendLine("ged                  - Generate Example Data to config file");
            sb.AppendLine("GetCommands | gc     - get supported commands");
            sb.AppendLine("console | c          - show console and write logs");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("Lordrak 2016");
            Console.WriteLine(sb.ToString());
        }
    }
}
