using System.IO;
using KN_Core;
using UnityEngine;

namespace KN_Lights {
  public class LightsSet {
    public const float DefaultIllumination = 10.0f;
    public const float DefaultRange = 0.3f;
    private const float InnerBrightnessScale = 0.5f;
    private const float InnerAngleScale = 2.0f;
    private const float InnerRangeScale = 0.7f;

    private static readonly int BaseColorMap = Shader.PropertyToID("_BaseColorMap");

    public KnCar Car { get; private set; }

    public GameObject Left { get; private set; }
    public GameObject Right { get; private set; }

    public GameObject LeftInner { get; private set; }
    public GameObject RightInner { get; private set; }

    public GameObject LeftIl { get; private set; }
    public GameObject RightIl { get; private set; }


    private bool enabled_;
    public bool Enabled {
      get => enabled_;
      set {
        enabled_ = value;
        if (enabled_) {
          EnabledLeft = enabledLeft_ && enabled_;
          EnabledRight = enabledRight_ && enabled_;
          Illumination = enabled_;
        }
        else {
          if (Left != null) {
            Left.SetActive(enabled_);
          }
          if (Right != null) {
            Right.SetActive(enabled_);
          }

          bool ilEnabled = quality_ >= Quality.Medium || own_;
          if (ilEnabled) {
            if (LeftIl != null) {
              LeftIl.SetActive(enabled_);
            }
            if (RightIl != null) {
              RightIl.SetActive(enabled_);
            }
          }

          if (!inverted_) {
            bool innerEnabled = quality_ >= Quality.Medium && own_ || quality_ >= Quality.High;
            if (innerEnabled) {
              if (LeftInner != null) {
                LeftInner.SetActive(enabled_);
              }
              if (RightInner != null) {
                RightInner.SetActive(enabled_);
              }
            }
          }
        }
      }
    }

    private bool illumination_;
    public bool Illumination {
      get => illumination_;
      set {
        if (quality_ >= Quality.Medium || own_) {
          illumination_ = value;
          if (LeftIl != null) {
            LeftIl.SetActive(enabledLeft_ && illumination_);
          }
          if (RightIl != null) {
            RightIl.SetActive(enabledRight_ && illumination_);
          }
        }
      }
    }

    private float ilIntensity_;
    public float IlIntensity {
      get => ilIntensity_;
      set {
        if (quality_ >= Quality.Medium || own_) {
          ilIntensity_ = value;
          if (GetIllumination(out var l, out var r)) {
            l.intensity = ilIntensity_;
            r.intensity = ilIntensity_;
          }
        }
      }
    }

    private float range_;
    public float Range {
      get => range_;
      set {
        if (quality_ >= Quality.Medium || own_) {
          range_ = value;
          if (GetIllumination(out var l, out var r)) {
            l.range = range_;
            r.range = range_;
          }
        }
      }
    }

    private bool enabledLeft_;
    public bool EnabledLeft {
      get => enabledLeft_;
      set {
        enabledLeft_ = value;
        if (Left != null) {
          Left.SetActive(enabledLeft_);
        }
        bool ilEnabled = quality_ >= Quality.Medium || own_;
        if (ilEnabled && LeftIl != null) {
          LeftIl.SetActive(enabledLeft_ && illumination_);
        }
        if (!inverted_) {
          bool innerEnabled = quality_ >= Quality.Medium && own_ || quality_ >= Quality.High;
          if (innerEnabled && LeftInner != null) {
            LeftInner.SetActive(enabledLeft_);
          }
        }
      }
    }

    private bool enabledRight_;
    public bool EnabledRight {
      get => enabledRight_;
      set {
        enabledRight_ = value;
        if (Right != null) {
          Right.SetActive(enabledRight_);
        }
        bool ilEnabled = quality_ >= Quality.Medium || own_;
        if (ilEnabled && RightIl != null) {
          RightIl.SetActive(enabledRight_ && illumination_);
        }
        if (!inverted_) {
          bool innerEnabled = quality_ >= Quality.Medium && own_ || quality_ >= Quality.High;
          if (innerEnabled && RightInner != null) {
            RightInner.SetActive(enabledRight_);
          }
        }
      }
    }

