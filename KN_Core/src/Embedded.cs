using System;
using System.IO;
using System.Reflection;
using KN_Loader;
using UnityEngine;

namespace KN_Core {
  public static class Embedded {
    private static Assembly assembly_;

    public static void Initialize() {
      assembly_ = Assembly.GetExecutingAssembly();
    }

    public static Texture2D LoadEmbeddedTexture(string path) {
      return LoadEmbeddedTexture(assembly_, $"KN_Core.Resources.{path}");
    }

    public static Texture2D LoadEmbeddedTexture(string path, Color32 mask) {
      return LoadEmbeddedTexture(assembly_, $"KN_Core.Resources.{path}", mask);
    }

    public static Texture2D LoadEmbeddedTexture(Assembly assembly, string path) {
      return LoadEmbeddedTexture(assembly, path, new Color32(0xff, 0xff, 0xff, 0xff));
    }

    public static Texture2D LoadEmbeddedTexture(Assembly assembly, string path, Color32 mask) {
      const int size = 4;

      var tex = new Texture2D(4, 4);

      using (var stream = assembly.GetManifestResourceStream(path)) {
        using (var memoryStream = new MemoryStream()) {
          if (stream != null) {
            stream.CopyTo(memoryStream);
            tex.LoadImage(memoryStream.ToArray());

            for (int i = 0; i < tex.width; ++i) {
              for (int j = 0; j < tex.height; ++j) {
                tex.SetPixel(i, j, tex.GetPixel(i, j) * mask);
              }
            }

            tex.Apply(true);
          }
          else {
            for (int i = 0; i < size; ++i) {
              for (int j = 0; j < size; ++j) {
                tex.SetPixel(i, j, mask);
              }
            }
          }
          tex.Apply(false);
        }
      }
      return tex;
    }

    public static Stream LoadEmbeddedFile(string path) {
      return LoadEmbeddedFile(assembly_, $"KN_Core.Resources.{path}");
    }

    public static Stream LoadEmbeddedFile(Assembly assembly, string path) {
      try {
        return assembly.GetManifestResourceStream(path);
      }
      catch (Exception e) {
        Log.Write($"[KN_Core::Embedded]: Unable to load embedded file '{path}', {e.Message}");
      }
      return null;
    }
  }
}