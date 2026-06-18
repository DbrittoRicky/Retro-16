-- ============================================================
-- MILESTONE F: Hot-reload
-- What to check:
--   * Start the console pointing at this file
--   * Console shows a blue background with the message below
--   * While the console is running, edit this file and save:
--       change cls(1) to cls(3)  (dark green background)
--       change the print text to "RELOADED OK"
--   * Within ~1 second the console should show the new version
--   * _init() should re-run on reload (counter resets to 0)
--   * No crash, no need to restart the process
-- ============================================================

local reload_count = 0

function _init()
  reload_count = reload_count + 1
end

function _draw()
  cls(1)
  print("HOT RELOAD TEST", 10, 40, 7)
  print("EDIT THIS FILE", 12, 56, 6)
  print("AND SAVE IT", 18, 68, 5)
  print("LOADS:" .. reload_count, 40, 90, 10)
end
