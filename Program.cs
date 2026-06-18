using Raylib_cs;
using Retro_16.Core;
using Retro_16.Graphics;
using Retro_16.Scripting;

namespace Retro_16;

class Program
{
    public static int windowWidth = ConsoleConfig.WindowWidth;
    public static int windowHeight = ConsoleConfig.WindowHeight;
    public static int targetFPS = ConsoleConfig.TargetFps;

    static void Main(string[] args)
    {
        string cartPath = args.Length > 0 ? args[0] : Path.Combine("carts", "m_final.lua");

        if (!File.Exists(cartPath))
        {
            Console.WriteLine($"[ERROR] Cart not found: {cartPath}");
            Console.WriteLine("Available milestone carts:");
            Console.WriteLine("  carts/m1_framebuffer.lua");
            Console.WriteLine("  carts/m2_drawing.lua");
            Console.WriteLine("  carts/m3_font.lua");
            Console.WriteLine("  carts/m4_lua.lua");
            Console.WriteLine("  carts/m5_input.lua");
            Console.WriteLine("  carts/m6_hotreload.lua");
            Console.WriteLine("  carts/m_final.lua");
            return;
        }

        Console.WriteLine($"[FC] Starting with cart: {cartPath}");

        // ── 2. Open Raylib window ────────────────────────────────────
        Raylib.InitWindow(windowWidth, windowHeight, $"Retro-16 — {Path.GetFileName(cartPath)}");
        Raylib.SetTargetFPS(targetFPS);

        // ── 3. Build subsystems ──────────────────────────────────────
        using var framebuffer = new FrameBuffer(); // VRAM
        var draw = new DrawApi(framebuffer); // GPU instructions
        var text = new TextApi(framebuffer); // GPU instructions
        var input = new InputState(); // controller ports
        var lua = new LuaHost(draw, text, input); // CPU

        // ── 4. Set up hot-reload watcher ─────────────────────────────
        using var watcher = new CartridgeWatcher(cartPath); // The Game slot

        // ── 5. Helper: load (or reload) the current cart ─────────────
        void LoadCart()
        {
            try
            {
                string source = watcher.ReadSource();
                lua.Load(source);
                Console.WriteLine($"[FC] Loaded: {cartPath}");
            }
            catch (IOException ex)
            {
                // File may be briefly locked while editor is saving
                Console.WriteLine($"[FC] Read error (will retry): {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FC] Load failed: {ex.Message}");
            }
        }

        // ── 6. Initial load ──────────────────────────────────────────
        LoadCart();

        // ── 7. Main loop ─────────────────────────────────────────────
        while (!Raylib.WindowShouldClose())
        {
            // — Hot-reload check —
            if (watcher.ReloadPending)
            {
                // Small sleep so the editor has fully flushed the file
                Thread.Sleep(40);
                watcher.Acknowledge();
                LoadCart();
            }

            // — Update —
            input.Update();
            lua.Update();

            // — Draw —
            // Clear the framebuffer BEFORE calling Lua's _draw(),
            // so carts that skip cls() still start from a blank slate.
            framebuffer.Clear(0);
            lua.Draw();

            // Upload the CPU pixel buffer to the GPU texture
            framebuffer.Upload();

            // Raylib render pass: draw the scaled texture to the window
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);
            framebuffer.DrawToWindow();

            // If the Lua cart threw a runtime error, show a small
            // on-screen indicator. The full message prints to terminal.
            if (lua.HasError)
            {
                // Draw a red error bar along the top 8 pixels
                Raylib.DrawRectangle(0, 0, windowWidth, windowHeight, new Color(180, 0, 0, 200));
                Raylib.DrawText("LUA ERROR — see terminal", 10, 4, 16, Color.White);
            }

            Raylib.EndDrawing();
        }

        // ── 8. Cleanup ───────────────────────────────────────────────
        Raylib.CloseWindow();
    }

    public static void ReloadCart(string cartPath, LuaHost lua, CartridgeWatcher watcher)
    {
        try
        {
            string source = watcher.ReadSource();
            lua.Load(source);
            Console.WriteLine($"Reloaded: {cartPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Reload Failed: {ex.Message}");
        }
    }
}
