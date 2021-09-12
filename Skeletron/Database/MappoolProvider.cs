using DSharpPlus;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WAV_Bot_DSharp.Database.Interfaces;
using WAV_Bot_DSharp.Database.Models;

namespace WAV_Bot_DSharp.Database
{
    public class MappoolProvider : IMappoolProvider
    {
        private IDocumentStore store;

        private ILogger<MappoolProvider> logger;

        public MappoolProvider(DiscordClient client,
                               ILogger<MappoolProvider> logger)
        {
            this.store = DocumentStoreProvider.Store;

            this.logger = logger;
            logger.LogInformation("MappoolProvider loaded");
        }

        public void MapAdd(OfferedMap map)
        {
            using (IDocumentSession session = store.OpenSession())
            {
                session.Store(map);
                session.SaveChanges();
            }
        }

        public List<OfferedMap> GetAllMaps()
        {
            using (IDocumentSession session = store.OpenSession(new SessionOptions() { NoTracking = true }))
            {
                var alreadySubmitedMap = session.Query<OfferedMap>()
                                                .ToList();

                return alreadySubmitedMap;
            }
        }

        public List<OfferedMap> GetCategoryMaps(CompitCategory category)
        {
            using (IDocumentSession session = store.OpenSession(new SessionOptions() { NoTracking = true }))
            {
                var categoryMaps = session.Query<OfferedMap>()
                                          .Include(x => x.Votes)
                                          .Select(x => x)
                                          .Where(x => x.Category == category)
                                          .OrderBy(x => x.Votes.Count)
                                          .ToList();

                return categoryMaps;
            }
        }

        public bool CheckMapOffered(int mapId, CompitCategory category)
        {
            using (IDocumentSession session = store.OpenSession())
            {
                return session.Query<OfferedMap>()
                              .FirstOrDefault(x => x.BeatmapId == mapId &&
                                                   x.Category == category) is not null;
            }
        }

        public void ResetMappool()
        {
            using (IDocumentSession session = store.OpenSession())
            {
                var allMaps = session.Query<OfferedMap>()
                                          .ToList();

                foreach (var map in allMaps)
                    session.Delete(map);

                session.SaveChanges();
            }
        }

        public bool CheckUserSubmitedAny(string userId)
        {
            using (IDocumentSession session = store.OpenSession())
            {
                return session.Query<OfferedMap>()
                              .FirstOrDefault(x => x.SuggestedBy == userId &&
                                                   !x.AdminMap) is not null;
            }
        }

        public void MapVote(string userId, CompitCategory category, int beatmapId)
        {
            using (IDocumentSession session = store.OpenSession())
            {
                var map = session.Query<OfferedMap>()
                                 .FirstOrDefault(x => x.BeatmapId == beatmapId &&
                                                      x.Category == category);

                map.Votes.Add(userId);

                session.SaveChanges();
            }
        }

        public bool CheckUserVoted(string userId)
        {
            using (IDocumentSession session = store.OpenSession())
            {
                var maps = session.Query<OfferedMap>()
                                  .Include(x => x.Votes)
                                  .ToList();

                foreach (var map in maps)
                    if (map.Votes.Contains(userId))
                        return true;

                return false;
            }
        }

        public void MapRemove(CompitCategory category, int beatmapId)
        {
            using (IDocumentSession session = store.OpenSession())
            {
                var map = session.Query<OfferedMap>()
                                  .FirstOrDefault(x => x.Category == category &&
                                                       x.BeatmapId == beatmapId);

                if (map is null)
                {
                    logger.LogError($"Не удалось найти карту в методе MapRemove - {beatmapId}");
                    return;
                }

                session.Delete(map);
                session.SaveChanges();
            }
        }

        public MappoolSpectateStatus GetMappoolStatus()
        {
            using (IDocumentSession session = store.OpenSession())
            {
                var mappoolStatus = session.Query<MappoolSpectateStatus>()
                                           .FirstOrDefault();

                if (mappoolStatus is null)
                {
                    mappoolStatus = new MappoolSpectateStatus()
                    {
                        IsSpectating = false,

                        BeginnerMessageId = string.Empty,
                        AlphaMessageId = string.Empty,
                        BetaMessageId = string.Empty,
                        GammaMessageId = string.Empty,
                        DeltaMessageId = string.Empty,
                        EpsilonMessageId = string.Empty
                    };

                    session.Store(mappoolStatus);
                    session.SaveChanges();
                }

                return mappoolStatus;
            }
        }

        public void SetMappoolStatus(MappoolSpectateStatus spectateStatus)
        {
            using (IDocumentSession session = store.OpenSession())
            {
                var mappoolStatus = session.Query<MappoolSpectateStatus>()
                                           .FirstOrDefault();

                if (mappoolStatus is null)
                {
                    session.Store(spectateStatus);
                    session.SaveChanges();

                    return;
                }

                mappoolStatus.IsSpectating = spectateStatus.IsSpectating;
                mappoolStatus.AnnounceChannelId = spectateStatus.AnnounceChannelId;
                mappoolStatus.BeginnerMessageId = spectateStatus.BeginnerMessageId;
                mappoolStatus.AlphaMessageId = spectateStatus.AlphaMessageId;
                mappoolStatus.BetaMessageId = spectateStatus.BetaMessageId;
                mappoolStatus.GammaMessageId = spectateStatus.GammaMessageId;
                mappoolStatus.DeltaMessageId = spectateStatus.DeltaMessageId;
                mappoolStatus.EpsilonMessageId = spectateStatus.EpsilonMessageId;

                session.SaveChanges();
            }
        }
    }
}
