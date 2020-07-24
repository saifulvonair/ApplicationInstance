//######################################################################
//# FILENAME: ApplicationInstanceChecker
//#
//# DESCRIPTION:
//# 
//#
//# AUTHOR:		Mohammad Saiful Alam
//# POSITION:	Senior General Manager
//# E-MAIL:		saiful.alam@ bjitgroup.com
//# CREATE DATE: 2020/07/24
//#
//# Copyright (c): 
//######################################################################
using System;
using System.Runtime.InteropServices;
using System.Management;
using System.Diagnostics;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace ApplicationInstance
{
    class ApplicationInstanceChecker
    {
       public enum ApplicationRunning
        {
            NoUserRunning,
            SameUserRunning,
            DifferentUserRunning
        };

        /// <summary>
        /// Activates the main window of a given process
        /// </summary>
        /// <param name="process"></param>
        /// 
        [DllImportAttribute("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImportAttribute("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImportAttribute("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd,
                                                uint Msg,
                                                IntPtr wParam,
                                                IntPtr lParam);

        public static uint MSG_ID_SEND_MESSAGE = 0x911;

        /// <summary>
        /// Pass processID to brig it front.
        /// </summary>
        /// <param name="processId"></param>
        public static void activateProcessMainWindow(long processId)
        {
            Process process = Process.GetProcessById((int)processId);
            IntPtr handle = process.MainWindowHandle;
            if (handle != IntPtr.Zero)
            {
                ShowWindow(handle, 1);
                SetForegroundWindow(handle);
            }
            else
            {
                // TODO... Change this Text as per Application title...
                String lStrWindowTxt = "TestMutex";
                ShowToFront(lStrWindowTxt);                 
            }
        }

        /// <summary>
        /// Pass The Name of the widow to brig it front.
        /// </summary>
        /// <param name="windowName"></param>
        public static void ShowToFront(string windowName)
        {
            IntPtr firstInstance = FindWindow(null, windowName);
            ShowWindow(firstInstance, 1);
            SetForegroundWindow(firstInstance);
            SendMessage(firstInstance, MSG_ID_SEND_MESSAGE, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>
        /// Returns the process ID of a process with the same name as the current process
        /// </summary>
        /// <param name="sameUser">True to check processes for the current user only, false
        /// to check processes for other users only</param>
        /// <returns>PID of the process found, or 0 if no matching process is found</returns>
        public static long getSameNameProcess(bool sameUser)
        {
            Process currentProcess = Process.GetCurrentProcess();
            string currentProcessUser = "";

            //* Obtain user name for the current process

            ObjectQuery query = new ObjectQuery
                ("select * from Win32_Process where Name=\"" + currentProcess.ProcessName + ".exe\" and ProcessID=" + currentProcess.Id);
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection results = searcher.Get();
            foreach (ManagementObject result in results)
            {
                string[] o = new String[2];
                result.InvokeMethod("GetOwner", (object[])o);
                currentProcessUser = o[0];
            }

            //* Obtain processes with the same name of current process

            query = new ObjectQuery
                ("select * from Win32_Process where Name=\"" + currentProcess.ProcessName + ".exe\" and ProcessID<>" + currentProcess.Id);
            searcher = new ManagementObjectSearcher(query);
            results = searcher.Get();

            //* Search for a matching process

            foreach (ManagementObject result in results)
            {
                string[] o = new String[2];
                result.InvokeMethod("GetOwner", (object[])o);
                if ((o[0] == currentProcessUser && sameUser) || (o[0] != currentProcessUser && !sameUser))
                    return (long)(uint)result["ProcessID"];
            }

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        public static ApplicationRunning getUserTypeRurnningthisApplication()
        {
            string appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value.ToString();
            // unique id for global mutex - Global prefix means it is global to the machine
            string mutexId = string.Format("Global\\{{{0}}}", appGuid);
            bool createdNew;
            // edited by Jeremy Wiebe to add example of setting up security for multi-user usage
            // edited by 'Marc' to work also on localized systems (don't use just "Everyone") 
            var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);
            createdNew = true;

            try
            {
                Mutex mutex = new Mutex(true, mutexId, out createdNew, securitySettings);
            }
            catch (Exception e)
            {
                createdNew = false;
            }

            if (!createdNew)
            {
                //long sameUserRunning = 0;
                long wdwIntPtr = ApplicationInstanceChecker.getSameNameProcess(true);

                if (wdwIntPtr > 0)
                {
                    ApplicationInstanceChecker.activateProcessMainWindow((long)wdwIntPtr);
                    // TODO ..
                    //MessageBox.Show("Same user running...");
                    return ApplicationRunning.SameUserRunning;
                }
                // TODO ..
                //MessageBox.Show("Diff user running...");
                return ApplicationRunning.DifferentUserRunning;
            }

            return ApplicationRunning.NoUserRunning;

        }
    }
}
