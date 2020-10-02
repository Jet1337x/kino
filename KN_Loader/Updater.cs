using System;
using System.Diagnostics;
using System.IO;
using BepInEx;

namespace KN_Loader {
  public static class Updater {
    private static readonly string UpdaterPath = Path.GetTempPath() + Path.DirectorySeparatorChar + "KN_Updater.exe";

    public static void StartUpdater(int latestUpdater, bool forceUpdate, bool dev) {
      if (forceUpdate) {
        CheckForNewUpdater(latestUpdater);
      }

      string version = forceUpdate ? "0.0.0" : ModLoader.StringVersion;
      string args = $"{version} \"{(dev ? Paths.GameRootPath + Path.DirectorySeparatorChar + "KnUpdate" : Paths.PluginPath)}\" {dev}";

      Log.Write($"[KN_Loader::Updater]: Starting updater args: {args}");
      var proc = Process.Start(UpdaterPath, args);
      if (proc != null) {
        proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        Log.Write("[KN_Loader::Updater]: Updater started ...");
      }
      else {
        Log.Write("[KN_Loader::Updater]: Unable to start updater");
      }
    }

    public static void CheckForNewUpdater(int latestUpdater) {
      string oldUpdater = Paths.PluginPath + Path.DirectorySeparatorChar + "KN_Updater.exe";
      try {
        if (File.Exists(oldUpdater)) {
          File.Delete(oldUpdater);
        }
      }
      catch {
        // ignored
      }

      bool shouldDownload;
      if (File.Exists(UpdaterPath)) {
        Log.Write("[KN_Loader::Updater]: Checking updater version ...");
        var proc = Process.Start(UpdaterPath);
        if (proc != null) {
          proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
          proc.WaitForExit();
          int version = proc.ExitCode;

          shouldDownload = version != latestUpdater;
          Log.Write($"[KN_Loader::Updater]: Updater version: C: {version} / L: {latestUpdater}, download: {shouldDownload}");
        }
        else {
          Log.Write("[KN_Loader::Updater]: Unable to start updater");
          shouldDownload = true;
        }
      }
      else {
        shouldDownload = true;
      }

      if (shouldDownload) {
        DownloadNewUpdater(UpdaterPath);
      }
    }

    private static void DownloadNewUpdater(string path) {
      var bytes = WebDataLoader.DownloadNewUpdater();

      if (bytes == null) {
        return;
      }

      try {
        using (var memory = new MemoryStream(bytes)) {
          using (var fileStream = File.Open(path, FileMode.Create)) {
            memory.CopyTo(fileStream);
          }
        }
      }
      catch (Exception e) {
        Log.Write($"[KN_Loader::Updater]: Failed to save updater to disc, {e.Message}");
      }
    }
  }
}