using System;
using System.Linq;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

using Raven.Client.Documents;
using Raven.Client.Documents.Session;

using Skeletron.Database.Interfaces;
using Skeletron.Database.Models;

namespace Skeletron.Database
{
    public class WordsProvider : IWordsProvider
    {
        private IDocumentStore store;

        private ILogger<WordsProvider> logger;

        public WordsProvider(ILogger<WordsProvider> logger)
        {
            this.store = DocumentStoreProvider.Store;

            this.logger = logger;

            CheckWordyCollectionExistence();

            logger.LogInformation("WordsProvider loaded");
        }

        private void CheckWordyCollectionExistence()
        {
            using (IDocumentSession session = store.OpenSession())
            {
                try
                {
                    WordsCollection collection = session.Query<WordsCollection>()
                                                        .Include(x => x.Words)
                                                        .FirstOrDefault();

                    if (collection is null)
                    {
                        session.Store(new WordsCollection());
                        session.SaveChanges();
                        return;
                    }
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, "Error while checking WordyCollection existence");
                }
            }
        }

        public void AddWord(string word)
        {
            using (IDocumentSession session = store.OpenSession())
            {
                WordsCollection collection = session.Query<WordsCollection>()
                                    .Include(x => x.Words)
                                    .First();

                collection.Words.Add(word);

                session.SaveChanges();
            }
        }

        public bool CheckWord(string word)
        {
            using (IDocumentSession session = store.OpenSession())
            {
                WordsCollection collection = session.Query<WordsCollection>()
                                    .Include(x => x.Words)
                                    .First();

                if (collection.Words.Contains(word))
                    return true;

                return false;
            }
        }

        public void DeleteWord(string word)
        {
            using (IDocumentSession session = store.OpenSession())
            {
                WordsCollection collection = session.Query<WordsCollection>()
                                    .Include(x => x.Words)
                                    .First();

                if (collection.Words.Contains(word))
                    collection.Words.Remove(word);

                session.SaveChanges();
            }
        }

        public void ClearWords()
        {
            using (IDocumentSession session = store.OpenSession())
            {
                WordsCollection collection = session.Query<WordsCollection>()
                                    .Include(x => x.Words)
                                    .First();

                collection.Words.Clear();

                session.SaveChanges();
            }
        }

        public List<string> GetWords()
        {
            using (IDocumentSession session = store.OpenSession(new SessionOptions() { NoTracking = true }))
            {
                WordsCollection collection = session.Query<WordsCollection>()
                                    .Include(x => x.Words)
                                    .First();

                return collection.Words;
            }
        }
    }
}
