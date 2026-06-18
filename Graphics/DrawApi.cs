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

// low level primitives

public sealed class DrawApi
{
    private readonly FrameBuffer _fb;

    public DrawApi(FrameBuffer fb)
    {
        _fb = fb;
    }

    public void Cls(int c = 0) => _fb.Clear((byte)c);

    public void Pset(int x, int y, int color) => _fb.SetPixel(x, y, color);

    public int Pget(int x, int y) => _fb.GetPixel(x, y);

    // Draw a Line
    public void Line(int x0, int y0, int x1, int y1, int c)
    {
        //basic calculation to draw a line, pretty standard
        int dx = Math.Abs(x1 - x0);
        int sx = x0 < x1 ? 1 : -1; // stepping direction -> +1 = move to the right | -1 = move to the left
        int dy = -Math.Abs(y1 - y0);
        int sy = y0 < y1 ? 1 : -1; // +1 = move up | -1 = move down

        // The 'screen' is essentially grid of pixels, so drawing a perfectly straight line is not possible
        // The algorithm chooses the best set of discrete pixels that approximate that line.
        // this is where 'err' comes into the picture
        // err -> measure how far the pixel path has drifted away from the desired path
        // But since we set dy to negative as a balancing trick; err becomes a tug-of-war between dx and dy
        // if err > 0 -> dx > dy -> the line is shallow -> more horizontal steps needed
        // if err < 0 -> dx < dy -> the line is steep -> more vertical steps needed
        int err = dx + dy;

        while (true)
        {
            Pset(x0, y0, c);
            if (x0 == x1 && y0 == y1)
                break;

            // ideally, you want to change rows when the line crosses 0.5 mark
            // but that introduces floating point calculations which are slower
            // standard check would be err >= dy/2 -> floating numbers, slow.
            // instead we do 2 * err >= dy -> only integer, faster.
            int e2 = 2 * err;

            // these two conditions decide whether to step horizontally, vertically or diagonally
            if (e2 >= dy) // horizontal step required
            {
                err += dy;
                x0 += sx;
            }

            if (e2 <= dx) // vertical step required
            {
                err += dx;
                y0 += sy;
            }

            // if both conditons are met, a diagonal step is performed
        }
    }

    public void Rect(int x, int y, int w, int h, int c)
    {
        // use the Line function to draw 4 lines that form a rectangle

        Line(x, y, x + w - 1, y, c);
        Line(x + w - 1, y, x + w - 1, y + h - 1, c);
        Line(x + w - 1, y + h - 1, x, y + h - 1, c);
        Line(x, y + h - 1, x, y, c);
    }

    // Draw a rectangle filled with color
    // So, instead of just 4 lines, set every pixel that forms the rectangle
    public void Rectfill(int x, int y, int w, int h, int c)
    {
        for (int yy = y; yy < y + h; yy++)
        {
            for (int xx = x; xx < x + w; xx++)
            {
                Pset(xx, yy, c);
            }
        }
    }

    public void Circ(int cx, int cy, int r, int c)
    {
        int x = r;
        int y = 0;

        // err tracks whether the midpoint between two pixels lie inside or outside of the circle boundary
        int err = 0;

        while (x >= y)
        {
            // A circle is symmetric
            // We break it into 8 mirrored parts
            // Hence, 8 Pset()
            Pset(cx + x, cy + y, c);
            Pset(cx + y, cy + x, c);
            Pset(cx + x, cy - y, c);
            Pset(cx + y, cy - x, c);
            Pset(cx - x, cy + y, c);
            Pset(cx - y, cy + x, c);
            Pset(cx - x, cy - y, c);
            Pset(cx - y, cy - x, c);

            y++;

            // A circle is represented by x^2 + y^2 = r^2
            // since we step y in every iteration, the change must be added to err
            // since y -> y+1
            // change = (y+1)^2 + y^2 -> (y^2 + 2y + 1) - y^2 -> 2y + 1
            // so that change is added to err for path correction
            if (err <= 0)
            {
                err += 2 * y + 1;
            }

            if (err > 0)
            {
                // same as the change in y
                // change -> (x-1)^2 - x^2 -> (x^2 -2x + 1) - x^2 -> -2x + 1
                x--;
                err -= 2 * x + 1;
            }

            // Refer Midpoint Circle Algorithm
        }
    }

    public void Circfill(int cx, int cy, int r, int c)
    {
        for (int y = -r; y <= r; y++)
        for (int x = -r; x <= r; x++)
            if (x * x + y * y <= r * r)
                Pset(cx + x, cy + y, c);
    }
}
