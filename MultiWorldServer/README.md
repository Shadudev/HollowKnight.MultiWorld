# MultiWorld / ItemSync Server

Both MultiWorld and ItemSync games can be done with a single dedicated server instance.  
The server archive comes with a few additional files which are crucial dependencies of it.  

Note that these should not be updated unless explicitly mentioned by any other developer of the mods.  

## Setting Up a Local Server

1. Download the latest server available in the releases page.  
2. Extract the archive into a folder.  
3. Run the `MultiWorldServer.exe` once.
   Normally, it will create a file named `config.json` with the default settings.  
   If you have a previously used config file, you can copy it to the same folder as the server executable and it will be loaded.
4. Follow the next section to update your config file.  
   If you had uploaded the config file, close the server and start it again.

### Config File Instructions

A default config file may look like this:

```json
{
  "ListeningIP": "0.0.0.0",
  "ListeningPort": 38281,
  "ServerName": "Default Server"
}
```

- `ListeningIP` - by default, `0.0.0.0` means that the server will listen on all the available network interfaces.
  My recommendation would be to change it to a specific IP which you wish to give others to connect to, e.g. Hamachi / LAN / ...
- `ListeningPort` - choose whichever port you wish, just remember to update the port forward settings in your router if you wish for people to connect to you via WAN.
- `ServerName` - shows up after filling your server IP and pressing `Connect` in the ItemSync/MultiWorld mod menus.

## Updating a Local Server

It is a reason to celebrate, a new server version was released.  
Thankfully, it isn't complex to update your current server.

1. Download the latest server available in the releases page.  
2. Extract the archive into a folder.
3. Copy the `config.ini` file into the new server folder.
   If you wish to keep the logs/spoilers of your previously generated games, move the desired folders to the new server folder.  
4. If your older-version server is currently running, close it and start the new server executable.
 
