# Retro-16 — Build Plan v2

---

## Build 0 — Core Console

The foundation everything runs on. Scope is intentionally minimal — no sprites, no sound, no editor.

### Build Plans

**Window & Framebuffer**
- Raylib-cs window with a fixed **128×128 pixel internal framebuffer**
- **Screen scaling**: render the 128×128 buffer to the window at an integer scale (3× or 4×); the framebuffer is always 128×128 internally regardless of window size
- **16-color fixed palette** — PICO-8's palette is a practical starting choice

**Bitmap Font**
- A baked-in monospace pixel font (4×6 or 5×7 px per glyph) stored as a hardcoded byte array
- Without this, `print()` cannot be implemented — this must exist before any Lua API work begins

**MoonSharp Lua Runtime**
- Embed via NuGet
- Bind all API functions below as Lua globals

**Drawing API** (exposed to Lua)

| Function | Description |
|----------|-------------|
| `cls(c)` | Clear screen to color c |
| `pset(x,y,c)` / `pget(x,y)` | Set / get individual pixel |
| `line(x0,y0,x1,y1,c)` | Draw a line |
| `rect(x,y,w,h,c)` | Rectangle outline |
| `rectfill(x,y,w,h,c)` | Filled rectangle |
| `circ(x,y,r,c)` | Circle outline |
| `circfill(x,y,r,c)` | Filled circle |
| `print(str,x,y,c)` | Draw text using bitmap font |

**Input API**

| Function | Description |
|----------|-------------|
| `btn(b)` | Returns true while button b is held |
| `btnp(b)` | Returns true on the frame button b was first pressed |

Buttons: 0=left, 1=right, 2=up, 3=down, 4=A, 5=B

**Game Loop**
- Fixed 30fps tick
- Calls `_init()` once on load, then `_update()` and `_draw()` every frame
- All three functions are optional — missing ones are silently skipped

**Cartridge Loading**
- Load a plain `.lua` file from a path passed as a command-line argument
- `FileSystemWatcher` detects changes on disk and hot-reloads (re-runs `_init()`)

### Success Metric
Write a `.lua` file, run the console, and see a bouncing ball drawn with `circfill()` with a score displayed using `print()`.

---

## Build 1 — Cartridge Format, Sprites & Tilemap 

Upgrades the cartridge format and adds the data layer for real games. No visual editors yet — sprite and map data is authored as hex strings directly in the cartridge file.

> **Do the cartridge format migration first.** Everything else in this build depends on it.

### Build Plans

**Cartridge Format Migration**
- Replace the plain `.lua` file with a `.cart` text file containing labelled sections
- Section format:
  ```
  [lua]
  -- your code here

  [sprites]
  -- hex color index data, 128 chars per row x 128 rows

  [map]
  -- tile indices, 128 values per row x 64 rows

  [sfx]
  -- reserved for Build 2
  ```
- The `[lua]` section contains identical code to before — existing scripts just need to be wrapped
- Write a section splitter that routes each block to the appropriate subsystem

**Sprites**
- **Sprite sheet**: 128×128 px grid of 256 sprites at 8×8 pixels each, stored as indexed color data
- **Transparency rule**: color index 0 is **always transparent** when rendering sprites — this is required for correct layering and cannot be changed per-sprite
- Sprite sheet is initialized as all-zero (fully transparent) at startup; populated from the `[sprites]` section

| Function | Description |
|----------|-------------|
| `spr(n,x,y)` | Draw sprite n at (x,y) |
| `spr(n,x,y,w,h,flipx,flipy)` | Extended: multi-tile span + horizontal/vertical flip |
| `sget(x,y)` / `sset(x,y,c)` | Raw pixel read/write on the sprite sheet *(required by the sprite editor in Build 3)* |
| `fget(n,f)` | Get boolean flag f (0–7) on sprite n |
| `fset(n,f,v)` | Set boolean flag f on sprite n *(used for tile-based collision categories)* |

Sprite flags are stored in the `[sprites]` section alongside pixel data.

**Tilemap**
- Fixed size: **128×64 tiles** (consistent across all builds — do not change this later)
- Each cell stores a sprite index (0–255)

