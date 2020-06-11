using KN_Core;
using UnityEngine;

namespace KN_Lights {
  public class StaticLightData {
    private GameObject debugObject_;
    private bool dbObjects_;
    public bool DebugObjectsEnabled {
      get => dbObjects_;
      set {
        dbObjects_ = value;
        if (debugObject_ != null) {
          debugObject_.SetActive(dbObjects_);
        }
      }
    }

    private LightType type_;
    public LightType Type {
      get => type_;
      set {
        type_ = value;
        if (Light != null) {
          var l = Light.GetComponent<Light>();
          l.type = value;
        }
      }
    }
    public string Name { get; }

    public GameObject Light { get; private set; }

    private bool enabled_;
    public bool Enabled {
      get => enabled_;
      set {
        enabled_ = value;
        if (Light != null) {
          Light.SetActive(value);
        }
      }
    }

    private Color color_;
    public Color Color {
      get => color_;
      set {
        color_ = value;
        if (Light != null) {
          var l = Light.GetComponent<Light>();
          l.color = value;
        }
      }
    }

    private float brightness_;
    public float Brightness {
      get => brightness_;
      set {
        brightness_ = value;
        if (Light != null) {
          var l = Light.GetComponent<Light>();
          l.intensity = value;
        }
      }
    }

    private float angle_;
    public float Angle {
      get => angle_;
      set {
        angle_ = value;
        if (Light != null) {
          var l = Light.GetComponent<Light>();
          l.spotAngle = value;
        }
      }
    }

    private float range_;
    public float Range {
      get => range_;
      set {
        range_ = value;
        if (Light != null) {
          var l = Light.GetComponent<Light>();
          l.range = value;
        }
      }
    }

    public Vector3 Position {
      get => Light.transform.position;
      set {
        if (Parent != null) {
          Light.transform.localPosition = value;
        }
        else {
          Light.transform.position = value;
        }
      }
    }

    public Vector3 Rotation {
      get => Light.transform.eulerAngles;
      set {
        if (Parent != null) {
          Light.transform.localEulerAngles = value;
        }
        else {
          Light.transform.eulerAngles = value;
        }
      }
    }

    public Transform Parent { get; private set; }

    private static readonly int BaseColorMap = Shader.PropertyToID("_BaseColorMap");

    public StaticLightData(LightType type, string name, Transform transform) {
      Type = type;
      Name = name;
      enabled_ = true;
      color_ = Color.white;
      Parent = null;

      Initialize();

      var scale = new Vector3(0.1f, 0.15f, 0.1f);

      Light.transform.position = transform.position;
      Light.transform.rotation = transform.rotation;

      debugObject_.transform.position = transform.position;
      debugObject_.transform.rotation = transform.rotation;
      debugObject_.transform.localScale = scale;
      debugObject_.transform.parent = Light.transform;
    }

    public void Attach(TFCar car) {
      if (Light == null || TFCar.IsNull(car)) {
        return;
      }
      Light.transform.parent = car.Transform;
    }

    private void Initialize() {
      if (Light != null) {
        Object.Destroy(Light);
      }
      Light = new GameObject();
      Light.AddComponent<Light>();
      Light.SetActive(Enabled);

      var light = Light.GetComponent<Light>();
      MakeLight(ref light);

      InitializeDebug();
    }

    private void InitializeDebug() {
      var mat = new Material(Shader.Find("HDRP/Lit"));
      mat.SetTexture(BaseColorMap, Core.CreateTexture(Color.white));

      if (debugObject_ != null) {
        Object.Destroy(debugObject_);
      }
      debugObject_ = GameObject.CreatePrimitive(PrimitiveType.Capsule);
      debugObject_.GetComponent<MeshRenderer>().material = mat;
      debugObject_.SetActive(DebugObjectsEnabled);
    }

    private void MakeLight(ref Light light) {
      if (Type == LightType.Spot) {
        light.spotAngle = Angle;
        light.innerSpotAngle = 50.0f;
      }
      light.type = Type;
      light.color = Color;
      light.range = Range;
      light.intensity = Brightness;
    }

    public void Dispose() {
      Object.Destroy(Light);
    }
  }
}