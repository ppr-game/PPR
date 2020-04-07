# Press Press Revolution
A rhythm game where you need to use your whole keyboard to play
Inspired by [osu!](https://osu.ppy.sh) and [Dance Dance Revolution](https://en.wikipedia.org/wiki/Dance_Dance_Revolution)

## How to play
### Gameplay
Press the corresponding buttons when they hit a white line at the bottom

The blue arrows at the left indicate speed changes and can be ignored

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
### Editor controls
- `Any English letter`, `any number` or `-`, `=`, `[`, `]`, `;`, `'`, `,`, `.`, `/` - place a corresponding note at the current time (on the line)
- `Backspace` - remove all the notes at the current time (on the line)
- `Up/down arrows` - increase/decrease the speed by 10 BPM (`+ Shift` - 1 BPM)
- `Left/right arrows` - decrease/increase the health drain *(the value which will substracted from the **health** on **misses**)* (`+ Shift` - restorage (the opposite of drain))
