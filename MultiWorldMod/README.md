# Hollow Knight Randomizer 4 MultiWorld Add-on

For randomizer information, see: https://github.com/homothetyhk/RandomizerMod . 
Latest RandomizerMod release can be found on Scarab or the [Hollow Knight Speedrun Community discord](https://discord.gg/3JtHPsBjHD) - #rando-announcements text channel!  
This mod is heavily based on work done by many contributors before, see: https://github.com/CallumMoseley/HollowKnight.RandomizerMod

MultiWorld is an addon to the randomizer where items are not only scattered throughout your file, but through any number of linked game files. When picking up an item that belongs to another player, they receive it in their game. This allows co-operative randomizer playthroughs where you may need to pick up each other's progression to go forward.

## Features
- Usage of local randomizers, allowing players to play their favorite randomizer mods with all the features it has.
- Per player settings - Each player has full access to all settings of their randomizer.
- Nicknames - Players can set a nickname for themselves which will show up when picking up their items in other worlds
- Reloadable Save File - One can save and reload the save file, reconnect to the server and not lose any progress
- Support for room codes - When connected to the server and readying up, a room code can be specified, which can be use to coordinate readying up with other players. Room name can be reused after the multiworld game is generated.
And Server Features:
- Concurrent sessions - Once a randomization is generated, a random identifier is included with it which is used to spin up a new session when connecting to the server. This way, multiple concurrent rando sessions can run simultaneously on the same server
- Rejoinable sessions - Allows a player to retrieve their multiworld data from the server in case they have crashed before saving for the first time

## Getting Started
All mods are available on [Scarab](https://github.com/fifty-six/Scarab/releases/latest)!
By installing MultiWorld, all dependencies will be automatically installed!

After installing:
1. Open Hollow Knight and start a new file
2. Choose your settings
3. Sync your seed number and shareable Settings Code (found under `More Randomizer Settings -> Manage Settings Profiles -> Refresh / Apply` accordingly).
4. Begin Randomization and Proceed.
5. Click the MultiWorld button.
6. Enter the URL or domain of the server and click Connect.
7. Enter the nickname you would like to use in the "Nickname" field
8. Enter a room code to coordinate with other players, or leave blank for the default room (easier if only one group is trying to rando at once)
9. Click "Ready" to toggle your ready status. The readied players will be shown on the right side.
10. Once everyone you are playing with is connected and readied up, one player should click start, and this will begin the randomizer for everyone. The player who clicks start will provide the server the seed used for server side randomizations.

## Rejoining a Game
For cases where you start a Multiworld and one or more players crash before saving their game. 

For players who no longer have the save file, go to the file creation screen, set the same settings you've had before, connect to the same Multiworld server and click "Rejoin". This will allow you to continue playing the multiworld game as normal. 

To avoid this happening, I recommend benchwarping immediately once you load in, because this will save and ensure the file is created.

## Leaving a Game Early
For cases where you start a Multiworld and someone has to leave. Simply pause the game, click "Eject From MultiWorld" and you're good to go!

## Setting up a Server
Currently, I have an instance of the server running at 18.189.16.129.
If you want to host your own server, follow these instructions:

1. Download `MultiWorldServer.zip` from releases and extract it to wherever you would like to run the server from
2. Port forward 38281 to the machine running the server (look up tutorials online for your router)
3. Run `MultiWorldServer.exe`, and ensure that it is allowed through Windows firewall

## Server Commands

A few useful commands are implemented on the server:
1. `ready` - Gives a list of the current rooms and how many players are ready in each
2. `list` - Lists the currently active game sessions, and the players in each
3. `give <item> <session> <playerid>` - Sends `item` to player `playerId` in session `session`. Use this if an item gets lost somehow (crash or Alt-F4)

## Future Features
- Detailed Spoiler Logs - complete spoiler logs with all the players' info.