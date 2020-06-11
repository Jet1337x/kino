using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KN_Core;

namespace KN_Lights {
  public static class LightsConfigSerializer {
    public static bool Serialize(LightsConfigBase config, string file) {
      try {
        using (var memoryStream = new MemoryStream()) {
          using (var writer = new BinaryWriter(memoryStream)) {
            writer.Write(Config.Version);
            writer.Write(config.Lights.Count);
            foreach (var l in config.Lights) {
              l.Serialize(writer);
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

    public static bool Deserialize(Stream stream, out List<CarLights> lights) {
      try {
        lights = new List<CarLights>();
        using (var reader = new BinaryReader(stream)) {
          int version = reader.ReadInt32();
          int size = reader.ReadInt32();
          for (int i = 0; i < size; i++) {
            var cl = new CarLights();
            cl.Deserialize(reader, version);
            lights.Add(cl);
          }
        }
      }
      catch (Exception e) {
        Log.Write($"[KN_Lights]: Unable to read stream, {e.Message}");
        lights = default;
        return false;
      }
      return true;
    }

    public static bool Deserialize(string file, out List<CarLights> lights) {
      try {
        lights = new List<CarLights>();
        using (var memoryStream = new MemoryStream(File.ReadAllBytes(Config.BaseDir + file))) {
          return Deserialize(memoryStream, out lights);
        }
      }
      catch (Exception e) {
        Log.Write($"[KN_Lights]: Unable to read file '{file}', {e.Message}");
        lights = default;
        return false;
      }
    }
  }

  public class LightsConfigBase {
    public List<CarLights> Lights { get; protected set; }
    protected LightsConfigBase() {
      Lights = new List<CarLights>();
    }

    public void AddLights(CarLights lights) {
      int id = Lights.FindIndex(cl => cl.Car == lights.Car);
      if (id != -1) {
        Lights[id] = lights;
        return;
      }
      Lights.Add(lights);
    }
  }

  public class LightsConfig : LightsConfigBase {
    public LightsConfig() { }

    public LightsConfig(List<CarLights> lights) {
      Lights = lights;
    }

    public CarLights GetLights(int carId) {
      return Lights.FirstOrDefault(light => light.CarId == carId);
    }
  }

  public class NwLightsConfig : LightsConfigBase {
    public NwLightsConfig() { }
    public NwLightsConfig(List<CarLights> lights) {
      Lights = lights;
    }
    public CarLights GetLights(int carId, string user) {
      return Lights.FirstOrDefault(cl => cl.CarId == carId && cl.UserName == user);
    }
  }
}