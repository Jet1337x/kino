using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace KN_Core {
  public static class Controls {
    private static Dictionary<string, object[]> buttons_ = new Dictionary<string, object[]>();
    private static Dictionary<string, object[]> defaultButtons_ = new Dictionary<string, object[]>();

    private static string cKey_ = string.Empty;
    private static List<object> cValue_ = new List<object>();

    public static bool Key(string name) {
      try {
        var ls = buttons_[name];
        return IsPressed(ls);
      }
      catch (ArgumentNullException) {
        Log.Write($"Action '{name}' is null");
      }
      catch (KeyNotFoundException) {
        Log.Write($"Action '{name}' does not exists");
      }

      return false;
    }

    public static bool KeyDown(string name) {
      try {
        var ls = buttons_[name];
        return IsClicked(ls);
      }
      catch (ArgumentNullException) {
        Log.Write($"Action '{name}' is null");
      }
      catch (KeyNotFoundException) {
        Log.Write($"Action '{name}' does not exists");
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
      defaultButtons_["mode"] = new object[] {KeyCode.F1};
      defaultButtons_["teleport"] = new object[] {KeyCode.F2};
      defaultButtons_["player_names"] = new object[] {KeyCode.F3};

      defaultButtons_["fix_car"] = new object[] {KeyCode.Home, "joystick button 13"};
      defaultButtons_["disable_all"] = new object[] {KeyCode.End, "joystick button 12"};

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
    }

    public static void Validate() {
      buttons_[cKey_] = cValue_.ToArray();
      cValue_.Clear();

      foreach (var p in defaultButtons_) {
        if (!buttons_.ContainsKey(p.Key)) {
          buttons_[p.Key] = p.Value;
        }
      }

      var toRemove = new List<string>();
      foreach (var p in buttons_) {
        if (!defaultButtons_.ContainsKey(p.Key)) {
          toRemove.Add(p.Key);
        }
      }

      foreach (var key in toRemove) {
        buttons_.Remove(key);
      }
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