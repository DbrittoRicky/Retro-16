-- ============================================================
-- MILESTONE D: Lua runtime binding
-- What to check:
--   * _init() runs exactly once on load
--   * _update() runs every frame (counter increments)
--   * _draw() runs every frame and reads state set by _update
--   * All drawing API globals are callable from Lua:
--     cls, pset, pget, line, rect, rectfill, circ, circfill
--   * Math helpers work: flr, ceil, abs, sin, cos, rnd, min, max
--   * A Lua runtime error (uncomment line below) should NOT
--     crash the app — it should print to the terminal and show
--     an error overlay on screen, then continue next frame
-- ============================================================

local frame = 0
local x = 64

function _init()
  frame = 0
  x = 10
end

function _update()
  frame = frame + 1
  -- Move x using sin wave so we can verify sin() works
  x = 64 + flr(sin(frame / 60) * 40)
end

function _draw()
  cls(0)

  -- Draw a sine-wave oscillating circle to verify math helpers
  circfill(x, 64, 5, 8)

  -- Show frame counter to confirm _update is running
  print("FRAME:" .. frame, 4, 4, 7)

  -- Show current x to confirm sin() result flows through
  print("X=" .. x, 4, 14, 10)

  -- Draw a rectfill that uses flr()
  local w = flr(abs(sin(frame / 40)) * 40) + 4
  rectfill(4, 100, w, 8, 12)

  -- Uncomment to test error handling (should not crash):
  -- error("intentional test error")
end
