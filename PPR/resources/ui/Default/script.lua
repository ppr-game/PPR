function string.starts(str, start)
    return str:sub(1, #start) == start
end

function string.ends(str, ending)
    return ending == "" or str:sub(-#ending) == ending
end

function table.contains(table, value)
    for _, val in ipairs(table) do
        if value(val) then return true end
    end
    return false
end


game.subscribeEvent(ui.getElement("debugMenu.toggle"), "buttonClicked", function()
    if ui.getElement("debugMenu").enabled then
        ui.animateElement("debugMenu", "fadeOut", nil, nil)
    else
        ui.animateElement("debugMenu", "fadeIn", nil, nil)
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
    ui.animateElement("mainMenu", "fadeOut", function()
        ui.animateElement("levelSelect", "fadeIn", nil, nil)
    end, nil)
end)

game.subscribeEvent(ui.getElement("mainMenu.edit"), "buttonClicked", function()
    game.editing = true
    updateEditModeButton()
    ui.getElement("levelSelect.auto").enabled = false
    ui.animateElement("mainMenu", "fadeOut", function()
        ui.animateElement("levelSelect", "fadeIn", nil, nil)
    end, nil)
end)

game.subscribeEvent(ui.getElement("mainMenu.settings"), "buttonClicked", function()
    ui.animateElement("mainMenu", "fadeOut", function()
        ui.animateElement("settings", "fadeIn", nil, nil)
    end, nil)
end)

game.subscribeEvent(ui.getElement("mainMenu.exit"), "buttonClicked", function()
    game.exit()
end)

game.subscribeEvent(ui.getElement("mainMenu.sfml"), "buttonClicked", function()
    helper.openUrl("https://sfml-dev.org")
end)

game.subscribeEvent(ui.getElement("mainMenu.github"), "buttonClicked", function()
    helper.openUrl("https://github.com/ppr-game/PPR")
end)

game.subscribeEvent(ui.getElement("mainMenu.discord"), "buttonClicked", function()
    helper.openUrl("https://discord.gg/AuYUVs5")
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
    if reloadLevel then
        updateLevelName()
        game.loadLevel(lastLevel, lastDiff)
    end
    
    ui.getElement("game.player").enabled = not game.editing
    ui.getElement("game.editor").enabled = game.editing
end)

game.subscribeEvent(ui.getElement("game.skip"), "buttonClicked", function()
    game.trySkip()
end)

game.subscribeEvent(nil, "canSkip", function(canSkip)
    ui.getElement("game.skip").enabled = canSkip
end)

game.subscribeEvent(ui.getElement("game.playMusic"), "buttonClicked", function()
    game.playing = not game.playing
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
    ui.animateElement("lastStats", "fastFadeOut", function()
        reloadLevel = true
        ui.animateElement("game", "fastFadeIn", nil, nil)
    end, nil)
end)

game.subscribeEvent(ui.getElement("lastStats.player.continue"), "buttonClicked", function()
    ui.animateElement("lastStats", "fastFadeOut", function()
        game.playing = true
        reloadLevel = false
        ui.animateElement("game", "fastFadeIn", nil, nil)
    end, nil)
end)

game.subscribeEvent(ui.getElement("lastStats.editor.continue"), "buttonClicked", function()
    ui.animateElement("lastStats", "fastFadeOut", function()
        reloadLevel = false
        ui.animateElement("game", "fastFadeIn", nil, nil)
    end, nil)
end)

function onLevelSave()
    game.saveLevel(lastLevel, lastDiff)
end
for _, element in ipairs(ui.getElements("lastStats.save")) do game.subscribeEvent(element, "buttonClicked", onLevelSave) end

game.subscribeEvent(nil, "levelChanged", function()
    for _, element in ipairs(ui.getElements("lastStats.save")) do
        if game.changed then
            if not string.ends(element.text, "*") then
                element.width = element.width + 1
                element.text = element.text .. "*"
            end
        else
            if string.ends(element.text, "*") then
                element.text = element.text:sub(1, #element.text - 1)
                element.width = element.width - 1
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

function debugEnterMenu(menu)
	ui.animateElement(nil, "fadeOut", function()
        ui.getElement("debugMenu.toggle").enabled = true;
        ui.animateElement("debugMenu", "fadeIn", nil, nil)
        ui.animateElement(menu, "fadeIn", nil, nil)
    end, nil)
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
        for _, element in ipairs(ui.getElements("music.status.icon")) do
            element.text = "║"
        end
    else
        for _, element in ipairs(ui.getElements("music.status.icon")) do
            element.text = "►"
        end
    end
    for _, element in ipairs(ui.getElements("music.nowPlaying")) do
        element.text = "NOW PLAYING : " .. soundManager.currentMusicName
    end
end)

function gameStarted()
    updateEditModeButton()
    ui.animateElement("mainMenu", "startup", nil, nil)
end
game.subscribeEvent(nil, "gameStarted", gameStarted)

game.subscribeEvent(nil, "gameExited", function()
	game.exitTime = ui.getAnimationPreset("shutdown").time + 0.25
	ui.animateElement(nil, "shutdown", nil, nil)
end)

game.subscribeEvent(nil, "passedOrFailed", function()
	ui.animateElement("game", "fastFadeOut", function()
        ui.animateElement("lastStats", "fadeIn", nil, nil)
    end, nil)
end)

menus = { "mainMenu", "levelSelect", "game", "lastStats" }

function onBack()
	for _, menu in ipairs(menus) do
		if ui.getElement(menu).enabled then
			previousMenu = ui.getPreviousMenu(menu)
			fadeOutAnimation = "fadeOut"
			fadeInAnimation = "fadeIn"
			if menu == "game" then fadeOutAnimation = "fastFadeOut" end
			if previousMenu == "game" then fadeInAnimation = "fastFadeIn" end
			ui.animateElement(menu, fadeOutAnimation, function()
                ui.animateElement(previousMenu, fadeInAnimation, nil, nil)
            end, nil)
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
	
	ui.animateElement("levelSelect", "fastFadeOut", function()
        reloadLevel = true
        ui.animateElement("game", "fastFadeIn", nil, nil)
    end, nil)
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


scoreChange = 0
function updateRealtimeScore(newScore, oldScore)
    scoreChange = scoreChange + newScore - oldScore
	for _, element in ipairs(ui.getElements("realtime.score")) do
		element.text = "SCORE: " .. tostring(newScore)
	end
    for _, element in ipairs(ui.getElements("realtime.score.change")) do
        if not ui.stopElementAnimations(element.id, nil) then scoreChange = newScore - oldScore end
        if scoreChange <= 0 then return end
        element.text = "+" .. tostring(scoreChange)
        element.enabled = true
        ui.animateElement(element.id, "plainFadeOut", 0.5, false, nil, nil)
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
                local tags = element.tags
                tags[i] = accTag
                element.tags = tags
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
                local tags = element.tags
                tags[i] = comboTag
                element.tags = tags
                comboTagChanged = true
            end
        end
        if not comboTagChanged then element.tags = table.insert(element.tags, comboTag) end
    end
end

function updateRealtimeProgress(newValue, _)
    elem = ui.getElement("game.progressBar")
    elem.value = newValue
end
game.subscribeEvent(nil, "progressChanged", updateRealtimeProgress)

function updateRealtimeHealth(newValue, _)
    elem = ui.getElement("game.healthBar")
    elem.value = newValue
end
game.subscribeEvent(nil, "healthChanged", updateRealtimeHealth)

function updateRealtimeCombo(newValue)
    updateCombo("realtime.combo", newValue, scoreManager.accuracy, scoreManager.scores[1])
end
game.subscribeEvent(nil, "comboChanged", updateRealtimeCombo)

function updateRealtimeMaxCombo(newValue)
    updateCombo("realtime.maxCombo", newValue, scoreManager.accuracy, scoreManager.scores[1])
end
game.subscribeEvent(nil, "maxComboChanged", updateRealtimeMaxCombo)

function updateRealtimeScores(scoreIndex, newValue)
    local tag = "realtime."
    
    if scoreIndex == 1 then tag = tag .. "misses"
    elseif scoreIndex == 2 then tag = tag .. "hits"
    elseif scoreIndex == 3 then tag = tag .. "perfectHits"
    end
    
	for _, element in ipairs(ui.getElements(tag)) do
		element.text = tostring(newValue)
	end
end
game.subscribeEvent(nil, "scoresChanged", updateRealtimeScores)
