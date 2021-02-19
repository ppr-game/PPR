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
	if levelIndex == 0 then firstLevelListName = levelName end
	lastLevelListName = levelName
	
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
	
	ui.setElementsEnabled("levelSelect.scores", false)
	ui.setElementsEnabled("levelSelect.metadatas", false)
end

function generateLevelSelectDifficultyButton(difficultyIndex, levelName, difficultyName, difficulty)
	local diffName = string.upper(difficultyName)
	if difficultyName == "level" then diffName = "DEFAULT" end
	
	ui.createButton("levelSelect.difficulties." .. levelName .. ".difficulty." .. difficultyName, { "levelSelect.difficulty" }, 0, difficultyIndex, 30, 0, 0, "levelSelect.difficulties." .. levelName, diffName .. "(" .. difficulty .. ")", alignment.left)
	
	ui.createPanel("levelSelect.metadatas." .. levelName .. ".difficulty." .. difficultyName, { "levelSelect.metadatas" }, 0, 0, 0, 0, 0, 0, "levelSelect.metadatas")
	ui.createPanel("levelSelect.scores." .. levelName .. ".difficulty." .. difficultyName, { "levelSelect.scores" }, 0, 0, 0, 0, 0, 0, "levelSelect.scores")
	
	ui.setElementEnabled("levelSelect.metadatas." .. levelName .. ".difficulty." .. difficultyName, false)
	ui.setElementEnabled("levelSelect.scores." .. levelName .. ".difficulty." .. difficultyName, false)
end

function button_levelSelect_difficulty_onHover(id)
	local levelName, diffName = ui.getLevelAndDiffNamesFromButton(id)
	ui.currentSelectedDiff = diffName
	
	ui.setElementsEnabled("levelSelect.metadatas", false)
	ui.setElementEnabled("levelSelect.metadatas." .. levelName .. ".difficulty." .. diffName, true)

	ui.setElementsEnabled("levelSelect.scores", false)
	ui.setElementEnabled("levelSelect.scores." .. levelName .. ".difficulty." .. diffName, true)
end

function generateLevelSelectMetadata(levelName, difficultyName)
	local length, difficulty, bpm, author, lua, objectsCount, speedsCount = ui.getLevelMetadata(levelName, difficultyName)
	
	local id = "levelSelect.metadatas." .. levelName .. ".difficulty." .. difficultyName
	local tag = "levelSelect.metadata"
	
	ui.createText(id .. ".length", { tag, tag .. ".length" }, 0, 0, 0, 0, id, "LENGTH:" .. length, alignment.left, false, false)
	ui.createText(id .. ".difficulty", { tag, tag .. ".difficulty" }, 0, 1, 0, 0, id, "DIFFICULTY:" .. difficulty, alignment.left, false, false)
	ui.createText(id .. ".bpm", { tag, tag .. ".difficulty" }, 0, 2, 0, 0, id, "BPM:" .. bpm, alignment.left, false, false)
	ui.createText(id .. ".author", { tag, tag .. ".difficulty" }, 0, 3, 0, 0, id, "AUTHOR:" .. author, alignment.left, false, false)
	
	if lua then ui.createText(id .. ".lua", { tag, tag .. ".lua" }, 0, 34, 0, 0, id, "○ Lua Scripted", alignment.left, false, false) end
	
	ui.createText(id .. ".objectsCount", { tag, tag .. ".objectsCount" }, 0, 36, 0, 0, id, "objects:" .. objectsCount, alignment.left, false, false)
	ui.createText(id .. ".speedsCount", { tag, tag .. ".speedsCount" }, 0, 37, 0, 0, id, "speeds:" .. speedsCount, alignment.left, false, false)
end

