-- ============================================================
-- MILESTONE C: Bitmap font and print()
-- What to check:
--   * Text renders pixel-accurately (no native OS font used)
--   * Characters are clearly legible at the console resolution
--   * Different palette colors work for text (args 4th param)
--   * Uppercase and lowercase both work (font stores uppercase,
--     lowercase should be auto-upcased by TextApi)
--   * Numbers and punctuation render
--   * Characters missing from the font show as blank/space
--     rather than crashing
--   * Text positioned at different (x,y) coordinates
-- ============================================================

function _init()
end

function _draw()
  cls(1)

  -- White text on dark background
  print("HELLO WORLD", 4, 8, 7)

  -- Red text
  print("FANTASY CONSOLE", 4, 20, 8)

  -- Yellow number string
  print("0123456789", 4, 32, 10)

  -- Mixed case (should render same as uppercase)
  print("Build Zero", 4, 44, 11)

  -- Punctuation
  print("SCORE: 100", 4, 56, 7)

  -- Check boundary: text starting near right edge
  print("ABC", 104, 8, 9)

  -- Check boundary: text near bottom edge
  print("BTM", 4, 116, 12)

  -- Multiple colors on same line (two separate print calls)
  print("RED", 4,  80, 8)
  print("GRN", 30, 80, 11)
  print("BLU", 56, 80, 12)
end
