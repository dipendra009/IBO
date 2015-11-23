using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Server
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Application.Run(new ServerFrm());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Dipendra loves Shraddha....." + ex.ToString());
            }
                Application.ExitThread();
        }
    }
}
