using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using VkNet.Model.Attachments;

namespace Skeletron.Services.Interfaces;

public interface IVVTDEService
{
    VideoReply RequestVideoDownload(Video video);
    void StartVideoWait(Guid guid, DiscordMessage message);
}