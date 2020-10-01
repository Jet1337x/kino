using System;
using System.Reflection;

namespace KN_Updater {
  internal class Program {
    private const int Version = 1;

    private static string logPath_ = "";
    private static bool saveLog_ = false;

    public static void Main(string[] args) {
      const string octokit = "KN_Updater.Data.Octokit.dll";
      Embedded.Load(octokit, "Octokit.dll");

      AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;

      if (args.Length < 3) {
        Console.WriteLine($"Updater version: {Version}");
        Environment.Exit(Version);
        return;
      }

      Log.Init();

      string version = args[0];
      Log.Write($"Current version: {version}");

      string plugins = args[1];
      Log.Write($"Mod path: {plugins}");

      logPath_ = plugins;

      saveLog_ = Convert.ToBoolean(args[2]);

      var updater = new Updater();
      if (!updater.Initialize()) {
        Log.Write("Kino update failed. Exiting ...");
        SaveLog();
        return;
      }

      if (!updater.IsUpdateNeeded(version)) {
        Log.Write("No update needed. Exiting ...");
        SaveLog();
        return;
      }

      updater.Run(plugins);

      SaveLog();
    }

    private static void SaveLog() {
      if (saveLog_) {
        Log.Save(logPath_);
      }
    }

    private static Assembly AssemblyResolve(object sender, ResolveEventArgs args) {
      return Embedded.Get(args.Name);
    }
  }
}