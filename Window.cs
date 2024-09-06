using System;

namespace Flow.Launcher.Plugin.OpenWindowSearch;

internal class Window
{
    private const int SW_RESTORE = 9;
    private const int SW_MAXIMIZE = 3;

    public IntPtr Hwnd { get; set; }
    public string Title { get; set; }
    public string ApplicationName { get; set; }

    public void Activate()
    {
        if (NativeMethods.IsIconic(Hwnd))
        {
            NativeMethods.ShowWindow(Hwnd, SW_RESTORE);
        }

        bool isMaximized = NativeMethods.IsZoomed(Hwnd);

        NativeMethods.SetForegroundWindow(Hwnd);

        if (isMaximized)
        {
            NativeMethods.ShowWindow(Hwnd, SW_MAXIMIZE);
        }
    }
}