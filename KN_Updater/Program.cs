using System;
using System.Reflection;

namespace KN_Updater {
  internal class Program {
    private const int Version = 1;

    public static void Main(string[] args) {
      Log.Init("KN_UpdaterLog.txt");

      const string octokit = "KN_Updater.Data.Octokit.dll";
      Embedded.Load(octokit, "Octokit.dll");

      AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;

      if (args.Length < 2) {
        Log.Write($"Updater version: {Version}");
        Log.Save();
        Environment.Exit(Version);
        return;
      }

      string version = args[0];
      Log.Write($"Current version: {version}");

      string plugins = args[1];
      Log.Write($"Mod path: {plugins}");

      var updater = new Updater();
      if (!updater.Initialize()) {
        Log.Write("Kino update failed. Exiting ...");
        Log.Save();
        return;
      }

      if (!updater.IsUpdateNeeded(version)) {
        Log.Write("No update needed. Exiting ...");
        Log.Save();
        return;
      }

      updater.Run(plugins);

      Log.Save();
    }

    private static Assembly AssemblyResolve(object sender, ResolveEventArgs args) {
      return Embedded.Get(args.Name);
    }
  }
}