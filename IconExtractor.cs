using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Flow.Launcher.Plugin.OpenWindowSearch;

internal class IconExtractor : IDisposable
{
    private const int WM_GETICON = 0x007F;
    private const int ICON_SMALL = 0;
    private const int ICON_BIG = 1;
    private const int GCL_HICON = -14;

    private readonly ConcurrentDictionary<IntPtr, string> _iconCache = new ConcurrentDictionary<IntPtr, string>();
    private bool _disposed = false;

    private static IntPtr GetIconHandle(IntPtr hwnd)
    {
        IntPtr iconHandle = NativeMethods.SendMessage(hwnd, WM_GETICON, ICON_SMALL, 0);
        if (iconHandle == IntPtr.Zero)
        {
            iconHandle = NativeMethods.SendMessage(hwnd, WM_GETICON, ICON_BIG, 0);
        }
        if (iconHandle == IntPtr.Zero)
        {
            iconHandle = NativeMethods.GetClassLongPtr(hwnd, GCL_HICON);
        }
        return iconHandle;
    }

    public string GetIconPath(IntPtr windowHandle)
    {
        if (_iconCache.TryGetValue(windowHandle, out string cachedFilePath))
        {
            return cachedFilePath;
        }

        string tempFilePath = null;
        IntPtr iconHandle = GetIconHandle(windowHandle);

        if (iconHandle != IntPtr.Zero)
        {
            var tempfileDir = Path.Combine(Path.GetTempPath(), "OpenWindowSearch");
            Directory.CreateDirectory(tempfileDir);
            tempFilePath = Path.Combine(tempfileDir, Guid.NewGuid().ToString() + ".png");
            
            SaveIconToFile(iconHandle, tempFilePath);
        }

        _iconCache[windowHandle] = tempFilePath;

        return tempFilePath;
    }

    private static void SaveIconToFile(IntPtr iconHandle, string filePath)
    {
        using Icon icon = Icon.FromHandle(iconHandle);
        using MemoryStream ms = new MemoryStream();

        icon.ToBitmap().Save(ms, ImageFormat.Png);
        File.WriteAllBytes(filePath, ms.ToArray());
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                foreach (var filePath in _iconCache.Values)
                {
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        try
                        {
                            File.Delete(filePath);
                        }
                        catch (IOException)
                        {
                            // File might be in use, skip it
                        }
                    }
                }
                _iconCache.Clear();
            }

            _disposed = true;
        }
    }

    ~IconExtractor()
    {
        Dispose(false);
    }
}
