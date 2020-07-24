using ApplicationInstance;
using System;
using System.Windows.Forms;
using static ApplicationInstance.ApplicationInstanceChecker;

namespace TestGlobalMutex
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

            ApplicationRunning ppplicationRunning = ApplicationInstanceChecker.getUserTypeRurnningthisApplication();

            if (ppplicationRunning == ApplicationRunning.SameUserRunning)
            {
                // TODO ..Change the Message
                MessageBox.Show("Same user running...");
                return;
            }
            if (ppplicationRunning == ApplicationRunning.DifferentUserRunning)
            {
                // TODO ..Change the Message
                MessageBox.Show("Diff user running...");
                return;
            }
            
            Application.Run(new MainForm());       
        }
    }
}
