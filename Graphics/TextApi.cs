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

using Retro_16.Core;

namespace Retro_16.Graphics;

public class TextApi
{
    private readonly FrameBuffer _fb;
    private static readonly byte[] missingKeyFallback = BitmapFont.Glyphs[' '];

    public TextApi(FrameBuffer fb)
    {
        _fb = fb;
    }

    public void Print(string text, int x, int y, int color)
    {
        int cursorX = x;

        foreach (char raw in text)
        {
            // convert every character in the string to Uppercase
            // For now the Dictionary only has uppercase characters
            char ch = char.ToUpperInvariant(raw);

            // if a text contains a character that is not present in the Dictionary
            // replace it with a blank space
            if (!BitmapFont.Glyphs.TryGetValue(ch, out byte[]? rows))
            {
                rows = missingKeyFallback;
            }

            // the main iteration loop
            // iterate through every column for every row
            for (int row = 0; row < BitmapFont.GlyphHeight; row++)
            {
                // the 'out' keyword used in 'TryGetValue' pushes 'rows' outside of the 'if' statement scope
                // this is done under the hood by C#
                byte bits = rows[row];

                for (int col = 0; col < BitmapFont.GlyphWidth; col++)
                {
                    // Screen pixels count from left to right, but binary bits are shifted from right to left
                    // So, 'mask' technically acts as an inverter
                    // It flips the reading direction of the byte
                    // not using this mask would cause the computer to read the rightmost bit
                    // and draw it on the leftmost side of the screen
                    // meaning characters would appear mirrored
                    int mask = 1 << (BitmapFont.GlyphWidth - 1 - col);

                    // mask is essentially a '1' bit shifted to represent a column
                    // every iteration '1' shifts -> 10000 -> 01000 -> 00100 -> so on
                    // this if statement performs an 'and' operation between the bits we want to draw and the mask
                    // this eliminates any bits that are not of the current column
                    // eg -> bits = 10001 (the side walls of character 'A'), mask = 10000
                    // bits & mask = 10001 & 10000 -> the bits 'and'ed with the '0' bits of mask all become '0'
                    // the coordinates of the remaining '1' bit is taken to set pixel.
                    if ((bits & mask) != 0)
                    {
                        _fb.SetPixel(cursorX + col, y + row, color);
                    }
                }
            }
            cursorX += BitmapFont.Advance;
        }
    }
}
