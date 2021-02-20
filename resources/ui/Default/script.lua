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

function onPlay(caller, args)
	game.editing = false
	ui.getElement("levelSelect.auto").enabled = true
	updateAutoButton()
	ui.animateElement("mainMenu", "fadeOut", 0, 1/7, false)
	ui.animateElement("levelSelect", "fadeIn", 1/7, 1/7, true)
	game.generateLevelList()
end
ui.getElement("mainMenu.play").onClick.add(onPlay)

function onEdit(caller, args)
	game.editing = true
	ui.getElement("levelSelect.auto").enabled = false
	ui.animateElement("mainMenu", "fadeOut", 0, 1/7, false)
	ui.animateElement("levelSelect", "fadeIn", 1/7, 1/7, true)
	game.generateLevelList()
end
ui.getElement("mainMenu.edit").onClick.add(onEdit)

function onSettings(caller, args)
	ui.animateElement("mainMenu", "fadeOut", 0, 1/7, false)
	ui.animateElement("settings", "fadeIn", 1/7, 1/7, true)
end
ui.getElement("mainMenu.settings").onClick.add(onSettings)

function onExit(caller, args)
	game.exit()
end
ui.getElement("mainMenu.exit").onClick.add(onExit)

function onSFML(caller, args)
	helper.openURL("https://sfml-dev.org")
end
ui.getElement("mainMenu.sfml").onClick.add(onSFML)

function onGithub(caller, args)
	helper.openURL("https://github.com/ppr-game/PPR")
end
ui.getElement("mainMenu.github").onClick.add(onGithub)

function onDiscord(caller, args)
	helper.openURL("https://discord.gg/AuYUVs5")
end
ui.getElement("mainMenu.discord").onClick.add(onDiscord)

function onMusicStatusSwitch(caller, args)
	if soundManager.musicStatus == soundStatus.playing then
		soundManager.pauseMusic()
	else
		soundManager.playMusic()
	end
end
ui.getElement("mainMenu.music.pause").onClick.add(onMusicStatusSwitch)

function onMusicSwitch(caller, args)
	soundManager.switchMusic()
end
ui.getElement("mainMenu.music.switch").onClick.add(onMusicSwitch)

function onMusicStatusChange()
	if soundManager.musicStatus == soundStatus.playing then
		ui.getElement("mainMenu.music.pause").text = "║"
	else
		ui.getElement("mainMenu.music.pause").text = "►"
	end
	for i, element in ipairs(ui.getElements("music.nowPlaying")) do
		element.text = "NOW PLAYING : " .. soundManager.currentMusicName
	end
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

function onBack(caller, args)
	for i, menu in ipairs(menus) do
		if ui.getElement(menu).enabled then
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
for i, element in ipairs(ui.getElements("back")) do element.onClick.add(onBack) end

function onSwitchAuto(caller, args)
	game.auto = not game.auto
	updateAutoButton()
end
ui.getElement("levelSelect.auto").onClick.add(onSwitchAuto)

function updateAutoButton()
	ui.getElement("levelSelect.auto").selected = game.auto
end

function generateLevelSelectLevelButton(levelIndex, levelName)
	local levelButton = ui.createButton("levelSelect.levels.level." .. levelName, { "levelSelect.level" }, 0, levelIndex, 30, 0, 0, "levelSelect.levels", levelName, alignment.left)
	if levelIndex == 0 then firstLevelListName = levelName end
	lastLevelListName = levelName
	
	local diffPanel = ui.createPanel("levelSelect.difficulties." .. levelName, { "levelSelect.difficulties" }, 0, 0, 0, 0, 0, 0, "levelSelect.difficulties")
	diffPanel.enabled = false
	
	levelButton.onClick.add(onSelectLevel)
end

function onSelectLevel(caller, args)
	local levelName = ui.getLevelNameFromButton(args.id)
	soundManager.loadLevelMusic(levelName)
	ui.currentSelectedLevel = levelName
	
	-- Deselect all level buttons and then select the one we need
	for i, element in ipairs(ui.getElements("levelSelect.level")) do element.selected = false end
	ui.getElement(args.id).selected = true
	
	for i, element in ipairs(ui.getElements("levelSelect.difficulties")) do element.enabled = false end
	ui.getElement("levelSelect.difficulties." .. levelName).enabled = true
	
	for i, element in ipairs(ui.getElements("levelSelect.scores")) do element.enabled = false end
	for i, element in ipairs(ui.getElements("levelSelect.metadatas")) do element.enabled = false end
end

