using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Raven.Client.Documents;

namespace WAV_Raven_Test
{
    public class DocumentStoreProvider
    {
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
                    "http://192.168.2.71:8080"
                },

                // Define a default database (optional)
                Database = "WAVMembers",

                // Initialize the Document Store
            }.Initialize();

            return store;
        }
    }
}
