# Press Press Revolution
A rhythm game where you need to use your whole keyboard to play

Inspired by [osu!](https://osu.ppy.sh), [Beatmania](https://en.wikipedia.org/wiki/Beatmania) and [Dance Dance Revolution](https://en.wikipedia.org/wiki/Dance_Dance_Revolution)


## How to play
### Install levels
Unpack levels in `game's folder/levels` to play them

Download levels in my Discord server: https://discord.gg/AuYUVs5

**OR**

DM me in Discord: ConfiG#9039

### Gameplay
Press the corresponding buttons when they hit a white line at the bottom

There are also **holdable notes**, that are indicated with the `│` symbol and that you should **hold**, instead of just pressing,
also they only register **perfect hits** and **misses**

The blue arrows on the left indicate the speed changes and can be ignored

When you hit the note when it's perfectly on the line, you get a **perfect hit**, 1 **combo** point and 10 **score** points multiplied by your current **combo**,
 when you hit it right before or after it has passed the line, you get a **hit**, 1 **combo** point and 5 **score** points multiplied by your current **combo**,
 when you hit it too early or don't hit it at all, you get a **miss** and you break your **combo** *(the **combo** counter resets to 0)*
 
When you get a **hit** or **perfect hit**, you gain some amount of **health**, specified by the creator of the level

When you get a **miss**, you lose some amount of **health**, specified by the creator of the level

When your **health** hits 0, you fail

Your **health** is indicated with a red bar at the top of your screen

There's **perfect combo** and **full combo**

**Perfect combo** is when you don't have any **hits** and/or **misses** and only have **perfect hits**

**Full combo** is when you don't have any **misses**, but you have some **hits**

### Statistics
The stuff below the line is your **statistics**

The "SCORE" line shows your current amount of **score** points

The "ACCURACY" line displays your current **accuracy** in percentage *(the ratio of your **perfect hits**, **hits** and **misses**)*

The "COMBO" line displays your current **combo** and if you have **perfect** or **full** combo

The numbers on the **green**, **yellow** and **red** backgrounds are your current amount of **perfect hits**, **hits** and **misses** respectively

## How to create levels
To begin creating a level, press "EDIT" in the main menu, then press "NEW" at the top,
pause the game by pressing `Escape` on your keayboard, press "SAVE" and "EXIT",
then go to the folder, where your game is located at, go to the `levels` folder,
rename the `unnamed` folder to whatever you'd like your new level to be named,
go to the folder itself, open the `level.txt` file and change the `<unknown>` texts to
the difficulty of your level and the level author name (usually your nickname), save the file,
then get the music for your level, convert it to the `.ogg` format, if it isn't already,
put it in your level folder next to the `level.txt` file and rename it to `music.ogg`.
Now you can begin creating your level

*You can publish your level in my Discord server I linked above*

### Editor controls
- `Mouse Wheel Scroll` - scroll the time

- `PageUp/PageDown` - faster scrolling

- `Any English letter`, `any number` or `-`, `=`, `[`, `]`, `;`, `'`, `,`, `.`, `/` - place a corresponding note at the current time (on the line)

- `Backspace` - remove all the notes at the current time (on the line)

- `Up/down arrows` - increase/decrease the speed by 10 BPM (`+ Shift` - 1 BPM) (`+ Alt` - increase/decrease the guide lines spacing)

- `Left/right arrows` - decrease/increase the health drain *(the value which will be substracted from the **health** on **misses**)* (`+ Shift` - restorage (the opposite of drain))

- `F1/F2` - increase/decrease the initial music offset by 1 ms (`+ Shift` - 10 ms)

## Custom fonts
### How to install custom fonts
Unpack the font folder in `game's folder/resources/fonts`

Open the game and go to `Settings -> Graphics -> Font`, click the buttons on the right to choose the font (usually it's 1: Font name, 2: Font size, 3: Font scale)

### How to create custom fonts
Make an image of any size (1 image pixel = 1 screen pixel) with the characters you want to have in your font, save it as font.png, create mappings.txt and put the character size (`width,height`) on the first line and a list of the characters you have in your font in the direction of left to right and top to bottom on the second without separating them

You can add multiple fonts in your font folder, to do that, create folders in your font folder and put fonts there

**Higly recommended font folder hierarchy:** `[font name]/[font size (can be multiple)]/[font scale (usually x1 and x2 is enough, but you may want to add x3 and/or x4 if your font is too small or if you want to)]`

*You can use the default included font as an example*

## Custom color schemes
### How to install custom color schemes
Unpack the font folder in `game's folder/resources/colors`

Open the game and go to `Settings -> Graphics -> Colors`, click the buttons on the right to choose the color scheme

### How to create custom color schemes
The same as fonts but there's only one file in a folder (`colors.txt`), you can copy one of the default color schemes and use it as a template

## Custom sounds
### How to install custom sounds
Unpack the font folder in `game's folder/resources/audio`

Open the game and go to `Settings -> Audio -> Sounds`, click the buttons on the right to choose the sound pack

### How to create custom sounds
The same as fonts but there are only audio files in a folder, you can copy the default sound pack and use it as a template
