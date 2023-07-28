// See https://aka.ms/new-console-template for more information

using Firebase;
using Firebase.Auth;
using Newtonsoft.Json;
using System;


Console.WriteLine("starting application");

var client = new FirebaseAuthClient(new FirebaseAuthConfig()
{
  AuthDomain = ,
  ApiKey = ,
  HttpClient = ,
  JsonSettings = new JsonSerializerSettings() {Formatting = Formatting.Indented}
});

/* create a connection with firebase */
/* setup new markers */
/* get the info for markers */
