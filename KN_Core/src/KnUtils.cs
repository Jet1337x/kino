using System.Reflection;

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
  }
}