    public GameObject DebugLeft { get; private set; }
    public GameObject DebugRight { get; private set; }

    private bool debug_;
    public bool Debug {
      get => debug_;
      set {
        debug_ = value;
        if (DebugLeft != null) {
          DebugLeft.SetActive(debug_);
        }
        if (DebugRight != null) {
          DebugRight.SetActive(debug_);
        }
      }
    }

    private Color color_;
    public Color Color {
      get => color_;
      set {
        color_ = value;
        if (GetLights(out var l, out var r)) {
          l.color = color_;
          r.color = color_;
        }

        bool ilEnabled = quality_ >= Quality.Medium || own_;
        if (ilEnabled && GetIllumination(out var il, out var ir)) {
          il.color = color_;
          ir.color = color_;
        }
        if (!inverted_) {
          bool innerEnabled = quality_ >= Quality.Medium && own_ || quality_ >= Quality.High;
          if (innerEnabled && GetInners(out var li, out var ri)) {
            li.color = color_;
            ri.color = color_;
          }
        }
      }
    }

    private float pitch_;
    public float Pitch {
      get => pitch_;
      set {
        pitch_ = value;
        if (KnCar.IsNull(Car)) {
          return;
        }
        float pitch = inverted_ ? 180.0f - pitch_ : pitch_;

        var rot = Car.Transform.rotation;
        if (Left != null) {
          Left.transform.rotation = rot * Quaternion.AngleAxis(pitch, Vector3.right);
        }
        if (Right != null) {
          Right.transform.rotation = rot * Quaternion.AngleAxis(pitch, Vector3.right);
        }

        if (!inverted_) {
          bool innerEnabled = quality_ >= Quality.Medium && own_ || quality_ >= Quality.High;
          if (innerEnabled) {
            if (LeftInner != null) {
              LeftInner.transform.rotation = rot * Quaternion.AngleAxis(pitch, Vector3.right);
            }
            if (RightInner != null) {
              RightInner.transform.rotation = rot * Quaternion.AngleAxis(pitch, Vector3.right);
            }
          }
        }

        float angle = inverted_ ? 90.0f - pitch_ : 90.0f + pitch_;
        if (DebugLeft != null) {
          DebugLeft.transform.rotation = rot * Quaternion.AngleAxis(angle, Vector3.right);
        }
        if (DebugRight != null) {
          DebugRight.transform.rotation = rot * Quaternion.AngleAxis(angle, Vector3.right);
        }
      }
    }

    private float brightness_;
    public float Brightness {
      get => brightness_;
      set {
        brightness_ = value;
        if (GetLights(out var l, out var r)) {
          l.intensity = brightness_;
          r.intensity = brightness_;
        }

        if (!inverted_) {
          bool innerEnabled = quality_ >= Quality.Medium && own_ || quality_ >= Quality.High;
          if (innerEnabled && GetInners(out var li, out var ri)) {
            li.intensity = brightness_ * InnerBrightnessScale;
            ri.intensity = brightness_ * InnerBrightnessScale;
          }
        }
      }
    }

    private float angle_;
    public float Angle {
      get => angle_;
      set {
        angle_ = value;
        if (GetLights(out var l, out var r)) {
          l.spotAngle = angle_;
          r.spotAngle = angle_;
        }

        if (!inverted_) {
          bool innerEnabled = quality_ >= Quality.Medium && own_ || quality_ >= Quality.High;
          if (innerEnabled && GetInners(out var li, out var ri)) {
            li.spotAngle = angle_ * InnerAngleScale;
            ri.spotAngle = angle_ * InnerAngleScale;
          }
        }
      }
    }

