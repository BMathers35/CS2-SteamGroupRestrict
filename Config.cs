using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace SteamGroupRestrict;

public class GeneralSettings
{
    
    [JsonPropertyName("SteamGroupId")]
    public string SteamGroupId { get; set; } = "";
    
    [JsonPropertyName("SteamApiKey")]
    public string SteamApiKey { get; set; } = "";
    
    [JsonPropertyName("Prefix")]
    public string Prefix { get; set; } = "{Blue}[SGR]";
    
    [JsonPropertyName("CheckCommand")]
    public string CheckCommand { get; set; } = "group_check";
    
}

public class Messages
{
    
    [JsonPropertyName("Unauthorized")]
    public string Unauthorized { get; set; } = "{Red}You must have joined our steam group to use this command.";
    
    [JsonPropertyName("NotJoinedGroup")]
    public string NotJoinedGroup { get; set; } = "{Red}You don't seem to have joined our Steam group.";
    
    [JsonPropertyName("JoinedGroup")]
    public string JoinedGroup { get; set; } = "{Green}Congratulations! You have joined our Steam group and can start using commands.";
    
}

public class Config : IBasePluginConfig
{
    
    [JsonPropertyName("GeneralSettings")]
    public GeneralSettings GeneralSettings { get; set; } = new GeneralSettings();
    
    [JsonPropertyName("Messages")]
    public Messages Messages { get; set; } = new Messages();
    
    [JsonPropertyName("BlockedCommands")]
    public List<string> BlockedCommands { get; set; } = new List<string>();

    [JsonPropertyName("ConfigVersion")] 
    public int Version { get; set; } = 1;

}