ui.animations = {
	none = { },
	fadeOut = {
		background = { "bgR", "bgG", "bgB", "lerp(bgA, 0, time * (posRandom(x, y) * 3.5 + 1))" },
		foreground = { "fgR", "fgG", "fgB", "lerp(fgA, 0, time * (posRandom(x, y) * 3.5 + 1))" }
	},
	fadeIn = {
		background = { "bgR", "bgG", "bgB", "lerp(0, bgA, time * (posRandom(x, y) * 3.5 + 1))" },
		foreground = { "fgR", "fgG", "fgB", "lerp(0, fgA, time * (posRandom(x, y) * 3.5 + 1))" }
	}
}

function button_mainMenu_play_onClick()
	game.editing = false
	ui.setElementEnabled("levelSelect.auto", true)
	updateAutoButton()
	ui.animateElement("mainMenu", "fadeOut", 0, 1/7, false)
	ui.animateElement("levelSelect", "fadeIn", 1/7, 1/7, true)
	game.generateLevelList()
end

function button_mainMenu_edit_onClick()
	game.editing = true
	ui.setElementEnabled("levelSelect.auto", false)
	ui.animateElement("mainMenu", "fadeOut", 0, 1/7, false)
	ui.animateElement("levelSelect", "fadeIn", 1/7, 1/7, true)
	game.generateLevelList()
end

function button_mainMenu_settings_onClick()
	ui.animateElement("mainMenu", "fadeOut", 0, 1/7, false)
	ui.animateElement("settings", "fadeIn", 1/7, 1/7, true)
end

function button_mainMenu_exit_onClick()
	game.exit()
end

function button_mainMenu_sfml_onClick()
	helper.openURL("https://sfml-dev.org")
end

function button_mainMenu_github_onClick()
	helper.openURL("https://github.com/ppr-game/PPR")
end

function button_mainMenu_discord_onClick()
	helper.openURL("https://discord.gg/AuYUVs5")
end

function button_mainMenu_music_pause_onClick()
	if soundManager.musicStatus == soundStatus.playing then
		soundManager.pauseMusic()
	else
		soundManager.playMusic()
	end
end

function button_mainMenu_music_switch_onClick()
	soundManager.switchMusic()
end

function onMusicStatusChange()
	if soundManager.musicStatus == soundStatus.playing then
		ui.setElementText("mainMenu.music.pause", "║")
	else
		ui.setElementText("mainMenu.music.pause", "►")
	end
	ui.setElementsText("music.nowPlaying", "NOW PLAYING : " .. soundManager.currentMusicName)
end

function onGameStart()
	ui.animateElement(nil, "none", 0, 0, false)
	ui.animateElement("mainMenu", "fadeIn", 0, 1/0.5, true)
end

function onGameExit()
	game.exitTime = 1/0.75
	ui.animateElement(nil, "fadeOut", 0, 1/0.75, false)
end

function onPassOrFail()
	ui.animateElement("game", "fadeOut", 0, 1/10, false)
	ui.animateElement("lastStats", "fadeIn", 1/10, 1/7, true)
end

menus = { "mainMenu", "levelSelect", "lastStats" }

function button_back_onClick()
	for i, menu in ipairs(menus) do
		if ui.getElementEnabled(menu) then
			previousMenu = ui.getPreviousMenu(menu)
			fadeOutSpeed = 7
			fadeInSpeed = 7
			if menu == "game" then fadeOutSpeed = 10 end
			if previousMenu == "game" then fadeInSpeed = 10 end
			ui.animateElement(menu, "fadeOut", 0, 1/fadeOutSpeed, false)
			ui.animateElement(previousMenu, "fadeIn", 1/fadeOutSpeed, 1/fadeInSpeed, true)
		end
	end
end

function button_levelSelect_auto_onClick()
	game.auto = not game.auto
	updateAutoButton()
end

function updateAutoButton()
	ui.setButtonSelected("levelSelect.auto", game.auto)
end

function generateLevelSelectLevelButton(levelIndex, levelName)
	ui.createButton("levelSelect.levels.level." .. levelName, { "levelSelect.level" }, 0, levelIndex, 30, 0, 0, "levelSelect.levels", levelName, alignment.left)
	ui.createPanel("levelSelect.difficulties." .. levelName, { "levelSelect.difficulties" }, 0, 0, 0, 0, 0, 0, "levelSelect.difficulties")
	ui.setElementEnabled("levelSelect.difficulties." .. levelName, false)
end

