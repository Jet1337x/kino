using System;
using System.Collections.Generic;
using System.IO;

namespace KN_Updater {
  public static class Log {
    private static List<string> log_;


    public static void Init() {
      log_ = new List<string>(32);
    }

    public static void Write(string message) {
      Console.WriteLine(message);
      log_.Add(message);
    }

    public static void Save(string path) {
      string file = path + Path.DirectorySeparatorChar + "updater_log.txt";

      Write($"Saving log to '{file}' ...");

      using (TextWriter writer = new StreamWriter(file)) {
        foreach (string line in log_) {
          writer.WriteLine(line);
        }
      }
    }
  }
}