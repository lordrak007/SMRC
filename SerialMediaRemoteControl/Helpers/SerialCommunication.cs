using log4net;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using WindowsInput;


namespace SerialMediaRemoteControl.Helpers
{
    class SerialCommunication
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(SerialCommunication));
        SerialPort _instance;
        /// <summary>
        /// Initialize serial port. Gets settings directly from Config class
        /// </summary>
        public SerialCommunication()
        {
            init(Main.cfg.Communication.PortName, Main.cfg.Communication.BaudRate, Main.cfg.Communication.Parity, Main.cfg.Communication.DataBits);
        }
        /// <summary>
        /// Initialize serial port
        /// </summary>
        /// <param name="comPort">Com port name. Eg. COM2</param>
        /// <param name="baudRate">Baud rate. Eg. 9600</param>
        /// <param name="parity">Parity. Standard None</param>
        /// <param name="dataBits">Data bits. Standard 8</param>
        public SerialCommunication(string comPort, int baudRate, Parity parity, int dataBits)
        {
            init(comPort, baudRate, parity, dataBits);
        }


        public SerialPort Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;
                else
                    throw new Exception("Serial port not initialized. Initialize it first");
            }
            set { _instance = value; }
        }

        void init(string comPort, int baudRate, Parity parity, int dataBits)
        {
            _instance = new SerialPort(comPort, baudRate, parity, dataBits);
            _instance.DataReceived += _instance_DataReceived;
            try
            {
                _instance.Open();
            }
            catch (Exception ex)
            {
                log.Fatal("Fatal error while opening com port", ex);
            }
        }

        private void _instance_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadLine();
            log.DebugFormat("Data received:{0}", indata);
            Helpers.RequestInterface.ProcessRequest(indata);
        }

    }
}
