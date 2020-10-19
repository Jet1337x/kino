using System.IO;
using KN_Core;
using SyncMultiplayer;
using UnityEngine;

namespace KN_Lights {
  public class HazardLights : ISerializable {
    public const float DefaultBrightness = 10.0f;
    public const float DefaultRange = 1.0f;
    private const float HazardToggleTime = 0.85f;

    private static readonly int BaseColorMap = Shader.PropertyToID("_BaseColorMap");

    public KnCar Car { get; private set; }

    public int Id { get; private set; }

    public GameObject Fl { get; private set; }
    public GameObject Fr { get; private set; }
    public GameObject Rl { get; private set; }
    public GameObject Rr { get; private set; }

    public GameObject DebugFl { get; private set; }
    public GameObject DebugFr { get; private set; }
    public GameObject DebugRl { get; private set; }
    public GameObject DebugRr { get; private set; }

    private bool enabled_;
    public bool Enabled {
      get => enabled_;
      set {
        enabled_ = value;
        if (Fl != null) {
          Fl.SetActive(enabled_);
        }
        if (Fr != null) {
          Fr.SetActive(enabled_);
        }
        if (Rl != null) {
          Rl.SetActive(enabled_);
        }
        if (Rr != null) {
          Rr.SetActive(enabled_);
        }
      }
    }

    private bool discarded_;
    public bool Discarded {
      get => discarded_;
      set {
        if (discarded_ == value) {
          return;
        }
        discarded_ = value;
        if (discarded_) {
          Enabled = false;
        }
      }
    }

    private bool hazard_;
    public bool Hazard {
      get => hazard_;
      set {
        hazard_ = value;
        Reset(hazard_);
      }
    }

    private bool debug_;
    public bool Debug {
      get => debug_;
      set {
        debug_ = value;
        if (DebugFl != null) {
          DebugFl.SetActive(debug_);
        }
        if (DebugFr != null) {
          DebugFr.SetActive(debug_);
        }
        if (DebugRl != null) {
          DebugRl.SetActive(debug_);
        }
        if (DebugRr != null) {
          DebugRr.SetActive(debug_);
        }
      }
    }

    private Color color_;
    public Color Color {
      get => color_;
      set {
        color_ = value;
        if (GetLightsFront(out var fl, out var fr)) {
          fl.color = color_;
          fr.color = color_;
        }
        if (GetLightsRear(out var rl, out var rr)) {
          rl.color = color_;
          rr.color = color_;
        }
      }
    }

    private float rangeFront_;
    public float RangeFront {
      get => rangeFront_;
      set {
        rangeFront_ = value;
        if (GetLightsFront(out var fl, out var fr)) {
          fl.range = rangeFront_;
          fr.range = rangeFront_;
        }
      }
    }

    private float rangeRear_;
    public float RangeRear {
      get => rangeRear_;
      set {
        rangeRear_ = value;
        if (GetLightsRear(out var rl, out var rr)) {
          rl.range = rangeRear_;
          rr.range = rangeRear_;
        }
      }
    }

    private float brightnessFront_;
    public float BrightnessFront {
      get => brightnessFront_;
      set {
        brightnessFront_ = value;
        if (GetLightsFront(out var fl, out var fr)) {
          fl.intensity = brightnessFront_;
          fr.intensity = brightnessFront_;
        }
      }
    }

    private float brightnessRear_;
    public float BrightnessRear {
      get => brightnessRear_;
      set {
        brightnessRear_ = value;
        if (GetLightsRear(out var rl, out var rr)) {
          rl.intensity = brightnessRear_;
          rr.intensity = brightnessRear_;
        }
      }
    }

    private Vector3 offsetFr_;
    private Vector3 offsetFl_;
    public Vector3 OffsetFront {
      get => offsetFr_;
      set {
        offsetFr_ = value;
        offsetFl_ = value;
        offsetFl_.x = -offsetFl_.x;
        if (Fl != null) {
          Fl.transform.localPosition = offsetFl_;
        }
        if (Fr != null) {
          Fr.transform.localPosition = offsetFr_;
        }
      }
    }

    private Vector3 offsetRr_;
    private Vector3 offsetRl_;
    public Vector3 OffsetRear {
      get => offsetRr_;
      set {
        offsetRr_ = value;
        offsetRl_ = value;
        offsetRl_.x = -offsetRl_.x;
        if (Rl != null) {
          Rl.transform.localPosition = offsetRl_;
        }
        if (Rr != null) {
          Rr.transform.localPosition = offsetRr_;
        }
      }
    }

