using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Skeletron.Services.Interfaces;

using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text;
using DSharpPlus.Entities;

namespace Skeletron.Commands
{
    /// <summary>
    /// Предоставляет команды для osu!
    /// </summary>
    // [Hidden, RequireGuild]
    // public class RecognizerCommands : SkBaseCommandModule
    // {
    //     private IRecognizerService recognizer;
    //     private ILogger<RecognizerCommands> logger;
    //
    //     public RecognizerCommands(IRecognizerService osu, ILogger<RecognizerCommands> logger)
    //     {
    //         this.recognizer = osu;
    //         this.logger = logger;
    //
    //         logger.LogInformation("RecognizerCommands loaded");
    //     }
    //
    //     [Command("ocr"), Description("Permorm an OCR process and recognize text on image. Languages: ENG + RUS")]
    //     public async Task OcrImage(CommandContext commandContext)
    //     {
    //         if (commandContext.Message.Attachments is null || commandContext.Message.Attachments.Count == 0)
    //         {
    //             await commandContext.RespondAsync("Не обнаружено вложений");
    //             return;
    //         }
    //
    //         var attachment = commandContext.Message.Attachments.First();
    //         string fileName = attachment.FileName.ToLower();
    //
    //         // Ignore videofiles
    //         if ((fileName.EndsWith(".mp4") ||
    //              fileName.EndsWith(".avi") ||
    //              fileName.EndsWith(".mkv") ||
    //              fileName.EndsWith(".m4v") ||
    //              fileName.EndsWith(".webm") ||
    //              fileName.EndsWith(".mov") ||
    //              fileName.EndsWith(".mts") ||
    //              fileName.EndsWith(".flv") ||
    //              fileName.EndsWith(".3gp") ||
    //              fileName.EndsWith(".m2ts") ||
    //              fileName.EndsWith(".mpg") ||
    //              fileName.EndsWith(".tga")))
    //         {
    //             await commandContext.RespondAsync("Данное расширение не поддерживается");
    //             return;
    //         }
    //
    //         string res = await recognizer.PerformOcr(attachment);
    //
    //         StringBuilder sb = new StringBuilder();
    //
    //         sb.AppendLine("```");
    //         sb.AppendLine(res);
    //         sb.AppendLine("```");
    //
    //         await commandContext.RespondAsync(new DiscordEmbedBuilder()
    //             .WithTitle("Распознанный текст:")
    //             .WithDescription(sb.ToString())
    //             .WithThumbnail(attachment.Url));
    //     }
    // }
}
