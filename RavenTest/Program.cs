using System;
using System.Linq;
using System.Collections.Generic;

using Skeletron.Services.Models;

using Raven.Client;
using Raven.Client.Documents.Session;
using Skeletron.Database.Models;
using Skeletron.Utils;
using Raven.Client.Documents;
using Skeletron.Services;
using Skeletron.Database;

namespace WAV_Raven_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            WAV_Raven_Test.DocumentStoreProvider provider = new WAV_Raven_Test.DocumentStoreProvider();

            MappoolProvider mprovider = new MappoolProvider(null)
            {
                store = WAV_Raven_Test.DocumentStoreProvider.Store
            };

            Console.WriteLine(mprovider.MapsCount(CompitCategory.Gamma));
        }
    }
}
