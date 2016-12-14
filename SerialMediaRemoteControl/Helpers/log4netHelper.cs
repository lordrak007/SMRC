using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SerialMediaRemoteControl.Helpers
{
    public class log4netHelper
    {
        public static void AddConsoleAppender(bool color = false)
        {
            //var layout = new PatternLayout(@"%date [%thread] %-5level %c - %m%n");
            var layout = new PatternLayout(@"%-5level - %m%n");
            layout.ActivateOptions();
            if (color)
            {
                var _log4netAppender = new ColoredConsoleAppender
                {
                    Layout = layout,
                    Name = "ConsoleAppender"
                };
                _log4netAppender.AddMapping(new ColoredConsoleAppender.LevelColors
                {
                    Level = Level.Debug,
                    ForeColor = ColoredConsoleAppender.Colors.Cyan
                 | ColoredConsoleAppender.Colors.HighIntensity
                });
                _log4netAppender.AddMapping(new ColoredConsoleAppender.LevelColors
                {
                    Level = Level.Info,
                    ForeColor = ColoredConsoleAppender.Colors.Green
                        | ColoredConsoleAppender.Colors.HighIntensity
                });
                _log4netAppender.AddMapping(new ColoredConsoleAppender.LevelColors
                {
                    Level = Level.Warn,
                    ForeColor = ColoredConsoleAppender.Colors.Purple
                        | ColoredConsoleAppender.Colors.HighIntensity
                });
                _log4netAppender.AddMapping(new ColoredConsoleAppender.LevelColors
                {
                    Level = Level.Error,
                    ForeColor = ColoredConsoleAppender.Colors.Red
                        | ColoredConsoleAppender.Colors.HighIntensity
                });
                _log4netAppender.AddMapping(new ColoredConsoleAppender.LevelColors
                {
                    Level = Level.Fatal,
                    ForeColor = ColoredConsoleAppender.Colors.White
                        | ColoredConsoleAppender.Colors.HighIntensity,
                    BackColor = ColoredConsoleAppender.Colors.Red
                });
                _log4netAppender.ActivateOptions();
                BasicConfigurator.Configure(_log4netAppender);
            }
            else
            {
                var _log4netAppender = new ConsoleAppender
                {
                    Layout = layout,
                    Name = "ConsoleAppender"
                };
                _log4netAppender.ActivateOptions();
                BasicConfigurator.Configure(_log4netAppender);
            }
            
            
        }
    }

    /// <summary>
    /// Custom appender for log4net which send logs to Bubble notification
    /// </summary>
    public class BubbleAppender : AppenderSkeleton
    {
        public static BubbleAppender _instance;
        public BubbleAppender()
        { }
        protected override void Append(LoggingEvent LoggingEvent)
        {
            switch (LoggingEvent.Level.Name)
            {
                case "DEBUG":
                    //TrayIkona.ShowBaloonTip(2000, LoggingEvent.Level.Name, RenderLoggingEvent(LoggingEvent), ToolTipIcon.Info);
                    break;
                case "INFO":
                    //TrayIkona.ShowBaloonTip(2000, LoggingEvent.Level.Name, RenderLoggingEvent(LoggingEvent), ToolTipIcon.Info);
                    break;
                case "WARN":
                    TrayIkona.ShowBaloonTip(2000, LoggingEvent.Level.Name, RenderLoggingEvent(LoggingEvent), ToolTipIcon.Warning);
                    break;
                case "ERROR":
                    TrayIkona.ShowBaloonTip(2000, LoggingEvent.Level.Name, RenderLoggingEvent(LoggingEvent), ToolTipIcon.Error);
                    break;
                case "FATAL":
                    TrayIkona.ShowBaloonTip(2000, LoggingEvent.Level.Name, RenderLoggingEvent(LoggingEvent), ToolTipIcon.Error);
                    break;
            }
        }
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(Main));
        public static void Setup(bool Activate = true)
        {
            if (_instance == null)
                _instance = new BubbleAppender();
            _instance.Layout = new PatternLayout("%message %n");
            //_instance.Layout = new PatternLayout("%message%newline%exception");
            _instance.Name = "BubbleAppender";
            _instance.ActivateOptions();
            if (Activate)
            {
                BasicConfigurator.Configure(_instance);
            }
            else
                ((log4net.Repository.Hierarchy.Logger)log.Logger).RemoveAppender(_instance);
        }
    }


    /// <summary>
    /// Description of RichTextBoxAppender.
    /// http://stackoverflow.com/questions/2484822/richtextboxappender-using-log4net
    /// </summary>
    public class RichTextBoxAppender : AppenderSkeleton
    {
        #region Private Instance Fields
        private RichTextBox richTextBox = null;
        private Form containerForm = null;
        private LevelMapping levelMapping = new LevelMapping();
        private int maxTextLength = 100000;
        #endregion

        private delegate void UpdateControlDelegate(LoggingEvent loggingEvent);

        #region Constructor
        public RichTextBoxAppender(RichTextBox myRichTextBox) : base()
        {
            richTextBox = myRichTextBox;
            containerForm = (Form)richTextBox.Parent;
        }
        #endregion

        private void UpdateControl(LoggingEvent loggingEvent)
        {
            // There may be performance issues if the buffer gets too long
            // So periodically clear the buffer
            if (richTextBox.TextLength > maxTextLength)
            {
                richTextBox.Clear();
                richTextBox.AppendText(string.Format("(Cleared log length max: {0})\n", maxTextLength));
            }

            // look for a style mapping
            LevelTextStyle selectedStyle = levelMapping.Lookup(loggingEvent.Level) as LevelTextStyle;
            if (selectedStyle != null)
            {
                // set the colors of the text about to be appended
                richTextBox.SelectionBackColor = selectedStyle.BackColor;
                richTextBox.SelectionColor = selectedStyle.TextColor;

                // alter selection font as much as necessary
                // missing settings are replaced by the font settings on the control
                if (selectedStyle.Font != null)
                {
                    // set Font Family, size and styles
                    richTextBox.SelectionFont = selectedStyle.Font;
                }
                else if (selectedStyle.PointSize > 0 && richTextBox.Font.SizeInPoints != selectedStyle.PointSize)
                {
                    // use control's font family, set size and styles
                    float size = selectedStyle.PointSize > 0.0f ? selectedStyle.PointSize : richTextBox.Font.SizeInPoints;
                    richTextBox.SelectionFont = new Font(richTextBox.Font.FontFamily.Name, size, selectedStyle.FontStyle);
                }
                else if (richTextBox.Font.Style != selectedStyle.FontStyle)
                {
                    // use control's font family and size, set styles
                    richTextBox.SelectionFont = new Font(richTextBox.Font, selectedStyle.FontStyle);
                }
            }
            richTextBox.AppendText(RenderLoggingEvent(loggingEvent));
            //scroll to end?
            richTextBox.ScrollToCaret();
        }

        protected override void Append(LoggingEvent LoggingEvent)
        {
            if (richTextBox.InvokeRequired)
            {
                richTextBox.Invoke(
                    new UpdateControlDelegate(UpdateControl),
                    new object[] { LoggingEvent });
            }
            else
            {
                UpdateControl(LoggingEvent);
            }
        }

        public void AddMapping(LevelTextStyle mapping)
        {
            levelMapping.Add(mapping);
        }

        public override void ActivateOptions()
        {
            base.ActivateOptions();
            levelMapping.ActivateOptions();
        }

        protected override bool RequiresLayout { get { return true; } }
    }

    public class LevelTextStyle : LevelMappingEntry
    {
        private Color textColor;
        private Color backColor;
        private FontStyle fontStyle = FontStyle.Regular;
        private float pointSize = 0.0f;
        private bool bold = false;
        private bool italic = false;
        private string fontFamilyName = null;
        private Font font = null;

        public bool Bold { get { return bold; } set { bold = value; } }
        public bool Italic { get { return italic; } set { italic = value; } }
        public float PointSize { get { return pointSize; } set { pointSize = value; } }

        /// <summary>
        /// Initialize the options for the object
        /// </summary>
        /// <remarks>Parse the properties</remarks>
        public override void ActivateOptions()
        {
            base.ActivateOptions();
            if (bold) fontStyle |= FontStyle.Bold;
            if (italic) fontStyle |= FontStyle.Italic;

            if (fontFamilyName != null)
            {
                float size = pointSize > 0.0f ? pointSize : 8.25f;
                try
                {
                    font = new Font(fontFamilyName, size, fontStyle);
                }
                catch (Exception)
                {
                    font = new Font("Arial", 8.25f, FontStyle.Regular);
                }
            }
        }

        public Color TextColor { get { return textColor; } set { textColor = value; } }
        public Color BackColor { get { return backColor; } set { backColor = value; } }
        public FontStyle FontStyle { get { return fontStyle; } set { fontStyle = value; } }
        public Font Font { get { return font; } set { font = value; } }
    }



}
