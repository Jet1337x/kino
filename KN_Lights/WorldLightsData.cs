using System;
using System.Collections.Generic;
using System.IO;
using KN_Core;

namespace KN_Lights {
  public static class WorldLightsDataSerializer {
    public static bool Serialize(List<WorldLightsData> data, string file) {
      try {
        using (var memoryStream = new MemoryStream()) {
          using (var writer = new BinaryWriter(memoryStream)) {
            writer.Write(data.Count);
            foreach (var d in data) {
              d.Serialize(writer);
            }
            using (var fileStream = File.Open(Config.BaseDir + file, FileMode.Create)) {
              memoryStream.WriteTo(fileStream);
            }
          }
        }
      }
      catch (Exception e) {
        Log.Write($"[KN_Lights]: Unable to write file '{file}', {e.Message}");
        return false;
      }
      return true;
    }

    public static bool Deserialize(string file, out List<WorldLightsData> data) {
      try {
        using (var memoryStream = new MemoryStream(File.ReadAllBytes(Config.BaseDir + file))) {
          using (var reader = new BinaryReader(memoryStream)) {
            data = new List<WorldLightsData>();
            int size = reader.ReadInt32();
            for (int i = 0; i < size; i++) {
              var d = new WorldLightsData();
              d.Deserialize(reader);
              data.Add(d);
            }
          }
        }
      }
      catch (Exception e) {
        Log.Write($"[KN_Lights]: Unable to read file '{file}', {e.Message}");
        data = default;
        return false;
      }
      return true;
    }
  }

  public class WorldLightsData {
    public const string ConfigFile = "kn_wldata.knl";

    public float FogDistance;
    public float FogVolume;
    public float SunBrightness;
    public float SkyExposure;
    public float AmbientLight;
    public float SunTemp = 6300.0f;
    public string Map;

    public WorldLightsData(string map) {
      Map = map;
    }

    public WorldLightsData() { }

    public void Serialize(BinaryWriter writer) {
      writer.Write(FogDistance);
      writer.Write(FogVolume);
      writer.Write(SunBrightness);
      writer.Write(SkyExposure);
      writer.Write(AmbientLight);
      writer.Write(SunTemp);
      writer.Write(Map);
    }

    public void Deserialize(BinaryReader reader) {
      FogDistance = reader.ReadSingle();
      FogVolume = reader.ReadSingle();
      SunBrightness = reader.ReadSingle();
      SkyExposure = reader.ReadSingle();
      AmbientLight = reader.ReadSingle();
      SunTemp = reader.ReadSingle();
      Map = reader.ReadString();
    }
  }
}