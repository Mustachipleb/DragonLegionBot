using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Lavalink;

namespace DragonLegionBot.Commands
{
    class MusicModule : BaseCommandModule
    {
        [Command("join")]
        public async Task Join(CommandContext ctx)
        {
            var channel = ctx.Member.VoiceState.Channel;
            
            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return;
            }

            var node = lava.ConnectedNodes.Values.First();

            if (channel.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Not a valid voice channel.");
                return;
            }

            await node.ConnectAsync(channel);
            await ctx.RespondAsync($"Joined {channel.Name}!");
        }

        [Command("leave")]
        public async Task Leave(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var channel = lava.GetGuildConnection(ctx.Guild).Channel;
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return;
            }

            var node = lava.ConnectedNodes.Values.First();

            if (channel.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Not a valid voice channel.");
                return;
            }

            var conn = node.GetGuildConnection(channel.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            await conn.DisconnectAsync();
            await ctx.RespondAsync($"Left {channel.Name}!");
        }

        [Command("play")]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {
            //Important to check the voice state itself first, 
            //as it may throw a NullReferenceException if they don't have a voice state.
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await Join(ctx);
                conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            }
            
            //We don't need to specify the search type here
            //since it is YouTube by default.
            var loadResult = await node.Rest.GetTracksAsync(search);
            
            //If something went wrong on Lavalink's end                          
            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
                //or it just couldn't find anything.
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"Track search failed for {search}.");
                return;
            }
            
            var track = loadResult.Tracks.First();
            
            await conn.PlayAsync(track);

            await ctx.RespondAsync($"Now playing {track.Title}!");
        }

        [Command("play")]
        public async Task Play(CommandContext ctx, Uri url)
        {
            //Important to check the voice state itself first, 
            //as it may throw a NullReferenceException if they don't have a voice state.
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await Join(ctx);
                conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            }

            //We don't need to specify the search type here
            //since it is YouTube by default.
            var loadResult = await node.Rest.GetTracksAsync(url);

            //If something went wrong on Lavalink's end                          
            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
                //or it just couldn't find anything.
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync("Couldn't load track.");
                return;
            }

            var track = loadResult.Tracks.First();

            await conn.PlayAsync(track);

            await ctx.RespondAsync($"Now playing {track.Title}!");
        }
        

        [Command]
        public async Task Pause(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }

            await conn.PauseAsync();
        }
    }
}
