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

using MoonSharp.Interpreter;
using Retro_16.Core;
using Retro_16.Graphics;

namespace Retro_16.Scripting;

// Setting up the Lua host
// Most of the code here is by using the MoonSharp library
// it is better to refer the MoonSharp documentation for more info
public sealed class LuaHost
{
    private Script _script = null!;
    private DynValue? _initFn;
    private DynValue? _updateFn;
    private DynValue? _drawFn;

    private readonly DrawApi _draw;
    private readonly TextApi _text;
    private readonly InputState _input;

    /// <summary>The last Lua runtime error message, or null if none.</summary>
    public string? LastError { get; private set; }

    /// <summary>True if the last frame produced a Lua runtime error.</summary>
    public bool HasError => LastError is not null;

    public LuaHost(DrawApi draw, TextApi text, InputState input)
    {
        _draw = draw;
        _text = text;
        _input = input;
    }

    /// <summary>
    /// Parses and runs the cart source, then calls _init() if
    /// it is defined. Calling Load() again (e.g., on hot-reload)
    /// creates a completely fresh Lua state — no memory leaks
    /// from the previous cart instance.
    /// </summary>
    public void Load(string source)
    {
        LastError = null;

        // SoftSandbox removes dangerous modules (io, os, etc.)
        // while keeping math, table, string available.
        _script = new Script(CoreModules.Preset_SoftSandbox);

        RegisterApi();

        try
        {
            _script.DoString(source);
        }
        catch (SyntaxErrorException ex)
        {
            LastError = $"Syntax error: {ex.DecoratedMessage}";
            Console.WriteLine($"[LUA SYNTAX] {LastError}");
            return;
        }

        _initFn = GetOptionalFn("_init");
        _updateFn = GetOptionalFn("_update");
        _drawFn = GetOptionalFn("_draw");

        SafeCall(_initFn);
    }

    public void Update() => SafeCall(_updateFn);

    public void Draw() => SafeCall(_drawFn);

    // ── Private helpers ───────────────────────────────────────

    private void RegisterApi()
    {
        // Drawing
        _script.Globals["cls"] = (Action<int>)_draw.Cls;
        _script.Globals["pset"] = (Action<int, int, int>)_draw.Pset;
        _script.Globals["pget"] = (Func<int, int, int>)_draw.Pget;
        _script.Globals["line"] = (Action<int, int, int, int, int>)_draw.Line;
        _script.Globals["rect"] = (Action<int, int, int, int, int>)_draw.Rect;
        _script.Globals["rectfill"] = (Action<int, int, int, int, int>)_draw.Rectfill;
        _script.Globals["circ"] = (Action<int, int, int, int>)_draw.Circ;
        _script.Globals["circfill"] = (Action<int, int, int, int>)_draw.Circfill;

        // Text
        _script.Globals["print"] = (Action<string, int, int, int>)_text.Print;

        // Input
        _script.Globals["btn"] = (Func<int, bool>)_input.Btn;
        _script.Globals["btnp"] = (Func<int, bool>)_input.Btnp;

        // Math helpers (PICO-8 convention: sin/cos use turns, not radians)
        _script.Globals["flr"] = (Func<double, int>)(x => (int)Math.Floor(x));
        _script.Globals["ceil"] = (Func<double, int>)(x => (int)Math.Ceiling(x));
        _script.Globals["abs"] = (Func<double, double>)Math.Abs;
        _script.Globals["min"] = (Func<double, double, double>)Math.Min;
        _script.Globals["max"] = (Func<double, double, double>)Math.Max;
        _script.Globals["rnd"] = (Func<double, double>)(n => Random.Shared.NextDouble() * n);
        _script.Globals["sin"] = (Func<double, double>)(x => Math.Sin(x * Math.PI * 2.0));
        _script.Globals["cos"] = (Func<double, double>)(x => Math.Cos(x * Math.PI * 2.0));
        _script.Globals["sqrt"] = (Func<double, double>)Math.Sqrt;
    }

    private DynValue? GetOptionalFn(string name)
    {
        DynValue v = _script.Globals.Get(name);
        return v.Type == DataType.Function ? v : null;
    }

    private void SafeCall(DynValue? fn)
    {
        if (fn is null)
            return;
        LastError = null;

        try
        {
            _script.Call(fn);
        }
        catch (InterpreterException ex)
        {
            LastError = ex.DecoratedMessage;
            Console.WriteLine($"[LUA ERROR] {LastError}");
        }
    }
}
