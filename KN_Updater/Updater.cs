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
        Console.WriteLine("Failed to initialize remote ...");
        return false;
      }
      return true;
    }

    public bool IsUpdateNeeded(string version) {
      int current = VersionToInt(version);
      int remote = VersionToInt(remote_.LatestVersion);

      return remote > current;
    }

    public void Run(bool dev) {
      Console.WriteLine($"Updating kino to {remote_.LatestVersion}");

      var bytes = remote_.DownloadLatestRelease();

      int count = 0;
      while (true) {
        var proc = Process.GetProcessesByName(ProcName);
        if (proc.Length == 0 || dev) {
          string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
          string strWorkPath = Path.GetDirectoryName(strExeFilePath);
          Console.WriteLine($"Mod path: {strWorkPath}");

          if (UnzipModFiles(bytes, strWorkPath, dev)) {
            Console.WriteLine("Kino mod update completed!");
          }
          break;
        }

        ++count;
        Console.WriteLine($"CarX is still running ({count})");
        Thread.Sleep(1000);
      }

      if (dev) {
        Console.ReadKey();
      }
    }

    private static bool UnzipModFiles(byte[] bytes, string plugins, bool dev) {
      string updateDir = "KnUpdate" + Path.DirectorySeparatorChar;

      if (dev) {
        if (!Directory.Exists(updateDir)) {
          Directory.CreateDirectory(updateDir);
        }
      }

      Console.WriteLine($"Unzipping mod ({bytes.Length} bytes) ...");

      try {
        using (var memory = new MemoryStream(bytes)) {
          using (var zip = new ZipArchive(memory, ZipArchiveMode.Read)) {
            foreach (var entry in zip.Entries) {
              Console.WriteLine($"Processing {entry.Name} ...");
              using (var stream = entry.Open()) {
                string path = dev ? updateDir + entry.Name : plugins + Path.DirectorySeparatorChar + entry.Name;
                using (var fileStream = File.Open(path, FileMode.Create)) {
                  stream.CopyTo(fileStream);
                }
              }
            }
          }
        }
      }
      catch (Exception e) {
        Console.WriteLine($"Failed to unzip mod, {e.Message}");
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