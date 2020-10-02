using System.Reflection;
using BepInEx.Bootstrap;
using KN_Loader;
using UnityEngine;

namespace KN_Core {
  public static class KnUtils {
    public static void SetField(object target, string fieldName, object value) {
      var t = target.GetType();
      FieldInfo fi = null;

      while (t != null) {
        fi = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (fi != null) {
          break;
        }
        t = t.BaseType;
      }
      if (fi == null) {
        Log.Write($"[KN_Utils]: (SetField) Field '{fieldName}' not found in type hierarchy.");
      }
      fi?.SetValue(target, value);
    }

    public static object GetField(object target, string fieldName) {
      var t = target.GetType();
      FieldInfo fi = null;

      while (t != null) {
        fi = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (fi != null) {
          break;
        }
        t = t.BaseType;
      }
      if (fi == null) {
        Log.Write($"[KN_Utils]: (GetField) Field '{fieldName}' not found in type hierarchy.");
      }
      return fi?.GetValue(target);
    }

    public static MethodInfo GetMethod(object target, string methodName) {
      var t = target.GetType();
      MethodInfo mi = null;

      while (t != null) {
        mi = t.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (mi != null) {
          break;
        }
        t = t.BaseType;
      }
      if (mi == null) {
        Log.Write($"[KN_Utils]: (GetMethod) Method '{methodName}' not found in type hierarchy.");
      }
      return mi;
    }

    public static Texture2D CreateTexture(Color color) {
      var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false) {
        wrapMode = TextureWrapMode.Clamp
      };
      texture.SetPixel(0, 0, color);
      texture.Apply();

      return texture;
    }

    public static Color32 DecodeColor(int color) {
      return new Color32 {
        a = (byte) ((color >> 24) & 0xff),
        r = (byte) ((color >> 16) & 0xff),
        g = (byte) ((color >> 8) & 0xff),
        b = (byte) (color & 0xff)
      };
    }

    public static int EncodeColor(Color32 color) {
      return (color.a & 0xff) << 24 | (color.r & 0xff) << 16 | (color.g & 0xff) << 8 | (color.b & 0xff);
    }

    private static void KillFlyMod() {
      const string flyModGuid = "fly.mod.goat";

      if (Chainloader.PluginInfos.ContainsKey(flyModGuid)) {
        var flyMod = Chainloader.PluginInfos[flyModGuid];
        if (flyMod != null) {
          flyMod.Instance.enabled = false;
        }
      }
    }
  }
}