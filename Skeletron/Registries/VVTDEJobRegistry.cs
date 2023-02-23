using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using FluentScheduler;
using Skeletron.Commands.Interfaces;

namespace Skeletron.Commands;

public class VVTDEJobRegistry : Registry, IVVTDEJobRegistry
{
    private DiscordClient _client;
    
    public VVTDEJobRegistry(DiscordClient client)
    {
        _client = client;
    }
    
    public void StartWait(Guid guid, DiscordMessage message)
    {
        this.Schedule(() => VideoWaitHandler(guid, message))
            .WithName(guid.ToString())
            .ToRunEvery(5)
            .Seconds();
    }

    private void VideoWaitHandler(Guid guid, DiscordMessage message)
    {
        
    }
}

public class SampleJob : IJob
{
    public void Execute()
    {
        //throw new NotImplementedException();
    }
}