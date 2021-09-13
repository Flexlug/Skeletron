using System;

using Raven.Client.Documents;

using Skeletron.Configurations;

namespace Skeletron.Database
{
    public class DocumentStoreProvider
    {
        private static string IP;
        private static string Name;

        public DocumentStoreProvider(Settings settings)
        {
            IP = settings.DB_IP;
            Name = settings.DB_NAME;
        }

        // Use Lazy<IDocumentStore> to initialize the document store lazily. 
        // This ensures that it is created only once - when first accessing the public `Store` property.
        private static Lazy<IDocumentStore> store = new Lazy<IDocumentStore>(CreateStore);

        public static IDocumentStore Store => store.Value;

        private static IDocumentStore CreateStore()
        {
            IDocumentStore store = new DocumentStore()
            {
                // Define the cluster node URLs (required)
                Urls = new[]
                {
                    IP
                },

                // Define a default database (optional)
                Database = Name,

                // Initialize the Document Store
            }.Initialize();

            return store;
        }
    }
    
}
