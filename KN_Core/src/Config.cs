using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using BepInEx;

namespace KN_Core {
  internal enum ReadMode {
    None,
    Param,
    Controls
  }

  public class Config {
    public const int Version = 101;

    //cx stuff
    public const string CxUiCanvasName = "Root";
    public const string CxMainCameraTag = "MainCamera";

    public const string ConfigFile = "kn_config.xml";

    public const string FloatRegex = @"^[0-9]*(?:\.[0-9]*)?$";

    public static string BaseDir { get; private set; }
    public static string ReplaysDir { get; private set; }
    public static string VisualsDir { get; private set; }

    private readonly Dictionary<string, object> params_;
    private readonly Dictionary<string, object> defaultParams_;

    private bool initialized_;

    public Config() {
      params_ = new Dictionary<string, object>();
      defaultParams_ = new Dictionary<string, object>();

      BaseDir = Paths.PluginPath + Path.DirectorySeparatorChar + "KN_Base" + Path.DirectorySeparatorChar;
      if (!Directory.Exists(BaseDir)) {
        Directory.CreateDirectory(BaseDir);
      }

      ReplaysDir = BaseDir + Path.DirectorySeparatorChar + "replays" + Path.DirectorySeparatorChar;
      if (!Directory.Exists(ReplaysDir)) {
        Directory.CreateDirectory(ReplaysDir);
      }

      VisualsDir = BaseDir + Path.DirectorySeparatorChar + "visuals" + Path.DirectorySeparatorChar;
      if (!Directory.Exists(VisualsDir)) {
        Directory.CreateDirectory(VisualsDir);
      }

      Log.Write($"[KN_Core]: Base dir: '{BaseDir}'");
      LoadDefault();
    }

    public T Get<T>(string key) {
      try {
        return (T) params_[key];
      }
      catch (ArgumentNullException) {
        Log.Write($"Key '{key}' is null");
      }
      catch (KeyNotFoundException) {
        Log.Write($"Key '{key}' does not exists");
      }

      return default;
    }

    public void Set<T>(string key, T value) {
      try {
        params_[key] = value;
      }
      catch (ArgumentNullException) {
        Log.Write($"Key '{key}' is null");
      }
      catch (KeyNotFoundException) {
        Log.Write($"Key '{key}' does not exists");
      }
    }

    public void Write() {
      if (!initialized_) {
        return;
      }

      try {
        var settings = new XmlWriterSettings {Indent = true, IndentChars = "  ", Encoding = Encoding.UTF8};
        using (var writer = XmlWriter.Create(BaseDir + ConfigFile, settings)) {
          writer.WriteStartElement("config");

          //config values
          writer.WriteStartElement("params");
          foreach (var item in params_) {
            writer.WriteStartElement("item");

            writer.WriteAttributeString("key", item.Key);
            writer.WriteAttributeString("value", item.Value.ToString());
            writer.WriteAttributeString("type", item.Value.GetType().ToString());

            writer.WriteEndElement();
          }

          writer.WriteEndElement();

          Controls.Save(writer);

          writer.WriteEndElement();
        }
      }
      catch (Exception e) {
        Log.Write("Unable to write config: " + e.Message);
      }
    }

    public void Read() {
      try {
        var readMode = ReadMode.None;

        using (var reader = XmlReader.Create(BaseDir + ConfigFile)) {
          while (reader.Read()) {
            if (reader.NodeType == XmlNodeType.Element) {
              switch (reader.Name) {
                case "params":
                  readMode = ReadMode.Param;
                  break;
                case "controls":
                  readMode = ReadMode.Controls;
                  break;
              }
              Read(reader, readMode);
            }
          }
        }
      }
      catch (Exception e) {
        Log.Write("Unable to read config: " + e.Message);
      }

      Validate();
    }

    private void Read(XmlReader reader, ReadMode mode) {
      if (mode == ReadMode.Param) {
        if (!reader.HasAttributes) {
          return;
        }

        var key = reader.GetAttribute("key");
        var value = reader.GetAttribute("value");
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value)) {
          return;
        }

        var type = reader.GetAttribute("type");
        switch (type) {
          case "System.Single": {
            float.TryParse(value, out float val);
            params_[key] = val;
            break;
          }
          case "System.Boolean": {
            bool.TryParse(value, out bool val);
            params_[key] = val;
            break;
          }
        }
      }
      else if (mode == ReadMode.Controls) {
        Controls.Load(reader);
      }
    }

    private void LoadDefault() {
      defaultParams_["speed"] = 50.0f;
      defaultParams_["speed_multiplier"] = 2.0f;
      defaultParams_["freecam_speed"] = 5.0f;
      defaultParams_["freecam_speed_multiplier"] = 2.0f;

      defaultParams_["vinylcam_zoom"] = 0.0f;
      defaultParams_["vinylcam_shift_z"] = 0.0f;
      defaultParams_["vinylcam_shift_y"] = 0.0f;

      defaultParams_["r_points"] = false;
      defaultParams_["hide_cx_ui"] = true;
      defaultParams_["hide_names"] = false;
      defaultParams_["custom_backfire"] = true;
      defaultParams_["trash_autohide"] = false;
      defaultParams_["trash_autodisable"] = false;
      defaultParams_["custom_tach"] = false;

      defaultParams_["cl_discard_distance"] = 170.0f;

      defaultParams_["receive_udp"] = true;

      // air
      defaultParams_["air_use_controlKey"] = true;
      defaultParams_["air_step_max"] = 3.0f;
      defaultParams_["air_height_max"] = 0.5f;
      defaultParams_["air_height_min"] = 0.01f;

      Controls.LoadDefault();

      initialized_ = true;
    }

    private void Validate() {
      foreach (var p in defaultParams_) {
        if (!params_.ContainsKey(p.Key)) {
          params_[p.Key] = p.Value;
        }
        if (p.Key == "join_delay") {
          params_[p.Key] = p.Value;
        }
      }

      //remove old values
      var toRemove = new List<string>();
      foreach (var p in params_) {
        if (!defaultParams_.ContainsKey(p.Key)) {
          toRemove.Add(p.Key);
        }
      }

      foreach (var key in toRemove) {
        params_.Remove(key);
      }

      Controls.Validate();
    }
  }
}