| Function | Description |
|----------|-------------|
| `map(cx,cy,sx,sy,cw,ch)` | Draw a region of the tilemap to the screen |
| `mget(x,y)` / `mset(x,y,t)` | Read/write individual map tile *(required by the map editor in Build 4)* |
| `camera(x,y)` | Offset all subsequent drawing operations |

### Success Metric
Define sprite pixel data in `[sprites]`, lay out a level in `[map]`, and see a character sprite walk over a scrolling tiled map.

---

## Build 2 — Sound Engine

A minimal beeper-style audio system. SFX only — no music sequencer.

### Build Plans

**Audio Pipeline**
- Open a Raylib `AudioStream` on startup
- Synthesize audio in a background buffer at 44100 Hz
- Support 4 waveform types: **square, triangle, sawtooth, noise**

**SFX Format**
Each sound slot is a sequence of up to 32 notes. Each note defines:
- Pitch (frequency in Hz, or a note name like C4)
- Duration (in frames at 30fps)
- Volume (0–7)
- Waveform (square/triangle/sawtooth/noise)

**Sound API**

| Function | Description |
|----------|-------------|
| `sfx(n)` | Play sound slot n (0–31) |
| `sfx(-1)` | Stop all currently playing sounds |

**Cartridge Storage**
- 32 SFX slots in the `[sfx]` section
- 32 slots is sufficient for a basic console; each slot can be reused and triggered multiple times

### Success Metric
Trigger footstep sounds, a jump SFX, and a looping ambient tone from Lua.

---

## Build 3 — Editor Framework + Sprite Editor

The first visual tool. Split into two phases — **the editor shell must be stable before the sprite editor is built on top of it**, because the map editor and code editor in Builds 4 and 5 reuse the same shell.

### Phase A — Editor Framework

**Mode Switching**
- A single key (e.g., Escape) toggles between **Run mode** and **Editor mode**
- In Run mode: the framebuffer is active, Lua game loop runs, only `btn()`/`btnp()` input is processed
- In Editor mode: the game loop is paused, mouse + keyboard are routed to the active editor tab

**Tab/Toolbar System**
- A persistent toolbar rendered at the top or side of the editor view
- Tab buttons: **Sprite | Map | Sound | Code** (Sound and Code tabs are placeholders until Builds 4–5)
- Clicking a tab switches the active editor panel
- The toolbar must be implemented here — Builds 4 and 5 add their tabs to it, not build their own

**Canvas Scaling for Editing**
- The editor renders at the same 128×128 internal resolution, scaled up to the window
- Editor UI elements (palette, grid, toolbar) are drawn within this space using `rectfill()`, `print()`, and `pset()` — no external UI framework needed

### Phase B — Sprite Editor

**Edit Grid**
- The currently selected sprite displayed at **8× zoom** minimum (8×8 sprite → 64×64 display area)
- Each zoomed pixel is a clickable rectangle that paints with the active palette color

**Palette Strip**
- All 16 palette colors displayed as clickable swatches
- Active color is highlighted

**Sprite Sheet Browser**
- Thumbnail view of all 256 sprites from the sprite sheet
- Click any sprite to select it for editing
- Currently selected sprite is highlighted

**Tools**

| Tool | Behaviour |
|------|-----------|
| Pencil | Paint individual pixels |
| Fill bucket | Flood-fill a region with the active color |
| Eyedropper | Click a pixel to set active color to its value |
| Flip H / Flip V | Mirror the current sprite horizontally or vertically |

**Undo/Redo**
- 20-step history scoped to sprite editor operations
- Ctrl+Z / Ctrl+Y

**Save**
- Ctrl+S serializes the in-memory sprite sheet back to the `[sprites]` section of the `.cart` file

### Success Metric
Open the editor, draw a sprite pixel by pixel, Ctrl+S to save, switch to Run mode, and see it rendered via `spr()`.

---

## Build 4 — Map Editor

Straightforward — the editor framework from Build 3 handles all the scaffolding. The tile palette browser reuses the sprite sheet browser code.

### Build Plans

**Map Editor Tab**
- Accessible by clicking the **Map** tab on the editor toolbar from Build 3

**Tilemap Canvas**
- Renders the full 128×64 tile map (scrollable)
- Click a tile cell to stamp the active tile; click-drag to paint a region
- Current tile position shown in a status bar

