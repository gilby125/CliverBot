﻿//********************************************************************************************
//Author: Sergey Stoyan
//        sergey.stoyan@gmail.com
//        sergey_stoyan@yahoo.com
//        http://www.cliversoft.com
//        26 September 2006
//Copyright: (C) 2006, Sergey Stoyan
//********************************************************************************************
using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Configuration;
using System.Media;
using System.Web;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Management;

namespace Cliver
{
    public static class ProcessRoutines
    {
        public static bool IsElevated()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static bool IsProgramRunningAlready()
        {
            Process p = Process.GetCurrentProcess();
            return (from x in Process.GetProcessesByName(p.ProcessName) where x.MainModule.FileName == p.MainModule.FileName select x).Count() > 1;
        }

        public static void RunSingleProcessOnly()
        {
            try
            {
                GLOBAL_SINGLE_PROCESS_MUTEX = new Mutex(false, @"Global\CliverSoft_" + Log.EntryAssemblyName + "_SINGLE_PROCESS");
                // Wait a few seconds when contended, if another instance of the program is still in progress of shutting down.
                if (!GLOBAL_SINGLE_PROCESS_MUTEX.WaitOne(1000, false))
                {
                    string name = Application.ProductName == null ? Log.EntryAssemblyName : Application.ProductName;
                    LogMessage.Exit(name + " is already running, so this instance will exit.");
                }
            }
            catch (Exception e)
            {
                LogMessage.Error(e);
            }
        }
        static Mutex GLOBAL_SINGLE_PROCESS_MUTEX = null;

        /// <summary>
        /// Needed if this process needs some time to exit.
        /// </summary>
        public static void Exit()
        {
            if (GLOBAL_SINGLE_PROCESS_MUTEX != null)
                GLOBAL_SINGLE_PROCESS_MUTEX.ReleaseMutex();
            LogMessage.Exit(String.Empty);
        }

        public static void Restart(bool as_administarator = false)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.UseShellExecute = true;
            psi.WorkingDirectory = Environment.CurrentDirectory;
            psi.FileName = Application.ExecutablePath;
            if (as_administarator)
                psi.Verb = "runas";
            Process.Start(psi);
            ProcessRoutines.Exit();
        }

        public static bool IsProcessAlive(int process_id)
        {
            return GetProcess(process_id) != null;
        }

        public static bool IsProcessAlive(Process process)
        {
            return GetProcess(process.Id) != null;
        }

        public static Process GetProcess(int process_id)
        {
            try
            {
                return Process.GetProcessById(process_id);
            }
            catch
            {
                return null;
            }
        }

        public static bool KillProcessTree(int process_id)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + process_id);
            foreach (ManagementObject mo in searcher.Get())
                KillProcessTree(Convert.ToInt32(mo["ProcessID"]));
            
            Process p;
            try
            {
                p = Process.GetProcessById(process_id);
            }
            catch
            {
                return true;
            }
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    p.Kill();
                }
                catch
                {
                    // Process already exited.
                    return true;
                }
                p.WaitForExit(1000);
                if (!IsProcessRunning(p))
                    return true;
            }
            return false;
        }

        public static bool IsProcessRunning(Process p)
        {
            try
            {
                return !p.HasExited;
            }
            catch
            {//it was not started
                return false;
            }
        }
    }
}