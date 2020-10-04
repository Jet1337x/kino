using System.IO;
using KN_Core;

namespace KN_Lights {

  public class WorldLightsData : ISerializable {
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

    public bool Deserialize(BinaryReader reader, int version) {
      FogDistance = reader.ReadSingle();
      FogVolume = reader.ReadSingle();
      SunBrightness = reader.ReadSingle();
      SkyExposure = reader.ReadSingle();
      AmbientLight = reader.ReadSingle();
      SunTemp = reader.ReadSingle();
      Map = reader.ReadString();

      return true;
    }
  }
}