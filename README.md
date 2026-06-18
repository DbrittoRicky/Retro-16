# Retro-16

A fantasy console built from scratch in C#. It is a self-contained retro game runtime with a fixed 256×128 pixel screen, a 16-color palette, and a Lua scripting API — inspired by PICO-8 and TIC-80. Cartridges are plain `.lua` files (Build 0) that define three lifecycle functions: `_init`, `_update`, and `_draw`. The console runs them at a locked 30 FPS and hot-reloads them whenever the file changes on disk.

The project is built incrementally. **Build 0**, documented here, delivers the core runtime: window, framebuffer, drawing primitives, bitmap font, input, Lua bindings, and hot-reload. No sprites, no sound, no editor yet — those come in later builds.

---

## Tech Stack

| Layer | Technology | Role |
|---|---|---|
| Language | C# (.NET 10) | Host runtime |
| Windowing & rendering | [Raylib-cs](https://github.com/raylib-cs/raylib-cs) | Window creation, GPU texture upload, scaled display |
| Scripting | [MoonSharp](https://www.moonsharp.org/) | Embedded Lua 5.2 interpreter |
| Font | Hardcoded byte arrays | Baked-in 5×7 monospace bitmap font, no external files |
| Build | .NET CLI (`dotnet run`) | No build tools beyond the standard SDK |

---

## Project Structure

```
FantasyConsole/
├── Program.cs                  — main entry point and game loop
├── carts/                      — cartridge files (.lua)
│   └── test.lua
└── src/
    ├── Core/
    │   ├── ConsoleConfig.cs     — fixed hardware constants (resolution, scale, fps)
    │   ├── Framebuffer.cs      — pixel buffer + GPU texture upload
    │   ├── InputState.cs       — 6-button keyboard state (current + previous frame)
    │   └── CartridgeWatcher.cs — FileSystemWatcher for hot-reload
    ├── Graphics/
    │   ├── DrawApi.cs          — cls, pset, pget, line, rect, rectfill, circ, circfill
    │   ├── BitmapFont.cs       — hardcoded 5×7 glyph data for printable ASCII
    │   └── TextApi.cs          — print() renderer using BitmapFont
    └── Scripting/
        └── LuaHost.cs          — MoonSharp host, API binding, lifecycle call dispatch
```

---

## Getting Started

**Prerequisites:** .NET 10 SDK

```bash
git clone <repo-url>
cd FantasyConsole
dotnet run -- carts/test.lua
```

The window opens at 1280×640 (256×128 internal resolution scaled 5×). Edit `carts/test.lua` in any editor, save it, and the console reloads automatically without restarting.

### Writing a Cartridge

A cartridge is a plain `.lua` file. All three lifecycle functions are optional.

```lua
function _init()
  -- runs once on load and on every hot-reload
  x = 64
end

function _update()
  -- runs every frame before _draw
  if btn(0) then x = x - 1 end
  if btn(1) then x = x + 1 end
end

function _draw()
  -- runs every frame; framebuffer is already cleared to 0 before this
  cls(1)
  circfill(x, 64, 6, 8)
  print("HELLO", 4, 4, 7)
end
```

---

## Lua API Reference (Build 0)

### Drawing

| Function | Description |
|---|---|
| `cls(c)` | Clear the entire screen to palette color `c` (default 0) |
| `pset(x, y, c)` | Set pixel at `(x, y)` to palette color `c` |
| `pget(x, y)` | Return the palette index stored at `(x, y)` |
| `line(x0, y0, x1, y1, c)` | Draw a line using Bresenham's algorithm |
| `rect(x, y, w, h, c)` | Draw a hollow rectangle outline |
| `rectfill(x, y, w, h, c)` | Draw a solid filled rectangle |
| `circ(x, y, r, c)` | Draw a hollow circle outline |
| `circfill(x, y, r, c)` | Draw a solid filled disc |
| `print(str, x, y, c)` | Draw text using the built-in bitmap font |

### Input

| Function | Description |
|---|---|
| `btn(b)` | `true` every frame button `b` is held |
| `btnp(b)` | `true` only on the frame button `b` was first pressed |

**Button map:** `0` = Left, `1` = Right, `2` = Up, `3` = Down, `4` = Z (A), `5` = X (B)

### Math Helpers

| Function | Description |
|---|---|
| `flr(n)` | Floor to integer |
| `ceil(n)` | Ceiling to integer |
| `abs(n)` | Absolute value |
| `min(a, b)` / `max(a, b)` | Minimum / maximum |
| `rnd(n)` | Random float in `[0, n)` |
| `sin(t)` / `cos(t)` | Sine / cosine — input in **turns** (0.0–1.0), not radians |
| `sqrt(n)` | Square root |

> `sin` and `cos` use turns, not radians, matching PICO-8 convention. One full rotation = `1.0`, so `sin(0.25)` = 1.0.

### Palette

16 fixed colors, index 0–15 (PICO-8 palette):

| Index | Color | Index | Color |
|---|---|---|---|
| 0 | Black | 8 | Red |
| 1 | Dark Blue | 9 | Orange |
| 2 | Dark Purple | 10 | Yellow |
| 3 | Dark Green | 11 | Green |
| 4 | Brown | 12 | Blue |
| 5 | Dark Grey | 13 | Lavender |
| 6 | Light Grey | 14 | Pink |
| 7 | White | 15 | Peach |

> Color index `0` will be treated as **transparent** starting in Build 1 when sprites are introduced. Avoid using index 0 for anything other than "empty" in sprite data.

---

## How the Core System Works

### System Layers

```
.lua Cartridge
      │  source text
      ▼
   LuaHost  ◄──── CartridgeWatcher (hot-reload flag)
      │  calls C# delegates
      ├──────────────────────┐
      ▼                      ▼
   DrawApi               TextApi
   (primitives)          (print via BitmapFont)
      │                      │
      └──────────┬───────────┘
                 │  SetPixel()
                 ▼
           Framebuffer
           (_indices[] byte array)
                 │  reads
                 ▼
        ConsoleSpecs + Palette
        (pure constants)
```

`InputState` sits alongside `DrawApi` and `TextApi` — also wired into `LuaHost`, but reads from Raylib keyboard state rather than writing to the framebuffer.

---

### Full Frame Pipeline

#### Phase 1 — Startup (once)

```
Raylib.InitWindow(640, 640)
│
├── Framebuffer()
│     allocate byte[16384]     ← _indices, palette index per pixel
│     allocate Color[16384]    ← _upload, resolved RGBA per pixel
│     GenImageColor → GPU texture (128×128, point filter)
│
├── DrawApi(framebuffer)       ← drawing primitives
├── TextApi(framebuffer)       ← print() via BitmapFont glyphs
├── InputState()               ← bool[6] current + bool[6] previous
├── LuaHost(draw, text, input) ← MoonSharp, API not yet registered
└── CartridgeWatcher(path)     ← FileSystemWatcher on the .lua file
```

#### Phase 2 — Load (on startup and every hot-reload)

```
Read .lua source from disk
│
├── Create fresh MoonSharp Script(SoftSandbox)
│     (full state reset — no memory leak from prior load)
│
├── Register C# delegates as Lua globals
│     cls, pset, pget, line, rect, rectfill, circ, circfill
│     print, btn, btnp
│     flr, ceil, abs, min, max, rnd, sin, cos, sqrt
│
├── DoString(source)           ← parse + execute top-level Lua code
│
├── Look up _init, _update, _draw as DynValue?
│     (null if the cart does not define them — silently skipped)
│
└── Call _init() if present
```

#### Phase 3 — 30 FPS Loop (runs until window closes)

```
┌─ Every frame ───────────────────────────────────────────────────┐
│                                                                 │
│  1. Hot-reload check                                            │
│       CartridgeWatcher.ReloadPending?                           │
│           Thread.Sleep(40ms)  ← wait for editor to finish       │
│           Acknowledge()       ← clear the flag                  │
│           → Phase 2 again (fresh Lua state, _init re-runs)      │
│                                                                 │
│  2. InputState.Update()                                         │
│       copy _current[] → _previous[]                             │
│       poll Raylib.IsKeyDown() for all 6 keys → _current[]       │
│       btn()  reads _current[]                                   │
│       btnp() compares _current[] vs _previous[] (edge detect)   │
│                                                                 │
│  3. LuaHost.Update()                                            │
│       calls _update() if defined                                │
│       _update() may call btn/btnp, mutate Lua variables         │
│       runtime errors → stored in LastError, never thrown        │
│                                                                 │
│  4. Framebuffer.Clear(0)                                        │
│       fills _indices[] with 0 unconditionally                   │
│       cls() inside Lua overrides the clear color, it does       │
│       not replace this step                                     │
│                                                                 │
│  5. LuaHost.Draw()                                              │
│       calls _draw() if defined                                  │
│       _draw() calls cls / pset / line / circfill / print etc.   │
│       each call → DrawApi or TextApi → Framebuffer.SetPixel()   │
│       SetPixel writes a palette index (0–15) into _indices[]    │
│       nothing touches the GPU during this step                  │
│                                                                 │
│  6. Framebuffer.Upload()                                        │ 
│       for each of 32768 pixels:                                 │
│           _upload[i] = Palette.Colors[_indices[i]]              │
│       unsafe fixed pin on _upload[]                             │
│       Raylib.UpdateTexture(texture, ptr)  ← one GPU transfer    │
│                                                                 │
│  7. Raylib.BeginDrawing()                                       │
│       ClearBackground(Black)                                    │
│       DrawTexturePro(                                           │
│           source: 256×128,                                      │
│           dest:   1280×640,        ← 5× integer scale           │
│           filter: Point           ← nearest-neighbour, no blur  │
│       )                                                         │
│       if LuaHost.HasError → draw red error bar + terminal msg   │
│       Raylib.EndDrawing()  ← swap buffers, display frame        │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Key Design Decisions

**Lua never touches the GPU.** Every drawing call from Lua only writes bytes into `_indices[]`. The GPU is only updated once per frame in step 6. This means Lua code cannot put the renderer into a broken state.

**The framebuffer stores indices, not colors.** `_indices[]` holds values 0–15. The translation to RGBA happens once per frame in `Upload()`. This makes palette operations in future builds free — changing `ConsoleConfig.Palette[]` recolors the whole screen on the next frame without touching any pixel data.

**Hot-reload creates a completely fresh Lua state.** Calling `Load()` again discards the entire prior `Script` instance. There is no state leak between reloads — `_init()` always starts clean.

**Integer scale is enforced at the constant level.** `ConsoleSpecs.Scale` is a compile-time integer, and the window size is derived from it (`Width × Scale`). There is no runtime scaling logic — the GPU texture is always drawn into an exact pixel-aligned rectangle.

---

For future builds refer to `Fantasy_Console_Build_Plan_v2.md`

---

**AI Disclaimer:** I am not well versed with these types of projects, so most of the code is given by AI. However the files are filled with extensive comments explaining what I understood from the code.

---

## License

This project is licensed under the [GNU General Public License v3.0](COPYING).
