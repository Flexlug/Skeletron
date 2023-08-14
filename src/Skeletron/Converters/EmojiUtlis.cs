using System;
using DSharpPlus;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace Skeletron.Converters
{
    public class EmojiUtlis
    {
        private DiscordClient client;

        public EmojiUtlis(DiscordClient client)
        {
            this.client = client;
        }

        /// <summary>
        /// Конвертирует цифру в эмодзи. Только в пределах от 0 до 10.
        /// </summary>
        /// <param name="d">Конвертируемое число</param>
        /// <returns>Число в виде эмодзи</returns>
        public DiscordEmoji DigitToEmoji(int d)
        {
            switch (d)
            {
                case 0:
                    return DiscordEmoji.FromName(client, ":zero:");

                case 1:
                    return DiscordEmoji.FromName(client, ":one:");

                case 2:
                    return DiscordEmoji.FromName(client, ":two:");

                case 3:
                    return DiscordEmoji.FromName(client, ":three:");

                case 4:
                    return DiscordEmoji.FromName(client, ":four:");

                case 5:
                    return DiscordEmoji.FromName(client, ":five:");

                case 6:
                    return DiscordEmoji.FromName(client, ":six:");

                case 7:
                    return DiscordEmoji.FromName(client, ":seven:");

                case 8:
                    return DiscordEmoji.FromName(client, ":eight:");

                case 9:
                    return DiscordEmoji.FromName(client, ":nine:");

                case 10:
                    return DiscordEmoji.FromName(client, ":keycap_ten:");

                default:
                    throw new ArgumentOutOfRangeException("EmojiUtlis.Digit() method accepts input only from 0 to 10");
            }
        }

        public int EmojiToDigit(DiscordEmoji emoji)
        {
            int i = emoji.Name switch
            {
                "1️⃣" => 1,
                "2️⃣" => 2,
                "3️⃣" => 3,
                "4️⃣" => 4,
                "5️⃣" => 5,
                "6️⃣" => 6,
                "7️⃣" => 7,
                "8️⃣" => 8,
                "9️⃣" => 9,
                "🔟" => 10,
                _ => throw new ArgumentOutOfRangeException($"Couldn't convert emoji {emoji.Name} to digit")
            };

            return i;
        }
    }
}