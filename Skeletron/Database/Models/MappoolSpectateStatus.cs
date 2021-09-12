using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WAV_Bot_DSharp.Database.Models
{
    public class MappoolSpectateStatus
    {
        /// <summary>
        /// Указывает, включено ли сейчас отслеживание изменений в маппуле
        /// </summary>
        public bool IsSpectating { get; set; }

        /// <summary>
        /// Id канала, в котором будут публиковаться изменения и результаты маппула
        /// </summary>
        public string AnnounceChannelId { get; set; }

        /// <summary>
        /// Id сообщения, в котором публикуются изменения для маппула категории beginner
        /// </summary>
        public string BeginnerMessageId { get; set; }

        /// <summary>
        /// Id сообщения, в котором публикуются изменения для маппула категории alpha
        /// </summary>
        public string AlphaMessageId { get; set; }

        /// <summary>
        /// Id сообщения, в котором публикуются изменения для маппула категории beta
        /// </summary>
        public string BetaMessageId { get; set; }

        /// <summary>
        /// Id сообщения, в котором публикуются изменения для маппула категории gamma
        /// </summary>
        public string GammaMessageId { get; set; }

        /// <summary>
        /// Id сообщения, в котором публикуются изменения для маппула категории delta
        /// </summary>
        public string DeltaMessageId { get; set; }

        /// <summary>
        /// Id сообщения, в котором публикуются изменения для маппула категории epsilon
        /// </summary>
        public string EpsilonMessageId { get; set; }
    }
}
