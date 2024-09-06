using System;
using System.Collections.Generic;
using System.Linq;

using Flow.Launcher.Plugin;

namespace Flow.Launcher.Plugin.OpenWindowSearch;

/// <summary>
/// Plugin for searching and activating open windows.
/// </summary>
public class OpenWindowSearch : IPlugin, IDisposable
{
    private PluginInitContext _context;
    private IconExtractor _iconExtractor;
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenWindowSearch"/> class.
    /// </summary>
    public OpenWindowSearch()
    {
        _iconExtractor = new IconExtractor();
    }

    /// <inheritdoc/>
    public void Init(PluginInitContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public List<Result> Query(Query query)
    {
        var windows = WindowManager.GetOpenWindows();
        var matchingTitles = windows.Where(w => w.Title.ToLower().Contains(query.Search.ToLower()));

        return matchingTitles.Select(window => new Result
        {
            Title = window.Title,
            SubTitle = window.ApplicationName,
            IcoPath = _iconExtractor.GetIconPath(window.Hwnd) ?? "Images/app.png",
            Action = c =>
            {
                window.Activate();
                return true;
            }
        }).ToList();
    }        

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged resources and performs other cleanup operations.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _iconExtractor.Dispose();
            }
            _disposed = true;
        }
    }
}
