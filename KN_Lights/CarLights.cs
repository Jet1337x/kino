using System.IO;
using CarModelSystem;
using CarX;
using KN_Core;
using UnityEngine;

namespace KN_Lights {
  public class CarLights {
    public const float BrakePower = 2.5f;

    public GameObject HeadLightLeft { get; private set; }
    public GameObject HeadLightRight { get; private set; }
    public GameObject HeadLightLeftIl { get; private set; }
    public GameObject HeadLightRightIl { get; private set; }

    public GameObject TailLightLeft { get; private set; }
    public GameObject TailLightRight { get; private set; }

    private GameObject hlLCapsule_;
    private GameObject hlRCapsule_;
    private GameObject tlLCapsule_;
    private GameObject tlRCapsule_;
    private bool dbObjects_;
    public bool IsDebugObjectsEnabled {
      get => dbObjects_;
      set {
        dbObjects_ = value;
        if (hlLCapsule_ != null) {
          hlLCapsule_.SetActive(dbObjects_);
        }
        if (hlRCapsule_ != null) {
          hlRCapsule_.SetActive(dbObjects_);
        }
        if (tlLCapsule_ != null) {
          tlLCapsule_.SetActive(dbObjects_);
        }
        if (tlRCapsule_ != null) {
          tlRCapsule_.SetActive(dbObjects_);
        }
      }
    }

    public TFCar Car { get; private set; }

    public int CarId { get; private set; }
    public bool IsNetworkCar { get; private set; }
    public string UserName { get; private set; }

    private bool lightsEnabledIl_;
    public bool IsLightsEnabledIl {
      get => lightsEnabledIl_;
      set {
        lightsEnabledIl_ = value;
        if (HeadLightLeftIl != null) {
          HeadLightLeftIl.SetActive(IsHeadLightLeftEnabled && value);
        }
        if (HeadLightRightIl != null) {
          HeadLightRightIl.SetActive(IsHeadLightRightEnabled && value);
        }
      }
    }

    private bool lightEnabled_ = true;
    public bool IsLightsEnabled {
      get => lightEnabled_;
      set {
        lightEnabled_ = value;
        if (lightEnabled_) {
          HeadLightLeft.SetActive(IsHeadLightLeftEnabled);
          HeadLightRight.SetActive(IsHeadLightRightEnabled);
          HeadLightLeftIl.SetActive(IsLightsEnabledIl);
          HeadLightRightIl.SetActive(IsLightsEnabledIl);
          TailLightLeft.SetActive(IsTailLightLeftEnabled);
          TailLightRight.SetActive(IsTailLightRightEnabled);
        }
        else {
          HeadLightLeft.SetActive(false);
          HeadLightRight.SetActive(false);
          HeadLightLeftIl.SetActive(false);
          HeadLightRightIl.SetActive(false);
          TailLightLeft.SetActive(false);
          TailLightRight.SetActive(false);
        }
      }
    }

    private Color hlColor_;
    public Color HeadLightsColor {
      get => hlColor_;
      set {
        hlColor_ = value;
        if (GetHeadLights(out var l, out var r)) {
          l.color = hlColor_;
          r.color = hlColor_;
        }
        if (IsLightsEnabledIl) {
          var il = HeadLightLeftIl.GetComponent<Light>();
          var ir = HeadLightRightIl.GetComponent<Light>();
          if (il != null) {
            il.color = hlColor_;
          }
          if (ir != null) {
            ir.color = hlColor_;
          }
        }
      }
    }

    private float pitch_;
    public float Pitch {
      get => pitch_;
      set {
        pitch_ = value;
        if (TFCar.IsNull(Car)) {
          return;
        }
        var rot = Car.Transform.rotation;
        HeadLightLeft.transform.rotation = rot * Quaternion.AngleAxis(pitch_, Vector3.right);
        HeadLightRight.transform.rotation = rot * Quaternion.AngleAxis(pitch_, Vector3.right);

        if (hlLCapsule_ != null && hlRCapsule_ != null) {
          hlLCapsule_.transform.rotation = rot * Quaternion.AngleAxis(90.0f + pitch_, Vector3.right);
          hlRCapsule_.transform.rotation = rot * Quaternion.AngleAxis(90.0f + pitch_, Vector3.right);
        }
      }
    }

