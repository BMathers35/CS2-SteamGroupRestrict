# CS2-SteamGroupRestrict

### Description
This CSSharp plugin restricts chat commands to players who have not joined the steam group

### Requirments
- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp/) **Tested on v88**
- [Steam Api Key](https://steamcommunity.com/dev/apikey)

### Features
- **Restrict chat commands**
- **Steam group member control**

## Usage
This plugin checks for players who joined the steam group on the first login to the server, if the player joined the steam group on the first login to the server, they will be added to the allowed list.

Players who have not joined the steam group, i.e. are not on the allowed list, will be rechecked each time they respawn.

If a player logs out of the server, the player will be deleted from the allowed list.


### Installation
- Put the plugin files in the "/addons/counterstrikesharp/plugins/" folder.
- When you start the plugin, the configuration file will be automatically created in the path "/addons/counterstrikesharp/configs/plugins/SteamGroupRestrict". Open the file and fill in the fields according to your needs.
- After you have made all your settings, simply restart the plugin.

```json
{
  "GeneralSettings": {
    // Enter the Group ID of your Steam Group here (found on the Edit Group Profile page).
    "SteamGroupId": "10599306",
    // Obtain an API Key from this page https://steamcommunity.com/dev/apikey and fill in this field.
    "SteamApiKey": "3B90E1F828035F66D53DE0226ADA4BBA",
    "Prefix": "{Blue}[SGR]"
  },
  "Messages": {
    "Unauthorized": "{Red}You must have joined our steam group to use this command.",
    "NotJoinedGroup": "{Red}You don't seem to have joined our Steam group.",
    "JoinedGroup": "{Green}Congratulations! You have joined our Steam group and can start using commands."
  },
  "BlockedCommands": [
      // Commands to be added to this list must be preceded by ! or /. 
      // You can add as many commands as you want according to the examples below.
      "!knife",
      "!ws",
      "/knife",
      "/ws"
  ],
  "ConfigVersion": 1
}
```

### Commands

- **!group_check** - Check if you have joined the Steam group. (The command name can be edited from the configuration file)


### Roadmap
- Nothing for now

### Default config.json file;
```json
{
  "GeneralSettings": {
    "SteamGroupId": "-",
    "SteamApiKey": "-",
    "Prefix": "{Blue}[SGR]"
  },
  "Messages": {
    "Unauthorized": "{Red}You must have joined our steam group to use this command.",
    "NotJoinedGroup": "{Red}You don't seem to have joined our Steam group.",
    "JoinedGroup": "{Green}Congratulations! You have joined our Steam group and can start using commands."
  },
  "BlockedCommands": [],
  "ConfigVersion": 1
}
```
