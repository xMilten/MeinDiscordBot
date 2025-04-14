using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace MeinDiscordBot.Commands;
public class MyCommands : BaseCommandModule {
    [Command("ping")]
    [Description("Gibt Pong zurück")]
    //[RequireRoles(RoleCheckMode.Any, "Moderator", "Owner")]
    public async Task Ping(CommandContext ctx) {
        await ctx.Channel.SendMessageAsync("Pong").ConfigureAwait(false);
    }

    [Command("addChannel")]
    [Description("Fügt den Channel hinzu, wo der Befehl ausgeführt wurde")]
    public async Task AddChannel(CommandContext ctx) {
        if (Bot.SaveInfos.Channels.Exists(channel => channel.Id == ctx.Channel.Id)) {
            await SendMessageAndDelete($"Dieser Channel \"{ctx.Channel.Name}\" wurde bereits hinzugefügt", ctx).ConfigureAwait(false);
            return;
        } else if (Bot.SaveInfos.Channels.Exists(channel => channel.Name.Equals(ctx.Channel.Name))) {
            await SendMessageAndDelete($"Es gibt bereits ein Channel mit diesen Namen {ctx.Channel.Name}", ctx).ConfigureAwait(false);
            return;
        }

        Bot.SaveInfos.Channels.Add(new MyChannel(ctx.Channel.Name, ctx.Channel.Id));
        await Save();
        await SendMessage($"Channel \"{ctx.Channel.Name}\" wurde hinzugefügt", Bot.SaveInfos.Info).ConfigureAwait(false);
        await ctx.Message.DeleteAsync().ConfigureAwait(false);
    }

    [Command("setError")]
    [Description("In dem Channel, wo der Befehl ausgeführt wird, werden die Errors gelistet")]
    public async Task SetError(CommandContext ctx) {
        Bot.SaveInfos.Error = ctx.Channel.Id;
        await Save();
        await SendMessageAndDelete($"Erros werden nun in diesem Channel gelistet", ctx).ConfigureAwait(false);
    }

    [Command("setInfo")]
    [Description("In dem Channel, wo der Befehl ausgeführt wird, werden die Infos gelistet")]
    public async Task SetInfo(CommandContext ctx) {
        Bot.SaveInfos.Info = ctx.Channel.Id;
        await Save();
        await SendMessageAndDelete($"Infos werden nun in diesem Channel gelistet", ctx).ConfigureAwait(false);
    }

    [Command("create")]
    [Description("Erstellt einen Gegestand in dem Channel")]
    public async Task Create(CommandContext ctx, string name) {
        await DeleteAuthorMessage(ctx.Message).ConfigureAwait(false);
        MyChannel channel = Bot.SaveInfos.Channels.FirstOrDefault(channel => channel.Id == ctx.Channel.Id);
        if (channel == null) {
            await SendMessage($"Dieser Channel \"{ctx.Channel.Name}\" wurde nicht für Items hinzugefügt. Verwende !addChannel zum hinzufügen", Bot.SaveInfos.Error).ConfigureAwait(false);
            return;
        }

        if (channel.Messages.ContainsKey(name)) {
            await SendMessage($"Das Item {name} existiert bereits in diesem Channel {ctx.Channel.Name}", Bot.SaveInfos.Error).ConfigureAwait(false);
            return;
        }

        DiscordMessage newDiscordMessage = await SendMessage($"{name}: 0", channel.Id);
        channel.Messages.Add(name, newDiscordMessage.Id);
        await Save();
        await SendMessage($"Das Item {name} wurde im Channel {ctx.Channel.Name} hinzugefügt", Bot.SaveInfos.Info).ConfigureAwait(false);
    }

