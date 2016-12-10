using log4net;
using SerialMediaRemoteControl.Objects;
using SerialMediaRemoteControl.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SerialMediaRemoteControl.Helpers;

namespace SerialMediaRemoteControl
{
    class Main : ApplicationContext
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(Main));
        Helpers.Arguments arguments;
        public static Config cfg = new Config();
        SerialCommunication sc;

        public Main(string[] args)
        {
            arguments = new Helpers.Arguments(args);
            new TrayIkona(); //show try icon

            cfg = Config.Load();
            if (arguments["ged"] != null)
            {
                cfg.GenerateExampleData();
                log.Info("Example data generated. Exiting.");
                Application.Exit();
            }
            if (arguments["GetCommands"] != null || arguments["gc"] != null)
            {
                Enum.GetNames(typeof(WindowsInput.Native.VirtualKeyCode)).ToList().ForEach(item => Console.WriteLine(item.ToString()));
                Console.WriteLine("VOLUME_SET:90");
                Application.Exit();
            }

            if (Main.cfg.Processing.ShowErrorsInTrayBubble)
                BubbleAppender.Setup(true);
            Helpers.RequestInterface.Initialize();
            //start serial port
            sc = new SerialCommunication();


        }

        void printHelp()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("SMRC - easy way to control media via serial communication");
            sb.AppendLine();
            sb.AppendLine("help | h             - show help");
            sb.AppendLine("ged                  - Generate Example Data to config file");
            sb.AppendLine("GetCommands | gc     - get supported commands");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("Lordrak 2016");
        }
    }

    public class TrayIkona
    {
        private static NotifyIcon ikona;
        private static bool initialized = false;
        private static trayIconBaloonTip posledniTip = null;
        public TrayIkona()
        {
            //zakladni inicializace ikonky
            ikona = new NotifyIcon();
            ikona.MouseClick += Ikona_MouseClick;
            //ikona.MouseDoubleClick += Ikona_MouseDoubleClick;
            ikona.Icon = Resources.keyboard;
            ikona.Visible = true;

            InitializeContexMenu();
            initialized = true;

            
        }

        private void Ikona_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            
        }

        private void Ikona_MouseClick(object sender, MouseEventArgs e)
        {
            System.Threading.Thread.Sleep(200); //pokud to tu není, tak se nestihne spočítat, kolikrát se kliklo myší a i dvojklik se vyhodnotí jako jednoklik
            if (e.Button == MouseButtons.Left && e.Clicks < 2)
            {
                //zpobrazi posledni hlasku, pokud uzivatel klikne na ikonku
                if (posledniTip != null)
                    ikona.ShowBalloonTip(posledniTip.Timeout, posledniTip.TipTitle, posledniTip.TipText, posledniTip.TipIcon);
            }
        }

        void InitializeContexMenu()
        {
            List<MenuItem> items = new List<MenuItem>();
            items.Add(new MenuItem("Command window", CommandWindow));
            items.Add(new MenuItem("Exit", Exit));
            ikona.ContextMenu = new ContextMenu(items.ToArray());

        }



        #region Veřejné metody
        public static void ShowBaloonTip(int timeout, string tipTitle, string tipText, ToolTipIcon tipIcon)
        {
            if (!initialized)
                throw new Exception("TrayIkona is not inicialized");

            posledniTip = new trayIconBaloonTip(timeout, tipTitle, tipText, tipIcon);
            ikona.ShowBalloonTip(timeout, tipTitle, tipText, tipIcon);
        }
        #endregion Veřejné metody

        #region Menu metody
        void CommandWindow(object sender, EventArgs e)
        {
            new Forms.CommandWindow().Show();
        }




        void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            ikona.Visible = false;

            Application.Exit();
        }
        #endregion Menu metody



        /// <summary>
        /// Por uchovani volani ShowBaloonTip
        /// </summary>
        class trayIconBaloonTip
        {
            public trayIconBaloonTip(int timeout, string tipTitle, string tipText, ToolTipIcon tipIcon)
            {
                Timeout = timeout;
                TipIcon = tipIcon;
                TipText = tipText;
                TipTitle = tipTitle;
            }

            public int Timeout { get; internal set; }
            public string TipTitle { get; internal set; }
            public string TipText { get; internal set; }
            public ToolTipIcon TipIcon { get; internal set; }
        }
    }
}