    private float pitchTail_;
    public float PitchTail {
      get => pitchTail_;
      set {
        pitchTail_ = value;
        if (TFCar.IsNull(Car)) {
          return;
        }
        var rot = Car.Transform.rotation;
        TailLightLeft.transform.rotation = rot * Quaternion.AngleAxis(180.0f, Vector3.up) * Quaternion.AngleAxis(pitchTail_, -Vector3.right);
        TailLightRight.transform.rotation = rot * Quaternion.AngleAxis(180.0f, Vector3.up) * Quaternion.AngleAxis(pitchTail_, -Vector3.right);

        if (tlLCapsule_ != null && tlRCapsule_ != null) {
          tlLCapsule_.transform.rotation = rot * Quaternion.AngleAxis(180.0f, Vector3.up) * Quaternion.AngleAxis(90.0f + pitchTail_, -Vector3.right);
          tlRCapsule_.transform.rotation = rot * Quaternion.AngleAxis(180.0f, Vector3.up) * Quaternion.AngleAxis(90.0f + pitchTail_, -Vector3.right);
        }
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
        if (HeadLightLeftIl != null && IsLightsEnabledIl) {
          HeadLightLeftIl.SetActive(value);
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
        if (HeadLightRightIl != null && IsLightsEnabledIl) {
          HeadLightRightIl.SetActive(value);
        }
      }
    }

    private Vector3 hlOffset_;
    private Vector3 hlOffsetL_;
    public Vector3 HeadlightOffset {
      get => hlOffset_;
      set {
        hlOffset_ = value;
        hlOffsetL_ = value;
        hlOffsetL_.x = -hlOffsetL_.x;
        if (HeadLightLeft != null) {
          HeadLightLeft.transform.localPosition = hlOffsetL_;
          HeadLightRight.transform.localPosition = hlOffset_;
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

    private Vector3 tlOffset_;
    private Vector3 tlOffsetL_;
    public Vector3 TailLightOffset {
      get => tlOffset_;
      set {
        tlOffset_ = value;
        tlOffsetL_ = value;
        tlOffsetL_.x = -tlOffsetL_.x;
        if (TailLightLeft != null) {
          TailLightLeft.transform.localPosition = tlOffsetL_;
          TailLightRight.transform.localPosition = tlOffset_;
        }
      }
    }

    private CARXCar cxCar_;

    private static readonly int BaseColorMap = Shader.PropertyToID("_BaseColorMap");

    public CarLights() {
      Initialize();
    }

    public void Dispose() {
      Object.Destroy(HeadLightLeft);
      Object.Destroy(HeadLightRight);
      Object.Destroy(HeadLightLeftIl);
      Object.Destroy(HeadLightRightIl);
      Object.Destroy(TailLightLeft);
      Object.Destroy(TailLightRight);

      if (!TFCar.IsNull(Car)) {
        if (Singletone<Simulator>.instance) {
          Singletone<Simulator>.instance.OnUpdateWheelsEvent -= CarUpdate;
        }
      }
    }

    public CarLights Copy() {
      var lights = new CarLights {
        CarId = CarId,
        IsNetworkCar = IsNetworkCar,
        UserName = UserName,
        hlColor_ = hlColor_,
        pitch_ = pitch_,
        pitchTail_ = pitchTail_,
        hlBrightness_ = hlBrightness_,
        hlAngle_ = hlAngle_,
        tlBrightness_ = tlBrightness_,
        tlAngle_ = tlAngle_,
        hlLEnabled_ = hlLEnabled_,
        hlREnabled_ = hlREnabled_,
        HeadlightOffset = HeadlightOffset,
        tlLEnabled_ = tlLEnabled_,
        tlREnabled_ = tlREnabled_,
        TailLightOffset = TailLightOffset
      };
      return lights;
    }

    public void Attach(TFCar car) {
      Car = car;
      CarId = car.Id;
      IsNetworkCar = car.IsNetworkCar;
      UserName = car.Name;
      lightsEnabledIl_ = true;

      var position = car.Transform.position;
      var rotation = car.Transform.rotation;

      var headRotation = rotation * Quaternion.AngleAxis(Pitch, Vector3.right);
      var tailRotation = rotation * Quaternion.AngleAxis(180.0f, Vector3.up) * Quaternion.AngleAxis(PitchTail, -Vector3.right);
      var headRotationD = rotation * Quaternion.AngleAxis(90.0f + Pitch, Vector3.right);
      var tailRotationD = rotation * Quaternion.AngleAxis(180.0f, Vector3.up) * Quaternion.AngleAxis(90.0f + PitchTail, -Vector3.right);
      var capsuleScale = new Vector3(0.1f, 0.15f, 0.1f);

      Initialize();

      HeadLightLeft.transform.parent = car.Transform;
      HeadLightLeft.transform.position = position;
      HeadLightLeft.transform.rotation = headRotation;
      HeadLightLeft.transform.localPosition += hlOffsetL_;
      HeadLightLeftIl.transform.parent = HeadLightLeft.transform;
      HeadLightLeftIl.transform.position = HeadLightLeft.transform.position;
      hlLCapsule_.transform.parent = HeadLightLeft.transform;
      hlLCapsule_.transform.position = HeadLightLeft.transform.position;
      hlLCapsule_.transform.rotation = headRotationD;
      hlLCapsule_.transform.localScale = capsuleScale;

      HeadLightRight.transform.parent = car.Transform;
      HeadLightRight.transform.position = position;
      HeadLightRight.transform.rotation = headRotation;
      HeadLightRight.transform.localPosition += hlOffset_;
      HeadLightRightIl.transform.parent = HeadLightRight.transform;
      HeadLightRightIl.transform.position = HeadLightRight.transform.position;
      hlRCapsule_.transform.parent = HeadLightRight.transform;
      hlRCapsule_.transform.position = HeadLightRight.transform.position;
      hlRCapsule_.transform.rotation = headRotationD;
      hlRCapsule_.transform.localScale = capsuleScale;

      TailLightLeft.transform.parent = car.Transform;
      TailLightLeft.transform.position = position;
      TailLightLeft.transform.rotation = tailRotation;
      TailLightLeft.transform.localPosition += tlOffsetL_;
      tlLCapsule_.transform.parent = TailLightLeft.transform;
      tlLCapsule_.transform.position = TailLightLeft.transform.position;
      tlLCapsule_.transform.rotation = tailRotationD;
      tlLCapsule_.transform.localScale = capsuleScale;

      TailLightRight.transform.parent = car.Transform;
      TailLightRight.transform.position = position;
      TailLightRight.transform.rotation = tailRotation;
      TailLightRight.transform.localPosition += tlOffset_;
      tlRCapsule_.transform.parent = TailLightRight.transform;
      tlRCapsule_.transform.position = TailLightRight.transform.position;
      tlRCapsule_.transform.rotation = tailRotationD;
      tlRCapsule_.transform.localScale = capsuleScale;

      MakeLights();

      cxCar_ = Car.Base.GetComponent<CARXCar>();
      if (Singletone<Simulator>.instance) {
        Singletone<Simulator>.instance.OnUpdateWheelsEvent += CarUpdate;
      }
    }

    public void LateUpdate() {
      if (!TFCar.IsNull(Car) && cxCar_ != null) {
        float bb = cxCar_.brake > 0.2f ? tlBrightness_ * BrakePower : tlBrightness_;
        if (GetTailLights(out var l, out var r)) {
          l.intensity = bb;
          r.intensity = bb;
        }
      }
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

      if (HeadLightLeftIl != null) {
        Object.Destroy(HeadLightLeftIl);
      }
      HeadLightLeftIl = new GameObject();
      HeadLightLeftIl.AddComponent<Light>();
      HeadLightLeftIl.SetActive(IsLightsEnabledIl);

      if (HeadLightRightIl != null) {
        Object.Destroy(HeadLightRightIl);
      }
      HeadLightRightIl = new GameObject();
      HeadLightRightIl.AddComponent<Light>();
      HeadLightRightIl.SetActive(IsLightsEnabledIl);

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

      InitializeDebug();
    }

    private void InitializeDebug() {
      var matWhite = new UnityEngine.Material(Shader.Find("HDRP/Lit"));
      matWhite.SetTexture(BaseColorMap, Core.CreateTexture(Color.white));

      var matRed = new UnityEngine.Material(Shader.Find("HDRP/Lit"));
      matRed.SetTexture(BaseColorMap, Core.CreateTexture(Color.red));

      if (hlLCapsule_ != null) {
        Object.Destroy(hlLCapsule_);
      }
      hlLCapsule_ = GameObject.CreatePrimitive(PrimitiveType.Capsule);
      hlLCapsule_.GetComponent<MeshRenderer>().material = matWhite;
      hlLCapsule_.SetActive(IsDebugObjectsEnabled);

      if (hlRCapsule_ != null) {
        Object.Destroy(hlRCapsule_);
      }
      hlRCapsule_ = GameObject.CreatePrimitive(PrimitiveType.Capsule);
      hlRCapsule_.GetComponent<MeshRenderer>().material = matWhite;
      hlRCapsule_.SetActive(IsDebugObjectsEnabled);

      if (tlLCapsule_ != null) {
        Object.Destroy(tlLCapsule_);
      }
      tlLCapsule_ = GameObject.CreatePrimitive(PrimitiveType.Capsule);
      tlLCapsule_.GetComponent<MeshRenderer>().material = matRed;
      tlLCapsule_.SetActive(IsDebugObjectsEnabled);

      if (tlRCapsule_ != null) {
        Object.Destroy(tlRCapsule_);
      }
      tlRCapsule_ = GameObject.CreatePrimitive(PrimitiveType.Capsule);
      tlRCapsule_.GetComponent<MeshRenderer>().material = matRed;
      tlRCapsule_.SetActive(IsDebugObjectsEnabled);
    }

    private void MakeLights() {
      var hl = HeadLightLeft.GetComponent<Light>();
      var hr = HeadLightRight.GetComponent<Light>();
      MakeHeadLight(ref hl);
      MakeHeadLight(ref hr);

      var il = HeadLightLeftIl.GetComponent<Light>();
      var ir = HeadLightRightIl.GetComponent<Light>();
      MakeIllumination(ref il, ref ir);

      var tl = TailLightLeft.GetComponent<Light>();
      var tr = TailLightRight.GetComponent<Light>();
      MakeTailLight(ref tl);
      MakeTailLight(ref tr);
    }

    private void MakeIllumination(ref Light l, ref Light r) {
      l.type = LightType.Point;
      l.color = hlColor_;
      l.range = 1.5f;
      l.intensity = 5.0f;

      r.type = LightType.Point;
      r.color = hlColor_;
      r.range = 1.5f;
      r.intensity = 5.0f;
    }

    private void MakeHeadLight(ref Light light) {
      light.type = LightType.Spot;
      light.color = hlColor_;
      light.range = 50.0f;
      light.intensity = HeadLightBrightness;
      light.spotAngle = HeadLightAngle;
      light.innerSpotAngle = 50.0f;
      light.cookie = Lights.LightMask;
    }

    private void MakeTailLight(ref Light light) {
      light.type = LightType.Spot;
      light.color = Color.red;
      light.range = 6.0f;
      light.intensity = TailLightBrightness;
      light.spotAngle = TailLightAngle;
      light.innerSpotAngle = 10.0f;
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

    private void CarUpdate() {
      if (!TFCar.IsNull(Car)) {
        //todo(trbflxr): maybe it should be optional?
        bool enabled = IsLightsEnabled && (IsTailLightLeftEnabled || IsTailLightRightEnabled);
        Car.Base.carModel.SetLightsState(enabled, CarLightGroup.Brake);
      }
    }

    public void Serialize(BinaryWriter writer) {
      writer.Write(CarId);
      writer.Write(IsNetworkCar);
      writer.Write(UserName);
      writer.Write(Core.EncodeColor(hlColor_));
      writer.Write(pitch_);
      writer.Write(pitchTail_);
      writer.Write(hlBrightness_);
      writer.Write(hlAngle_);
      writer.Write(tlBrightness_);
      writer.Write(tlAngle_);
      writer.Write(lightsEnabledIl_);

      writer.Write(hlLEnabled_);
      writer.Write(hlREnabled_);
      WriteVec3(writer, HeadlightOffset);

      writer.Write(tlLEnabled_);
      writer.Write(tlREnabled_);
      WriteVec3(writer, TailLightOffset);
    }

    public void Deserialize(BinaryReader reader, int version) {
      CarId = reader.ReadInt32();
      IsNetworkCar = reader.ReadBoolean();
      UserName = reader.ReadString();
      hlColor_ = Core.DecodeColor(reader.ReadInt32());
      pitch_ = reader.ReadSingle();
      pitchTail_ = reader.ReadSingle();
      hlBrightness_ = reader.ReadSingle();
      hlAngle_ = reader.ReadSingle();
      tlBrightness_ = reader.ReadSingle();
      tlAngle_ = reader.ReadSingle();
      lightsEnabledIl_ = version != Config.Version || reader.ReadBoolean();

      hlLEnabled_ = reader.ReadBoolean();
      hlREnabled_ = reader.ReadBoolean();
      HeadlightOffset = ReadVec3(reader);

      tlLEnabled_ = reader.ReadBoolean();
      tlREnabled_ = reader.ReadBoolean();
      TailLightOffset = ReadVec3(reader);
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