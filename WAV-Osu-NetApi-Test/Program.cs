using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using WAV_Osu_NetApi;

using Newtonsoft.Json;
using WAV_Osu_NetApi.Bancho.Models;
using WAV_Osu_NetApi.Gatari.Models;

namespace WAV_Osu_NetApi_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Settings settings;
            using (StreamReader sr = new StreamReader("credentials.json"))
                settings = JsonConvert.DeserializeObject<Settings>(sr.ReadToEnd());

            BanchoApi api = new BanchoApi(settings.ClientId, settings.Secret);
            Console.WriteLine(api.ReloadToken());

            //GatariApi api = new GatariApi();

            //Beatmapset bm = api.GetBeatmapset(372510);


            User users = null;
            api.GetBeatmap(2201460);
            api.GetUserRecentScores(6885792, true, 3, 10);
            Console.WriteLine(users);

            #region Get best scores
            //List<Score> scores = api.GetUserBestScores(9604150, 100);

            ///Console.WriteLine($"Scores count: {scores.Count}");

            //string scores_s = JsonConvert.SerializeObject(scores);
            //using (StreamWriter sw = new StreamWriter("best_scores_mindblock.json"))
            //    sw.Write(scores_s);

            //Console.ReadKey();
            #endregion

            #region Bancho tracker
            //DateTime last_score = DateTime.Now;
            //while (true)
            //{
            //    List<WAV_Osu_NetApi.Models.Bancho.Score> new_scores = new List<WAV_Osu_NetApi.Models.Bancho.Score>();
            //    List<WAV_Osu_NetApi.Models.Bancho.Score> available_scores = api.GetUserRecentScores("6885792", true, 10);

            //    DateTime latest_score = last_score;
            //    foreach (var score in available_scores)
            //        if (score.created_at > last_score)
            //        {
            //            new_scores.Add(score);
            //            if (latest_score < score.created_at)
            //                latest_score = score.created_at;
            //        }

            //    if (new_scores.Count != 0)
            //    {
            //        Console.WriteLine();
            //        foreach (var score in new_scores)
            //        {
            //            Console.WriteLine($"Title: {score.beatmapset.title} [{score.beatmap.version}]");
            //            Console.WriteLine($"Artist: {score.beatmapset.artist}");
            //            Console.WriteLine($"{score.rank}, {score.accuracy}%, {score.pp}, {score.statistics.count_300}, {score.statistics.count_100}, {score.statistics.count_50}, {score.statistics.count_miss}");
            //            Console.WriteLine();
            //        }

            //        last_score = latest_score;
            //    }
            //    else
            //    {
            //        Console.Write(".");
            //    }

            //    Thread.Sleep(10000);
            //}
            #endregion

            #region Gatari tracker
            /*
            DateTime last_score = DateTime.Now - TimeSpan.FromDays(3);
            while (true)
            {
                List<WAV_Osu_NetApi.Models.Gatari.Score> new_scores = new List<WAV_Osu_NetApi.Models.Gatari.Score>();
                List<WAV_Osu_NetApi.Models.Gatari.Score> available_scores = api.GetUserRecentScores(21129, true, 3);

                //Console.WriteLine(available_scores.Last().time);

                DateTime latest_score = last_score;
                foreach (var score in available_scores)
                    if (score.time > last_score)
                    {
                        new_scores.Add(score);
                        if (latest_score < score.time)
                            latest_score = score.time;
                    }

                if (new_scores.Count != 0)
                {
                    Console.WriteLine();
                    foreach (var score in new_scores)
                    {
                        Console.WriteLine($"Title: {score.beatmap.song_name}");
                        Console.WriteLine($"{score.ranking}, {score.accuracy}%, {score.pp}, {score.count_300}, {score.count_100}, {score.count_50}, {score.count_miss}");
                        Console.WriteLine();
                    }

                    last_score = latest_score;
                }
                else
                {
                    Console.Write(".");
                }

                Thread.Sleep(10000);
            }
            */
            #endregion

            #region Bancho search

            //var bms = api.Search("Oznei Haman wa Mou Iranai [jump training]", WAV_Osu_NetApi.Bancho.QuerryParams.MapType.Any);
            //foreach (var bm in bms)
            //    Console.WriteLine($"{bm.title}\n{bm.artist}");

            #endregion

            #region Bancho TryGetUsr

            User validUser, invalidUser;

            //api.TryGetUser(9604150, out validUser);
            //api.TryGetUser(687687654, out invalidUser);

            //Console.WriteLine(validUser);
            //Console.WriteLine(invalidUser);

            #endregion

            #region Bancho Get Beatmapset

            //Beatmapset bm = api.GetBeatmapset(372510);


            #endregion

            //api.TryRetrieveBeatmap(1114721);

        }
    }
}