    [Command("remove")]
    [Description("Entfernt den Gegenstand aus dem Channel")]
    public async Task Remove(CommandContext ctx, string name) {
        await DeleteAuthorMessage(ctx.Message).ConfigureAwait(false);
        MyChannel channel = Bot.SaveInfos.Channels.FirstOrDefault(channel => channel.Id == ctx.Channel.Id);
        if (channel == null) {
            await SendMessage($"Dieser Channel \"{ctx.Channel.Name}\" wurde nicht für Items hinzugefügt. Verwende !addChannel zum hinzufügen", Bot.SaveInfos.Error).ConfigureAwait(false);
            return;
        }

        DiscordChannel discordChannel = await Bot.Client.GetChannelAsync(channel.Id).ConfigureAwait(false);

        if (channel.Messages.TryGetValue(name, out ulong messageId)) {
            DiscordMessage discordMessage = await discordChannel.GetMessageAsync(messageId);
            channel.Messages.Remove(name);
            await discordMessage.DeleteAsync().ConfigureAwait(false);
            await Save();
            await SendMessage($"Das Item {name} wurde aus dem Channel {ctx.Channel.Name} entfernt", Bot.SaveInfos.Info).ConfigureAwait(false);
        } else {
            await SendMessage($"In diesem Channel \"{ctx.Channel.Name}\" befindet sich kein Item mit dem Namen {name}. Verwende !create zum erstellen", Bot.SaveInfos.Error).ConfigureAwait(false);
        }
    }

    [Command("add")]
    [Description("Erhöt die Menge eines Items im Channel")]
    public async Task Add(CommandContext ctx, string name, int count) {
        await AddSubHelper(ctx, name, count, true);
    }

    [Command("sub")]
    [Description("Erhöt die Menge eines Items")]
    public async Task Sub(CommandContext ctx, string name, int count) {
        await AddSubHelper(ctx, name, count, false);
    }

    private async Task AddSubHelper(CommandContext ctx, string name, int count, bool isAdd) {
        await DeleteAuthorMessage(ctx.Message).ConfigureAwait(false);
        MyChannel channel = Bot.SaveInfos.Channels.FirstOrDefault(channel => channel.Id == ctx.Channel.Id);
        if (channel == null) {
            await SendMessage($"Dieser Channel \"{ctx.Channel.Name}\" wurde nicht für Items hinzugefügt. Verwende !addChannel zum hinzufügen", Bot.SaveInfos.Error).ConfigureAwait(false);
            return;
        }

        DiscordChannel discordChannel = await Bot.Client.GetChannelAsync(channel.Id).ConfigureAwait(false);

        if (channel.Messages.TryGetValue(name, out ulong messageId)) {
            DiscordMessage discordMessage = await discordChannel.GetMessageAsync(messageId);
            int contentValue = int.Parse(discordMessage.Content.Split(" ").Last());
            string aenderung = string.Empty;
            if (isAdd) {
                contentValue += count;
                aenderung = "erhöht";
            } else {
                contentValue -= count;
                aenderung = "gesenkt";
            }

            discordMessage = await discordMessage.ModifyAsync($"{name}: {contentValue}").ConfigureAwait(false);
            channel.Messages[name] = discordMessage.Id;
            await Save();
            await SendMessage($"Die Menge des Items \"{name}\" im Channel \"{ctx.Channel.Name}\" wurde um {count} auf {contentValue} {aenderung}", Bot.SaveInfos.Info).ConfigureAwait(false);
        } else {
            await SendMessage($"In diesem Channel \"{ctx.Channel.Name}\" befindet sich kein Item mit dem Namen {name}. Verwende !create zum erstellen", Bot.SaveInfos.Error).ConfigureAwait(false);
        }
    }

    private async Task DeleteAuthorMessage(DiscordMessage authorMessage) {
        await authorMessage.DeleteAsync().ConfigureAwait(false);
    }

    private async Task SendMessageAndDelete(string msgParam, CommandContext ctx) {
        DiscordMessage msg = await ctx.Channel.SendMessageAsync(msgParam).ConfigureAwait(false);
        await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        await ctx.Message.DeleteAsync().ConfigureAwait(false);
        await msg.DeleteAsync().ConfigureAwait(false);
    }

    private async Task<DiscordMessage> SendMessage(string msg, ulong channelId) {
        if (channelId == 0) return null;
        DiscordChannel channel = await Bot.Client.GetChannelAsync(channelId);
        return await channel.SendMessageAsync(msg).ConfigureAwait(false);
    }

    private async Task Save() {
        await SaveFileUtils.SimpleWrite(Bot.SaveInfos);
    }
}