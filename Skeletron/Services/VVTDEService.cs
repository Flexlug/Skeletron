using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using FluentScheduler;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Skeletron.Commands.Interfaces;
using Skeletron.Configurations;
using Skeletron.Services.Interfaces;
using VkNet.Model.Attachments;

namespace Skeletron.Services;

public class VVTDEService : IVVTDEService
{
    private ILogger<VVTDEService> _logger;
    private VVTDEBridge.VVTDEBridgeClient _client;
    private IVVTDEJobRegistry _registry;

    public VVTDEService(ILogger<VVTDEService> logger,
        IVVTDEJobRegistry registry,
        Settings settings)
    {
        _logger = logger;
        _registry = registry;

        using var channel = GrpcChannel.ForAddress(settings.VVTDEAddress);
        _client = new VVTDEBridge.VVTDEBridgeClient(channel);

        _logger.LogInformation($"{nameof(VVTDEService)} initialized");
    }

    public VideoReply RequestVideoDownload(Video video)
    {
        var request = new VideoRequest()
        {
            Url = video.UploadUrl.AbsoluteUri
        };
        
        _logger.LogDebug("Created request: {RequestUrl}", request.Url);
        
        var reply = _client.RequestDownloadVideo(request);
        _logger.LogDebug("VVTDE returned {Reply}", reply);

        return reply;
    }

    public void StartVideoWait(Guid guid, DiscordMessage message)
        => _registry.StartWait(guid, message);
}