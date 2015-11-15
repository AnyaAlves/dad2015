using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using Process = EnvDTE.Process;

namespace SESDAD.Managing {
    /// <summary>
    /// Class ProcessExtender
    /// Attach to other processes in order to debug.
    /// </summary>
    public static class DebugTools {
        /// <summary>
        /// Attaches Visual Studio (2012) to the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public static void Attach(this System.Diagnostics.Process process) {
            // Reference visual studio core
            DTE dte = (DTE)Marshal.GetActiveObject(@"VisualStudio.DTE.11.0");

            // Register the IOleMessageFilter to handle any threading errors.
            MessageFilter.Register();

            // Try loop - visual studio may not respond the first time.
            int tryCount = 50;
            while (tryCount-- > 0) {
                try {
                    EnvDTE.Processes processes = dte.Debugger.LocalProcesses;
                    Process DTEprocess = processes.Cast<Process>().Where(
                        proc => proc.ProcessID == process.Id).First();
                    DTEprocess.Attach();   
                    break;
                }
                catch (COMException) {
                    System.Threading.Thread.Sleep(500);
                }
            }
            // and turn off the IOleMessageFilter.
            MessageFilter.Revoke();
        }
    }

    public class MessageFilter : IOleMessageFilter {
        //
        // Class containing the IOleMessageFilter
        // thread error-handling functions.

        // Start the filter.
        public static void Register() {
            IOleMessageFilter newFilter = new MessageFilter();
            IOleMessageFilter oldFilter = null;
            CoRegisterMessageFilter(newFilter, out oldFilter);
        }

        // Done with the filter, close it.
        public static void Revoke() {
            IOleMessageFilter oldFilter = null;
            CoRegisterMessageFilter(null, out oldFilter);
        }

        //
        // IOleMessageFilter functions.
        // Handle incoming thread requests.
        int IOleMessageFilter.HandleInComingCall(int dwCallType,
          System.IntPtr hTaskCaller, int dwTickCount, System.IntPtr
          lpInterfaceInfo) {
            //Return the flag SERVERCALL_ISHANDLED.
            return 0;
        }

        // Thread call was rejected, so try again.
        int IOleMessageFilter.RetryRejectedCall(System.IntPtr
          hTaskCallee, int dwTickCount, int dwRejectType) {
            if (dwRejectType == 2)
            // flag = SERVERCALL_RETRYLATER.
            {
                // Retry the thread call immediately if return >=0 & 
                // <100.
                return 99;
            }
            // Too busy; cancel call.
            return -1;
        }

        int IOleMessageFilter.MessagePending(System.IntPtr hTaskCallee,
          int dwTickCount, int dwPendingType) {
            //Return the flag PENDINGMSG_WAITDEFPROCESS.
            return 2;
        }

        // Implement the IOleMessageFilter interface.
        [DllImport("Ole32.dll")]
        private static extern int
          CoRegisterMessageFilter(IOleMessageFilter newFilter, out 
          IOleMessageFilter oldFilter);
    }

    [ComImport(), Guid("00000016-0000-0000-C000-000000000046"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    interface IOleMessageFilter {
        [PreserveSig]
        int HandleInComingCall(
            int dwCallType,
            IntPtr hTaskCaller,
            int dwTickCount,
            IntPtr lpInterfaceInfo);

        [PreserveSig]
        int RetryRejectedCall(
            IntPtr hTaskCallee,
            int dwTickCount,
            int dwRejectType);

        [PreserveSig]
        int MessagePending(
            IntPtr hTaskCallee,
            int dwTickCount,
            int dwPendingType);
    }
}

