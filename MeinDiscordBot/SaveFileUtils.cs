using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

namespace MeinDiscordBot;
public static class SaveFileUtils {
    private static readonly JsonSerializerOptions _options = new() { WriteIndented = true };
    private static readonly string _fileName = "save.json";

    public static async Task SimpleWrite(object obj) {
        Bot.BotLog("Speichere Save...");
        await using var fileStream = File.Create(_fileName);
        await JsonSerializer.SerializeAsync(fileStream, obj, _options);
        Bot.BotLog("Save gespeichert");
    }

    public static async Task<SaveInfos> SimpleRead() {
        Bot.BotLog("Lade Save...");
        await using var json = File.OpenRead(_fileName);
        var channels = await JsonSerializer.DeserializeAsync<SaveInfos>(json);
        Bot.BotLog("Save geladen");
        return channels;
    }
}
