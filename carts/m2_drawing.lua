-- ============================================================
-- MILESTONE B: Drawing primitives
-- What to check:
--   * cls() fills background
--   * line() draws diagonal and horizontal/vertical lines
--   * rect() draws a hollow border — no fill inside
--   * rectfill() draws a solid filled rectangle
--   * circ() draws a hollow circle outline only
--   * circfill() draws a solid filled disc
--   * All shapes stay within 0..127 bounds
--   * Shapes use different palette colors so they are distinct
-- ============================================================

function _init()
end

function _draw()
  cls(1)  -- clear to black

  -- Line: diagonal from top-left to bottom-right (yellow, 10)
  line(0, 0, 127, 127, 10)

  -- Line: horizontal across the middle (dark grey, 5)
  line(0, 64, 127, 64, 10)

  -- rect: hollow white border inset by 4px
  rect(4, 4, 40, 40, 7)

  -- rectfill: small filled orange block in top area
  rectfill(20, 20, 30, 14, 9)

  -- circ: hollow circle centered mid-screen (blue, 12)
  circ(64, 64, 20, 12)

  -- circfill: small filled disc (pink, 14)
  circfill(100, 30, 8, 14)

  -- circfill: another disc bottom-left (green, 11)
  circfill(20, 100, 6, 11)
end
