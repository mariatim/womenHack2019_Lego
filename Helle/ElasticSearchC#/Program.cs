using Flurl;
using Flurl.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElasticSearch
{
    class Program
    {
        const string LEGO = "https://services.slingshot.lego.com";//api/v3/_cat/aliases";
        static object Headers = new
        {
            x_api_key = "YOUR API KEY HERE",
            User_Agent = "WomenhackCPH"
        };

        static void Main(string[] args)
        {
            Run();
            Console.ReadLine();
        }

        async static void Run()
        {
            //GET https://services.slingshot.lego.com/api/v3/_cat/aliases?help=false&v=true
            var response = LEGO
                .AppendPathSegment("api")
                .AppendPathSegment("v3")
                .AppendPathSegment("_cat")
                .AppendPathSegment("aliases")
                .SetQueryParams(new
                {
                    help = false,
                    v = false
                })
                .WithHeaders(Headers)
                .AllowAnyHttpStatus()
                .GetStringAsync().Result;
            Console.WriteLine(response);

            var searchterm = Console.ReadLine();
            //Get the first searh result
            dynamic searchResponse = LEGO
                .AppendPathSegment("api")
                .AppendPathSegment("v3")
                .AppendPathSegment("character")
                .AppendPathSegment("_search")
                .WithHeaders(Headers)
                .SetQueryParams(new
                {
                    scroll = "1m"
                })
                .AllowAnyHttpStatus()
                .PostJsonAsync(new
                {
                    size = 100
                })
                .ReceiveJson()
                .Result;

            //Continue scrolling through the searchterm
            foreach (var result in searchResponse.hits.hits)
            {
                if (result._source.media != null)
                    Console.WriteLine(result._source.media.image.url);
            }

            dynamic secondSearchResponse;
            var scrollId = searchResponse._scroll_id;
            do
            {
                //Conduct the search and save the response
                secondSearchResponse = Search(scrollId);

                //Object to hold all urls for download
                List<string> urls = new List<string>();

                //Iterate over all results and extract the images
                foreach (var result in secondSearchResponse.hits.hits)
                {
                    if (result._source.media != null)
                    {
                        urls.Add(result._source.media.image.url);
                        Console.WriteLine(result._source.media.image.url);
                    }
                }

                await urls.ForEachAsync(async url =>
                {
                    try
                    {
                        await url.DownloadFileAsync("c:\\downloads");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });

                //Save the scrollId from this searchResponse to be used in the next search
                scrollId = secondSearchResponse._scroll_id;
            } while (secondSearchResponse.hits.hits.Count > 0);

        }

        public static dynamic Search(string searchId)
        {
            return LEGO
                .AppendPathSegment("api")
                .AppendPathSegment("v3")
                .AppendPathSegment("_search")
                .AppendPathSegment("scroll")
                .WithHeaders(Headers)
                .SetQueryParams(new
                {
                    scroll = "1m"
                })
                .AllowAnyHttpStatus()
                .PostJsonAsync(new
                {
                    //scroll = "1m",
                    scroll_id = searchId
                })
                .ReceiveJson()
                .Result;
        }

        public async static void DownloadPicture(string url)
        {
            await url.DownloadFileAsync("c:\\downloads");
        }
    }

    static class Extensions
    {
        public static Task ForEachAsync<T>(this IEnumerable<T> sequence, Func<T, Task> action)
        {
            return Task.WhenAll(sequence.Select(action));
        }
    }
}
