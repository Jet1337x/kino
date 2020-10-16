using System.IO;
using KN_Core;
using UnityEngine;

namespace KN_Lights {
  public class LightsSet {
    private const float HeadLightsIllumination = 13.0f;
    private const float TaliLightsIllumination = 10.0f;

    private static readonly int BaseColorMap = Shader.PropertyToID("_BaseColorMap");

    public KnCar Car { get; private set; }

    public GameObject Left { get; private set; }
    public GameObject Right { get; private set; }

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
          if (LeftIl != null) {
            LeftIl.SetActive(enabled_);
          }
          if (RightIl != null) {
            RightIl.SetActive(enabled_);
          }
        }
      }
    }

    private bool illumination_;
    public bool Illumination {
      get => illumination_;
      set {
        illumination_ = value;
        if (LeftIl != null) {
          LeftIl.SetActive(enabledLeft_ && illumination_);
        }
        if (RightIl != null) {
          RightIl.SetActive(enabledRight_ && illumination_);
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
        if (LeftIl != null) {
          LeftIl.SetActive(enabledLeft_ && illumination_);
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
        if (RightIl != null) {
          RightIl.SetActive(enabledRight_ && illumination_);
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
        if (GetIllumination(out var il, out var ir)) {
          il.color = color_;
          ir.color = color_;
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
        var rot = Car.Transform.rotation;
        if (Left != null) {
          Left.transform.rotation = rot * Quaternion.AngleAxis(pitch_, Vector3.right);
        }
        if (Right != null) {
          Right.transform.rotation = rot * Quaternion.AngleAxis(pitch_, Vector3.right);
        }

        if (DebugLeft != null) {
          DebugLeft.transform.rotation = rot * Quaternion.AngleAxis(90.0f + pitch_, Vector3.right);
        }
        if (DebugRight != null) {
          DebugRight.transform.rotation = rot * Quaternion.AngleAxis(90.0f + pitch_, Vector3.right);
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
        if (LeftIl != null) {
          LeftIl.transform.localPosition = offsetLeft_;
        }
        if (RightIl != null) {
          RightIl.transform.localPosition = offsetRight_;
        }
      }
    }

    public LightsSet(Color color, float pitch, float brightness, float angle, Vector3 offset, bool enabledLeft, bool enabledRight) {
      illumination_ = true;
      enabledLeft_ = enabledLeft;
      enabledRight_ = enabledRight;
      enabled_ = enabledLeft_ || enabledRight_;
      color_ = color;
      pitch_ = pitch;
      brightness_ = brightness;
      angle_ = angle;
      Offset = offset;
    }

    public LightsSet(BinaryReader reader) {
      Deserialize(reader);

      illumination_ = true;
      enabled_ = enabledLeft_ || enabledRight_;
    }

    public void Dispose() {
      if (Left != null) {
        Object.Destroy(Left);
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
      if (RightIl != null) {
        Object.Destroy(RightIl);
      }
      if (DebugRight != null) {
        Object.Destroy(DebugRight);
      }
    }

    public LightsSet Copy() {
      var set = new LightsSet(color_, pitch_, brightness_, angle_, Offset, enabledLeft_, enabledRight_);
      return set;
    }

    public void Attach(KnCar car, bool tailLights, Color debug) {
      Car = car;

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

      Initialize(debug);

      Left.transform.parent = car.Transform;
      Left.transform.position = position;
      Left.transform.rotation = lightsRotation;
      Left.transform.localPosition += offsetLeft_;
      LeftIl.transform.parent = car.Transform;
      LeftIl.transform.position = Left.transform.position;
      DebugLeft.transform.parent = Left.transform;
      DebugLeft.transform.position = Left.transform.position;
      DebugLeft.transform.rotation = debugRotation;
      DebugLeft.transform.localScale = capsuleScale;

      Right.transform.parent = car.Transform;
      Right.transform.position = position;
      Right.transform.rotation = lightsRotation;
      Right.transform.localPosition += offsetRight_;
      RightIl.transform.parent = car.Transform;
      RightIl.transform.position = Right.transform.position;
      DebugRight.transform.parent = Right.transform;
      DebugRight.transform.position = Right.transform.position;
      DebugRight.transform.rotation = debugRotation;
      DebugRight.transform.localScale = capsuleScale;

      MakeLights(tailLights);
    }

    private void Initialize(Color debug) {
      Dispose();

      Left = new GameObject();
      Left.AddComponent<Light>();
      Left.SetActive(enabledLeft_);
      LeftIl = new GameObject();
      LeftIl.AddComponent<Light>();
      LeftIl.SetActive(enabledLeft_);

      Right = new GameObject();
      Right.AddComponent<Light>();
      Right.SetActive(enabledRight_);
      RightIl = new GameObject();
      RightIl.AddComponent<Light>();
      RightIl.SetActive(enabledRight_);

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

    private void MakeLights(bool tailLights) {
      var ll = Left.GetComponent<Light>();
      var lr = Right.GetComponent<Light>();
      if (!tailLights) {
        MakeHeadLight(ref ll, color_, brightness_, angle_);
        MakeHeadLight(ref lr, color_, brightness_, angle_);
      }
      else {
        MakeTailLight(ref ll, brightness_, angle_);
        MakeTailLight(ref lr, brightness_, angle_);
      }

      var il = LeftIl.GetComponent<Light>();
      var ir = RightIl.GetComponent<Light>();
      MakeIllumination(ref il, color_, tailLights ? TaliLightsIllumination : HeadLightsIllumination);
      MakeIllumination(ref ir, color_, tailLights ? TaliLightsIllumination : HeadLightsIllumination);
    }

    public void SetIntensity(float val) {
      if (GetLights(out var l, out var r)) {
        l.intensity = val;
        r.intensity = val;
      }
    }

    public static void MakeHeadLight(ref Light light, Color color, float brightness, float angle) {
      light.type = LightType.Spot;
      light.color = color;
      light.range = 50.0f;
      light.intensity = brightness;
      light.spotAngle = angle;
      light.innerSpotAngle = 50.0f;
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

    public static void MakeIllumination(ref Light l, Color color, float power) {
      l.type = LightType.Point;
      l.color = color;
      l.range = 0.3f;
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

    public void Serialize(BinaryWriter writer) {
      writer.Write(EnabledLeft);
      writer.Write(EnabledRight);
      writer.Write(KnUtils.EncodeColor(Color));
      writer.Write(Pitch);
      writer.Write(Brightness);
      writer.Write(Angle);
      KnUtils.WriteVec3(writer, Offset);
    }

    public void Deserialize(BinaryReader reader) {
      enabledLeft_ = reader.ReadBoolean();
      enabledRight_ = reader.ReadBoolean();
      color_ = KnUtils.DecodeColor(reader.ReadInt32());
      pitch_ = reader.ReadSingle();
      brightness_ = reader.ReadSingle();
      angle_ = reader.ReadSingle();
      Offset = KnUtils.ReadVec3(reader);
    }
  }
}