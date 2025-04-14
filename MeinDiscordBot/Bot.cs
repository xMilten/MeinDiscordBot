using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using MeinDiscordBot.Commands;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Channels;

namespace MeinDiscordBot;
public class Bot {
    public static DiscordClient Client { get; private set; }
    public CommandsNextExtension Commands { get; private set; }
    public static SaveInfos SaveInfos { get; set; }

    public async Task RunAysnc() {
        if (File.Exists("save.json")) {
            SaveInfos = await SaveFileUtils.SimpleRead();
        } else {
            SaveInfos = new SaveInfos();
        }

        var json = string.Empty;

        using(var fs = File.OpenRead("config.json"))
        using (var sr = new StreamReader(fs, new UTF8Encoding(false)))

        json = await sr.ReadToEndAsync().ConfigureAwait(false);

        var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

        var config = new DiscordConfiguration() {
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents,
            Token = configJson.Token,
            TokenType = TokenType.Bot,
            AutoReconnect = true
        };

        Client = new DiscordClient(config);
        //Client.Intents.AddIntent(DiscordIntents.All);

        Client.Ready += OnClientReady;

        var commandsConfig = new CommandsNextConfiguration {
            StringPrefixes = new string[] { configJson.Prefix },
            EnableDms = false,
            EnableMentionPrefix = true
        };

        Commands = Client.UseCommandsNext(commandsConfig);

        Commands.RegisterCommands<MyCommands>();

        await Client.ConnectAsync();

        await Task.Delay(-1);
    }

    private Task OnClientReady(DiscordClient client, ReadyEventArgs e) {
        BotLog("Bot bereit.");
        return Task.CompletedTask;
    }

    public static void BotLog(string message) {
        Console.WriteLine($"[{DateTime.Now}] {message}");
    }
}