    private bool hazardMode_;
    private float hazardTimer_;

    public HazardLights() {
      InitDefault();
    }

    public HazardLights(int id) {
      Id = id;
      InitDefault();
    }

    private HazardLights(int id, Color color, float brightnessFront, float rangeFront, float brightnessRear, float rangeRear, Vector3 offsetFront, Vector3 offsetRear) {
      Id = id;
      enabled_ = false;
      hazardMode_ = false;
      color_ = color;
      brightnessFront_ = brightnessFront;
      rangeFront_ = rangeFront;
      brightnessRear_ = brightnessRear;
      rangeRear_ = rangeRear;
      OffsetFront = offsetFront;
      OffsetRear = offsetRear;
    }

    private void InitDefault() {
      enabled_ = false;
      hazardMode_ = false;
      color_ = new Color32(0xd7, 0x90, 0x00, 0xff);
      brightnessFront_ = DefaultBrightness;
      rangeFront_ = DefaultRange;
      brightnessRear_ = DefaultBrightness;
      rangeRear_ = DefaultRange;
      OffsetFront = new Vector3(0.6f, 0.6f, 2.2f);
      OffsetRear = new Vector3(0.6f, 0.6f, -2.2f);
    }

    public HazardLights Copy() {
      var light = new HazardLights(Id, color_, brightnessFront_, rangeFront_, brightnessRear_, rangeRear_, OffsetFront, OffsetRear);
      return light;
    }

    public void Dispose() {
      if (Fl != null) {
        Object.Destroy(Fl);
      }
      if (Fr != null) {
        Object.Destroy(Fr);
      }
      if (Rl != null) {
        Object.Destroy(Rl);
      }
      if (Rr != null) {
        Object.Destroy(Rr);
      }
      if (DebugFl != null) {
        Object.Destroy(DebugFl);
      }
      if (DebugFr != null) {
        Object.Destroy(DebugFr);
      }
      if (DebugRl != null) {
        Object.Destroy(DebugRl);
      }
      if (DebugRr != null) {
        Object.Destroy(DebugRr);
      }
    }

    public void Attach(KnCar car) {
      enabled_ = false;
      Car = car;

      var position = car.Transform.position;

      var capsuleScale = new Vector3(0.1f, 0.1f, 0.1f);

      Initialize();

      Fl.transform.parent = car.Transform;
      Fl.transform.position = position;
      Fl.transform.localPosition += offsetFl_;
      DebugFl.transform.parent = Fl.transform;
      DebugFl.transform.position = Fl.transform.position;
      DebugFl.transform.localScale = capsuleScale;

      Fr.transform.parent = car.Transform;
      Fr.transform.position = position;
      Fr.transform.localPosition += offsetFr_;
      DebugFr.transform.parent = Fr.transform;
      DebugFr.transform.position = Fr.transform.position;
      DebugFr.transform.localScale = capsuleScale;

      Rl.transform.parent = car.Transform;
      Rl.transform.position = position;
      Rl.transform.localPosition += offsetRl_;
      DebugRl.transform.parent = Rl.transform;
      DebugRl.transform.position = Rl.transform.position;
      DebugRl.transform.localScale = capsuleScale;

      Rr.transform.parent = car.Transform;
      Rr.transform.position = position;
      Rr.transform.localPosition += offsetRr_;
      DebugRr.transform.parent = Rr.transform;
      DebugRr.transform.position = Rr.transform.position;
      DebugRr.transform.localScale = capsuleScale;

      MakeLights();

      Enabled = false;
    }

    private void Initialize() {
      Dispose();

      Fl = new GameObject();
      Fl.AddComponent<Light>();
      Fl.SetActive(enabled_);

      Fr = new GameObject();
      Fr.AddComponent<Light>();
      Fr.SetActive(enabled_);

      Rl = new GameObject();
      Rl.AddComponent<Light>();
      Rl.SetActive(enabled_);

      Rr = new GameObject();
      Rr.AddComponent<Light>();
      Rr.SetActive(enabled_);

      InitializeDebug();
    }

