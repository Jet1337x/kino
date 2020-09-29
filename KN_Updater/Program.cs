using System;
using System.Reflection;

namespace KN_Updater {
  internal class Program {
    public static void Main(string[] args) {
      const string octokit = "KN_Updater.Data.Octokit.dll";
      Embedded.Load(octokit, "Octokit.dll");

      AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;

      if (args.Length < 2) {
        Console.WriteLine("Unable to get current version, bad argc");
        return;
      }

      string version = args[0];
      Console.WriteLine($"Current version: {version}");

      bool devMode = Convert.ToBoolean(args[1]);
      if (devMode) {
        Console.WriteLine("DEV MODE");
      }

      var updater = new Updater();
      if (!updater.Initialize()) {
        Console.WriteLine("Kino update failed. Exiting ...");
        return;
      }

      if (!updater.IsUpdateNeeded(version)) {
        Console.WriteLine("No update needed. Exiting ...");
        return;
      }

      updater.Run(devMode);
    }

    private static Assembly AssemblyResolve(object sender, ResolveEventArgs args) {
      return Embedded.Get(args.Name);
    }
  }
}