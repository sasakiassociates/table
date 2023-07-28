// See https://aka.ms/new-console-template for more information

using Firebase;
using Firebase.Auth;
using Firebase.Auth.Providers;
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
        private static readonly string url = "https://magpietable-default-rtdb.firebaseio.com/.json";
        private static HttpClient client = new()
        {
            BaseAddress = new Uri(url) 
        };

        public static async Task Main()
        {
            await GetAsync(client);
        }

        static async Task GetAsync(HttpClient client)
        {
            using HttpResponseMessage response = await client.GetAsync("https://magpietable-default-rtdb.firebaseio.com/.json");

            response.EnsureSuccessStatusCode();
                //.WriteRequestToConsole();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine(jsonResponse.ToString());
        }
    }
}

/* create a connection with firebase */
/* setup new markers */
/* get the info for markers */