function generateLevelSelectScores(levelName, difficultyName)
	local levelScores = ui.getLevelScores(levelName, difficultyName)
	for i, scoreTable in ipairs(levelScores) do
		local score, accuracy, maxCombo, scores = unpack(scoreTable)
	
		local parentId = "levelSelect.scores." .. levelName .. ".difficulty." .. difficultyName
		local id = parentId .. ".number." .. i
		local tag = "levelSelect.score"
	
		local baseY = (i - 1) * 4
	
		ui.createPanel(id, { tag, tag .. ".panel" }, 0, 0, 0, 4, 0, 0, parentId)
		if i == 1 then ui.changeElementSize(parentId, 0, 3) else ui.changeElementSize(parentId, 0, 4) end
	
		ui.createText(id .. ".score", { tag, tag .. ".score", tag .. ".score.number." .. i }, 0, baseY, 0, 0, id, "SCORE: " .. score, alignment.left, false, false)
		
		local horDivPos = string.len(accuracy) + 1
		local maxComboPos = horDivPos + 1
		ui.createText(id .. ".accuracy", { tag, tag .. ".accuracy", tag .. ".accuracy.number." .. i }, 0, baseY + 1, 0, 0, id, accuracy .. "%", alignment.left, false, false)
		ui.createText(id .. ".accComboDiv", { tag, tag .. ".accComboDiv", tag .. ".accComboDiv.number." .. i }, horDivPos, baseY + 1, 0, 0, id, "│", alignment.left, false, false)
		ui.createText(id .. ".maxCombo", { tag, tag .. ".maxCombo", tag .. ".maxCombo.number." .. i }, maxComboPos, baseY + 1, 0, 0, id, maxCombo .. "x", alignment.left, false, false)
		
		generateMiniScores(id, tag, 0, baseY + 2, 0, 0, scores)
		
		local endY = baseY + 3
		local dividerText = "├───────────────────────┤"
		if endY == 27 then dividerText = "├───────────────────────┼" end
		ui.createText(id .. ".divider", { tag, tag .. ".divider", tag .. ".divider.number." .. i, lastTag }, -1, endY, 0, 0, id, dividerText, alignment.left, false, false)
	end
end

function generateMiniScores(id, tag, x, y, anchorX, anchorY, scores)
	ui.createText(id .. ".miniScores.misses", { tag, tag .. ".miniScores", tag .. ".miniScores.misses", id .. ".miniscores" }, x, y, anchorX, anchorY, id, scores[1], alignment.left, false, false)
	
	local x1 = x + string.len(scores[1]) + 1
	ui.createText(id .. ".miniScores.hits", { tag, tag .. ".miniScores", tag .. ".miniScores.hits", id .. ".miniscores" }, x1, y, anchorX, anchorY, id, scores[2], alignment.left, false, false)

	local x2 = x1 + string.len(scores[2]) + 1
	ui.createText(id .. ".miniScores.perfectHits", { tag, tag .. ".miniScores", tag .. ".miniScores.perfectHits", id .. ".miniscores" }, x2, y, anchorX, anchorY, id, scores[3], alignment.left, false, false)
end

function scrollElement(maskId, movingId, delta)
	local movingBounds = ui.getElementBounds(movingId)
	local maskBounds = ui.getElementBounds(maskId)
	
	local canScrollUp = movingBounds.min.Y < maskBounds.min.Y
	local canScrollDown = movingBounds.max.Y > maskBounds.max.Y
	
	if delta < 0 and canScrollDown or delta > 0 and canScrollUp then
		ui.moveElement(movingId, 0, delta)
	end
end

function scrollElements(maskId, movingTag, firstId, lastId, delta)
	local firstBounds = ui.getElementBounds(firstId)
	local lastBounds = ui.getElementBounds(lastId)
	local maskBounds = ui.getElementBounds(maskId)
	
	local canScrollUp = firstBounds.min.Y < maskBounds.min.Y
	local canScrollDown = lastBounds.max.Y > maskBounds.max.Y
	
	if delta < 0 and canScrollDown or delta > 0 and canScrollUp then
		ui.moveElements(movingTag, 0, delta)
	end
end

function mask_levelSelect_levels_onScroll(id, delta)
	local firstId = "levelSelect.levels.level." .. firstLevelListName
	local lastId = "levelSelect.levels.level." .. lastLevelListName
	if ui.elementExists(firstId) and ui.elementExists(lastId) then
		scrollElements(id, "levelSelect.level", firstId, lastId, delta)
	end
end

function mask_levelSelect_difficulties_onScroll(id, delta)
	scrollElement(id, "levelSelect.difficulties." .. ui.currentSelectedLevel .. ".difficulty." .. ui.currentSelectedDiff, delta)
end

function mask_levelSelect_scores_onScroll(id, delta)
	scrollElement(id, "levelSelect.scores." .. ui.currentSelectedLevel .. ".difficulty." .. ui.currentSelectedDiff, delta)
end

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
