using System.IO;
using KN_Core;
using UnityEngine;

namespace KN_Lights {
  public class DashLight {
    public const float DefaultBrightness = 2.0f;
    public const float DefaultRange = 0.49f;

    private static readonly int BaseColorMap = Shader.PropertyToID("_BaseColorMap");

    public KnCar Car { get; private set; }

    public GameObject Light { get; private set; }
    public GameObject DebugObject { get; private set; }

    private bool enabled_;
    public bool Enabled {
      get => enabled_;
      set {
        enabled_ = value;
        if (Light != null) {
          Light.SetActive(enabled_);
        }
      }
    }

    private bool debug_;
    public bool Debug {
      get => debug_;
      set {
        debug_ = value;
        if (DebugObject != null) {
          DebugObject.SetActive(debug_);
        }
      }
    }

    private Color color_;
    public Color Color {
      get => color_;
      set {
        color_ = value;
        if (GetLight(out var l)) {
          l.color = color_;
        }
      }
    }

    private float range_;
    public float Range {
      get => range_;
      set {
        range_ = value;
        if (GetLight(out var l)) {
          l.range = range_;
        }
      }
    }

    private float brightness_;
    public float Brightness {
      get => brightness_;
      set {
        brightness_ = value;
        if (GetLight(out var l)) {
          l.intensity = brightness_;
        }
      }
    }

    private Vector3 offset_;
    public Vector3 Offset {
      get => offset_;
      set {
        offset_ = value;
        if (Light != null) {
          Light.transform.localPosition = offset_;
        }
      }
    }

    public DashLight(Color color, float brightness, float range, Vector3 offset, bool enabled) {
      enabled_ = enabled;
      color_ = color;
      brightness_ = brightness;
      range_ = range;
      Offset = offset;
    }

    public DashLight(BinaryReader reader) {
      Deserialize(reader);
    }

    public DashLight Copy() {
      var light = new DashLight(color_, brightness_, range_, Offset, enabled_);
      return light;
    }

    public void Dispose() {
      if (Light != null) {
        Object.Destroy(Light);
      }
      if (DebugObject != null) {
        Object.Destroy(DebugObject);
      }
    }

    public void Attach(KnCar car) {
      Car = car;

      var position = car.Transform.position;

      var capsuleScale = new Vector3(0.1f, 0.1f, 0.1f);

      Initialize();

      Light.transform.parent = car.Transform;
      Light.transform.position = position;
      Light.transform.localPosition += offset_;
      DebugObject.transform.parent = Light.transform;
      DebugObject.transform.position = Light.transform.position;
      DebugObject.transform.localScale = capsuleScale;

      MakeLights();
    }

    private void Initialize() {
      Dispose();

      Light = new GameObject();
      Light.AddComponent<Light>();
      Light.SetActive(enabled_);

      InitializeDebug();
    }

    private void InitializeDebug() {
      var material = new Material(Shader.Find("HDRP/Lit"));
      material.SetTexture(BaseColorMap, KnUtils.CreateTexture(Color.cyan));

      DebugObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
      DebugObject.GetComponent<MeshRenderer>().material = material;
      DebugObject.SetActive(Debug);
    }

    private void MakeLights() {
      var light = Light.GetComponent<Light>();
      MakeLight(ref light, color_, brightness_, range_);
    }

    public static void MakeLight(ref Light light, Color color, float power, float range) {
      light.type = LightType.Point;
      light.color = color;
      light.range = range;
      light.intensity = power;
    }

    private bool GetLight(out Light light) {
      light = Light.GetComponent<Light>();
      return light != null;
    }

    public void Serialize(BinaryWriter writer) {
      writer.Write(Enabled);
      writer.Write(KnUtils.EncodeColor(Color));
      writer.Write(Range);
      writer.Write(Brightness);
      KnUtils.WriteVec3(writer, Offset);
    }

    public void Deserialize(BinaryReader reader) {
      enabled_ = reader.ReadBoolean();
      color_ = KnUtils.DecodeColor(reader.ReadInt32());
      range_ = reader.ReadSingle();
      brightness_ = reader.ReadSingle();
      Offset = KnUtils.ReadVec3(reader);
    }
  }
}