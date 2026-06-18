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

using Raylib_cs;

namespace Retro_16.Core;

// The APIs to handle game input
public class InputState
{
    // Limit input ot 6 keys
    // pretty standard for retro consoles
    private readonly KeyboardKey[] keys =
    {
        KeyboardKey.Left,
        KeyboardKey.Right,
        KeyboardKey.Up,
        KeyboardKey.Down,
        KeyboardKey.Z,
        KeyboardKey.X,
    };

    // will hold boolean values for each key respectively
    // if _current[i] = true then keys[i] is pressed
    // using _current and _previous as parallel arrays helps to distinguish
    // wether a player is holding down a button or just tapped it for the first time
    private readonly bool[] _current = new bool[6];
    private readonly bool[] _previous = new bool[6];

    public void Update()
    {
        // values in _current are copied to _previous
        // then _current is updated
        // this means _previous holds past values
        // these past values then help to calculate changes
        // determining if a player is holding a button or just tapped it
        // is just comparing _current and _previous parallely
        Array.Copy(_current, _previous, _current.Length);

        for (int i = 0; i < keys.Length; i++)
        {
            _current[i] = Raylib.IsKeyDown(keys[i]);
        }
    }

    public bool Btn(int b)
    {
        if (b < 0 || b > _current.Length)
            return false;
        return _current[b];
    }

    public bool Btnp(int b)
    {
        if (b < 0 || b > _current.Length)
            return false;
        return _current[b] && !_previous[b];
    }
}
