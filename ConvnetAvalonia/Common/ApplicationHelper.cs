using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Threading;

namespace ConvnetAvalonia.Common
{
    public static class ApplicationHelper
    {
        #region DoEvents
        /// <summary>
        /// Forces the WPF message pump to process all enqueued messages
        /// that are above the input parameter DispatcherPriority.
        /// </summary>
        /// <param name="priority">The DispatcherPriority to use
        /// as the lowest level of messages to get processed</param>
        //[SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void DoEvents(DispatcherPriority priority)
        {
            Func<object, object> functionDelegate = new Func<object, object>(ExitFrameOperation);
            DispatcherFrame frame = new DispatcherFrame();
            DispatcherOperation dispatcherOperation = Dispatcher.UIThread.InvokeAsync(() => functionDelegate, priority);
            Dispatcher.UIThread.PushFrame(frame);

            if (dispatcherOperation.Status != DispatcherOperationStatus.Completed)
                dispatcherOperation.Abort();
        }


        /// <summary>
        /// Forces the WPF message pump to process all enqueued messages
        /// that are DispatcherPriority.Background or above
        /// </summary>
        //[SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void DoEvents()
        {
            DoEvents(DispatcherPriority.Background);
        }


        /// <summary>
        /// Stops the dispatcher from continuing
        /// </summary>
        private static object ExitFrameOperation(object obj)
        {
            ((DispatcherFrame)obj).Continue = false;
            return null;
        }
        #endregion

        public static bool CheckForInternetConnection()
        {
            var result = false;
            using (var p = new Ping())
            {
                try
                {
                    PingReply reply = p.Send(@"https://www.google.com", 3000);
                    if (reply.Status == IPStatus.Success)
                        return true;
                }
                catch { }
            }
            
            return result;
        }

        public static IEnumerable<string> ToCsv<T>(IEnumerable<T> list)
        {
            var fields = typeof(T).GetFields();
            var properties = typeof(T).GetProperties();

            foreach (var @object in list)
            {
                yield return string.Join(",", fields.Select(x => (x.GetValue(@object) ?? string.Empty).ToString()).Concat(properties.Select(p => (p.GetValue(@object, null) ?? string.Empty).ToString())).ToArray());
            }
        }

        public static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        /*
        public static bool IsValidFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            if (Path.GetInvalidFileNameChars().Any(x => fileName.Contains(x)))
                return false;

            return true;
        }

        public static bool IsValidPathName(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            if (Path.GetInvalidPathChars().Any(x => path.Contains(x)))
                return false;

            return true;
        }
        */
    }
}
