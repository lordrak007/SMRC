using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsInput;

namespace SerialMediaRemoteControl.Helpers
{
    public class Request
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(Request));
        InputSimulator IS = new InputSimulator();
        Dictionary<string, string> mapCommands = new Dictionary<string, string>();
        
        public Request()
        {
            //load mapping from config
            Main.cfg.MapCommands.ForEach(i => mapCommands.Add(i.CustomKey, i.MappedValue));
        }

        private void parseDo(string input)
        {
            string[] inArray = input.Split(Main.cfg.Processing.ValueSeparator.ToCharArray());
            var val = inArray.Length == 2 ? inArray[1] : ""; //parse value
            if (inArray.Length == 2)
                log.DebugFormat("Found value \"{0}\" for entry \"{1}\"", inArray[0], inArray[1]);
            //try find Key in user mapping or use Key direct
            var command = mapCommands.ContainsKey(inArray[0]) ? mapCommands[inArray[0]] : inArray[0];
            if (mapCommands.ContainsKey(inArray[0]))
                log.DebugFormat("Found user mapping: {0}->{1}", inArray[0], mapCommands[inArray[0]]);

            log.InfoFormat("Executing command {0}", command);
            if (command == "VOLUME_SET")
            {
                int volValue = 0;
                if (int.TryParse(val, out volValue))
                    SystemVolumChanger.SetVolume(volValue);
                else
                    log.ErrorFormat("Value \"{0}\" for VOLUME_SET is not number!", val);
            }
            else if (Enum.GetNames(typeof(WindowsInput.Native.VirtualKeyCode)).Contains(command))
            {
                //if this Value is present in VirtualKeyCode then use it directly
                IS.Keyboard.KeyPress((WindowsInput.Native.VirtualKeyCode)Enum.Parse(typeof(WindowsInput.Native.VirtualKeyCode), command));
            }
            else
            {
                log.ErrorFormat(string.Format("Unknown command \"{0}\"", inArray[0]));
                //throw new Exception(string.Format("Unknown command \"{0}\"", inArray[0]));
            }


        }
        /// <summary>
        /// Try run command
        /// </summary>
        /// <param name="input">Command name include value: Eg: VOLUME_SET:90</param>
        /// <returns>True if command execute successfully</returns>
        public bool ProcessRequest(string input)
        {
            try
            {
                parseDo(input);
                return true;
            }
            catch (Exception ex)
            {
                log.WarnFormat("Error executing command. {0}", ex.Message);
                return false;
            } 
        }

        /// <summary>
        /// Custom mappings
        /// </summary>
        public Dictionary<string, string> MapCommands
        {
            get { return mapCommands; }
            set { mapCommands = value; }
        }


    }

    public class RequestInterface
    {
        static Request req;
        static bool init = false;

        /// <summary>
        /// Initialize Request class
        /// </summary>
        public static void Initialize()
        {
            if (req == null)
                req = new Request();
        }
        /// <summary>
        /// Basic test if it is initialized
        /// </summary>
        static void testInit()
        {
            if (req == null)
            {
                Initialize();
                //throw new Exception("RequestInterface is not inicitalized. Call Initialize() first");
            }
        }
        /// <summary>
        /// Map variable MapCommands
        /// </summary>
        public static Dictionary<string, string> MapCommands
        {
            get
            {
                testInit();
                return req.MapCommands;
            }
            set
            {
                testInit();
                req.MapCommands = value;
            }
        }
        /// <summary>
        /// Run command
        /// </summary>
        /// <param name="input">Command name</param>
        /// <returns>True if command execute successfully</returns>
        public static bool ProcessRequest(string input)
        {
            testInit();
            return req.ProcessRequest(input);
        }
    }
}
