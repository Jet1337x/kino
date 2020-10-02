#if NO_UNITY_LOG
using System;
#else
using UnityEngine;
#endif

namespace KN_Loader {
  public static class Log {
    public static void Write(string message) {
#if NO_UNITY_LOG
      Console.WriteLine(message);
#else
      Debug.Log(message);
#endif
    }
  }
}