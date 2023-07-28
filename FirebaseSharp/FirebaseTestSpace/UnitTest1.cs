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
    { }

    [Test]
    public void SetConnectionToFireBase()
    {
      Console.WriteLine("hello world");
      var input = new FireBaseSetupInput("some url");
      Assert.IsTrue(input.isValid);
    }

    [Test]
    public async Task GetUpdatedValuesFromFirebase()
    { }



  }

}
