# BeyondBedtime

Take control of your sleep schedule in Stardew Valley! **BeyondBedtime** is a SMAPI mod that allows you to stay up past the dreaded 2:00 AM pass-out time. The mod seamlessly extends the night, keeps the HUD clock ticking, smoothly transitions the nighttime lighting, and is fully multiplayer compatible.

## Features

- **Customizable Pass-Out Time:** Choose exactly when your farmer should pass out. Want to stay up until 4:00 AM to finish the Skull Cavern? Now you can!
- **Seamless HUD Clock:** The in-game HUD clock continues to tick past 2:00 AM in 10-minute increments, just like vanilla time.
- **Lighting Transitions:** The mod dynamically handles the transition of ambient and window lighting during extended nights, including optional settings for morning light curves.
- **Daily Toggle Hotkey:** Pressed for time? Use a hotkey to completely disable passing out for the current day!
- **Generic Mod Config Menu (GMCM) Support:** Fully integrated with GMCM so you can change all settings in-game without editing config files.
- **Multiplayer Compatible:** Features a decentralized architecture. Configurations are strictly local to each player. This means you can play with friends who have different pass-out times—or even friends who don't have the mod installed at all! The game will simply wait for everyone to go to sleep or pass out.

## Installation

1. Install the latest version of [SMAPI](https://smapi.io/).
2. (Optional but highly recommended) Install [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) to easily edit settings in-game.
3. Download this mod and unzip it into your `Stardew Valley/Mods` folder.
4. Run the game using SMAPI!

## Configuration

If you have **Generic Mod Config Menu** installed, you can configure the mod directly from the Stardew Valley title screen. Otherwise, nach you start the game once, you can edit the `config.json` file created in the mod's folder.

### Available Options

* **Mod Enabled:** Master toggle to enable or disable the entire mod's functionality. (Default: `true`)
* **Pass-Out Time:** The exact time you will pass out. You can set this past 2:00 AM or even earlier if you prefer. (Default: `2:00 AM`, Max.: `6:00 AM`)
* **Toggle Pass-Out Key:** The hotkey to press to toggle your ability to pass out on/off for the current day. (Default: `RightAlt`)
* **Morning Light Start Time:** The time when the morning light transition should begin. Set to `Off` to disable. (Default: `Off`)
* **Morning Light Curve Power:** Adjusts how quickly the morning light fades in. `1.0` is a linear fade, `2.0` is a squared curve (starts slower, finishes faster). (Default: `2.0`)

## Multiplayer Details

BeyondBedtime is built from the ground up to respect Stardew Valley's multiplayer mechanics:
- **Client-Side Configurations:** Your settings (pass-out time, lighting, hotkeys) only apply to you. Player A can pass out at 3:00 AM while Player B passes out at 5:00 AM without conflict.
- **Host Agnostic:** You can use this mod even if the host or other players do not have it installed. The server will automatically wait for the last active player to go to bed or pass out before ending the day.
- **Synchronized Virtual Clock:** In multiplayer, the extended clock respects game pauses and cutscenes just like vanilla time, preventing desynchronization.

## Compatibility
- Requires Stardew Valley 1.6+
- Requires SMAPI 4.0.0+
