using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;



namespace SerialMediaRemoteControl.Helpers
{
    class SerialCommunication
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(SerialCommunication));
        BackgroundWorker _portSearcher;
        SafeSerialPort _instance;
        /// <summary>
        /// Initialize serial port. Gets settings directly from Config class
        /// </summary>
        public SerialCommunication()
        {
            //init _portSearcher
            if (_portSearcher == null)
            {
                _portSearcher = new BackgroundWorker();
                _portSearcher.DoWork += _portSearcher_DoWork;
                _portSearcher.WorkerSupportsCancellation = true;
            }
            // try search arduino (anything) connected to serial port
            if (Main.cfg.Communication.DeviceAutoSearch)
            {
                _portSearcher.RunWorkerAsync();
            }
            else
               init(Main.cfg.Communication.PortName, Main.cfg.Communication.BaudRate, Main.cfg.Communication.Parity, Main.cfg.Communication.DataBits);
        }

        /// <summary>
        /// Start auto reconnecting
        /// </summary>
        void autoReconnect()
        {
            _portSearcher.RunWorkerAsync(true);


        }

        private void _portSearcher_DoWork(object sender, DoWorkEventArgs e)
        {
            bool reconnecting = false;
            if (e.Argument != null)
                reconnecting = (bool)e.Argument;

            log.Info("Waiting device to connect");
            TrayIkona.ShowBaloonTip(5000, "Waiting", "Waiting device to connect", System.Windows.Forms.ToolTipIcon.Info);
            
            int[] baudRates = { 4800, 9600, 14400, 19200, 38400, 57600 };
            if (reconnecting)
                baudRates = new int[] { Main.cfg.Communication.BaudRate };

            string foundPort = "";
            int foundBaudRate = 0;
            while (true)
            {
                List<string> connectedPorts = new List<string>();
                if (reconnecting)
                    connectedPorts.Add(Main.cfg.Communication.PortName);
                else
                    connectedPorts = SerialPort.GetPortNames().ToList();

                string ask = Main.cfg.Communication.DeviceHeloMessage;
                string responce = Main.cfg.Communication.DeviceEhloMessage;

                foreach (string port in connectedPorts)
                {
                    foreach (int baudRate in baudRates)
                    {
                        SerialPort sp = new SerialPort(port, baudRate, Parity.None, 8);
                        try
                        {
                            bool responceOk = false;
                            sp.Open();
                            if (!string.IsNullOrEmpty(ask))
                                sp.WriteLine(ask);
                            if (!string.IsNullOrEmpty(responce))
                            {
                                sp.ReadTimeout =2000;
                                int tries = 0;
                                while (!responceOk)
                                {
                                    if (tries > 4)
                                        break;
                                    try
                                    {
                                        tries++;
                                        if (sp.ReadLine() == responce)
                                            responceOk = true;
                                    }
                                    catch
                                    { }
                                }
                                if (!responceOk)
                                    continue; //if we occurst responce and reveice another, continue searching
                            }
                            
                            foundPort = port;
                            foundBaudRate = baudRate;

                            break; //break baud rate foreach
                        }
                        catch
                        {   
                            if (string.IsNullOrEmpty(responce)) //when want responce, there is timenou allready
                                System.Threading.Thread.Sleep(3000); //sleep when connation fail
                            continue;
                        }
                        finally
                        {
                            if(sp.IsOpen)
                                sp.Close();
                        }


                    }
                    if (!string.IsNullOrEmpty(foundPort) && foundBaudRate != 0)
                        break;//break Port foreach when i have someone allready
                }

                // we have found something, so configure it and exit loop
                if (!string.IsNullOrEmpty(foundPort) && foundBaudRate != 0)
                {
                    string message = string.Format("Found device at {0} with baudRate {1}", foundPort, foundBaudRate);
                    log.Info(message);
                    TrayIkona.ShowBaloonTip(5000, "Connected", message, System.Windows.Forms.ToolTipIcon.Info);
                    //save setting temp. in settings
                    Main.cfg.Communication.PortName = foundPort;
                    Main.cfg.Communication.BaudRate = foundBaudRate;
                    Main.cfg.Communication.Parity = Parity.None;
                    Main.cfg.Communication.DataBits = 8;
                    // connect with detected settings
                    init(foundPort, foundBaudRate, Parity.None, 8);
                    break;
                }
                else // maybe log only debug and continue searching
                {
                    log.Debug("Autosearch don't found any device. I will try it again");
                }  
            }
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


        public SafeSerialPort Instance
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
            _instance = new SafeSerialPort(comPort, baudRate, parity, dataBits);
            _instance.DataReceived += _instance_DataReceived;
            _instance.Disconnected += _instance_Disconnected;
            
            try
            {
                _instance.Open();
            }
            catch (Exception ex)
            {
                log.Fatal("Fatal error while opening com port. "+ex.Message);
            }
        }

        private void _instance_Disconnected(SafeSerialPort ssp, EventArgs e)
        {
            log.WarnFormat("Device on {0} has been disconnected", _instance.PortName);
            autoReconnect();
        }

        private void _instance_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadLine().Trim();
            log.DebugFormat("Data received:{0}", indata);
            Helpers.RequestInterface.ProcessRequest(indata);
        }


    }

    /// <summary>
    /// Resolves usb to serial handlig bug - http://stackoverflow.com/questions/13408476/detecting-when-a-serialport-gets-disconnected
    /// </summary>
    class SafeSerialPort : SerialPort
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(SafeSerialPort));
        private System.IO.Stream theBaseStream;
        System.Timers.Timer aliveChecker = new System.Timers.Timer();

        #region Event handlers
        public event DisconnectedHandler Disconnected;
        public EventArgs e = null;
        public delegate void DisconnectedHandler(SafeSerialPort ssp, EventArgs e);
        #endregion Event handlers
        
        
        /// <summary>
        /// Initialize class
        /// </summary>
        void init()
        {
            //init timer
            aliveChecker.Interval = 10000;
            aliveChecker.Elapsed += AliveChecker_Elapsed;
            //aliveChecker.Start();
        }

        private void AliveChecker_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if(!IsOpen && !SerialPort.GetPortNames().Contains(PortName))
            {
                aliveChecker.Stop(); //stop timer until new connection
                Disconnected?.Invoke(this, e);
            }    
        }

        #region contructors
        public SafeSerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
            : base(portName, baudRate, parity, dataBits, stopBits)
        {
            init();
        }

        public SafeSerialPort(string portName, int baudRate, Parity parity, int dataBits)
            : base(portName, baudRate, parity, dataBits)
        {
            init();
        }

        public SafeSerialPort(string portName, int baudRate, Parity parity) : base(portName, baudRate, parity)
        {
            init();
        }

        public SafeSerialPort(string portName, int baudRate) : base(portName, baudRate)
        {
            init();
        }

        public SafeSerialPort(string portName) : base(portName)
        {
            init();
        }
        #endregion contructors

        public new void Open()
        {
            try
            {
                base.Open();
                theBaseStream = BaseStream;
                GC.SuppressFinalize(BaseStream);
                aliveChecker.Start();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public new void Dispose()
        {
            Dispose(true);
        }

        protected override void Dispose(bool disposing)
        {
            aliveChecker.Stop();
            if (disposing && (base.Container != null))
            {
                base.Container.Dispose();
            }
            try
            {
                if (theBaseStream.CanRead)
                {
                    theBaseStream.Close();
                    GC.ReRegisterForFinalize(theBaseStream);
                }
            }
            catch
            {
                // ignore exception - bug with USB - serial adapters.
            }
            base.Dispose(disposing);
        }

        
    }
}
