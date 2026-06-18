// Retro-16
// Copyright (C) 2026  DbrittoRicky
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace Retro_16;

// Hot reloading system
// A developer can keep the console running and make changes to their code/assets
// in an external editor.
// The moment they save their changes, they reflect on the console without needing
// to restart it.

public sealed class CartridgeWatcher : IDisposable
{
    private readonly string _path;
    private readonly FileSystemWatcher _watcher;

    /// <summary>True when a file-change event has been received.</summary>
    public bool ReloadPending { get; private set; }

    public CartridgeWatcher(string path)
    {
        _path = Path.GetFullPath(path);

        string dir = Path.GetDirectoryName(_path)!;
        string filter = Path.GetFileName(_path);

        _watcher = new FileSystemWatcher(dir, filter)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
            EnableRaisingEvents = true,
        };

        // Both Changed and Created cover different editor save
        // strategies (overwrite vs. write-new-then-rename).
        _watcher.Changed += OnChanged;
        _watcher.Created += OnChanged;
    }

    /// <summary>Reads and returns the full text of the cart file.</summary>
    public string ReadSource() => File.ReadAllText(_path);

    /// <summary>
    /// Call after handling the reload to stop repeated reloads
    /// from a single save event.
    /// </summary>
    public void Acknowledge() => ReloadPending = false;

    private void OnChanged(object? sender, FileSystemEventArgs e) => ReloadPending = true;

    public void Dispose() => _watcher.Dispose();
}
