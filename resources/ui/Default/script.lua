function string.starts(str, start)
    return str:sub(1, #start) == start
end

function string.ends(str, ending)
    return ending == "" or str:sub(-#ending) == ending
end


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

ANIMATION_TIME = 1/7
GAME_ANIMATION_TIME = 1/10

START_ANIMATION_TIME = 1/0.5
EXIT_ANIMATION_TIME = 1/0.75

game.subscribeEvent(ui.getElement("debugMenu.toggle"), "buttonClicked", function()
    if ui.getElement("debugMenu").enabled then
        ui.animateElement("debugMenu", "fadeOut", ANIMATION_TIME, false, nil)
    else
        ui.animateElement("debugMenu", "fadeIn", ANIMATION_TIME, true, nil)
    end
end)

game.subscribeEvent(ui.getElement("debugMenu.reloadSettings"), "buttonClicked", function()
    ui.reload()
    gameStarted()
end)

game.subscribeEvent(ui.getElement("debugMenu.exit"), "buttonClicked", function()
    game.exit()
end)

game.subscribeEvent(ui.getElement("debugMenu.editMode"), "buttonClicked", function()
    game.editing = not game.editing
    updateEditModeButton()
end)

game.subscribeEvent(ui.getElement("mainMenu.play"), "buttonClicked", function()
    game.editing = false
    updateEditModeButton()
    ui.getElement("levelSelect.auto").enabled = true
    updateAutoButtons()
    ui.animateElement("mainMenu", "fadeOut", ANIMATION_TIME, false, function()
        ui.animateElement("levelSelect", "fadeIn", ANIMATION_TIME, true, nil)
    end)
end)

game.subscribeEvent(ui.getElement("mainMenu.edit"), "buttonClicked", function()
    game.editing = true
    updateEditModeButton()
    ui.getElement("levelSelect.auto").enabled = false
    ui.animateElement("mainMenu", "fadeOut", ANIMATION_TIME, false, function()
        ui.animateElement("levelSelect", "fadeIn", ANIMATION_TIME, true, nil)
    end)
end)

game.subscribeEvent(ui.getElement("mainMenu.settings"), "buttonClicked", function()
    ui.animateElement("mainMenu", "fadeOut", ANIMATION_TIME, false, function()
        ui.animateElement("settings", "fadeIn", ANIMATION_TIME, true, nil)
    end)
end)

game.subscribeEvent(ui.getElement("mainMenu.exit"), "buttonClicked", function()
    game.exit()
end)

game.subscribeEvent(ui.getElement("mainMenu.sfml"), "buttonClicked", function()
    helper.openURL("https://sfml-dev.org")
end)

game.subscribeEvent(ui.getElement("mainMenu.github"), "buttonClicked", function()
    helper.openURL("https://github.com/ppr-game/PPR")
end)

game.subscribeEvent(ui.getElement("mainMenu.discord"), "buttonClicked", function()
    helper.openURL("https://discord.gg/AuYUVs5")
end)

game.subscribeEvent(ui.getElement("mainMenu.music.pause"), "buttonClicked", function()
    if soundManager.musicStatus == soundStatus.playing then
        soundManager.pauseMusic()
    else
        soundManager.playMusic()
    end
end)

game.subscribeEvent(ui.getElement("mainMenu.music.switch"), "buttonClicked", function()
    soundManager.switchMusic()
end)

function onAuto()
    game.auto = not game.auto
    updateAutoButtons()
end
for _, element in ipairs(ui.getElements("auto")) do game.subscribeEvent(element, "buttonClicked", onAuto) end

game.subscribeEvent(ui.getElement("levelSelect"), "elementEnabled", function()
    game.generateLevelList()
end)

reloadLevel = true
game.subscribeEvent(ui.getElement("game"), "elementEnabled", function()
    if not reloadLevel then return end
    updateLevelName()
    game.loadLevel(lastLevel, lastDiff)
end)

game.subscribeEvent(ui.getElement("lastStats"), "elementEnabled", function()
    updateLevelName()
    
    local pause = true
    local pass = false
    local fail = false
    
    if not game.editing and game.statsState ~= "pause" then
    	pause = false
    	pass = game.statsState == "pass"
    	fail = not pass
    end
    
    if pause then game.playing = false end
    
    for _, element in ipairs(ui.getElements("pause")) do element.enabled = pause end
    for _, element in ipairs(ui.getElements("pass")) do element.enabled = pass end
    for _, element in ipairs(ui.getElements("fail")) do element.enabled = fail end
    
    ui.getElement("lastStats.player").enabled = not game.editing
    ui.getElement("lastStats.editor").enabled = game.editing
end)

game.subscribeEvent(ui.getElement("levelSelect"), "elementDisabled", function()
    for _, element in ipairs(ui.getElements("levelSelect.temporary")) do
        ui.deleteElement(element.id)
    end
end)

game.subscribeEvent(ui.getElement("levelSelect.levels"), "maskScrolled", function(elem, delta)
	if firstLevelListName ~= nil and lastLevelListName ~= nil then
		local firstId = "levelSelect.levels.level." .. firstLevelListName
		local lastId = "levelSelect.levels.level." .. lastLevelListName
		if ui.elementExists(firstId) and ui.elementExists(lastId) then
			scrollElements(elem.id, "levelSelect.level", firstId, lastId, delta)
		end
	end
end)

game.subscribeEvent(ui.getElement("levelSelect.difficulties"), "maskScrolled", function(elem, delta)
    if ui.currentSelectedLevel ~= nil and ui.currentSelectedDiff ~= nil then
        scrollElement(elem.id, "levelSelect.difficulties." .. ui.currentSelectedLevel .. ".difficulty." .. ui.currentSelectedDiff, delta)
    end
end)

game.subscribeEvent(ui.getElement("levelSelect.scores"), "maskScrolled", function(elem, delta)
    if ui.currentSelectedLevel ~= nil and ui.currentSelectedDiff ~= nil then
        local movingId = "levelSelect.scores." .. ui.currentSelectedLevel .. ".difficulty." .. ui.currentSelectedDiff
        scrollElement(elem.id, movingId, delta)
        for _, element in ipairs(ui.getElements(movingId .. ".divider")) do
            if element.globalPosition.Y == 39 then
                element.text = "├───────────────────────┼"
            else
                element.text = "├───────────────────────┤"
            end
        end
    end
end)

game.subscribeEvent(ui.getElement("lastStats.restart"), "buttonClicked", function()
    ui.animateElement("lastStats", "fadeOut", GAME_ANIMATION_TIME, false, function()
        reloadLevel = true
        ui.animateElement("game", "fadeIn", GAME_ANIMATION_TIME, true, nil)
    end)
end)

game.subscribeEvent(ui.getElement("lastStats.player.continue"), "buttonClicked", function()
    ui.animateElement("lastStats", "fadeOut", GAME_ANIMATION_TIME, false, function()
        game.playing = true
        reloadLevel = false
        ui.animateElement("game", "fadeIn", GAME_ANIMATION_TIME, true, nil)
    end)
end)

game.subscribeEvent(ui.getElement("lastStats.editor.continue"), "buttonClicked", function()
    ui.animateElement("lastStats", "fadeOut", GAME_ANIMATION_TIME, false, function()
        reloadLevel = false
        ui.animateElement("game", "fadeIn", GAME_ANIMATION_TIME, true, nil)
    end)
end)

function onLevelSave()
    game.saveLevel(lastLevel, lastDiff)
end
for _, element in ipairs(ui.getElements("lastStats.save")) do game.subscribeEvent(element, "buttonClicked", onLevelSave) end

game.subscribeEvent(nil, "levelChanged", function()
    for _, element in ipairs(ui.getElements("lastStats.save")) do
        if game.changed then
            if not string.ends(element.text, "*") then
                element.text = element.text .. "*"
                element.width = element.width + 1
            end
        else
            if string.ends(element.text, "*") then
                element.width = element.width - 1
                element.text = element.text:sub(1, #element.text - 1)
            end
        end
    end
end)

counter = 0
function setDebugStatus(text)
    print(text)
    if ui.elementExists("debugMenu.debugStatus") then
        for _, element in ipairs(ui.getElements("debugStatus")) do
            element.position = element.position + vector2i(0, 1)
            if element.position.y > 10 then ui.deleteElement(element.id) end
        end
        ui.createText("debugMenu.debugStatus.status." .. tostring(counter), { "debugMenu", "debugStatus" }, 0, 0, 1, 0, "debugMenu.debugStatus", text, alignment.right, true, false)
        counter = counter + 1
    end
end

function registerDebugAnimationOutput(id)
    game.subscribeEvent(ui.getElement(id), "elementEnabled", function(elem)
        setDebugStatus(elem.id .. " enabled")
    end)
    game.subscribeEvent(ui.getElement(id), "elementDisabled", function(elem)
        setDebugStatus(elem.id .. " disabled")
    end)
    game.subscribeEvent(ui.getElement(id), "animationStarted", function(elem, animation)
        setDebugStatus(elem.id .. " started " .. animation)
    end)
    game.subscribeEvent(ui.getElement(id), "animationFinished", function(elem, animation)
        setDebugStatus(elem.id .. " finished " .. animation)
    end)
end

--registerDebugAnimationOutput("levelSelect")

function debugEnterMenu(menu)
	ui.animateElement(nil, "fadeOut", ANIMATION_TIME, false, function()
        ui.getElement("debugMenu.toggle").enabled = true;
        ui.animateElement("debugMenu", "fadeIn", ANIMATION_TIME, true, nil)
        ui.animateElement(menu, "fadeIn", ANIMATION_TIME, true, nil)
    end)
end

function registerDebugMenuButton(menu)
    game.subscribeEvent(ui.getElement("debugMenu." .. menu), "buttonClicked", function()
        debugEnterMenu(menu)
    end)
end

registerDebugMenuButton("mainMenu")
registerDebugMenuButton("settings")
registerDebugMenuButton("levelSelect")
registerDebugMenuButton("game")
registerDebugMenuButton("lastStats")

function updateEditModeButton()
	ui.getElement("debugMenu.editMode").selected = game.editing
end

game.subscribeEvent(nil, "musicStatusChanged", function()
    if soundManager.musicStatus == soundStatus.playing then
        ui.getElement("mainMenu.music.pause").text = "║"
    else
        ui.getElement("mainMenu.music.pause").text = "►"
    end
    for _, element in ipairs(ui.getElements("music.nowPlaying")) do
        element.text = "NOW PLAYING : " .. soundManager.currentMusicName
    end
end)

function gameStarted()
    updateEditModeButton()
    ui.animateElement("mainMenu", "fadeIn", START_ANIMATION_TIME, true, nil)
end
game.subscribeEvent(nil, "gameStarted", gameStarted)

game.subscribeEvent(nil, "gameExited", function()
	game.exitTime = EXIT_ANIMATION_TIME + 0.25
	ui.animateElement(nil, "fadeOut", EXIT_ANIMATION_TIME, false, nil)
end)

game.subscribeEvent(nil, "passedOrFailed", function()
	ui.animateElement("game", "fadeOut", GAME_ANIMATION_TIME, false, function()
        ui.animateElement("lastStats", "fadeIn", ANIMATION_TIME, true, nil)
    end)
end)

menus = { "mainMenu", "levelSelect", "lastStats" }

function onBack()
	for _, menu in ipairs(menus) do
		if ui.getElement(menu).enabled then
			previousMenu = ui.getPreviousMenu(menu)
			fadeOutTime = ANIMATION_TIME
			fadeInTime = ANIMATION_TIME
			if menu == "game" then fadeOutTime = GAME_ANIMATION_TIME end
			if previousMenu == "game" then fadeInTime = GAME_ANIMATION_TIME end
			ui.animateElement(menu, "fadeOut", fadeOutTime, false, function()
                ui.animateElement(previousMenu, "fadeIn", fadeInTime, true, nil)
            end)
		end
	end
	firstLevelListName = nil
	lastLevelListName = nil
end
for _, element in ipairs(ui.getElements("back")) do game.subscribeEvent(element, "buttonClicked", onBack) end

function updateAutoButtons()
    for _, element in ipairs(ui.getElements("auto")) do element.selected = game.auto end
end

function generateLevelSelectLevelButton(levelIndex, levelName)
	local levelButton = ui.createButton("levelSelect.levels.level." .. levelName, { "levelSelect.temporary", "levelSelect.level" }, 0, levelIndex, 30, 0, 0, "levelSelect.levels", levelName, alignment.left)
	if levelIndex == 0 then firstLevelListName = levelName end
	lastLevelListName = levelName
	
	local diffPanel = ui.createPanel("levelSelect.difficulties." .. levelName, { "levelSelect.temporary", "levelSelect.difficulties" }, 0, 0, 0, 0, 0, 0, "levelSelect.difficulties")
	diffPanel.enabled = false
	
    game.subscribeEvent(levelButton, "buttonClicked", onSelectLevel)
end
game.subscribeEvent(nil, "generateLevelSelectLevelButton", generateLevelSelectLevelButton)

function onSelectLevel(button)
	local levelName = ui.getLevelNameFromButton(button.id)
	soundManager.loadLevelMusic(levelName)
	ui.currentSelectedLevel = levelName
	
	-- Deselect all level buttons and then select the one we need
	for _, element in ipairs(ui.getElements("levelSelect.level")) do element.selected = false end
	button.selected = true
	
	for _, element in ipairs(ui.getElements("levelSelect.difficulties")) do element.enabled = false end
	ui.getElement("levelSelect.difficulties." .. levelName).enabled = true
	
	for _, element in ipairs(ui.getElements("levelSelect.scores")) do element.enabled = false end
	for _, element in ipairs(ui.getElements("levelSelect.metadatas")) do element.enabled = false end
end

function generateLevelSelectDifficultyButton(difficultyIndex, levelName, difficultyName, difficulty)
	local diffName = getDisplayDifficultyName(difficultyName)
	
	local difficultyButton = ui.createButton("levelSelect.difficulties." .. levelName .. ".difficulty." .. difficultyName, { "levelSelect.difficulty" }, 0, difficultyIndex, 30, 0, 0, "levelSelect.difficulties." .. levelName, diffName .. "(" .. difficulty .. ")", alignment.left)
	
	local metadataPanel = ui.createPanel("levelSelect.metadatas." .. levelName .. ".difficulty." .. difficultyName, { "levelSelect.temporary", "levelSelect.metadatas" }, 0, 0, 0, 0, 0, 0, "levelSelect.metadatas")
	local scoresPanel = ui.createPanel("levelSelect.scores." .. levelName .. ".difficulty." .. difficultyName, { "levelSelect.temporary", "levelSelect.scores" }, 0, 0, 0, 0, 0, 0, "levelSelect.scores")
	
	metadataPanel.enabled = false
	scoresPanel.enabled = false

    game.subscribeEvent(difficultyButton, "buttonHovered", onSelectDifficulty)
    game.subscribeEvent(difficultyButton, "buttonClicked", onLevelEnter)
end
game.subscribeEvent(nil, "generateLevelSelectDifficultyButton", generateLevelSelectDifficultyButton)

function getDisplayDifficultyName(difficultyName)
	local diffName = string.upper(difficultyName)
	if difficultyName == "level" then diffName = "DEFAULT" end
	return diffName
end

function onSelectDifficulty(button)
	local levelName, diffName = ui.getLevelAndDiffNamesFromButton(button.id)
	ui.currentSelectedDiff = diffName
	
	for _, element in ipairs(ui.getElements("levelSelect.metadatas")) do element.enabled = false end
	ui.getElement("levelSelect.metadatas." .. levelName .. ".difficulty." .. diffName).enabled = true

	for _, element in ipairs(ui.getElements("levelSelect.scores")) do element.enabled = false end
	ui.getElement("levelSelect.scores." .. levelName .. ".difficulty." .. diffName).enabled = true
end

function onLevelEnter(button)
	lastLevel, lastDiff = ui.getLevelAndDiffNamesFromButton(button.id)
	lastLength, lastDifficulty, lastBpm, lastAuthor, lastLua, lastObjectsCount, lastSpeedsCount = ui.getLevelMetadata(lastLevel, lastDiff)
	
	ui.animateElement("levelSelect", "fadeOut", GAME_ANIMATION_TIME, false, function()
        reloadLevel = true
        ui.animateElement("game", "fadeIn", GAME_ANIMATION_TIME, true, nil)
    end)
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
game.subscribeEvent(nil, "generateLevelSelectMetadata", generateLevelSelectMetadata)

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
	
		ui.createPanel(id, { tag, tag .. ".panel" }, 1, 0, 0, 4, 0, 0, parentId)
		local sizeChangeVector = vector2i(0, 4)
		if i == 1 then sizeChangeVector = vector2i(0, 3) end
		parentElement.size = parentElement.size + sizeChangeVector
	
		ui.createText(id .. ".score", { tag, uniTag .. ".score", tag .. ".score", tag .. ".score.number." .. i }, 0, baseY, 0, 0, id, "SCORE: " .. tostring(score), alignment.left, false, false)
		
		local accTag = uniTag .. ".accuracy." .. getAccuracyTagSuffix(accuracy)
		
		local comboTagSuffix = getComboTagSuffix(accuracy, scores[1])
		local comboTag = uniTag .. ".combo." .. comboTagSuffix
		local maxComboTag = uniTag .. ".maxCombo." .. comboTagSuffix
		
		local horDivPos = string.len(accuracyStr) + 1
		local maxComboPos = horDivPos + 1
		ui.createText(id .. ".accuracy", { tag, uniTag .. ".accuracy", tag .. ".accuracy", tag .. ".accuracy.number." .. i, accTag }, 0, baseY + 1, 0, 0, id, accuracyStr .. "%", alignment.left, false, false)
		ui.createText(id .. ".accComboDiv", { tag, uniTag .. ".accComboDiv", tag .. ".accComboDiv", tag .. ".accComboDiv.number." .. i }, horDivPos, baseY + 1, 0, 0, id, "│", alignment.left, false, false)
		ui.createText(id .. ".maxCombo", { tag, uniTag .. ".combo", uniTag .. ".maxCombo", tag .. ".maxCombo", tag .. ".maxCombo.number." .. i, comboTag, maxComboTag }, maxComboPos, baseY + 1, 0, 0, id, tostring(maxCombo) .. "x", alignment.left, false, false)
		
		generateMiniScores(id, tag, 0, baseY + 2, 0, 0, scores)
		
		local endY = baseY + 3
		local dividerText = "├───────────────────────┤"
		if endY == 27 then dividerText = "├───────────────────────┼" end
		ui.createText(id .. ".divider", { tag, tag .. ".divider", parentId .. ".divider", tag .. ".divider.number." .. i }, -1, endY, 0, 0, id, dividerText, alignment.left, false, false)
	end
end
game.subscribeEvent(nil, "generateLevelSelectScores", generateLevelSelectScores)

function getAccuracyTagSuffix(accuracy)
	if accuracy >= 100 then return "good" end
	if accuracy >= 70 then return "ok" end
	return "bad"
end

function getComboTagSuffix(accuracy, misses)
	if accuracy >= 100 then return "perfectCombo" end
	if misses <= 0 then return "fullCombo" end
	return "combo"
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
		for _, element in ipairs(ui.getElements(movingTag)) do
			element.position = element.position + moveVector
		end
	end
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

function updateLevelName()
	local levelName = lastLevel
	local difficultyName = lastDiff
	local author = lastAuthor
	
	if levelName == nil then levelName = "nil" end
	if difficultyName == nil then difficultyName = "nil" end
	if author == nil then author = "nil" end
	
	for _, element in ipairs(ui.getElements("levelName")) do
		element.text = levelName .. " [" .. getDisplayDifficultyName(difficultyName) .. "] : " .. author
	end
end


function updateRealtimeScore(newScore, _)
	for _, element in ipairs(ui.getElements("realtime.score")) do
		element.text = "SCORE: " .. tostring(newScore)
	end
end
game.subscribeEvent(nil, "scoreChanged", updateRealtimeScore)

function updateRealtimeAccuracy(newAccuracy)
	for _, element in ipairs(ui.getElements("realtime.accuracy")) do
		element.text = "ACCURACY: " .. tostring(newAccuracy) .. "%"
        
        local accTag = "score.accuracy." .. getAccuracyTagSuffix(newAccuracy)
        local accTagChanged = false
        for i, tag in ipairs(element.tags) do
            if string.starts(tag, "score.accuracy.") then
                element.tags[i] = accTag
                accTagChanged = true
            end
        end
        if not accTagChanged then element.tags = table.insert(element.tags, accTag) end
	end
end
game.subscribeEvent(nil, "accuracyChanged", updateRealtimeAccuracy)

function updateCombo(textTag, combo, accuracy, misses)
    for _, element in ipairs(ui.getElements(textTag)) do
        local tagSuffix = getComboTagSuffix(accuracy, misses)
        if tagSuffix == "perfectCombo" then
            element.text = "PERFECT COMBO: " .. tostring(combo)
        elseif tagSuffix == "fullCombo" then
            element.text = "FULL COMBO: " .. tostring(combo)
        else
            element.text = "MAX COMBO: " .. tostring(combo)
        end
        
        local comboTag = "score.combo." .. tagSuffix
        local comboTagChanged = false
        for i, tag in ipairs(element.tags) do
            if string.starts(tag, "score.combo.") then
                element.tags[i] = comboTag
                comboTagChanged = true
            end
        end
        if not comboTagChanged then element.tags = table.insert(element.tags, comboTag) end
    end
end

function updateRealtimeCombo(newCombo)
    updateCombo("realtime.combo", newCombo, scoreManager.accuracy, scoreManager.scores[1])
end
game.subscribeEvent(nil, "comboChanged", updateRealtimeCombo)

function updateRealtimeMaxCombo(newMaxCombo)
    updateCombo("realtime.maxCombo", newMaxCombo, scoreManager.accuracy, scoreManager.scores[1])
end
game.subscribeEvent(nil, "maxComboChanged", updateRealtimeMaxCombo)

function updateRealtimeScores(scoreIndex)
    local tag = "realtime."
    
    if scoreIndex == 1 then tag = tag .. "misses"
    elseif scoreIndex == 2 then tag = tag .. "hits"
    elseif scoreIndex == 3 then tag = tag .. "perfectHits"
    end
    
	for _, element in ipairs(ui.getElements(tag)) do
		element.text = tostring(scoreManager.scores[scoreIndex])
	end
end
game.subscribeEvent(nil, "scoresChanged", updateRealtimeScores)
