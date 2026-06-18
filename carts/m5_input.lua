-- ============================================================
-- MILESTONE E: Input — btn() and btnp()
-- What to check:
--   * Dot moves left/right/up/down while arrow keys are HELD
--     (btn continuous movement)
--   * Press Z (A button) to change dot color — should only
--     register ONCE per press, not every frame (btnp edge)
--   * Press X (B button) to reset position to center
--   * Color indicator in corner shows which buttons are held
--   * The dot correctly clamps to screen boundaries
-- Button map: 0=left 1=right 2=up 3=down 4=Z(A) 5=X(B)
-- ============================================================

local px = 64
local py = 64
local color = 8
local colors = {8, 9, 10, 11, 12, 13, 14, 7}
local color_idx = 1

function _init()
  px = 64
  py = 64
  color_idx = 1
  color = colors[color_idx]
end

function _update()
  -- Continuous movement while held
  if btn(0) then px = px - 2 end
  if btn(1) then px = px + 2 end
  if btn(2) then py = py - 2 end
  if btn(3) then py = py + 2 end

  -- Clamp to screen bounds
  if px < 4  then px = 4  end
  if px > 255 then px = 255 end
  if py < 4  then py = 4  end
  if py > 123 then py = 123 end

  -- btnp: cycle color on single press of Z
  if btnp(4) then
    color_idx = color_idx + 1
    if color_idx > #colors then color_idx = 1 end
    color = colors[color_idx]
  end

  -- btnp: reset on single press of X
  if btnp(5) then
    px = 64
    py = 64
  end
end

function _draw()
  cls(1)

  -- Guide text
  print("ARROWS:MOVE", 4, 4, 7)
  print("Z:COLOR X:RESET", 4, 12, 5)

  -- Draw the moveable dot
  circfill(px, py, 4, color)

  -- Button state indicator strip along the bottom
  -- Shows which buttons are currently held
  local labels = {"L","R","U","D","A","B"}
  for i = 0, 5 do
    local bx = 4 + i * 20
    if btn(i) then
      rectfill(bx, 112, 16, 12, color)
      print(labels[i+1], bx + 4, 115, 0)
    else
      rect(bx, 112, 16, 12, 5)
      print(labels[i+1], bx + 4, 115, 5)
    end
  end
end
