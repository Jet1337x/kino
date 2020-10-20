using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using KN_Loader;
using UnityEngine;

namespace KN_Core {
  public static class Controls {
    private static Dictionary<string, object[]> buttons_ = new Dictionary<string, object[]>();
    private static readonly Dictionary<string, object[]> defaultButtons_ = new Dictionary<string, object[]>();

    private static string cKey_ = string.Empty;
    private static readonly List<object> cValue_ = new List<object>();

    public static bool Key(string name) {
      try {
        if (buttons_.ContainsKey(name)) {
          var ls = buttons_[name];
          return IsPressed(ls);
        }
      }
      catch (ArgumentNullException) {
        Log.Write($"[KN_Core::Controls]: Action '{name}' is null");
      }
      catch (KeyNotFoundException) {
        Log.Write($"[KN_Core::Controls]: Action '{name}' does not exists");
      }

      return false;
    }

    public static bool KeyDown(string name) {
      try {
        if (buttons_.ContainsKey(name)) {
          var ls = buttons_[name];
          return IsClicked(ls);
        }
      }
      catch (ArgumentNullException) {
        Log.Write($"[KN_Core::Controls]: Action '{name}' is null");
      }
      catch (KeyNotFoundException) {
        Log.Write($"[KN_Core::Controls]: Action '{name}' does not exists");
      }

      return false;
    }

    public static void Load(XmlReader reader) {
      if (!reader.HasAttributes) {
        return;
      }

      var key = reader.GetAttribute("key");
      if (!string.IsNullOrEmpty(key)) {
        if (cKey_ != key) {
          if (cKey_ == string.Empty) {
            cKey_ = key;
            return;
          }

          buttons_[cKey_] = cValue_.ToArray();
          cValue_.Clear();

          cKey_ = key;
        }
      }
      else {
        var value = reader.GetAttribute("value");
        if (string.IsNullOrEmpty(value)) {
          return;
        }

        if (Enum.TryParse(value, out KeyCode keyCode)) {
          cValue_.Add(keyCode);
        }
        else {
          cValue_.Add(value);
        }
      }
    }

    public static void Save(XmlWriter writer) {
      Log.Write("[KN_Core::Controls]: Saving controls ...");

      writer.WriteStartElement("controls");

      //buttons
      writer.WriteStartElement("buttons");
      foreach (var item in buttons_) {
        writer.WriteStartElement("action");

        writer.WriteAttributeString("key", item.Key);

        foreach (var b in item.Value) {
          writer.WriteStartElement("button");
          writer.WriteAttributeString("value", b.ToString());
          writer.WriteEndElement();
        }

        writer.WriteEndElement();
      }

      writer.WriteEndElement();

      writer.WriteEndElement();
    }

    public static void LoadDefault() {
      defaultButtons_["gui"] = new object[] {KeyCode.F4};
      defaultButtons_["cinematic_mode"] = new object[] {KeyCode.F3};
      defaultButtons_["mode"] = new object[] {KeyCode.F1};
      defaultButtons_["teleport"] = new object[] {KeyCode.F2};

      defaultButtons_["fix_car"] = new object[] {KeyCode.Home};
      defaultButtons_["disable_all"] = new object[] {KeyCode.End};

      defaultButtons_["cam_align"] = new object[] {KeyCode.Mouse1};
      defaultButtons_["freecam_rotation"] = new object[] {KeyCode.Mouse1};

      defaultButtons_["forward"] = new object[] {KeyCode.W};
      defaultButtons_["backward"] = new object[] {KeyCode.S};
      defaultButtons_["right"] = new object[] {KeyCode.D};
      defaultButtons_["left"] = new object[] {KeyCode.A};
      defaultButtons_["up"] = new object[] {KeyCode.E};
      defaultButtons_["down"] = new object[] {KeyCode.Q};
      defaultButtons_["fast"] = new object[] {KeyCode.LeftShift};
      defaultButtons_["slow"] = new object[] {KeyCode.LeftControl};

      defaultButtons_["toggle_hazards"] = new object[] {KeyCode.PageUp};
      defaultButtons_["toggle_lights"] = new object[] {KeyCode.PageDown};

      // air
      defaultButtons_["air_mode"] = new object[] {KeyCode.None};
      defaultButtons_["air_controlKey"] = new object[] {KeyCode.LeftAlt};

      defaultButtons_["air_default"] = new object[] {KeyCode.None};
      defaultButtons_["air_up_all"] = new object[] {KeyCode.None};
      defaultButtons_["air_down_all"] = new object[] {KeyCode.None};

      defaultButtons_["air_up_front"] = new object[] {KeyCode.None};
      defaultButtons_["air_down_front"] = new object[] {KeyCode.None};

      defaultButtons_["air_up_rear"] = new object[] {KeyCode.None};
      defaultButtons_["air_down_rear"] = new object[] {KeyCode.None};

      defaultButtons_["air_frontLeft"] = new object[] {KeyCode.None};
      defaultButtons_["air_frontRight"] = new object[] {KeyCode.None};
      defaultButtons_["air_rearLeft"] = new object[] {KeyCode.None};
      defaultButtons_["air_rearRight"] = new object[] {KeyCode.None};
    }

    public static void Validate() {
      Log.Write("[KN_Core::Controls]: Validating controls ...");

      buttons_[cKey_] = cValue_.ToArray();
      cValue_.Clear();

      foreach (var p in defaultButtons_) {
        if (!buttons_.ContainsKey(p.Key)) {
          buttons_[p.Key] = p.Value;
        }
      }

      buttons_ = buttons_.Where(p => defaultButtons_.ContainsKey(p.Key)).ToDictionary(p => p.Key, p => p.Value);
    }

    private static bool IsPressed(IEnumerable<object> buttons) {
      bool pressed = false;
      foreach (var b in buttons) {
        if (b.GetType() == typeof(KeyCode)) {
          pressed = Input.GetKey((KeyCode) b);
        }

        if (pressed) {
          return true;
        }

        if (b is string s) {
          pressed = Input.GetKey(s);
        }

        if (pressed) {
          return true;
        }
      }

      return false;
    }

    private static bool IsClicked(IEnumerable<object> buttons) {
      bool pressed = false;
      foreach (var b in buttons) {
        if (b.GetType() == typeof(KeyCode)) {
          pressed = Input.GetKeyDown((KeyCode) b);
        }

        if (pressed) {
          return true;
        }

        if (b is string s) {
          pressed = Input.GetKeyDown(s);
        }

        if (pressed) {
          return true;
        }
      }

      return false;
    }
  }
}