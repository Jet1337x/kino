using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace KN_Updater {

  public class Embedded {
    private static Dictionary<string, Assembly> assemblies_;

    public static void Load(string embeddedResource, string fileName) {
      if (assemblies_ == null) {
        assemblies_ = new Dictionary<string, Assembly>();
      }

      byte[] bytes;
      var currentAssembly = Assembly.GetExecutingAssembly();

      using (var stream = currentAssembly.GetManifestResourceStream(embeddedResource)) {
        if (stream == null) {
          Console.WriteLine($"'{embeddedResource}' is not found in Embedded Resources.");
          return;
        }

        bytes = new byte[(int) stream.Length];
        stream.Read(bytes, 0, (int) stream.Length);

        try {
          var assembly = Assembly.Load(bytes);
          assemblies_.Add(assembly.FullName, assembly);
          return;
        }
        catch {
          // ignored
        }
      }

      bool fileOk;
      string tempFile;

      using (var sha1 = new SHA1CryptoServiceProvider()) {
        string fileHash = BitConverter.ToString(sha1.ComputeHash(bytes)).Replace("-", string.Empty);

        tempFile = Path.GetTempPath() + fileName;

        if (File.Exists(tempFile)) {
          string fileHash2 = BitConverter.ToString(sha1.ComputeHash(File.ReadAllBytes(tempFile))).Replace("-", string.Empty);
          fileOk = fileHash == fileHash2;
        }
        else {
          fileOk = false;
        }
      }

      if (!fileOk) {
        File.WriteAllBytes(tempFile, bytes);
      }

      var newAssembly = Assembly.LoadFile(tempFile);
      assemblies_.Add(newAssembly.FullName, newAssembly);
    }

    public static Assembly Get(string assemblyFullName) {
      if (assemblies_ == null || assemblies_.Count == 0) {
        return null;
      }

      if (assemblies_.ContainsKey(assemblyFullName)) {
        return assemblies_[assemblyFullName];
      }

      return null;
    }
  }
}