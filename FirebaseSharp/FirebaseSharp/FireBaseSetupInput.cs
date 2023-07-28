using System;

namespace Sasaki.FirebaseSharp
{

  public class FireBaseSetupInput
  {

    public bool isValid {get;private set;}

    public FireBaseSetupInput(string url)
    {
      Console.WriteLine($"connecting to {url}");

      isValid = true;
    }
  }

}
