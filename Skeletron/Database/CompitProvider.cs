﻿using System.Linq;
using System.Collections.Generic;

using Raven.Client.Documents;
using Raven.Client.Documents.Session;

using Skeletron.Database.Models;
using Skeletron.Database.Interfaces;

using OsuNET_Api;

using Microsoft.Extensions.Logging;

namespace Skeletron.Database
{
    public class CompitProvider : ICompitProvider
    {
        private IDocumentStore store;

        private BanchoApi api;
        private GatariApi gapi;

        private ILogger<CompitProvider> logger;

        public CompitProvider(BanchoApi api,
                                 GatariApi gapi,
                                 ILogger<CompitProvider> logger)
        {
            this.store = DocumentStoreProvider.Store;

            this.api = api;
            this.gapi = gapi;

            this.logger = logger;
            logger.LogInformation("WAVCompitProvider loaded");
        }

        public CompitionProfile GetCompitProfile(string uid)
        {
            using (IDocumentSession session = store.OpenSession(new SessionOptions() { NoTracking = true }))
            {
                WAVMembers member = session.Query<WAVMembers>()
                                          .Include(x => x.CompitionProfile)
                                          .FirstOrDefault(x => x.DiscordUID == uid);

                return member?.CompitionProfile;
            }
        }

        public void AddCompitProfile(string uid, CompitionProfile compitProfile)
        {
            using (IDocumentSession session = store.OpenSession())
            {
                WAVMembers member = session.Query<WAVMembers>()
                                          .Include(x => x.OsuServers)
                                          .FirstOrDefault(x => x.DiscordUID == uid);

                member.CompitionProfile = compitProfile;

                session.SaveChanges();
            }
        }

        public List<CompitScore> GetUserScores(string uid)
        {
            using (IDocumentSession session = store.OpenSession(new SessionOptions() { NoTracking = true }))
            {
                List<CompitScore> scores = session.Query<CompitScore>()
                                                  .Where(x => x.DiscordUID == uid)
                                                  .ToList();

                return scores;
            }
        }

        public List<CompitScore> GetAllScores()
        {
            using (IDocumentSession session = store.OpenSession())
            {
                List<CompitScore> scores = session.Query<CompitScore>()
                                                  .ToList();

                return scores;
            }
        }

        public void DeleteAllScores()
        {
            using (IDocumentSession session = store.OpenSession())
            {
                List<CompitScore> scores = session.Query<CompitScore>()
                                                 .ToList();

                foreach (var score in scores)
                    session.Delete(score);

                session.SaveChanges();
            }
        }

        public void SubmitScore(CompitScore score)
        {
            using (IDocumentSession session = store.OpenSession())
            {
                session.Store(score);
                session.SaveChanges();
            }
        }

        public List<CompitScore> GetCategoryBestScores(CompitCategory category)
        {
            using (IDocumentSession session = store.OpenSession(new SessionOptions() { NoTracking = true }))
            {
                IEnumerable<CompitScore> rawScores = session.Query<CompitScore>().ToList();


                List<IGrouping<string, CompitScore>> scoresGroups = rawScores.Select(x => x)
                                                               .Where(x => x.Category == category)
                                                               .GroupBy(x => x.Nickname)
                                                               .ToList();

                List<CompitScore> scores = scoresGroups.Select(x => x.Select(x => x)
                                                                     .OrderByDescending(x => x.Score)
                                                                     .First())
                                                       .ToList();

                return scores;
            }
        }

        public CompitInfo GetCompitionInfo()
        {
            using (IDocumentSession session = store.OpenSession())
            {
                CompitInfo compitInfo = session.Query<CompitInfo>().FirstOrDefault();

                if (compitInfo is null)
                {
                    compitInfo = new CompitInfo();

                    session.Store(compitInfo);
                    session.SaveChanges();
                }

                return compitInfo;
            }
        }

        public bool CheckScoreExists(string uid)
        {
            using (IDocumentSession session = store.OpenSession())
            {
                CompitScore compitInfo = session.Query<CompitScore>().FirstOrDefault(x => x.ScoreId == uid);

                return compitInfo is not null;
            }
        }

        public void SetCompitionInfo(CompitInfo info)
        {
            using (IDocumentSession session = store.OpenSession())
            {
                CompitInfo oldInfo = session.Query<CompitInfo>().FirstOrDefault();

                if (oldInfo is null)
                    return;

                session.Delete(oldInfo);
                session.Store(info);

                session.SaveChanges();
            }
        }

        public void SetNonGrata(string uid, bool toggle)
        {
            using (IDocumentSession session = store.OpenSession(new SessionOptions() { NoTracking = true }))
            {
                WAVMembers member = session.Query<WAVMembers>()
                                          .Include(x => x.CompitionProfile)
                                          .FirstOrDefault(x => x.DiscordUID == uid);

                member.CompitionProfile.NonGrata = toggle;

                session.SaveChanges();
            }
        }
    }
}