using System;
using System.Collections.Generic;
using System.IO;

namespace KN_Core {
  public struct ExhaustFifeData {
    public int CarId;
    public float MaxTime;
    public float FlamesTrigger;
    public float Volume;

    public ExhaustFifeData(int carId,float time, float trigger, float volume) {
      CarId = carId;
      MaxTime = time;
      FlamesTrigger = trigger;
      Volume = volume;
    }

    public void Serialize(BinaryWriter writer) {
      writer.Write(CarId);
      writer.Write(MaxTime);
      writer.Write(FlamesTrigger);
      writer.Write(Volume);
    }

    public void Deserialize(BinaryReader reader) {
      CarId = reader.ReadInt32();
      MaxTime = reader.ReadSingle();
      FlamesTrigger = reader.ReadSingle();
      Volume = reader.ReadSingle();
    }
  }

  public static class ExhaustSerializer {
    public static bool Serialize(List<ExhaustFifeData> data, string file) {
      try {
        using (var memoryStream = new MemoryStream()) {
          using (var writer = new BinaryWriter(memoryStream)) {
            writer.Write(Config.Version);
            writer.Write(data.Count);
            foreach (var e in data) {
              e.Serialize(writer);
            }
            using (var fileStream = File.Open(Config.BaseDir + file, FileMode.Create)) {
              memoryStream.WriteTo(fileStream);
            }
          }
        }
      }
      catch (Exception e) {
        Log.Write($"[KN_Core]: Unable to write file '{file}', {e.Message}");
        return false;
      }
      return true;
    }

    public static bool Deserialize(Stream stream, out List<ExhaustFifeData> data) {
      try {
        data = new List<ExhaustFifeData>();
        using (var reader = new BinaryReader(stream)) {
          reader.ReadInt32(); //unused
          int size = reader.ReadInt32();
          for (int i = 0; i < size; i++) {
            var e = new ExhaustFifeData();
            e.Deserialize(reader);
            data.Add(e);
          }
        }
      }
      catch (Exception e) {
        Log.Write($"[KN_Core]: Unable to read stream, {e.Message}");
        data = default;
        return false;
      }
      return true;
    }

    public static bool Deserialize(string file, out List<ExhaustFifeData> data) {
      try {
        data = new List<ExhaustFifeData>();
        using (var memoryStream = new MemoryStream(File.ReadAllBytes(Config.BaseDir + file))) {
          return Deserialize(memoryStream, out data);
        }
      }
      catch (Exception e) {
        Log.Write($"[KN_Core]: Unable to read file '{file}', {e.Message}");
        data = default;
        return false;
      }
    }
  }
}