**Tile Palette Panel**
- Shows all 256 sprites from the sprite sheet as selectable tiles
- Reuses the sprite sheet browser component from Build 3 — no need to rebuild it

**Tools**

| Tool | Behaviour |
|------|-----------|
| Stamp | Paint tile cells with the active tile |
| Eraser | Set tile cells to index 0 (empty/transparent) |
| Fill | Flood-fill a region with the active tile |

**Pan/Scroll**
- Arrow keys or click-drag on an empty area to scroll the map canvas

**Save**
- Ctrl+S serializes the map data back to the `[map]` section of the `.cart` file

### Success Metric
Design a full game level in the map editor, switch to Run mode, and have it render correctly with `map()` and scroll with `camera()`.

---

## Build 5 — Code Editor 

A basic in-console text editor. Intentionally minimal — external editors (VS Code, Sublime, etc.) remain a fully valid workflow and often preferred for larger projects. This editor exists for convenience when making small in-session edits.

### Build Plans

**Code Editor Tab**
- Accessible via the **Code** tab on the editor toolbar

**Text Buffer**
- A `List<string>` of lines representing the Lua source
- Loaded from the `[lua]` section of the `.cart` file when entering editor mode

**Cursor & Navigation**

| Key | Action |
|-----|--------|
| Arrow keys | Move cursor |
| Home / End | Jump to start / end of line |
| Ctrl+Home / Ctrl+End | Jump to first / last line |
| PgUp / PgDn | Scroll by visible page |

**Basic Editing**
- Typing inserts characters at cursor position
- Backspace / Delete remove characters
- Enter inserts a new line

**Display**
- Rendered using the built-in bitmap font from Build 0
- Fixed-width glyphs; line numbers in a left gutter

**Lua Syntax Highlighting**
- Single-pass tokenizer colours: keywords, strings, comments, numbers
- Uses distinct colors from the 16-color palette
- Does not need to be perfect — approximate highlighting is fine

**Error Display**
- When `_update()` or `_draw()` throws a Lua runtime error, the Run view displays the error message and line number as an overlay

**Hotkeys**

| Key | Action |
|-----|--------|
| Ctrl+R | Save code, hot-compile, and restart the cartridge |
| Ctrl+S | Save code to the `[lua]` section of the `.cart` file |

> **No undo/redo** — keeping scope tight. Use an external editor for anything complex enough to need undo.

### Success Metric
Write a small Lua program entirely inside the console editor, press Ctrl+R, and see it run.

---

## Full Timeline Summary

| Build | Feature | Time | Cumulative |
|-------|---------|------|------------|
| **0** | Core console, Lua runtime, drawing API, input, hot-reload | 3–5 weeks | ~5 weeks |
| **1** | Cartridge format, sprites, tilemap, camera | 3–4 weeks | ~9 weeks |
| **2** | Sound engine (32 SFX slots, 4 waveforms) | 2–3 weeks | ~12 weeks |
| **3** | Editor framework + sprite editor | 4–5 weeks | ~17 weeks |
| **4** | Map editor | 2–3 weeks | ~20 weeks |
| **5** | Code editor | 3–4 weeks | ~24 weeks |

**Total: approximately 20–24 weeks focused, or 8–10 months part-time.**

---

## Dependency Map

The following components must exist before the next build can start:

```
Build 0
  └─ Bitmap font        ──► required by print() in Build 0
  └─ Framebuffer        ──► required by all drawing in Builds 1–5
  └─ Lua runtime        ──► required by all API calls in Builds 1–5

Build 1
  └─ Cartridge format   ──► required by [sfx] in Build 2 and all editors in Builds 3–5
  └─ sget()/sset()      ──► required by sprite editor pixel painting in Build 3
  └─ mget()/mset()      ──► required by map editor tile stamping in Build 4

Build 2
  └─ [sfx] section      ──► required by sound editor tab (if added later)

Build 3 — Phase A (Editor Framework)
  └─ Tab/toolbar shell  ──► required by map tab in Build 4 and code tab in Build 5
  └─ Mode switch        ──► required by all editors
  └─ Sprite sheet browser ► required by tile palette in Build 4 (reuse, not rebuild)
```
