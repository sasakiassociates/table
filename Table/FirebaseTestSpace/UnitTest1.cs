using Firebase.Auth;
using NUnit.Framework;
using Sasaki.FirebaseSharp;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace FirebaseTestSpace
{

  public class Tests
  {
    [SetUp]
    public void Setup()
    {
        string testJson = @"
        { 
            ""test"": {
                ""id"": ""1"",
                ""x"" : ""100"",
                ""y"" : ""200""
            },
            ""test2"": {
                ""id"": ""2"",
                ""x"" : ""110"",
                ""y"" : ""210""
            },
            ""test3"": {
                ""id"": ""3"",
                ""x"" : ""120"",
                ""y"" : ""220""
            }
        }";
     }

    [Test]
    public void SetConnectionToFireBase()
    {
      Console.WriteLine("hello world");
      var input = new FireBaseSetupInput("some url");
      Assert.IsTrue(input.isValid);
    }

    [Test]
    public async Task GetUpdatedValuesFromFirebase()
    { 
        Sasaki.FirebaseSharp.HTTPRequests requester = new("https://magpietable-default-rtdb.firebaseio.com/.json");
            await requester.GetAsync();
            Assert.IsNotEmpty(requester.ToString());
    }

    [Test]
    public async Task SetNewValuesInFirebase()
        {
            Sasaki.FirebaseSharp.HTTPRequests sender = new("https://magpietable-default-rtdb.firebaseio.com/.json");
            await sender.PostAsync();
            Assert.IsNotEmpty(sender.ToString());
        }


  }

}
