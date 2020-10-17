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
        if (GetLights(out var fl, out var fr, out var rl, out var rr)) {
          fl.color = color_;
          fr.color = color_;
          rl.color = color_;
          rr.color = color_;
        }
      }
    }

    private float range_;
    public float Range {
      get => range_;
      set {
        range_ = value;
        if (GetLights(out var fl, out var fr, out var rl, out var rr)) {
          fl.range = range_;
          fr.range = range_;
          rl.range = range_;
          rr.range = range_;
        }
      }
    }

    private float brightness_;
    public float Brightness {
      get => brightness_;
      set {
        brightness_ = value;
        if (GetLights(out var fl, out var fr, out var rl, out var rr)) {
          fl.intensity = brightness_;
          fr.intensity = brightness_;
          rl.intensity = brightness_;
          rr.intensity = brightness_;
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

    private HazardLights(int id, Color color, float brightness, float range, Vector3 offsetFront, Vector3 offsetRear) {
      Id = id;
      enabled_ = false;
      hazardMode_ = false;
      color_ = color;
      brightness_ = brightness;
      range_ = range;
      OffsetFront = offsetFront;
      OffsetRear = offsetRear;
    }

    private void InitDefault() {
      enabled_ = false;
      hazardMode_ = false;
      color_ = new Color32(0xd7, 0x90, 0x00, 0xff);
      brightness_ = DefaultBrightness;
      range_ = DefaultRange;
      OffsetFront = new Vector3(0.6f, 0.6f, 2.2f);
      OffsetRear = new Vector3(0.6f, 0.6f, -2.2f);
    }

    public HazardLights Copy() {
      var light = new HazardLights(Id, color_, brightness_, range_, OffsetFront, OffsetRear);
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

      MakeLight(ref fl, color_, brightness_, range_);
      MakeLight(ref fr, color_, brightness_, range_);
      MakeLight(ref rl, color_, brightness_, range_);
      MakeLight(ref rr, color_, brightness_, range_);
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

    private bool GetLights(out Light fl, out Light fr, out Light rl, out Light rr) {
      fl = Fl.GetComponent<Light>();
      fr = Fr.GetComponent<Light>();
      rl = Rl.GetComponent<Light>();
      rr = Rr.GetComponent<Light>();
      return fl != null && fr != null && rl != null && rr != null;
    }

    public void Serialize(BinaryWriter writer) {
      writer.Write(Id);
      writer.Write(KnUtils.EncodeColor(Color));
      writer.Write(Range);
      writer.Write(Brightness);
      KnUtils.WriteVec3(writer, OffsetFront);
      KnUtils.WriteVec3(writer, OffsetRear);
    }
    public bool Deserialize(BinaryReader reader, int version) {
      Id = reader.ReadInt32();
      color_ = KnUtils.DecodeColor(reader.ReadInt32());
      range_ = reader.ReadSingle();
      brightness_ = reader.ReadSingle();
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

      udp.Send(data);
    }

    public void OnUdpData(SmartfoxDataPackage data) {
      Hazard = data.Data.GetBool("hz");
    }
  }
}