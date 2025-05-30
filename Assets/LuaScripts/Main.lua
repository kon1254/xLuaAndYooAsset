-- 2025年05月29日 21:28:39 星期四
Main = {}

local GameApp = require("Game.GameApp")

function Main.Init()
    -- 初始化游戏
    print("Game initialized")

    GameApp.EnterGame()
end

function Main.Update()
    print("Game updated")
end
