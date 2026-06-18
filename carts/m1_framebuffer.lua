-- ============================================================
-- MILESTONE A: Framebuffer + Pixel-perfect scaling
-- What to check:
--   * Window opens at 640x640 (128x128 x5 scale)
--   * Background fills with a solid dark blue (palette color 1)
--   * A 2x2 block of red pixels (color 8) sits at (10,10)
--   * A 2x2 block of green (color 11) sits at (116,116)
--   * No blurring — pixels must be hard-edged squares
--   * pget() reads back correctly (shown via colored dot at center)
-- ============================================================

function _init()
end

function _draw()
  -- Fill entire screen with dark blue (palette index 1)
  cls(1)

  -- Top-left corner marker: 2x2 red block
  pset(10, 10, 8)
  pset(11, 10, 8)
  pset(10, 11, 8)
  pset(11, 11, 8)

  -- Bottom-right corner marker: 2x2 green block
  pset(116, 116, 11)
  pset(117, 116, 11)
  pset(116, 117, 11)
  pset(117, 117, 11)

  -- Center pixel: white (7)
  pset(64, 64, 7)

  -- Verify pget() by reading the center pixel color
  -- and drawing a ring of pixels matching that color
  local c = pget(64, 64)   -- should return 7 (white)
  pset(62, 64, c)
  pset(66, 64, c)
  pset(64, 62, c)
  pset(64, 66, c)
end
