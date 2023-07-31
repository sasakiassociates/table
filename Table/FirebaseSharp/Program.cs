// See https://aka.ms/new-console-template for more information

using Newtonsoft.Json;
using Sasaki.FirebaseSharp;
using System;
using System.Net;

// Google's Firebase Admin SDK is the only part built out in C#, but does not allow for realtime database access (ref: "https://firebase.google.com/docs/admin/setup")
// This is a test to see if HTTPRequest can be an alternative
// Http Get works to get the data, but I'm not sure how to set up a listener to get the data in realtime

namespace Sasaki.FirebaseSharp
{
    public class HTTPRequests
    {
        //private static string url = "https://magpietable-default-rtdb.firebaseio.com/.json";
        //private static readonly HttpClient client = new();
        private static HttpClient client = new();

        public HTTPRequests(string url_)
        {
            client = new()
            {
                BaseAddress = new Uri(url_)
            };
            
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                               new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static void Main()
        {
            
        }

        public async Task GetAsync()
        {
            using HttpResponseMessage response = await client.GetAsync("https://magpietable-default-rtdb.firebaseio.com/.json");

            response.EnsureSuccessStatusCode();
                //.WriteRequestToConsole();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine(jsonResponse.ToString());
        }

        public async Task PostAsync(string values_)
        {
            var content = new FormUrlEncodedContent(values_);

            var response = await client.PostAsync("https://magpietable-default-rtdb.firebaseio.com/.json", content);

            var responseString = await response.Content.ReadAsStringAsync();

            Console.WriteLine(responseString);
        }
    }
}

/* create a connection with firebase */
/* setup new markers */
/* get the info for markers */
