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
            Config.GeneralSettings.SteamApiKey == "-" || 
            Config.GeneralSettings.SteamGroupId == "-")
            return HookResult.Continue;
        
        _ = Task.Run(() => CheckPlayerGroups(player));
        return HookResult.Continue;
        
    }

    private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        
        CCSPlayerController player = @event.Userid;

        if (player == null || !player.IsValid || player.IsBot ||
            Config.GeneralSettings.SteamApiKey == "-" || 
            Config.GeneralSettings.SteamGroupId == "-")
            return HookResult.Continue;
        
        _ = Task.Run(() => CheckPlayerGroups(player));
        return HookResult.Continue;
        
    }

    private HookResult OnClientDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        
        CCSPlayerController player = @event.Userid;

        if (player == null || !player.IsValid || player.IsBot ||
            Config.GeneralSettings.SteamApiKey == "-" || 
            Config.GeneralSettings.SteamGroupId == "-")
            return HookResult.Continue;

        string steamId64 = new SteamID(player.SteamID).SteamId64.ToString();

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
        if (command.StartsWith("!") || command.StartsWith("/"))
        {
            
            if (Config.BlockedCommands.Any() && Config.BlockedCommands.Contains(command))
            {
            
                string steamId64 = new SteamID(player.SteamID).SteamId64.ToString();
            
                if (_playersList.Contains(steamId64))
                {
                
                    return HookResult.Continue;
                
                }
                else
                {
                    
                    player.PrintToChat(ReplaceTags($"{Config.GeneralSettings.Prefix} {Config.Messages.Unauthorized}"));
                    return HookResult.Handled;
                
                }
            
            }
            
        }
        
        return HookResult.Continue;
        
    }

    private HookResult OnPlayerChatTeam(CCSPlayerController? player, CommandInfo info)
    {
        
        if (player == null || !player.IsValid || player.IsBot) return HookResult.Continue;
        
        string command = info.GetArg(1);
        if (command.StartsWith("!") || command.StartsWith("/"))
        {
            
            if (Config.BlockedCommands.Any() && Config.BlockedCommands.Contains(command))
            {
            
                string steamId64 = new SteamID(player.SteamID).SteamId64.ToString();
            
                if (_playersList.Contains(steamId64))
                {
                
                    return HookResult.Continue;
                
                }
                else
                {
                
                    player.PrintToChat(ReplaceTags($"{Config.GeneralSettings.Prefix} {Config.Messages.Unauthorized}"));
                    return HookResult.Handled;
                
                }
            
            }
            
        }
        
        return HookResult.Continue;
        
    }

    private bool CheckPlayerGroups(CCSPlayerController? player)
    {

        if (player == null || !player.IsValid || player.IsBot) return false;
        
        if (Config.GeneralSettings.SteamApiKey == "-" || Config.GeneralSettings.SteamGroupId == "-")
            return false;
        
        string steamId = new SteamID(player.SteamID).SteamId64.ToString();

        if (_playersList.Contains(steamId))
        {
            
            return true;
            
        }
        else
        {
            
            string apiUrl = $"https://api.steampowered.com/ISteamUser/GetUserGroupList/v1/?key={Config.GeneralSettings.SteamApiKey}&steamid={steamId}";

            try
            {
                
                var httpClient = new HttpClient();
                Task.Run(async () =>
                {

                    JsonElement jsonData = await httpClient.GetFromJsonAsync<JsonElement>(apiUrl);
                    dynamic? response = jsonData.Deserialize<dynamic>();

                    if (jsonData.TryGetProperty("response", out var responseProperty) &&
                        responseProperty.ValueKind == JsonValueKind.Object)
                    {

                        bool success = responseProperty.GetProperty("success").GetBoolean();
                        if (success)
                        {
                            
                            foreach (var group in responseProperty.GetProperty("groups").EnumerateArray())
                            {
                            
                                string? groupId = group.GetProperty("gid").GetString();
                                
                                if (groupId == Config.GeneralSettings.SteamGroupId)
                                {
                                    _playersList.Add(steamId);
                                    return true;
                                }
                            
                            }
                            
                        }

                    }

                    return false;
                    
                });

            }
            catch (Exception e)
            {
                
                Console.WriteLine($"[SteamGroupRestrict] An error occurred: {e.Message}");
                return false;
                
            }
            
        }

        return false;

    }

    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    private void PlayerGroupCheck(CCSPlayerController? player, CommandInfo info)
    {
        
        if (player == null || !player.IsValid || player.IsBot) return;
        if (Config.GeneralSettings.SteamApiKey == "-" || Config.GeneralSettings.SteamGroupId == "-") return;

        Task.Run(() =>
        {
            
            Server.NextFrame(() =>
            {
                
                if (CheckPlayerGroups(player))
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
    
    private string ReplaceTags(string message)
    {
        
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
    
}