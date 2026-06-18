-- ============================================================
-- MILESTONE FINAL: Full Build 0 validation
-- What to check (all at once):
--   * All drawing primitives working
--   * print() rendering correctly
--   * btn() for continuous movement
--   * btnp() for single-press events
--   * Sine wave math helper working
--   * Score counter incrementing correctly
--   * Bouncing ball stays in bounds
--   * Hot-reload: edit the background color and save
-- ============================================================

local bx = 64
local by = 64
local bdx = 1.5
local bdy = 1.2
local score = 0
local px = 64
local py = 100

function _init()
  bx = 64
  by = 30
  bdx = 1.5
  bdy = 1.2
  score = 0
  px = 64
  py = 100
end

function _update()
  -- Bounce ball
  bx = bx + bdx
  by = by + bdy
  if bx < 4 or bx > 255 then bdx = -bdx  score = score + 1 end
  if by < 4 or by > 80  then bdy = -bdy  score = score + 1 end

  -- Move player with arrow keys
  if btn(0) then px = px - 2 end
  if btn(1) then px = px + 2 end
  if px < 4  then px = 4  end
  if px > 245 then px = 245 end

  -- Press Z to reset score
  if btnp(4) then score = 0 end
end

function _draw()
  cls(1)

  -- Play field border
  rect(0, 0, 256, 86, 3)

  -- Divider
  line(0, 86, 256, 86, 5)

  -- Bouncing ball
  circfill(flr(bx), flr(by), 4, 8)

  -- Player paddle
  rectfill(px - 10, 80, 20, 4, 11)

  -- HUD
  print("SCORE:" .. score, 4, 90, 7)
  print("LEFT RIGHT:MOVE", 4, 100, 5)
  print("Z:RESET SCORE", 4, 110, 5)
end