function button_levelSelect_level_onClick(id)
	local levelName = ui.getLevelNameFromButton(id)
	soundManager.loadLevelMusic(levelName)
	ui.currentSelectedLevel = levelName
	
	-- Deselect all level buttons and then select the one we need
	ui.setButtonsSelected("levelSelect.level", false)
	ui.setButtonSelected(id, true)
	
	ui.setElementsEnabled("levelSelect.difficulties", false)
	ui.setElementEnabled("levelSelect.difficulties." .. levelName, true)
end
--function button_levelSelect_level_onClick(uid)
--	levelName = ui.getLevelNameFromButton("levelSelect", uid)
--	soundManager.loadLevelMusic(levelName)
--	ui.currentSelectedLevel = levelName
--	
--	-- Deselect all level buttons and then select the one we need
--	ui.setButtonSelected("levelSelect.level", false)
--	ui.setUniqueButtonSelected("levelSelect", uid, true)
--	
--	ui.setElementEnabled("levelSelect.difficulties", false)
--	ui.setUniqueElementEnabled("levelSelect", "levelSelect.difficulties." .. levelName, true)
--	
--	ui.setElementEnabled("levelSelect.scores", false)
--	
--	ui.setElementEnabled("levelSelect.metadatas", false)
--end

function generateLevelSelectDifficultyButton(difficultyIndex, levelName, difficultyName, difficulty)
	local diffName = string.upper(difficultyName)
	if difficultyName == "level" then
		diffName = "DEFAULT"
	end
	
	ui.createButton("levelSelect.difficulties." .. levelName .. ".difficulty." .. difficultyName, { "levelSelect.difficulty" }, 0, difficultyIndex, 30, 0, 0, "levelSelect.difficulties." .. levelName, diffName .. "(" .. difficulty .. ")", alignment.left)
	
	--generateLevelSelectMetadata(levelName, difficultyName)
	--generateLevelSelectScores(levelName, difficultyName)
end

--function generateLevelSelectMetadata(levelName, difficultyName)
--	length, difficulty, bpm, author, lua, objectsCount, speedsCount = ui.getLevelMetadata(levelName, difficultyName)
--	
--	local uid = "levelSelect.metadata." .. levelName
--	local id = "levelSelect.metadata"
--	
--	ui.createPanel("levelSelect", uid, id, 0, 0, 0, 0, 0, 0, "levelSelect.allMetadatas")
--	
--	ui.createText("levelSelect", uid .. ".length", id .. ".length", 0, 0, 0, 0, uid, "LENGTH:" .. length, align.left, false, false)
--	ui.createText("levelSelect", uid .. ".difficulty", id .. ".difficulty", 0, 1, 0, 0, uid, "DIFFICULTY:" .. difficulty, align.left, false, false)
--	ui.createText("levelSelect", uid .. ".bpm", id .. ".difficulty", 0, 2, 0, 0, uid, "BPM:" .. bpm, align.left, false, false)
--	ui.createText("levelSelect", uid .. ".author", id .. ".difficulty", 0, 3, 0, 0, uid, "AUTHOR:" .. author, align.left, false, false)
--	
--	if lua then ui.createText("levelSelect", uid .. ".lua", id .. ".lua", 0, 34, 0, 0, uid, "○ Contains Lua", align.left, false, false) end
--	
--	ui.createText("levelSelect", uid .. ".objectsCount", id .. ".objectsCount", 0, 36, 0, 0, uid, "objects:" .. objectsCount, align.left, false, false)
--	ui.createText("levelSelect", uid .. ".speedsCount", id .. ".speedsCount", 0, 37, 0, 0, uid, "speeds:" .. speedsCount, align.left, false, false)
--end

--function generateLevelSelectScores(levelName, difficultyName)
--	local uid = "levelSelect.scores." .. levelName
--	local id = "levelSelect.scores"
--	
--	ui.createPanel("levelSelect", uid, id, 0, 0, 0, 0, 0, 0, "levelSelect.allScores")
--	
--	for i, (score, accuracy, maxCombo, scores) in ipairs(ui.getLevelScores()) do
--		local baseY = i * 4
--	
--		ui.createText("levelSelect", uid .. "." .. i .. ".score", id .. ".score", 0, baseY, 0, 0, uid, "SCORE: " .. score, align.left, false, false)
--		
--		local horDivPos = string.len(accuracy) + 1
--		local maxComboPos = horDivPos + 2
--		ui.createText("levelSelect", uid .. "." .. i .. ".accuracy", id .. ".accuracy", 0, baseY + 1, 0, 0, uid, accuracy, align.left, false, false)
--		ui.createText("levelSelect", uid .. "." .. i .. ".accComboDiv", id .. ".accComboDiv", horDivPos, baseY + 1, 0, 0, uid, "│", align.left, false, false)
--		ui.createText("levelSelect", uid .. "." .. i .. ".maxCombo", id .. ".maxCombo", maxComboPos, baseY + 1, 0, 0, uid, maxCombo, align.left, false, false)
--		
--		generateMiniScores("levelSelect", uid, id, 0, baseY + 2, 0, 0, scores)
--		
--		local endY = baseY + 3
--		local dividerText = "├───────────────────────┤"
--		if endY == 39 then dividerText = "├───────────────────────┼" end
--		ui.createText("levelSelect", uid .. "." .. i .. ".divider", id .. ".divider", 0, endY, 0, 0, uid, dividerText, align.left, false, false)
--	end
--end

