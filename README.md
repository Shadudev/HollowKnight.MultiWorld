# Hollow Knight Randomizer 4.0 ItemSync Add-on

For randomizer information, see: https://github.com/homothetyhk/RandomizerMod .  
This mod is heavily based on work done by many contributors before, see: https://github.com/CallumMoseley/HollowKnight.RandomizerMod

ItemSync is an addon to the randomizer where any randomized item pickup will be shared between all the players in a "room".

## Features
- Usage of local randomizers, allowing players to play their favorite (multiworld supporting) randomizer mod with all the features it has.
- Reloadable Save File - One can save and reload the save file, reconnect to the server and not lose any progress
- Support for room codes - When connected to the server and readying up, a room code can be specified, which can be use to coordinate readying up with other players
- Concurrent sessions - Once a randomization is generated, a random identifier is included with it which is used to spin up a new session when connecting to the server. This way, multiple concurrent rando sessions can run simultaneously on the same server.

## Getting Started
All mods are available on [Scarab]()!

After installing:
1. Open Hollow Knight and start a new file
2. Choose your settings
3. Sync your seed number and shareable Settings Code (found under `More Randomizer Settings -> Manage Settings Profiles -> Refresh / Apply` accordingly).
4. Begin Randomization and Proceed.
5. Click the ItemSync button.
6. Enter the URL or domain of the server and click Connect.
7. Enter the nickname you would like to use in the "Nickname" field
8. Enter a room code to coordinate with other players, or leave blank for the default room (easier if only one group is trying to rando at once)
9. Click "Ready" to toggle your ready status. The readied players will be shown on the right side.
10. Once everyone you are playing with is connected and readied up, one player should click start, and this will begin the randomizer for everyone. The player who clicks start will provide the server the seed used for server side randomizations.

## Setting up a Server
Currently, I have an instance of the server running at `18.189.16.129`.
If you want to host your own server, follow these instructions:

1. Download `MultiWorldServer.zip` from releases and extract it to wherever you would like to run the server from
2. Port forward 38282 to the machine running the server (look up tutorials online for your router)
3. Run `MultiWorldServer.exe`, and ensure that it is allowed through Windows firewall

## Server Commands

A few useful commands are implemented on the server:
1. `ready` - Gives a list of the current rooms and how many players are ready in each
2. `list` - Lists the currently active game sessions, and the players in each
3. `give <item> <session> <playerid>` - Sends `item` to player `playerId` in session `session`. Use this if an item gets lost somehow (crash or Alt-F4)
