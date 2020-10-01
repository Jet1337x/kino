using System.Collections.Generic;
using System.IO;

namespace KN_Updater {
  public static class Log {
    private static List<string> log_;

    private static string file_;

    public static void Init(string file) {
      file_ = file;
      log_ = new List<string>(32);
    }

    public static void Write(string message) {
      log_.Add(message);
    }

    public static void Save() {
      using (TextWriter writer = new StreamWriter(file_)) {
        foreach (string s in log_) {
          writer.WriteLine(s);
        }
      }
    }
  }
}