    private Vector3 offsetRight_;
    private Vector3 offsetLeft_;
    public Vector3 Offset {
      get => offsetRight_;
      set {
        offsetRight_ = value;
        offsetLeft_ = value;
        offsetLeft_.x = -offsetLeft_.x;
        if (Left != null) {
          Left.transform.localPosition = offsetLeft_;
        }
        if (Right != null) {
          Right.transform.localPosition = offsetRight_;
        }

        bool ilEnabled = own_ || quality_ >= Quality.Medium;
        if (ilEnabled) {
          if (LeftIl != null) {
            LeftIl.transform.localPosition = offsetLeft_;
          }
          if (RightIl != null) {
            RightIl.transform.localPosition = offsetRight_;
          }
        }

        if (!inverted_) {
          bool innerEnabled = quality_ >= Quality.Medium && own_ || quality_ >= Quality.High;
          if (innerEnabled) {
            if (LeftInner != null) {
              LeftInner.transform.localPosition = offsetLeft_;
            }
            if (RightInner != null) {
              RightInner.transform.localPosition = offsetRight_;
            }
          }
        }
      }
    }

    private Quality quality_;
    private bool own_;
    private bool inverted_;
    private Color debugColor_;

    public LightsSet(Quality quality, bool own, Color color, float ilIntensity, float range, float pitch, float brightness,
      float angle, Vector3 offset, bool enabledLeft, bool enabledRight, bool inverted) {

      inverted_ = inverted;
      own_ = own;
      quality_ = quality;
      ilIntensity_ = ilIntensity;
      range_ = range;
      enabledLeft_ = enabledLeft;
      enabledRight_ = enabledRight;
      enabled_ = enabledLeft_ || enabledRight_;
      color_ = color;
      pitch_ = pitch;
      brightness_ = brightness;
      angle_ = angle;
      Offset = offset;

      debugColor_ = Color.white;
    }

    public LightsSet(BinaryReader reader, bool inverted) {
      inverted_ = inverted;
      Deserialize(reader);

      debugColor_ = Color.white;
      enabled_ = enabledLeft_ || enabledRight_;
    }

    public void Dispose() {
      if (Left != null) {
        Object.Destroy(Left);
      }
      if (LeftInner != null) {
        Object.Destroy(LeftInner);
      }
      if (LeftIl != null) {
        Object.Destroy(LeftIl);
      }
      if (DebugLeft != null) {
        Object.Destroy(DebugLeft);
      }

      if (Right != null) {
        Object.Destroy(Right);
      }
      if (RightInner != null) {
        Object.Destroy(RightInner);
      }
      if (RightIl != null) {
        Object.Destroy(RightIl);
      }
      if (DebugRight != null) {
        Object.Destroy(DebugRight);
      }
    }

    public LightsSet Copy() {
      var set = new LightsSet(quality_, own_, color_, ilIntensity_, range_, pitch_, brightness_, angle_, Offset, enabledLeft_, enabledRight_, inverted_);
      return set;
    }

