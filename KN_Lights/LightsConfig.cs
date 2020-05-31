using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using KN_Core;

namespace KN_Lights {
  public class LcBase {
    public static bool Serialize(object config, string file) {
      try {
        var xns = new XmlSerializerNamespaces();
        xns.Add(string.Empty, string.Empty);
        var serializer = new XmlSerializer(config.GetType());

        using (var writer = new StreamWriter(Config.BaseDir + file)) {
          serializer.Serialize(writer.BaseStream, config, xns);
        }
      }
      catch (Exception e) {
        Log.Write($"[KN_Lights]: Unable to write file '{file}', {e.Message}");
        return false;
      }
      return true;
    }

    public static T Deserialize<T>(string file) {
      try {
        var serializer = new XmlSerializer(typeof(T));
        using (var reader = new StreamReader(Config.BaseDir + file)) {
          var deserialized = (T) serializer.Deserialize(reader.BaseStream);
          return deserialized;
        }
      }
      catch (Exception e) {
        Log.Write($"[KN_Lights]: Unable to read file '{file}', {e.Message}");
        return default;
      }
    }
  }

  [Serializable]
  public class LightsConfig : LcBase {
    public List<CarLights> CarLights { get; private set; }

    public LightsConfig() {
      CarLights = new List<CarLights>();
    }
  }

  [Serializable]
  public class NwLightsConfig : LcBase {
    public List<CarLights> CarLights { get; private set; }

    public NwLightsConfig() {
      CarLights = new List<CarLights>();
    }
  }
}