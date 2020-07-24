
using ApplicationInstance;
using System;
using System.Windows.Forms;

namespace TestGlobalMutex
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        //WINDOW MESSAGE HANDLER
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);        // call default p
            if (m.Msg == (int)ApplicationInstanceChecker.MSG_ID_SEND_MESSAGE)
            {
                ApplicationInstanceChecker.ShowWindow(Handle, 1);
                ApplicationInstanceChecker.SetForegroundWindow(Handle);
             }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            
        }
    }
}