    public void Attach(Quality quality, bool own, KnCar car, bool tailLights, Color debug) {
      Car = car;
      own_ = own;
      quality_ = quality;
      inverted_ = tailLights;
      debugColor_ = debug;

      bool ilEnabled = quality_ >= Quality.Medium || own_;
      bool innerEnabled = !inverted_ && (quality_ >= Quality.Medium && own_ || quality_ >= Quality.High);

      var position = car.Transform.position;
      var rotation = car.Transform.rotation;

      var capsuleScale = new Vector3(0.1f, 0.15f, 0.1f);

      Quaternion lightsRotation;
      Quaternion debugRotation;
      if (!tailLights) {
        lightsRotation = rotation * Quaternion.AngleAxis(Pitch, Vector3.right);
        debugRotation = rotation * Quaternion.AngleAxis(90.0f + Pitch, Vector3.right);
      }
      else {
        lightsRotation = rotation * Quaternion.AngleAxis(180.0f, Vector3.up) * Quaternion.AngleAxis(Pitch, -Vector3.right);
        debugRotation = rotation * Quaternion.AngleAxis(180.0f, Vector3.up) * Quaternion.AngleAxis(90.0f + Pitch, -Vector3.right);
      }

      Initialize(debug, ilEnabled, innerEnabled);

      Left.transform.parent = car.Transform;
      Left.transform.position = position;
      Left.transform.rotation = lightsRotation;
      Left.transform.localPosition += offsetLeft_;
      if (innerEnabled) {
        LeftInner.transform.parent = car.Transform;
        LeftInner.transform.position = position;
        LeftInner.transform.rotation = lightsRotation;
        LeftInner.transform.localPosition += offsetLeft_;
      }
      if (ilEnabled) {
        LeftIl.transform.parent = car.Transform;
        LeftIl.transform.position = Left.transform.position;
      }

      DebugLeft.transform.parent = Left.transform;
      DebugLeft.transform.position = Left.transform.position;
      DebugLeft.transform.rotation = debugRotation;
      DebugLeft.transform.localScale = capsuleScale;

      Right.transform.parent = car.Transform;
      Right.transform.position = position;
      Right.transform.rotation = lightsRotation;
      Right.transform.localPosition += offsetRight_;
      if (innerEnabled) {
        RightInner.transform.parent = car.Transform;
        RightInner.transform.position = position;
        RightInner.transform.rotation = lightsRotation;
        RightInner.transform.localPosition += offsetRight_;
      }
      if (ilEnabled) {
        RightIl.transform.parent = car.Transform;
        RightIl.transform.position = Right.transform.position;
      }

      DebugRight.transform.parent = Right.transform;
      DebugRight.transform.position = Right.transform.position;
      DebugRight.transform.rotation = debugRotation;
      DebugRight.transform.localScale = capsuleScale;

      MakeLights(tailLights, ilEnabled, innerEnabled);

      Enabled = true;
    }

    private void Initialize(Color debug, bool il, bool inner) {
      Dispose();

      Left = new GameObject();
      Left.AddComponent<Light>();
      Left.SetActive(enabledLeft_);
      if (inner) {
        LeftInner = new GameObject();
        LeftInner.AddComponent<Light>();
        LeftInner.SetActive(enabledLeft_);
      }
      if (il) {
        LeftIl = new GameObject();
        LeftIl.AddComponent<Light>();
        LeftIl.SetActive(enabledLeft_);
      }

      Right = new GameObject();
      Right.AddComponent<Light>();
      Right.SetActive(enabledRight_);
      if (inner) {
        RightInner = new GameObject();
        RightInner.AddComponent<Light>();
        RightInner.SetActive(enabledRight_);
      }
      if (il) {
        RightIl = new GameObject();
        RightIl.AddComponent<Light>();
        RightIl.SetActive(enabledRight_);
      }

      InitializeDebug(debug);
    }

    private void InitializeDebug(Color debug) {
      var material = new Material(Shader.Find("HDRP/Lit"));
      material.SetTexture(BaseColorMap, KnUtils.CreateTexture(debug));

      DebugLeft = GameObject.CreatePrimitive(PrimitiveType.Capsule);
      DebugLeft.GetComponent<MeshRenderer>().material = material;
      DebugLeft.SetActive(Debug);

      DebugRight = GameObject.CreatePrimitive(PrimitiveType.Capsule);
      DebugRight.GetComponent<MeshRenderer>().material = material;
      DebugRight.SetActive(Debug);
    }