function generateLevelSelectDifficultyButton(difficultyIndex, levelName, difficultyName, difficulty)
	local diffName = string.upper(difficultyName)
	if difficultyName == "level" then diffName = "DEFAULT" end
	
	local difficultyButton = ui.createButton("levelSelect.difficulties." .. levelName .. ".difficulty." .. difficultyName, { "levelSelect.difficulty" }, 0, difficultyIndex, 30, 0, 0, "levelSelect.difficulties." .. levelName, diffName .. "(" .. difficulty .. ")", alignment.left)
	
	local metadataPanel = ui.createPanel("levelSelect.metadatas." .. levelName .. ".difficulty." .. difficultyName, { "levelSelect.metadatas" }, 0, 0, 0, 0, 0, 0, "levelSelect.metadatas")
	local scoresPanel = ui.createPanel("levelSelect.scores." .. levelName .. ".difficulty." .. difficultyName, { "levelSelect.scores" }, 0, 0, 0, 0, 0, 0, "levelSelect.scores")
	
	metadataPanel.enabled = false
	scoresPanel.enabled = false
	
	difficultyButton.onHover.add(onSelectDifficulty)
end

function onSelectDifficulty(caller, args)
	local levelName, diffName = ui.getLevelAndDiffNamesFromButton(args.id)
	ui.currentSelectedDiff = diffName
	
	for i, element in ipairs(ui.getElements("levelSelect.metadatas")) do element.enabled = false end
	ui.getElement("levelSelect.metadatas." .. levelName .. ".difficulty." .. diffName).enabled = true

	for i, element in ipairs(ui.getElements("levelSelect.scores")) do element.enabled = false end
	ui.getElement("levelSelect.scores." .. levelName .. ".difficulty." .. diffName).enabled = true
end

function generateLevelSelectMetadata(levelName, difficultyName)
	local length, difficulty, bpm, author, lua, objectsCount, speedsCount = ui.getLevelMetadata(levelName, difficultyName)
	
	local id = "levelSelect.metadatas." .. levelName .. ".difficulty." .. difficultyName
	local tag = "levelSelect.metadata"
	
	ui.createText(id .. ".length", { tag, tag .. ".length" }, 0, 0, 0, 0, id, "LENGTH:" .. length, alignment.left, false, false)
	ui.createText(id .. ".difficulty", { tag, tag .. ".difficulty" }, 0, 1, 0, 0, id, "DIFFICULTY:" .. difficulty, alignment.left, false, false)
	ui.createText(id .. ".bpm", { tag, tag .. ".bpm" }, 0, 2, 0, 0, id, "BPM:" .. bpm, alignment.left, false, false)
	ui.createText(id .. ".author", { tag, tag .. ".author", tag .. ".author." .. author }, 0, 3, 0, 0, id, "AUTHOR:", alignment.left, false, false)
	ui.createText(id .. ".author.text", { tag, tag .. ".author", tag .. ".author.text." .. author }, 7, 3, 0, 0, id, author, alignment.left, false, false)
	
	if lua then ui.createText(id .. ".lua", { tag, tag .. ".lua" }, 0, 34, 0, 0, id, "○ Lua Scripted", alignment.left, false, false) end
	
	ui.createText(id .. ".objectsCount", { tag, tag .. ".objectsCount" }, 0, 36, 0, 0, id, "objects:" .. objectsCount, alignment.left, false, false)
	ui.createText(id .. ".speedsCount", { tag, tag .. ".speedsCount" }, 0, 37, 0, 0, id, "speeds:" .. speedsCount, alignment.left, false, false)
end

function generateLevelSelectScores(levelName, difficultyName)
	local levelScores = ui.getLevelScores(levelName, difficultyName)
	for i, scoreTable in ipairs(levelScores) do
		local score, accuracy, maxCombo, scores = unpack(scoreTable)
		local accuracyStr = tostring(accuracy)
	
		local parentId = "levelSelect.scores." .. levelName .. ".difficulty." .. difficultyName
		local parentElement = ui.getElement(parentId)
		local id = parentId .. ".number." .. i
		local uniTag = "score"
		local tag = "levelSelect." .. uniTag
	
		local baseY = (i - 1) * 4
	
		ui.createPanel(id, { tag, tag .. ".panel" }, 0, 0, 0, 4, 0, 0, parentId)
		local sizeChangeVector = vector2i(0, 4)
		if i == 1 then sizeChangeVector = vector2i(0, 3) end
		parentElement.size = parentElement.size + sizeChangeVector
	
		ui.createText(id .. ".score", { tag, uniTag .. ".score", tag .. ".score", tag .. ".score.number." .. i }, 0, baseY, 0, 0, id, "SCORE: " .. tostring(score), alignment.left, false, false)
		
		local accTag = uniTag .. ".accuracy."
		if accuracy >= 100 then accTag = accTag .. "good" elseif accuracy >= 70 then accTag = accTag .. "ok" else accTag = accTag .. "bad" end
		
		local maxComboTag = uniTag .. ".maxCombo."
		if accuracy >= 100 then maxComboTag = maxComboTag .. "perfect_combo" elseif scores[1] <= 0 then maxComboTag = maxComboTag .. "full_combo" else maxComboTag = maxComboTag .. "combo" end
		
		local horDivPos = string.len(accuracyStr) + 1
		local maxComboPos = horDivPos + 1
		ui.createText(id .. ".accuracy", { tag, uniTag .. ".accuracy", tag .. ".accuracy", tag .. ".accuracy.number." .. i, accTag }, 0, baseY + 1, 0, 0, id, accuracyStr .. "%", alignment.left, false, false)
		ui.createText(id .. ".accComboDiv", { tag, uniTag .. ".accComboDiv", tag .. ".accComboDiv", tag .. ".accComboDiv.number." .. i }, horDivPos, baseY + 1, 0, 0, id, "│", alignment.left, false, false)
		ui.createText(id .. ".maxCombo", { tag, uniTag .. ".maxCombo", tag .. ".maxCombo", tag .. ".maxCombo.number." .. i, maxComboTag }, maxComboPos, baseY + 1, 0, 0, id, tostring(maxCombo) .. "x", alignment.left, false, false)
		
		generateMiniScores(id, tag, 0, baseY + 2, 0, 0, scores)
		
		local endY = baseY + 3
		local dividerText = "├───────────────────────┤"
		if endY == 27 then dividerText = "├───────────────────────┼" end
		ui.createText(id .. ".divider", { tag, tag .. ".divider", parentId .. ".divider", tag .. ".divider.number." .. i }, -1, endY, 0, 0, id, dividerText, alignment.left, false, false)
	end
