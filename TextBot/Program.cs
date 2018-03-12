using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using XWolf;

namespace TextBot
{
    static class Program
    {
        public const string PROCESSOR_C_SHARP = "pcs";
        public const string VERSION = "1.1";
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            FileAssociation.AssociateMe(PROCESSOR_C_SHARP, "XWolf_TextProcessor", "Text processor");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
