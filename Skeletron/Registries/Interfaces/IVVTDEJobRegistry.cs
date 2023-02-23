using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace Skeletron.Commands.Interfaces;

public interface IVVTDEJobRegistry
{
    /// <summary>
    /// Start to await for video download
    /// </summary>
    /// <param name="guid">Video GUID. For Fetch GRPC method</param>
    /// <param name="message">Discord message with sent video link. Updates after download complete</param>
    /// <returns></returns>
    void StartWait(Guid guid, DiscordMessage message);
}