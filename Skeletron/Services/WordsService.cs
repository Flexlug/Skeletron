using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;

using Microsoft.Extensions.Logging;

using Skeletron.Database.Interfaces;
using Skeletron.Services.Interfaces;

namespace Skeletron.Services
{
    public class WordsService : IWordsService
    {
        private IWordsProvider wordyDB;

        private DiscordClient client;
        private DiscordGuild wavGuild;
        private ILogger<WordsService> logger;
        private const ulong WORDS_CHANNEL_ID = 861903437976961034;

        public WordsService(IWordsProvider wordyDB,
                            DiscordClient client,
                            DiscordGuild guild,
                            ILogger<WordsService> logger)
        {
            this.wordyDB = wordyDB;
            this.client = client;
            this.wavGuild = guild;

            this.logger = logger;

            this.client.MessageCreated += Client_CheckWordsMessage;

            logger.LogInformation("WordsService loaded");
        }

        private async Task Client_CheckWordsMessage(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            if (e.Channel.Id != WORDS_CHANNEL_ID)
                return;

            if (e.Author.IsBot)
                return;

            string checkingWord = e.Message.Content.ToLower();

            logger.LogInformation($"Triggered event Client_CheckWordsMessage with param {e.Message.Content} by {e.Message.Author.Username}");

            if (CheckWord(checkingWord))
            {
                await e.Message.DeleteAsync();

                DiscordMember member = await wavGuild.GetMemberAsync(e.Author.Id);
                DiscordDmChannel dm = await member.CreateDmChannelAsync();

                await dm.SendMessageAsync($"Ваше сообщение было удалено из канала words, т.к. слово такое слово уже есть - {checkingWord}");
                return;
            }

            AddWord(checkingWord);
        }

        public void AddWord(string word) => wordyDB.AddWord(word);
        public void DeleteWord(string word) => wordyDB.DeleteWord(word);
        public bool CheckWord(string word) => wordyDB.CheckWord(word);
        public void ClearWords() => wordyDB.ClearWords();
        public List<string> GetWords() => wordyDB.GetWords();
    }
}