    private void MakeLights(bool tailLights, bool illumination, bool inner) {
      const float range = 25.0f;

      var ll = Left.GetComponent<Light>();
      var lr = Right.GetComponent<Light>();
      if (!tailLights) {
        MakeHeadLight(ref ll, color_, brightness_, angle_, range);
        MakeHeadLight(ref lr, color_, brightness_, angle_, range);
        if (inner) {
          var li = LeftInner.GetComponent<Light>();
          var ri = RightInner.GetComponent<Light>();
          MakeHeadLight(ref li, color_, brightness_ * InnerBrightnessScale, angle_ * InnerAngleScale, range * InnerRangeScale);
          MakeHeadLight(ref ri, color_, brightness_ * InnerBrightnessScale, angle_ * InnerAngleScale, range * InnerRangeScale);
        }
      }
      else {
        MakeTailLight(ref ll, brightness_, angle_);
        MakeTailLight(ref lr, brightness_, angle_);
      }

      if (illumination) {
        var il = LeftIl.GetComponent<Light>();
        var ir = RightIl.GetComponent<Light>();
        MakeIllumination(ref il, color_, ilIntensity_, range_);
        MakeIllumination(ref ir, color_, ilIntensity_, range_);
      }
    }

    public void SetIntensity(float val) {
      if (GetLights(out var l, out var r)) {
        l.intensity = val;
        r.intensity = val;
      }
    }

    public void SetIlluminationIntensity(float val) {
      bool ilEnabled = quality_ >= Quality.Medium || own_;
      if (ilEnabled && GetIllumination(out var l, out var r)) {
        l.intensity = val;
        r.intensity = val;
      }
    }

    public static void MakeHeadLight(ref Light light, Color color, float brightness, float angle, float range) {
      light.type = LightType.Spot;
      light.color = color;
      light.range = range;
      light.intensity = brightness;
      light.spotAngle = angle;
      light.innerSpotAngle = 90.0f;
      light.cookie = Lights.LightMask;
    }

    public static void MakeTailLight(ref Light light, float brightness, float angle) {
      light.type = LightType.Spot;
      light.color = Color.red;
      light.range = 6.0f;
      light.intensity = brightness;
      light.spotAngle = angle;
      light.innerSpotAngle = 10.0f;
      light.cookie = Lights.LightMask;
    }

    public static void MakeIllumination(ref Light l, Color color, float power, float range) {
      l.type = LightType.Point;
      l.color = color;
      l.range = range;
      l.intensity = power;
    }

    private bool GetLights(out Light left, out Light right) {
      left = Left.GetComponent<Light>();
      right = Right.GetComponent<Light>();
      return left != null && right != null;
    }

    private bool GetIllumination(out Light left, out Light right) {
      left = LeftIl.GetComponent<Light>();
      right = RightIl.GetComponent<Light>();
      return left != null && right != null;
    }

    private bool GetInners(out Light left, out Light right) {
      left = LeftInner.GetComponent<Light>();
      right = RightInner.GetComponent<Light>();
      return left != null && right != null;
    }

    public void Serialize(BinaryWriter writer) {
      writer.Write(EnabledLeft);
      writer.Write(EnabledRight);
      writer.Write(KnUtils.EncodeColor(Color));
      writer.Write(Pitch);
      writer.Write(Brightness);
      writer.Write(Angle);
      writer.Write(IlIntensity);
      writer.Write(Range);
      KnUtils.WriteVec3(writer, Offset);
    }

    public void Deserialize(BinaryReader reader) {
      enabledLeft_ = reader.ReadBoolean();
      enabledRight_ = reader.ReadBoolean();
      color_ = KnUtils.DecodeColor(reader.ReadInt32());
      pitch_ = reader.ReadSingle();
      brightness_ = reader.ReadSingle();
      angle_ = reader.ReadSingle();
      ilIntensity_ = reader.ReadSingle();
      range_ = reader.ReadSingle();
      Offset = KnUtils.ReadVec3(reader);
    }

    public void ApplyQuality(Quality quality) {
      Dispose();
      Attach(quality, own_, Car, inverted_, debugColor_);
    }
  }
}