using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SerialMediaRemoteControl.Objects
{
    /// <summary>
    /// Main configuration class
    /// </summary>
    [Serializable]
    public class Config
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(Config));

        public Config()
        {
            MapCommands = new List<MapCommand>();
            Communication = new Communication();
            Processing = new Processing();
        }

        readonly static string configFile = "config.cfg";
        public Communication Communication { get; set; }
        public List<MapCommand> MapCommands { get; set; }

        public Processing Processing { get; set; }

        public void GenerateExampleData()
        {
            Communication.PortName = "COM2";
            Communication.Parity = System.IO.Ports.Parity.None;
            Communication.BaudRate = 9600;
            Communication.DataBits = 8;
            Communication.DeviceAutoSearch = false;
            Communication.DeviceHeloMessage = "RemoteInit";
            Communication.DeviceEhloMessage = "InitComplete";

            MapCommands.Add(new MapCommand("VU", "VOLUME_UP"));
            MapCommands.Add(new MapCommand("VD", "VOLUME_DOWN"));
            Config.Save(this);
        }

        /// <summary>
        /// Save configuration to file
        /// </summary>
        /// <param name="c">Config class instance</param>
        public static void Save(Config c)
        {
            try
            {
                XmlUtility.SerializeToFile<Config>(c, configFile);
            }
            catch (Exception ex)
            {
                log.Error("Can not save configuration.", ex);
            }
        }
        /// <summary>
        /// Load configuration from file
        /// </summary>
        /// <returns>Config class instance</returns>
        public static Config Load()
        {
            try
            {
                return XmlUtility.DeserializeFromFile<Config>(configFile);
            }
            catch (Exception ex)
            {
                log.Error("Can not load configuration.", ex);
                return null;
            }
        }



    }

    /// <summary>
    /// Communication settings
    /// </summary>
    public class Communication
    {
        public Communication()
        {
            DeviceAutoSearch = false;
            PortName = "COM4";
            BaudRate = 9600;
            Parity = System.IO.Ports.Parity.None;
            DataBits = 8;
        }
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public System.IO.Ports.Parity Parity { get; set; }
        public int DataBits { get; set; }
        /// <summary>
        /// Try autodetect arduino
        /// </summary>
        public bool DeviceAutoSearch { get; set; }
        /// <summary>
        /// Test message to arduino. Initial message to start communication.
        /// </summary>
        public string DeviceHeloMessage { get; set; }
        /// <summary>
        /// Responce from arduino. Responce from device. Used to detect if is it device what we are looking for.
        /// </summary>
        public string DeviceEhloMessage { get; set; }
        /// <summary>
        /// Preffered baudRates when testing connection
        /// </summary>
        public int[] DeviceAutoSearchPrefferedBaudRates { get; set; }
    }

    /// <summary>
    /// Processing settings
    /// </summary>
    public class Processing
    {
        public Processing()
        {
            ValueSeparator = ":";
            //CaseSensitiveCommands = false;
            ShowErrorsInTrayBubble = false;
        }
        public string ValueSeparator { get; set; }
        //public bool CaseSensitiveCommands { get; set; }

        public bool ShowErrorsInTrayBubble { get; set; }
}

    /// <summary>
    /// Mapping class. Mapp custom command to command VirtualKeyCode
    /// </summary>
    public class MapCommand
    {
        public MapCommand()
        { }
        public MapCommand(string customKey, string mappedValue)
        {
            CustomKey = customKey;
            MappedValue = mappedValue;
        }
        /// <summary>
        /// Incoming value
        /// </summary>
        [XmlAttribute]
        public string CustomKey { get; set; }
        /// <summary>
        /// Command in VirtualKeyCode lib
        /// </summary>
        [XmlAttribute]
        public string MappedValue { get; set; }
    }



    public class XmlUtility
    {

        /// <summary>
        /// Serialize to string.
        /// </summary>
        /// <typeparam name="T">object type (class)</typeparam>
        /// <param name="o">object to serialize</param>
        /// <returns>xml data</returns>
        public static string SerializeToString<T>(T o)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, o);
                return writer.ToString();
            }
        }


        /// <summary>
        /// Deserialize from string.
        /// </summary>
        /// <typeparam name="T">object type (class)</typeparam>
        /// <param name="xml">xml data</param>
        /// <returns>object to deserialize</returns>
        public static T DeserializeFromString<T>(string xml)
        {
            using (StringReader reader = new StringReader(xml))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// Serialize to file.
        /// </summary>
        /// <typeparam name="T">object type (class)</typeparam>
        /// <param name="o">object to serialize</param>
        /// <param name="path">path to file</param>
        public static void SerializeToFile<T>(T o, string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StreamWriter writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, o);
            }
        }

        /// <summary>
        /// Deserialize from file.
        /// </summary>
        /// <typeparam name="T">object type (class)</typeparam>
        /// <param name="path">path to file</param>
        /// <returns>object to deserialize</returns>
        public static T DeserializeFromFile<T>(string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// Serialize to xml document
        /// </summary>
        /// <typeparam name="T">object type (class)</typeparam>
        /// <param name="o">object to serialize</param>
        /// <returns>XmlDocument</returns>
        public static XmlDocument SerializeToXmlDocument<T>(T obj)
        {
            XmlDocument document = new XmlDocument();
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            using (XmlWriter writer = document.CreateNavigator().AppendChild())
                serializer.Serialize(writer, obj);

            return document;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document"></param>
        /// <returns></returns>
        public static T DeserializeFromXmlDocument<T>(XmlDocument document)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (XmlNodeReader reader = new XmlNodeReader(document.DocumentElement))
                return (T)serializer.Deserialize(reader);
        }
    }

}
