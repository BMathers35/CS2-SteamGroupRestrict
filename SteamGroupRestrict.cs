using System.Net.Http.Json;
using System.Text.Json;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;

namespace SteamGroupRestrict;

[MinimumApiVersion(115)]
public class SteamGroupRestrict : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "SteamGroupRestrict";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "BMathers";
    private int ModuleConfigVersion => 1;
    public required Config Config { get; set; }
    private readonly List<string> _playersList = new List<string>();

    public override void Load(bool hotReload)
    {
        base.Load(hotReload);
        
        Console.WriteLine(" ");
        Console.WriteLine("  ____  _                        ____                       ____           _        _      _   ");
        Console.WriteLine(" / ___|| |_ ___  __ _ _ __ ___  / ___|_ __ ___  _   _ _ __ |  _ \\ ___  ___| |_ _ __(_) ___| |_ ");
        Console.WriteLine(" \\___ \\| __/ _ \\/ _` | '_ ` _ \\| |  _| '__/ _ \\| | | | '_ \\| |_) / _ \\/ __| __| '__| |/ __| __|");
        Console.WriteLine("  ___) | ||  __/ (_| | | | | | | |_| | | | (_) | |_| | |_) |  _ <  __/\\__ \\ |_| |  | | (__| |_ ");
        Console.WriteLine(" |____/ \\__\\___|\\__,_|_| |_| |_|\\____|_|  \\___/ \\__,_| .__/|_| \\_\\___||___/\\__|_|  |_|\\___|\\__|");
        Console.WriteLine("                                                     |_|                                       ");
        Console.WriteLine("					>> Version: " + ModuleVersion);
        Console.WriteLine("					>> Author: " + ModuleAuthor);
        Console.WriteLine(" ");
        
        RegisterEventHandler<EventPlayerConnectFull>(OnClientConnect);
        RegisterEventHandler<EventPlayerDisconnect>(OnClientDisconnect);
        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        AddCommandListener("say", OnPlayerChat);
        AddCommandListener("say_team", OnPlayerChatTeam);
        AddCommand($"css_{Config.GeneralSettings.CheckCommand}", "Check the Steam group join status.", PlayerGroupCheck);
        
    }
    
    public void OnConfigParsed(Config config)
    {
        if (config.Version < ModuleConfigVersion)
        {
            Console.WriteLine($"[SteamGroupRestrict] You are using an old configuration file. Version you are using:{config.Version} - New Version: {ModuleConfigVersion}");
        }

        Config = config;
    }

    private HookResult OnClientConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        
        CCSPlayerController player = @event.Userid;

        if (player == null || !player.IsValid || player.IsBot ||
            !IsValidConfigString(Config.GeneralSettings.SteamApiKey) || 
            !IsValidConfigString(Config.GeneralSettings.SteamGroupId))
            return HookResult.Continue;
        
        string steamid = player.SteamID.ToString(); // We save the steamid in a string variable so we can use it in the task.
        _ = Task.Run(async () => await CheckPlayerGroups(steamid)); // Since this is a task, do not pass ccsplayercontroller to it.
        return HookResult.Continue;
        
    }

    private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {   
        CCSPlayerController player = @event.Userid;

        if (player == null || !player.IsValid || player.IsBot ||
            !IsValidConfigString(Config.GeneralSettings.SteamApiKey) || 
            !IsValidConfigString(Config.GeneralSettings.SteamGroupId))
            return HookResult.Continue;
        
        string steamid = player.SteamID.ToString();
        _ = Task.Run(async () => await CheckPlayerGroups(steamid));
        return HookResult.Continue;
        
    }

    private HookResult OnClientDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        
        CCSPlayerController player = @event.Userid;

        if (player == null || !player.IsValid || player.IsBot ||
            !IsValidConfigString(Config.GeneralSettings.SteamApiKey) || 
            !IsValidConfigString(Config.GeneralSettings.SteamGroupId))
            return HookResult.Continue;

        // string steamId64 = new SteamID(player.SteamID).SteamId64.ToString(); // No need to use the steamid constructor when the steamid is already available.
        string steamId64 = player.SteamID.ToString();

        if (_playersList.Contains(steamId64))
        {
            _playersList.Remove(steamId64);
        }
        
        return HookResult.Continue;
    }

    private HookResult OnPlayerChat(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || player.IsBot) return HookResult.Continue;
        
        string command = info.GetArg(1);
        if (!command.StartsWith("!") && !command.StartsWith("/"))
        {
            return HookResult.Continue;   
        }

        if (!Config.BlockedCommands.Any() || !Config.BlockedCommands.Contains(command))
        {
            return HookResult.Continue;
        }

        string steamId64 = new SteamID(player.SteamID).SteamId64.ToString();

        if (_playersList.Contains(steamId64))
        {
            return HookResult.Continue;
        }
        // Else is not needes here, since you're already returning if the player is in the list.
        // So i removed it for less nesting.

        player.PrintToChat(ReplaceTags($"{Config.GeneralSettings.Prefix} {Config.Messages.Unauthorized}"));
        return HookResult.Handled;
    }

    private HookResult OnPlayerChatTeam(CCSPlayerController? player, CommandInfo info)
    {

        if (player == null || !player.IsValid || player.IsBot) return HookResult.Continue;

        string command = info.GetArg(1);
        if (!command.StartsWith("!") && !command.StartsWith("/"))
        {
            return HookResult.Continue;
        }

        if (!Config.BlockedCommands.Any() || !Config.BlockedCommands.Contains(command))
        {
            return HookResult.Continue;
        }

        string steamId64 = new SteamID(player.SteamID).SteamId64.ToString();

        if (_playersList.Contains(steamId64))
        {
            return HookResult.Continue;
        }

        player.PrintToChat(ReplaceTags($"{Config.GeneralSettings.Prefix} {Config.Messages.Unauthorized}"));
        return HookResult.Handled;
    }

    // This method can easily be async since you're running it in a task.
    private async Task<bool> CheckPlayerGroups(string steamId)
    {
        
        if (!IsValidConfigString(Config.GeneralSettings.SteamGroupId) || !IsValidConfigString(Config.GeneralSettings.SteamGroupId))
            return false;

        if (string.IsNullOrEmpty(steamId))
        {
            return false;
        }

        if (_playersList.Contains(steamId))
        {
            return true;
        }

        string apiUrl = $"https://api.steampowered.com/ISteamUser/GetUserGroupList/v1/?key={Config.GeneralSettings.SteamApiKey}&steamid={steamId}";

        try
        {
            /* Perhaps look into RestClient, it's (imo) a good http client nuget package.
             * It adds a extra package to upload, which is something that's up to you.
             * However it reduces the amount of code you have to write for a http request.
            */

            using var httpClient = new HttpClient(); // Add using here to dispose the object after usage.

            JsonElement jsonData = await httpClient.GetFromJsonAsync<JsonElement>(apiUrl);
            dynamic? response = jsonData.Deserialize<dynamic>();

            if (!jsonData.TryGetProperty("response", out var responseProperty) ||
                responseProperty.ValueKind != JsonValueKind.Object)
            {
                Console.WriteLine("[SteamGroupRestrict] An error occurred: Response is null or not an object.");
                return false;
            }
            bool success = responseProperty.GetProperty("success").GetBoolean();

            if (!success)
            {
                return false;
            }

            foreach (var group in responseProperty.GetProperty("groups").EnumerateArray())
            {
                string? groupId = group.GetProperty("gid").GetString();

                if (groupId != Config.GeneralSettings.SteamGroupId)
                {
                    continue;
                }

                _playersList.Add(steamId);
                return true;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"[SteamGroupRestrict] An error occurred: {e.Message}");
            return false;
        }

        Console.WriteLine("[SteamGroupRestrict] An error occurred: An unknown error occurred. Returning false.");
        return false;
    }

    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    private void PlayerGroupCheck(CCSPlayerController? player, CommandInfo info)
    {
        
        if (player == null || !player.IsValid || player.IsBot) return;
        if (!IsValidConfigString(Config.GeneralSettings.SteamApiKey) || !IsValidConfigString(Config.GeneralSettings.SteamGroupId)) return;

        string steamid = player.SteamID.ToString();
        Task.Run(async () =>
        {
            bool result = await CheckPlayerGroups(steamid); // Run it in a task, so it doesn't block the server thread.

            Server.NextFrame(() =>
            {
                
                if (result)
                {
                    player.PrintToChat(ReplaceTags($"{Config.GeneralSettings.Prefix} {Config.Messages.JoinedGroup}"));
                }
                else
                {
                    player.PrintToChat(ReplaceTags($"{Config.GeneralSettings.Prefix} {Config.Messages.NotJoinedGroup}"));
                }
                
            });
            
            return Task.CompletedTask;
            
        });
    }
    

    // This method should be static, since it doesn't use any instance members.
    private static string ReplaceTags(string message)
    {
        /* You can use a dictionary for this, so you don't have to use two arrays
         * or use a method like this: https://github.com/WidovV/cs2-connectionlogs/blob/9ba36f4ee11c60570e5d9d63c8e8aeffffa2c92d/ConnectionLogs/CFG.cs#L12
        */
        string[] colorPatterns =
        {
            "{Default}", "{White}", "{Darkred}", "{Green}", "{LightYellow}", "{LightBlue}", "{Olive}", "{Lime}", "{Red}",
            "{LightPurple}", "{Purple}", "{Grey}", "{Yellow}", "{Gold}", "{Silver}", "{Blue}", "{DarkBlue}", "{BlueGrey}", 
            "{Magenta}", "{LightRed}", "{Orange}"
        };
        string[] colorReplacements =
        {
            "\x01", "\x01", "\x02", "\x04", "\x09", "\x0B", "\x05", "\x06", "\x07", "\x03", "\x0E", "\x08", "\x09", "\x10",
            "\x0A", "\x0B", "\x0C", "\x0A", "\x0E", "\x0F", "\x10"
        };

        for (var i = 0; i < colorPatterns.Length; i++)
            message = "\u200e" + message.Replace(colorPatterns[i], colorReplacements[i]);

        return message;
        
    }

    private static bool IsValidConfigString(string value) => !string.IsNullOrEmpty(value) && value != "-"; // This is a "lambda expression body method"
}