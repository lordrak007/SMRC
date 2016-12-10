using System.Windows.Forms;

namespace SerialMediaRemoteControl
{
    class Program
    {
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main(args));           
        }
    }
}
