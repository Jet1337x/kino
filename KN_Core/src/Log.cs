#if NO_UNITY_LOG
using System;
#endif
using UnityEngine;

namespace KN_Core {
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