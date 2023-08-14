using System;
using Raven.Client.Documents;

using System.Security.Cryptography.X509Certificates;

using Skeletron.Configurations;

namespace Skeletron.Database
{
    public class DocumentStoreProvider
    {
        private static string IP;
        private static string Name;
        private static string CertPath;

        public DocumentStoreProvider(Settings settings)
        {
            IP = settings.DB_IP;
            Name = settings.DB_NAME;
            CertPath = settings.DB_CERT;
        }

        // Use Lazy<IDocumentStore> to initialize the document store lazily. 
        // This ensures that it is created only once - when first accessing the public `Store` property.
        private static Lazy<IDocumentStore> store = new Lazy<IDocumentStore>(CreateStore);

        public static IDocumentStore Store => store.Value;

        private static IDocumentStore CreateStore()
        {
            X509Certificate2 certificate = new X509Certificate2(CertPath);

            IDocumentStore store = new DocumentStore()
            {
                // Define the cluster node URLs (required)
                Urls = new[]
                {
                    IP
                },

                // Define a default database (optional)
                Database = Name,

                Certificate = certificate

                // Initialize the Document Store
            }.Initialize();

            return store;
        }
    }
    
}
