# Hollow Knight Randomizer 3.0 MultiWorld Add-on

For randomizer information, see: https://github.com/ShaduDev/HollowKnight.RandomizerMod .  
Latest RandomizerMod release can be found on the [Hollow Knight Speedrun Community discord](https://discord.gg/3JtHPsBjHD) - #rando-announcements text channel!  
This mod is heavily based on work done by many contributors before, see: https://github.com/CallumMoseley/HollowKnight.RandomizerMod

A multiworld is an addon to the randomizer where items are not only scattered throughout your file, but through any number of linked game files. When picking up an item that belongs to another player, it is sent over the internet and they receive it in their game. This allows co-operative randomizer playthroughs, where you may need to pick up each other's progression to go forward.

## Features
- Usage of local randomizers, allowing players to play their favorite (multiworld supporting) randomizer mod with all the features it has.
- Per player settings - Each player has full access to all settings of the randomizer, meaning they make their own choice of which item pools to randomize, which skips are allowed in logic for their world, whether items/areas/rooms are randomized, and starting location
- Nicknames - Players can set a nickname for themselves which will show up when picking up their items in other worlds
- Detailed Spoiler Logs - complete spoiler logs with all the players' info.
- Reloadable Save File - One can save and reload the save file, reconnect to the server and not lose any progress
- Support for room codes - When connected to the server and readying up, a room code can be specified, which can be use to coordinate readying up with other players
- Concurrent sessions - Once a randomization is generated, a random identifier is included with it which is used to spin up a new session when connecting to the server. This way, multiple concurrent rando sessions can run simultaneously on the same server
- Rejoinable sessions - Allows a player to retrieve their multiworld data from the server in case they have crashed before saving for the first time
- (Mostly) compatible with BingoUI - Counters may pop up at strange times, but they should be correct including items sent from other players

## Getting Started
1. Install the Modding API, Serecore, Vasi and Benchwarp if you haven't already (this can be done through the mod installer: https://radiance.host/mods/ModInstaller.exe)
2.1. Download `MultiWorldMod.zip` from the releases page on github: https://github.com/ShaduDev/HollowKnight.MultiWorld/releases
2.2. Download `RandomizerMod.zip` from the releases page on github: https://github.com/Shadudev/HollowKnight.RandomizerMod/releases/
3. Extract files from downloaded zip files and copy them to `Hollow Knight/hollow_knight_Data/Managed/Mods`.

This is all that is needed in terms of setup. To play multiworld:

1. Open Hollow Knight and start a new file
2. Enter the URL address of the server, and set Multiworld to "Yes". This will connect to the server. If the connection fails, Multiworld will stay set to "No"
3. Enter the nickname you would like to use in the "Nickname" field
4. Enter a room code to coordinate with other players, or leave blank for the default room (easier if only one group is trying to rando at once)
5. Configure your randomizer settings however you would like
6. Click "Ready" to toggle your ready status. The buttton will show how many players in the room are currently ready. You can keep changing your settings till game starts. Click again to become unready
7. Once everyone you are playing with is connected and readied up, one player should click start, and this will begin the randomizer for everyone. The player who clicks start will provide the server the seed used for server side randomizations.

## Rejoining a Game
For cases where you start a Multiworld and one or more players crash before saving their game. 

For players who no longer have the save file, go to the file creation screen, set the same settings you've had before, connect to the same Multiworld server and click "Rejoin". This will allow you to continue playing the multiworld game as normal. 

To avoid this happening, I recommend benchwarping immediately once you load in, because this will save and ensure the file is created.

## Leaving a Game Early
For cases where you start a Multiworld and someone has to leave. Simply pause the game, click "Eject From MultiWorld" and you're good to go!

## Setting up a Server
Currently, I have an instance of the server running at 18.188.208.46.
If you want to host your own server, follow these instructions:

1. Download `MultiWorldServer.zip` from releases and extract it to wherever you would like to run the server from
2. Port forward 38281 to the machine running the server (look up tutorials online for your router)
3. Run `MultiWorldServer.exe`, and ensure that it is allowed through Windows firewall

## Server Commands

A few useful commands are implemented on the server:
1. `ready` - Gives a list of the current rooms and how many players are ready in each
2. `list` - Lists the currently active game sessions, and the players in each
3. `give <item> <session> <playerid>` - Sends `item` to player `playerId` in session `session`. Use this if an item gets lost somehow (crash or Alt-F4)

## Future Plans/Known Issues
- Others' charms notch costs' icons are not displayed in shops
- Picking up someone else's White Fragment before having one yourself causes a glitched Kingsoul charm to appear in your inventory
- RandoMapMod 0.3.11 is mostly supported, newer versions aren't due to old MultiWorld specific behavior. Feel free to check the official repo for updates https://github.com/CaptainDapper/HollowKnight.RandoMapMod
