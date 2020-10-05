# ![Press Press Revolution](banner.png)
A rhythm game where you need to use your whole keyboard to play

Gameplay inspired by [osu!](https://osu.ppy.sh), [Beatmania](https://en.wikipedia.org/wiki/Beatmania) and [Dance Dance Revolution](https://en.wikipedia.org/wiki/Dance_Dance_Revolution)

Visuals inspired by [Cogmind](https://store.steampowered.com/app/722730/Cogmind)

## Screenshots
<details>
 <summary>hinkik - Time Leaper</summary>
 
 ![hinkik - Time Leaper](screenshots/Screenshot-0.png)
</details>

<details>
 <summary>LeaF - Aleph-0</summary>
 
 ![LeaF - Aleph-0](screenshots/Screenshot-1.png)
</details>


## How to play
### Install levels
Unpack levels in `game's folder/levels` to play them

(Temporary) Download levels in the [Discord server](https://discord.gg/AuYUVs5)

### Gameplay
Press the corresponding buttons when they hit a white line at the bottom

There are also **holdable notes**, that are indicated with the `│` symbol and that you should **hold**, instead of just pressing,
also they only register **perfect hits** and **misses**

The blue arrows on the left indicate the speed changes and can be ignored

When you hit the note when it's perfectly on the line (or near it, depending on the current BPM), you get a **perfect hit**, 1 **combo** point and 10 **score** points multiplied by your current **combo**,
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
pause the game by pressing `Escape` on your keyboard, press "SAVE" and "EXIT",
then go to the folder, where your game is located at, go to the `levels` folder,
rename the `unnamed` folder to whatever you'd like your new level to be named,
go to the folder itself, open the `level.txt` file and change `<unknown>` to the level author name (usually your nickname) 
(you can also edit that in the template (levels/\_template) level so that you don't have to do that every time), save the file,
then get the music for your level, convert it to the `.ogg` format, if it isn't already,
put it in your level folder next to the `level.txt` file and rename it to `music.ogg`.
Now you can begin creating your level

*You can publish your levels in the Discord server linked above*

### Editor controls
- `Mouse Wheel Scroll` - scroll the time

- `Page Up/Down` - scroll the time by 10 offset

- `Drag the progress bar line at the top` - scroll the time with your mouse

- `Any English letter`, `any number` or `-`, `=`, `[`, `]`, `;`, `'`, `,`, `.`, `/` - place a corresponding note on the line (`+ Shift` - place a hold note)

- `Backspace` - remove all the notes on the line

- `Up/down arrows` - increase/decrease the speed by 10 BPM (`+ Shift` - 1 BPM) (`+ Alt` - increase/decrease the guide lines spacing)

#### *In case you want to use the keyboard instead of the UI buttons*
- `Left/right arrows` - decrease/increase the health drain *(the value which will be subtracted from the **health** on **misses**)* (`+ Shift` - restorage (the opposite of drain))

- `F1/F2` - increase/decrease the initial music offset by 1 ms (`+ Shift` - 10 ms)

### Creator guidelines
Note: These are just guidelines and you don't _have_ to follow them but it's recommended

#### Use multiplications of the current BPM
For example, if the *perfect* BPM at the current place is 120, then make it 240, 360, 480 etc.
to make this place fit the speed of the song and (sometimes) make it easier to read

#### Use the keyboard "lines" for (at least obviously) different parts and keys/"positions" for the current note (or just draw pars that seem to fit to the current place)
##### Lines

For example, if the current part of the song/level is going on the ASDF line and then the instrument changes, 
the line should change too (for example too QWER)

There are 4 lines in total you can use (1234, QWER, ASDF and ZXCV)

##### Notes

Use the keyboard keys to show the *relation* of the current note to the next note, 
a specific key doesn't always have to be one exact note

That means that if the next note is higher than the current note, then put the next in-game note one key to the right 
(for example if the current note is 'g', then the next note will be 'h')

Or if the next note is higher than just higher than the current note, place the in-game note two or three or how much 
keys you think it's relevant in the current situation

Same goes for lower notes but place them to the left

#### Leave some space before putting the first note
Give the players time to prepare and see what they need to press, usually about 1-3 seconds is enough

If the song starts right away you can start the map at the next part of the song or the next after that or 
where you think it's relevant

#### Don't make the player press more than 2 keys at the same time
Because [keyboard ghosting](https://www.google.com/search?q=keyboard+ghosting)

## Custom fonts
### How to install custom fonts
Unpack the font folder in `game's folder/resources/fonts`

Open the game and go to `Settings -> Graphics -> Font`, click the buttons on the right to choose the font (usually it's 1: Font name, 2: Font size, 3: Font scale)

### How to create custom fonts
Make an image of any size (1 image pixel = 1 screen pixel) with the characters you want to have in your font, save it as font.png, create mappings.txt and put the character size (`width,height`) on the first line and a list of the characters you have in your font in the direction of left to right and top to bottom on the second without separating them

You can add multiple fonts in your font folder, to do that, create folders in your font folder and put fonts there

**Highly recommended font folder hierarchy:** `[font name]/[font size (can be multiple)]/[font scale (usually x1 and x2 is enough, but you may want to add x3 and/or x4 if your font is too small or if you want to)]`

*You can use the default included font as an example*

## Custom color schemes
### How to install custom color schemes
Unpack the font folder in `game's folder/resources/colors`

Open the game and go to `Settings -> Graphics -> Colors`, click the buttons on the right to choose the color scheme

### How to create custom color schemes
The same as fonts but there's only one file in a folder (`colors.txt`), you can look for the default color schemes for an example

(`# some text` is a comment and is ignored by the game)

## Custom sounds
### How to install custom sounds
Unpack the font folder in `game's folder/resources/audio`

Open the game and go to `Settings -> Audio -> Sounds`, click the buttons on the right to choose the sound pack

### How to create custom sounds
The same as fonts but there are only audio files in a folder, you can copy the default sound pack and use it as a template