end

function generateMiniScores(id, tag, x, y, anchorX, anchorY, scores)
	local missesCount = tostring(scores[1])
	local hitsCount = tostring(scores[2])
	local perfectHitsCount = tostring(scores[3])

	ui.createText(id .. ".miniScores.misses", { tag, "miniScores", tag .. ".miniScores", "miniScores.misses", tag .. ".miniScores.misses", id .. ".miniScores" }, x, y, anchorX, anchorY, id, missesCount, alignment.left, false, false)
	
	local x1 = x + string.len(missesCount) + 1
	ui.createText(id .. ".miniScores.hits", { tag, "miniScores", tag .. ".miniScores", "miniScores.hits", tag .. ".miniScores.hits", id .. ".miniScores" }, x1, y, anchorX, anchorY, id, hitsCount, alignment.left, false, false)

	local x2 = x1 + string.len(hitsCount) + 1
	ui.createText(id .. ".miniScores.perfectHits", { tag, "miniScores", tag .. ".miniScores", "miniScores.perfectHits", tag .. ".miniScores.perfectHits", id .. ".miniScores" }, x2, y, anchorX, anchorY, id, perfectHitsCount, alignment.left, false, false)
end

function scrollElement(maskId, movingId, delta)
	local movingBounds = ui.getElement(movingId).bounds
	local maskBounds = ui.getElement(maskId).bounds
	
	local canScrollUp = movingBounds.min.Y < maskBounds.min.Y
	local canScrollDown = movingBounds.max.Y > maskBounds.max.Y
	
	if delta < 0 and canScrollDown or delta > 0 and canScrollUp then
		local element = ui.getElement(movingId)
		element.position = element.position + vector2i(0, delta)
	end
end

function scrollElements(maskId, movingTag, firstId, lastId, delta)
	local firstBounds = ui.getElement(firstId).bounds
	local lastBounds = ui.getElement(lastId).bounds
	local maskBounds = ui.getElement(maskId).bounds
	
	local canScrollUp = firstBounds.min.Y < maskBounds.min.Y
	local canScrollDown = lastBounds.max.Y > maskBounds.max.Y
	
	if delta < 0 and canScrollDown or delta > 0 and canScrollUp then
		moveVector = vector2i(0, delta);
		for i, element in ipairs(ui.getElements(movingTag)) do
			element.position = element.position + moveVector
		end
	end
end

function levelsScrolled(caller, args)
	local firstId = "levelSelect.levels.level." .. firstLevelListName
	local lastId = "levelSelect.levels.level." .. lastLevelListName
	if ui.elementExists(firstId) and ui.elementExists(lastId) then
		scrollElements(args.id, "levelSelect.level", firstId, lastId, args.delta)
	end
end
ui.getElement("levelSelect.levels").onScroll.add(levelsScrolled)

function difficultiesScrolled(caller, args)
	scrollElement(args.id, "levelSelect.difficulties." .. ui.currentSelectedLevel .. ".difficulty." .. ui.currentSelectedDiff, args.delta)
end
ui.getElement("levelSelect.difficulties").onScroll.add(difficultiesScrolled)

function scoresScrolled(called, args)
	local movingId = "levelSelect.scores." .. ui.currentSelectedLevel .. ".difficulty." .. ui.currentSelectedDiff
	scrollElement(args.id, movingId, args.delta)
	for i, element in ipairs(ui.getElements(movingId .. ".divider")) do
		if element.globalPosition.Y == 39 then element.text = "├───────────────────────┼" else element.text = "├───────────────────────┤" end
	end
end
ui.getElement("levelSelect.scores").onScroll.add(scoresScrolled)

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
