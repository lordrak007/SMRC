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

        static void Main(string[] args)
        {
            var cfg = Config.Load();
            if (cfg == null)
                cfg = new Config();

            var arguments = new Helpers.Arguments(args);
            if (arguments["ged"] != null)
            {
                cfg.GenerateExampleData();
                Environment.Exit(0);
            }
            if (arguments["GetCommands"] != null || arguments["gc"] != null)
            {
                Enum.GetNames(typeof(WindowsInput.Native.VirtualKeyCode)).ToList().ForEach(item => Console.WriteLine(item.ToString()));
                Console.WriteLine("VOLUME_SET:90");
                Environment.Exit(0);
            }
            if (arguments["help"] != null || arguments["h"] != null)
            {
                printHelp();
                Environment.Exit(0);
            }

            FreeConsole();

            //Run main application
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main(args));           
        }

        static void printHelp()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("SMRC - easy way to control media via serial communication");
            sb.AppendLine();
            sb.AppendLine("help | h             - show help");
            sb.AppendLine("ged                  - Generate Example Data to config file");
            sb.AppendLine("GetCommands | gc     - get supported commands");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("Lordrak 2016");
            Console.WriteLine(sb.ToString());
        }
    }
}