--function generateMiniScores(layout, uid, id, x, y, anchorX, anchorY, scores)
--	ui.createText(layout, uid .. ".miniScores.misses", id .. ".miniScores.misses", x, y, anchorX, anchorY, uid, scores[1], align.left, false, false)
--	
--	local x1 = x + string.len(scores[1]) + 1
--	ui.createText(layout, uid .. ".miniScores.hits", id .. ".miniScores.hits", x1, y, anchorX, anchorY, uid, scores[2], align.left, false, false)
--
--	local x2 = x1 + string.len(scores[2]) + 1
--	ui.createText(layout, uid .. ".miniScores.perfectHits", id .. ".miniScores.perfectHits", x2, y, anchorX, anchorY, uid, scores[3], align.left, false, false)
--end
--
--function generateScores(layout, uid, id, x, y, anchorX, anchorY, scores)
--	ui.createText(layout, uid .. ".scores.misses.title", id .. ".scores.misses.title", x, y, anchorX, anchorY, uid, "MISSES:", align.left, false, false)
--	ui.createText(layout, uid .. ".scores.misses", id .. ".scores.misses", x + 15, y, anchorX, anchorY, uid, scores[1], align.left, false, false)
--	
--	ui.createText(layout, uid .. ".scores.hits.title", id .. ".scores.hits.title", x, y + 2, anchorX, anchorY, uid, "HITS:", align.left, false, false)
--	ui.createText(layout, uid .. ".scores.hits", id .. ".scores.hits", x + 15, y + 2, anchorX, anchorY, uid, scores[2], align.left, false, false)
--
--	ui.createText(layout, uid .. ".scores.perfectHits.title", id .. ".scores.perfectHits.title", x, y + 4, anchorX, anchorY, uid, "PERFECT HITS:", align.left, false, false)
--	ui.createText(layout, uid .. ".scores.perfectHits", id .. ".scores.perfectHits", x + 15, y + 4, anchorX, anchorY, uid, scores[3], align.left, false, false)
--end
--
--function button_levelSelect_difficulty_onHover(uid)
--	levelName, diffName = ui.getLevelAndDiffNamesFromButton("levelSelect", uid)
--	ui.currentSelectedDiff = diffName
--
--	ui.setElementEnabled("levelSelect.scores", false)
--	ui.setUniqueElementEnabled("levelSelect", "levelSelect.scores." .. levelName, true)
--	
--	ui.setElementEnabled("levelSelect.metadatas", false)
--	ui.setUniqueElementEnabled("levelSelect", "levelSelect.metadatas." .. levelName, true)
--end
--
--lastLevel = ""
--lastDiff = ""
--
--function button_levelSelect_difficulty_onClick(uid)
--	levelName, diffName = ui.getLevelAndDiffNamesFromButton("levelSelect", uid)
--
--	lastLevel = levelName
--	lastDiff = diffName
--	ui.transitionLayouts("lastStats", "game", "fadeOut", "fadeIn", 7, 10)
--end
--
--function onMenuSwitch(oldLayout, newLayout)
--	if oldLayout == "lastStats" and newLayout == "game" then
--		game.loadLevel(lastLevel, lastDiff)
--	end
--	if newLayout == "lastStats" then
--		local pause = true
--		local pass = false
--		local fail = false
--	
--		if !game.editing and game.statsState != "pause" then
--			pause = false
--			if game.statsState == "pass" then
--				pass = true
--			else
--				fail = true
--			end
--		end
--		
--		ui.setUniqueElementEnabled("lastStats", "lastStats.subtitle.pause", pause)
--		ui.setUniqueElementEnabled("lastStats", "lastStats.subtitle.pass", pass)
--		ui.setUniqueElementEnabled("lastStats", "lastStats.subtitle.fail", fail)
--		
--		ui.setUniqueElementEnabled("lastStats", "lastStats.player", !game.editing)
--		ui.setUniqueElementEnabled("lastStats", "lastStats.editor", game.editing)
--		
--		generateScores("lastStats", "lastStats.player", "lastStats.player", 25, 16, 0, 0, scoreManager.scores)
--	end
--end
