using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using log4net;
using SerialMediaRemoteControl.Helpers;
using log4net.Core;
using log4net.Config;
using log4net.Layout;

namespace SerialMediaRemoteControl.Forms
{
    public partial class CommandWindow : Form
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(Main));
        RichTextBoxAppender rba;

        public CommandWindow()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.keyboard;
            this.FormClosing += CommandWindow_FormClosing;
            setupAppender();
        }

        private void CommandWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            rba.Threshold = Level.Off;
        }

        private void btSend_Click(object sender, EventArgs e)
        {
            string tmp = comboBox.Text; // theat because when you select value from combobox and then it is deleted (use it more then 10 times) command will be empty string
            comboBox.Items.Add(tmp);
            if (comboBox.Items.Count > 10)
                comboBox.Items.RemoveAt(0);
            Helpers.RequestInterface.ProcessRequest(tmp);
            comboBox.Text = tmp;
        }


        void setupAppender()
        {
            rba = new RichTextBoxAppender(richTextBoxLog);
            rba.Threshold = Level.All;
            //rba.Layout = new PatternLayout("%date{dd-MM-yyyy HH:mm:ss.fff} %5level %message %n");
            rba.Layout = new PatternLayout("%message %n");
            LevelTextStyle ilts = new LevelTextStyle();
            ilts.Level = Level.Info;
            ilts.TextColor = Color.Black;
            ilts.PointSize = 10.0f;
            rba.AddMapping(ilts);
            LevelTextStyle dlts = new LevelTextStyle();
            dlts.Level = Level.Debug;
            dlts.TextColor = Color.Blue;
            dlts.PointSize = 10.0f;
            rba.AddMapping(dlts);
            LevelTextStyle wlts = new LevelTextStyle();
            wlts.Level = Level.Warn;
            wlts.TextColor = Color.Orange;
            wlts.PointSize = 10.0f;
            rba.AddMapping(wlts);
            LevelTextStyle elts = new LevelTextStyle();
            elts.Level = Level.Error;
            elts.TextColor = Color.Crimson;
            elts.BackColor = Color.Cornsilk;
            elts.PointSize = 10.0f;
            rba.AddMapping(elts);

            BasicConfigurator.Configure(rba);
            rba.ActivateOptions();



            //RollingFileAppender fa = new RollingFileAppender();
            //fa.AppendToFile = true;
            //fa.Threshold = log4net.Core.Level.All;
            //fa.RollingStyle = RollingFileAppender.RollingMode.Size;
            //fa.MaxFileSize = 100000;
            //fa.MaxSizeRollBackups = 3;
            //fa.File = dskPath + @"\FgPleoraLog.txt";
            //fa.Layout = new log4net.Layout.PatternLayout("%date{dd-MM-yyyy HH:mm:ss.fff} %5level %message (%logger{1}:%line)%n");
            //log4net.Config.BasicConfigurator.Configure(fa);
            //fa.ActivateOptions();

        }
    }
}
