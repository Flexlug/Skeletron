using System.Threading.Tasks;
using VkNet.Model.Attachments;

namespace Skeletron.Services.Interfaces;

public interface IVVTDEService
{
    public string RequestVideoDownload(Video video);
}