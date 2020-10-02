using System;
using System.IO;
using System.Reflection;

namespace KN_Updater {
  internal static class Program {
    private const int Version = 02;

    private static string version_ = "0.0.0";
    private static string modPath_ = "";
    private static bool saveLog_;

    public static void Main(string[] args) {
      const string octokit = "KN_Updater.Data.Octokit.dll";
      Embedded.Load(octokit, "Octokit.dll");

      AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;

      bool core123 = false;
      if (args.Length < 3) {
        core123 = bool.TryParse(args[1], out bool dummy);

        if (!core123) {
          Console.WriteLine($"Updater version: {Version}");
          Environment.Exit(Version);
          return;
        }
        Console.WriteLine("Running on old v123 core");
      }

      Log.Init();

      version_ = args[0];
      Log.Write($"Current version: {version_}");

      if (core123) {
        string exeFilePath = Assembly.GetExecutingAssembly().Location;
        string workPath = Path.GetDirectoryName(exeFilePath);

        modPath_ = workPath;
      }
      else {
        modPath_ = args[1];
        Log.Write($"Mod path: {modPath_}");

        saveLog_ = Convert.ToBoolean(args[2]);
      }

      var updater = new Updater();
      if (!updater.Initialize()) {
        Log.Write("Kino update failed. Exiting ...");
        SaveLog();
        return;
      }

      if (!updater.IsUpdateNeeded(version_)) {
        Log.Write("No update needed. Exiting ...");
        SaveLog();
        return;
      }

      updater.Run(modPath_);

      SaveLog();
    }

    private static void SaveLog() {
      if (saveLog_) {
        Log.Save(modPath_);
      }
    }

    private static Assembly AssemblyResolve(object sender, ResolveEventArgs args) {
      return Embedded.Get(args.Name);
    }
  }
}