using System.IO;
using KN_Core;
using UnityEngine;

namespace KN_Lights {
  public class CarLights {
    public GameObject HeadLightLeft { get; private set; }
    public GameObject HeadLightRight { get; private set; }

    public GameObject TailLightLeft { get; private set; }
    public GameObject TailLightRight { get; private set; }

    public TFCar Car { get; private set; }

    public int CarId { get; private set; }
    public bool IsNetworkCar { get; private set; }
    public string UserName { get; private set; }

    private float pitch_;
    public float Pitch {
      get => pitch_;
      set {
        pitch_ = value;
        if (Car == null || Car.Base == null) {
          return;
        }
        var rot = Car.Transform.rotation;
        HeadLightLeft.transform.rotation = rot * Quaternion.AngleAxis(pitch_, Vector3.right);
        HeadLightRight.transform.rotation = rot * Quaternion.AngleAxis(pitch_, Vector3.right);
      }
    }

    private float pitchTail_;
    public float PitchTail {
      get => pitchTail_;
      set {
        pitchTail_ = value;
        if (Car == null || Car.Base == null) {
          return;
        }
        var rot = Car.Transform.rotation;
        HeadLightLeft.transform.rotation = rot * Quaternion.AngleAxis(180.0f, Vector3.up) * Quaternion.AngleAxis(pitchTail_, -Vector3.right);
        HeadLightRight.transform.rotation = rot * Quaternion.AngleAxis(180.0f, Vector3.up) * Quaternion.AngleAxis(pitchTail_, -Vector3.right);
      }
    }

    private float hlBrightness_;
    public float HeadLightBrightness {
      get => hlBrightness_;
      set {
        hlBrightness_ = value;
        if (GetHeadLights(out var l, out var r)) {
          l.intensity = hlBrightness_;
          r.intensity = hlBrightness_;
        }
      }
    }

    private float hlAngle_;
    public float HeadLightAngle {
      get => hlAngle_;
      set {
        hlAngle_ = value;
        if (GetHeadLights(out var l, out var r)) {
          l.spotAngle = hlAngle_;
          r.spotAngle = hlAngle_;
        }
      }
    }

    private float tlBrightness_;
    public float TailLightBrightness {
      get => tlBrightness_;
      set {
        tlBrightness_ = value;
        if (GetTailLights(out var l, out var r)) {
          l.intensity = tlBrightness_;
          r.intensity = tlBrightness_;
        }
      }
    }

    private float tlAngle_;
    public float TailLightAngle {
      get => tlAngle_;
      set {
        tlAngle_ = value;
        if (GetTailLights(out var l, out var r)) {
          l.spotAngle = tlAngle_;
          r.spotAngle = tlAngle_;
        }
      }
    }

    private bool hlLEnabled_;
    public bool IsHeadLightLeftEnabled {
      get => hlLEnabled_;
      set {
        hlLEnabled_ = value;
        if (HeadLightLeft != null) {
          HeadLightLeft.SetActive(hlLEnabled_);
        }
      }
    }

    private Vector3 hlLOffset_;
    public Vector3 HeadlightOffsetLeft {
      get => hlLOffset_;
      set {
        hlLOffset_ = value;
        if (HeadLightLeft != null) {
          HeadLightLeft.transform.localPosition = hlLOffset_;
        }
      }
    }

    private bool hlREnabled_;
    public bool IsHeadLightRightEnabled {
      get => hlREnabled_;
      set {
        hlREnabled_ = value;
        if (HeadLightRight != null) {
          HeadLightRight.SetActive(hlREnabled_);
        }
      }
    }

    private Vector3 hlROffset_;
    public Vector3 HeadlightOffsetRight {
      get => hlROffset_;
      set {
        hlROffset_ = value;
        if (HeadLightRight != null) {
          HeadLightRight.transform.localPosition = hlROffset_;
        }
      }
    }

    private bool tlLEnabled_;
    public bool IsTailLightLeftEnabled {
      get => tlLEnabled_;
      set {
        tlLEnabled_ = value;
        if (TailLightLeft != null) {
          TailLightLeft.SetActive(tlLEnabled_);
        }
      }
    }

    private Vector3 tlLOffset_;
    public Vector3 TaillightOffsetLeft {
      get => tlLOffset_;
      set {
        tlLOffset_ = value;
        if (TailLightLeft != null) {
          TailLightLeft.transform.localPosition = tlLOffset_;
        }
      }
    }

    private bool tlREnabled_;
    public bool IsTailLightRightEnabled {
      get => tlREnabled_;
      set {
        tlREnabled_ = value;
        if (TailLightRight != null) {
          TailLightRight.SetActive(tlREnabled_);
        }
      }
    }

    private Vector3 tlROffset_;
    public Vector3 TaillightOffsetRight {
      get => tlROffset_;
      set {
        tlROffset_ = value;
        if (TailLightRight != null) {
          TailLightRight.transform.localPosition = tlROffset_;
        }
      }
    }

    public CarLights() {
      Initialize();
    }

