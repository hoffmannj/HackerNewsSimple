using HackerNews.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace HackerNews
{
    class Program
    {
        private const string TOPSTORIESURL = "https://hacker-news.firebaseio.com/v0/topstories.json";
        private const string STORYITEMURL = "https://hacker-news.firebaseio.com/v0/item/{0}.json";

        static void Main(string[] args)
        {
            var postscount = GetArgumentNumber(args);
            if (postscount == -1)
            {
                Console.WriteLine("Usage: hackernews.exe --posts <n>");
                Console.WriteLine("Where: 1 <= n <= 100");
                return;
            }

            var ids = GetStoryIDs();
            if (ids == null)
            {
                Console.WriteLine("Couldn't get the story ID list.");
                return;
            }

            var resultList = new List<OutputItem>();
            foreach (var id in ids)
            {
                var item = GetItem(id, resultList.Count + 1);
                if (item != null) resultList.Add(item);
                if (resultList.Count == postscount) break;
            }
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(resultList, Newtonsoft.Json.Formatting.Indented));
        }

        private static int GetArgumentNumber(string[] args)
        {
            int number;
            if (args.Length != 2 || args[0] != "--posts" || !int.TryParse(args[1], out number)) return -1;
            return int.Parse(args[1]);
        }

        private static List<int> GetStoryIDs()
        {
            var response = GetResponse(TOPSTORIESURL);
            if (string.IsNullOrEmpty(response)) return null;
            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(response);
            }
            catch
            {
                return null;
            }
        }

        private static OutputItem GetItem(int id, int rank)
        {
            var response = GetResponse(string.Format(STORYITEMURL, id));
            if (string.IsNullOrEmpty(response)) return null;
            try
            {
                var hnItem = Newtonsoft.Json.JsonConvert.DeserializeObject<HackerNewsItem>(response);
                return new OutputItem
                {
                    Author = hnItem.By,
                    Comments = hnItem.Descendants.HasValue ? hnItem.Descendants.Value : 0,
                    Points = hnItem.Score.HasValue ? hnItem.Score.Value : 0,
                    Rank = rank,
                    Title = hnItem.Title,
                    Uri = hnItem.Url
                };
            }
            catch
            {
                return null;
            }
        }

        private static bool IsValid(OutputItem item)
        {
            if (string.IsNullOrEmpty(item.Title) || item.Title.Length > 256) return false;
            if (string.IsNullOrEmpty(item.Author) || item.Author.Length > 256) return false;
            if (!Uri.IsWellFormedUriString(item.Uri, UriKind.Absolute)) return false;
            if (item.Points < 0 || item.Comments < 0 || item.Rank < 0) return false;
            return true;
        }

        private static string GetResponse(string url)
        {
            using (var client = new HttpClient())
            try
            {
                return client.GetStringAsync(url).Result;
            }
            catch
            {
                return string.Empty;
            }
    }
    }
}
