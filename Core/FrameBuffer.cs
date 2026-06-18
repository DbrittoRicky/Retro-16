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

using System.Numerics;
using Raylib_cs;

namespace Retro_16.Core;

// The rendering pipeline for the console
// Runs Every frame in 4 stages
// 1. Draw pixels using the 1 byte palette numbers (_pixels)
// 2. Convert those bytes into actual 32-bit colors (_colorbuffer)
// 3. Push those colors from RAM to GPU memeory (_texture) -> The GPU uses texture data which is why we 'LoadTextureFromImage'
// 4. Scale the texure and present the final image on the screen (DrawToWindow)
//
// Upto the 3rd phase the GPU will hold a perfect low res texture of the game at that specific frame
// The 4th phase Scales that texture to yeild a higher resolution.

public class FrameBuffer : IDisposable
{
    private readonly byte[] _pixels;
    private readonly Color[] _colorBuffer;
    private Image _image;
    private Texture2D _texture;

    public int Width => ConsoleConfig.ScreenWidth;
    public int Height => ConsoleConfig.ScreenHeight;

    public FrameBuffer()
    {
        // Set the array size for _pixels and _colorBuffer -> screen resolution
        _pixels = new byte[Width * Height];
        _colorBuffer = new Color[Width * Height];

        _image = Raylib.GenImageColor(Width, Height, Color.Black);
        _texture = Raylib.LoadTextureFromImage(_image);
        Raylib.UnloadImage(_image);

        Raylib.SetTextureFilter(_texture, TextureFilter.Point);
    }

    // Fill the screen with a single Color
    public void Clear(byte color = 0)
    {
        Array.Fill(_pixels, (byte)(color & 0x0F));
    }

    public void SetPixel(int x, int y, int color)
    {
        // uint saves the '<0' check for negative coordinates
        // negative numbers cause integer overflow in uint (Basically get converted to a very large number, typically in billions)
        if ((uint)x >= Width || (uint)y >= Height)
            return;

        // since the screen is represented as a 1D array, the way to calculate exact pixel position is y*Width + x
        // y*Width -> Skips y rows of width = Width
        // + x -> moves to the exact column on the current row
        // The final result is an index in the array that represents a pixel at (x, y) location on the screen
        _pixels[y * Width + x] = (byte)(color & 0x0F);
    }

    public byte GetPixel(int x, int y)
    {
        if ((uint)x >= Width || (uint)y >= Height)
            return 0;
        return _pixels[y * Width + x];
    }

    // Takes the indexed(4-bit) colors, translates them to 32-bit colors and uploads to screen
    public unsafe void Upload()
    {
        for (int i = 0; i <= _pixels.Length - 1; i++)
        {
            // Palette is defined as an array of tuples(byte r, b, g)
            var (r, g, b) = ConsoleConfig.Palette[_pixels[i]];

            _colorBuffer[i] = new Color(r, g, b, (byte)255); // convert the tuple values into a Color
        }

        // only need a ptr to the first element since array elements are stored sequentially in memory
        fixed (Color* ptr = _colorBuffer) // fixed in memory -> the grabage collector will not move the array
        {
            Raylib.UpdateTexture(_texture, ptr); // Update the texture which is then to be drawn on the screen
        }
    }

    // draw the texture on the window(scaled width and height)
    public void DrawToWindow()
    {
        Raylib.DrawTexturePro(
            _texture,
            new Rectangle(0, 0, Width, Height), // What part of the texture to sample from
            new Rectangle(0, 0, ConsoleConfig.WindowWidth, ConsoleConfig.WindowHeight), // Where to place that sample on the physical screen
            Vector2.Zero,
            0f,
            Color.White
        );
    }

    // Manual garbage cleanup
    public void Dispose()
    {
        Raylib.UnloadTexture(_texture);
    }
}
