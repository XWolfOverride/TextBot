using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace TextBot
{
    public partial class FHelp : Form
    {
        private FHelp()
        {
            InitializeComponent();
            LoadHelp();
        }

        public static void Execute()
        {
            using (FHelp f = new FHelp())
                f.ShowDialog();
        }

        private void LoadHelp()
        {
            var array = Encoding.Unicode.GetBytes(Properties.Resources.HelpPage);
            webBrowser1.DocumentStream = new MemoryStream(array);
        }

        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (e.Url.AbsoluteUri != "about:blank")
            {
                Process.Start(e.Url.AbsoluteUri);
                e.Cancel = true;
            }
        }
    }
}