    public void Attach(TFCar car, string userName) {
      Car = car;
      CarId = car.Id;
      IsNetworkCar = car.IsNetworkCar;
      UserName = userName;

      var position = car.Transform.position;
      var rotation = car.Transform.rotation;

      var headRotation = rotation * Quaternion.AngleAxis(Pitch, Vector3.right);
      var tailRotation = rotation * Quaternion.AngleAxis(180.0f, Vector3.up) * Quaternion.AngleAxis(PitchTail, -Vector3.right);

      Initialize();

      HeadLightLeft.transform.parent = car.Transform;
      HeadLightLeft.transform.position = position;
      HeadLightLeft.transform.rotation = headRotation;
      HeadLightLeft.transform.localPosition += HeadlightOffsetLeft;

      HeadLightRight.transform.parent = car.Transform;
      HeadLightRight.transform.position = position;
      HeadLightRight.transform.rotation = headRotation;
      HeadLightRight.transform.localPosition += HeadlightOffsetRight;

      TailLightLeft.transform.parent = car.Transform;
      TailLightLeft.transform.position = position;
      TailLightLeft.transform.rotation = tailRotation;
      TailLightLeft.transform.localPosition += TaillightOffsetLeft;

      TailLightRight.transform.parent = car.Transform;
      TailLightRight.transform.position = position;
      TailLightRight.transform.rotation = tailRotation;
      TailLightRight.transform.localPosition += TaillightOffsetRight;

      MakeLights();
    }

    private void Initialize() {
      if (HeadLightLeft != null) {
        Object.Destroy(HeadLightLeft);
      }
      HeadLightLeft = new GameObject();
      HeadLightLeft.AddComponent<Light>();
      HeadLightLeft.SetActive(IsHeadLightLeftEnabled);

      if (HeadLightRight != null) {
        Object.Destroy(HeadLightRight);
      }
      HeadLightRight = new GameObject();
      HeadLightRight.AddComponent<Light>();
      HeadLightRight.SetActive(IsHeadLightRightEnabled);

      if (TailLightLeft != null) {
        Object.Destroy(TailLightLeft);
      }
      TailLightLeft = new GameObject();
      TailLightLeft.AddComponent<Light>();
      TailLightLeft.SetActive(IsTailLightLeftEnabled);

      if (TailLightRight != null) {
        Object.Destroy(TailLightRight);
      }
      TailLightRight = new GameObject();
      TailLightRight.AddComponent<Light>();
      TailLightRight.SetActive(IsTailLightRightEnabled);
    }

    private void MakeLights() {
      var hl = HeadLightLeft.GetComponent<Light>();
      var hr = HeadLightRight.GetComponent<Light>();
      MakeHeadLight(ref hl);
      MakeHeadLight(ref hr);

      var tl = TailLightLeft.GetComponent<Light>();
      var tr = TailLightRight.GetComponent<Light>();
      MakeTailLight(ref tl);
      MakeTailLight(ref tr);
    }

    private void MakeHeadLight(ref Light light) {
      light.type = LightType.Spot;
      light.color = Color.white;
      light.range = 150.0f;
      light.intensity = HeadLightBrightness;
      light.spotAngle = HeadLightAngle;
      light.innerSpotAngle = 50.0f;
      light.cookie = Lights.LightMask;
    }

    private void MakeTailLight(ref Light light) {
      light.type = LightType.Spot;
      light.color = Color.red;
      light.range = 150.0f;
      light.intensity = TailLightBrightness;
      light.spotAngle = TailLightAngle;
      light.innerSpotAngle = 50.0f;
      light.cookie = Lights.LightMask;
    }

    private bool GetHeadLights(out Light left, out Light right) {
      left = HeadLightLeft.GetComponent<Light>();
      right = HeadLightRight.GetComponent<Light>();
      return left != null && right != null;
    }

    private bool GetTailLights(out Light left, out Light right) {
      left = TailLightLeft.GetComponent<Light>();
      right = TailLightRight.GetComponent<Light>();
      return left != null && right != null;
    }

    public void Serialize(BinaryWriter writer) {
      writer.Write(CarId);
      writer.Write(IsNetworkCar);
      writer.Write(UserName);
      writer.Write(pitch_);
      writer.Write(pitchTail_);
      writer.Write(hlBrightness_);
      writer.Write(hlAngle_);
      writer.Write(tlBrightness_);
      writer.Write(tlAngle_);

      writer.Write(hlLEnabled_);
      WriteVec3(writer, hlLOffset_);

      writer.Write(hlREnabled_);
      WriteVec3(writer, hlROffset_);

      writer.Write(tlLEnabled_);
      WriteVec3(writer, tlLOffset_);

      writer.Write(tlREnabled_);
      WriteVec3(writer, tlROffset_);
    }

    public void Deserialize(BinaryReader reader) {
      CarId = reader.ReadInt32();
      IsNetworkCar = reader.ReadBoolean();
      UserName = reader.ReadString();
      pitch_ = reader.ReadSingle();
      pitchTail_ = reader.ReadSingle();
      hlBrightness_ = reader.ReadSingle();
      hlAngle_ = reader.ReadSingle();
      tlBrightness_ = reader.ReadSingle();
      tlAngle_ = reader.ReadSingle();

      hlLEnabled_ = reader.ReadBoolean();
      hlLOffset_ = ReadVec3(reader);

      hlREnabled_ = reader.ReadBoolean();
      hlROffset_ = ReadVec3(reader);

      tlLEnabled_ = reader.ReadBoolean();
      tlLOffset_ = ReadVec3(reader);

      tlREnabled_ = reader.ReadBoolean();
      tlROffset_ = ReadVec3(reader);
    }

    private static void WriteVec3(BinaryWriter writer, Vector3 vec) {
      writer.Write(vec.x);
      writer.Write(vec.y);
      writer.Write(vec.z);
    }

    private static Vector3 ReadVec3(BinaryReader reader) {
      return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
    }
  }
}