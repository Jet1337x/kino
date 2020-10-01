using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace KN_Updater {
  public class Updater {
    private const string ProcName = "Drift Racing Online";

    private readonly Remote remote_;

    public Updater() {
      remote_ = new Remote();
    }

    public bool Initialize() {
      if (!remote_.Initialize()) {
        Log.Write("Failed to initialize remote ...");
        return false;
      }
      return true;
    }

    public bool IsUpdateNeeded(string version) {
      int current = VersionToInt(version);
      int remote = VersionToInt(remote_.LatestVersion);

      return remote > current;
    }

    public void Run(string modPath) {
      Log.Write($"Updating kino to {remote_.LatestVersion}");

      var bytes = remote_.DownloadLatestRelease();

      int count = 0;
      while (true) {
        var proc = Process.GetProcessesByName(ProcName);
        if (proc.Length == 0) {
          if (UnzipModFiles(bytes, modPath)) {
            Log.Write("Kino mod update completed!");
          }
          break;
        }

        ++count;
        Log.Write($"CarX is still running ({count})");
        Thread.Sleep(1000);
      }
    }

    private static bool UnzipModFiles(byte[] bytes, string modPath) {
      if (!Directory.Exists(modPath)) {
        Directory.CreateDirectory(modPath);
      }

      Log.Write($"Unzipping mod ({bytes.Length} bytes) ...");

      try {
        using (var memory = new MemoryStream(bytes)) {
          using (var zip = new ZipArchive(memory, ZipArchiveMode.Read)) {
            foreach (var entry in zip.Entries) {
              Log.Write($"Processing {entry.Name} ...");
              using (var stream = entry.Open()) {
                string path = modPath + Path.DirectorySeparatorChar + entry.Name;
                try {
                  using (var fileStream = File.Open(path, FileMode.Create)) {
                    stream.CopyTo(fileStream);
                  }
                }
                catch (Exception e) {
                  Log.Write($"Unable to copy file: '{path}', {e.Message}");
                }
              }
            }
          }
        }
      }
      catch (Exception e) {
        Log.Write($"Failed to unzip mod, {e.Message}");
        return false;
      }
      return true;
    }

    private static int VersionToInt(string version) {
      string v = version.Replace(".", "");
      v = v.Replace("v", "");
      v = v.Replace("f", "");

      int intVersion = Convert.ToInt32(v);

      return intVersion;
    }
  }
}