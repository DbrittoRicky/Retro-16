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

namespace Retro_16.Core;

public class ConsoleConfig
{
    // Console "Hardware Specs"
    public const int ScreenWidth = 256;
    public const int ScreenHeight = 128;

    public const int Scale = 5; // 128x128 -> 640x640
    public const int TargetFps = 30;

    public const int WindowWidth = ScreenWidth * Scale;
    public const int WindowHeight = ScreenHeight * Scale;

    // 16-color palette
    public static readonly (byte R, byte G, byte B)[] Palette = new[]
    {
        ((byte)0, (byte)0, (byte)0),
        ((byte)29, (byte)43, (byte)83),
        ((byte)126, (byte)37, (byte)83),
        ((byte)0, (byte)135, (byte)81),
        ((byte)171, (byte)82, (byte)54),
        ((byte)95, (byte)87, (byte)79),
        ((byte)194, (byte)195, (byte)199),
        ((byte)255, (byte)241, (byte)232),
        ((byte)255, (byte)0, (byte)77),
        ((byte)255, (byte)163, (byte)0),
        ((byte)255, (byte)236, (byte)39),
        ((byte)0, (byte)228, (byte)54),
        ((byte)41, (byte)173, (byte)255),
        ((byte)131, (byte)118, (byte)156),
        ((byte)255, (byte)119, (byte)168),
        ((byte)255, (byte)204, (byte)170),
    };
}
