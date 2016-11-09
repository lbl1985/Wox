using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading.Tasks;
using Wox.Plugin;

using HWND = System.IntPtr;


/// <summary>Contains functionality to get all the open windows.</summary>
public static class OpenWindowGetter
{
    /// <summary>Returns a dictionary that contains the handle and title of all the open windows.</summary>
    /// <returns>A dictionary that contains the handle and title of all the open windows.</returns>
    public static IDictionary<HWND, string> GetOpenWindows()
    {
        HWND shellWindow = GetShellWindow();
        Dictionary<HWND, string> windows = new Dictionary<HWND, string>();

        EnumWindows(delegate (HWND hWnd, int lParam)
        {
            if (hWnd == shellWindow) return true;
            if (!IsWindowVisible(hWnd)) return true;

            int length = GetWindowTextLength(hWnd);
            if (length == 0) return true;

            StringBuilder builder = new StringBuilder(length);
            GetWindowText(hWnd, builder, length + 1);

            windows[hWnd] = builder.ToString();
            return true;

        }, 0);

        return windows;
    }

    private delegate bool EnumWindowsProc(HWND hWnd, int lParam);

    [DllImport("USER32.DLL")]
    private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

    [DllImport("USER32.DLL")]
    private static extern int GetWindowText(HWND hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("USER32.DLL")]
    private static extern int GetWindowTextLength(HWND hWnd);

    [DllImport("USER32.DLL")]
    private static extern bool IsWindowVisible(HWND hWnd);

    [DllImport("USER32.DLL")]
    private static extern IntPtr GetShellWindow();
}


namespace SwitchProcess
{
    
    public class Main : IPlugin
    {
        public const int SW_RESTORE = 9;
        public List<Result> Query(Query query)
        {
            List<Result> resList = new List<Result>{ };

            foreach (KeyValuePair<IntPtr, string> window in OpenWindowGetter.GetOpenWindows())
            {
                string windowTitle = window.Value;
                IntPtr windowHandle = window.Key;

                if (windowTitle != "Wox")
                {

                    string ProcessPath = "";
                    try
                    {
                        ProcessPath = GetProcessPath(windowHandle);
                    }
                    catch (Exception e)
                    {

                    }
                    resList.Add(new Result
                    {
                        Title = window.Value,
                        SubTitle = ProcessPath,
                        IcoPath = ProcessPath,
                        Action = e =>
                        {
                            // after user select the item

                            // return false to tell Wox don't hide query window, otherwise Wox will hide it automatically
                            BringToForeground(windowHandle);
                            return true;
                        }
                    });
                }
            }


            //var results1 = _win32s.AsParallel().Select(p => p.Result(query.Search, _context.API));
            var resList1 = resList.Where(p => p.Title.ToLower().Contains(query.Search.ToLower())).ToList();
            //resList = resList.Select()
            return resList1;
        }

        public void Init(PluginInitContext context)
        {

        }

        private void BringToForeground(IntPtr extHandle)
        {
            if (IsIconic(extHandle))
            {
                ShowWindow(extHandle, SW_RESTORE);
            }
            SetForegroundWindow(extHandle);            
        }

        private string GetProcessPath(IntPtr hwnd)
        {
            uint pid = 0;
            GetWindowThreadProcessId(hwnd, out pid);
            Process proc = Process.GetProcessById((int)pid);
            return proc.MainModule.FileName.ToString();
        }

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr handle);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr handle, int nCmdShow);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out uint processId);


    }
}
