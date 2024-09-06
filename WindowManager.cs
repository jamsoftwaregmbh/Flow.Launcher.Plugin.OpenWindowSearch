using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Flow.Launcher.Plugin.OpenWindowSearch;

internal class WindowManager
{
    internal static List<Window> GetOpenWindows()
    {
        var windows = new List<Window>();
        NativeMethods.EnumWindows((hwnd, lParam) =>
        {
            if (NativeMethods.IsWindowVisible(hwnd) && NativeMethods.GetWindowTextLength(hwnd) > 0)
            {
                windows.Add(new Window
                {
                    Hwnd = hwnd,
                    Title = GetWindowText(hwnd),
                    ApplicationName = GetApplicationName(hwnd)
                });
            }
            return true;
        }, IntPtr.Zero);
        return windows;
    }
        
    private static string GetWindowText(IntPtr hWnd)
    {
        int length = NativeMethods.GetWindowTextLength(hWnd);
        if (length == 0) return string.Empty;

        var builder = new System.Text.StringBuilder(length + 1);
        NativeMethods.GetWindowText(hWnd, builder, builder.Capacity);
        return builder.ToString();
    }

    private static string GetApplicationName(IntPtr hwnd)
    {
        NativeMethods.GetWindowThreadProcessId(hwnd, out int processId);
        try
        {
            using var process = Process.GetProcessById(processId);
            return Path.GetFileNameWithoutExtension(process.MainModule?.FileName ?? string.Empty);
        }
        catch
        {
            return string.Empty;
        }
    }
}
