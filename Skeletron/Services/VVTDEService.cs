using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Skeletron.Configurations;
using Skeletron.Services.Interfaces;
using VkNet.Model.Attachments;

namespace Skeletron.Services;

public class VVTDEService : IVVTDEService
{
    private ILogger<VVTDEService> _logger;
    private VVTDEBridge.VVTDEBridgeClient _client;

    public VVTDEService(ILogger<VVTDEService> logger,
        Settings settings)
    {
        _logger = logger;

        using var channel = GrpcChannel.ForAddress(settings.VVTDEAddress);
        _client = new VVTDEBridge.VVTDEBridgeClient(channel);

        _logger.LogInformation($"{nameof(VVTDEService)} initialized");
    }

    public string RequestVideoDownload(Video video)
    {
        var request = new VideoRequest()
        {
            Guid = Guid.NewGuid().ToString(),
            Url = video.UploadUrl.AbsoluteUri
        };
        _logger.LogDebug("Created request: {RequestGuid}, {RequestUrl}", request.Guid, request.Url);
        
        var reply = _client.RequestDownloadVideo(request);
        _logger.LogDebug("VVTDE returned {Reply}", reply);
        
        return request.Guid;
    }
}