    private void InitializeDebug() {
      var material = new Material(Shader.Find("HDRP/Lit"));
      material.SetTexture(BaseColorMap, KnUtils.CreateTexture(Color.yellow));

      DebugFl = GameObject.CreatePrimitive(PrimitiveType.Capsule);
      DebugFl.GetComponent<MeshRenderer>().material = material;
      DebugFl.SetActive(Debug);

      DebugFr = GameObject.CreatePrimitive(PrimitiveType.Capsule);
      DebugFr.GetComponent<MeshRenderer>().material = material;
      DebugFr.SetActive(Debug);

      DebugRl = GameObject.CreatePrimitive(PrimitiveType.Capsule);
      DebugRl.GetComponent<MeshRenderer>().material = material;
      DebugRl.SetActive(Debug);

      DebugRr = GameObject.CreatePrimitive(PrimitiveType.Capsule);
      DebugRr.GetComponent<MeshRenderer>().material = material;
      DebugRr.SetActive(Debug);
    }

    private void MakeLights() {
      var fl = Fl.GetComponent<Light>();
      var fr = Fr.GetComponent<Light>();
      var rl = Rl.GetComponent<Light>();
      var rr = Rr.GetComponent<Light>();

      MakeLight(ref fl, color_, brightnessFront_, rangeFront_);
      MakeLight(ref fr, color_, brightnessFront_, rangeFront_);
      MakeLight(ref rl, color_, brightnessRear_, rangeRear_);
      MakeLight(ref rr, color_, brightnessRear_, rangeRear_);
    }

    private void Reset(bool enabled) {
      hazardMode_ = enabled;
      Enabled = enabled;
      hazardTimer_ = 0.0f;
    }

    public void LateUpdate() {
      if (!hazardMode_ || Discarded) {
        return;
      }

      hazardTimer_ += Time.deltaTime;
      if (hazardTimer_ >= HazardToggleTime) {
        hazardTimer_ = 0.0f;
        Enabled = !Enabled;
      }
    }

    public static void MakeLight(ref Light light, Color color, float power, float range) {
      light.type = LightType.Point;
      light.color = color;
      light.range = range;
      light.intensity = power;
    }

    private bool GetLightsFront(out Light fl, out Light fr) {
      fl = Fl.GetComponent<Light>();
      fr = Fr.GetComponent<Light>();
      return fl != null && fr != null;
    }

    private bool GetLightsRear(out Light rl, out Light rr) {
      rl = Rl.GetComponent<Light>();
      rr = Rr.GetComponent<Light>();
      return rl != null && rr != null;
    }

    public void Serialize(BinaryWriter writer) {
      writer.Write(Id);
      writer.Write(KnUtils.EncodeColor(Color));
      writer.Write(RangeFront);
      writer.Write(BrightnessFront);
      writer.Write(RangeRear);
      writer.Write(BrightnessRear);
      KnUtils.WriteVec3(writer, OffsetFront);
      KnUtils.WriteVec3(writer, OffsetRear);
    }
    public bool Deserialize(BinaryReader reader, int version) {
      Id = reader.ReadInt32();
      color_ = KnUtils.DecodeColor(reader.ReadInt32());
      rangeFront_ = reader.ReadSingle();
      brightnessFront_ = reader.ReadSingle();
      rangeRear_ = reader.ReadSingle();
      brightnessRear_ = reader.ReadSingle();
      OffsetFront = KnUtils.ReadVec3(reader);
      OffsetRear = KnUtils.ReadVec3(reader);

      return true;
    }

    public void Send(int id, Udp udp) {
      var data = new SmartfoxDataPackage(PacketId.Subroom);
      data.Add("1", (byte) 25);
      data.Add("type", Udp.TypeHazard);

      data.Add("id", id);
      data.Add("hz", Hazard);

      data.Add("fx", OffsetFront.x);
      data.Add("fy", OffsetFront.y);
      data.Add("fz", OffsetFront.z);

      data.Add("rx", OffsetRear.x);
      data.Add("ry", OffsetRear.y);
      data.Add("rz", OffsetRear.z);

      udp.Send(data);
    }

    public void OnUdpData(SmartfoxDataPackage data) {
      Hazard = data.Data.GetBool("hz");

      float x = data.Data.GetFloat("fx");
      float y = data.Data.GetFloat("fy");
      float z = data.Data.GetFloat("fz");
      OffsetFront = new Vector3(x, y, z);

      x = data.Data.GetFloat("rx");
      y = data.Data.GetFloat("ry");
      z = data.Data.GetFloat("rz");
      OffsetRear = new Vector3(x, y, z);
    }
  }
}