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

    public float Pitch { get; set; }
    public float PitchTail { get; set; }

    public float HeadLightBrightness { get; set; }
    public float HeadLightAngle { get; set; }

    public float TailLightBrightness { get; set; }
    public float TailLightAngle { get; set; }

    public bool IsHeadLightLeftEnabled { get; set; }
    public Vector3 HeadlightOffsetLeft { get; set; }

    public bool IsHeadLightRightEnabled { get; set; }
    public Vector3 HeadlightOffsetRight { get; set; }

    public bool IsTailLightLeftEnabled { get; set; }
    public Vector3 TaillightOffsetLeft { get; set; }

    public bool IsTailLightRightEnabled { get; set; }
    public Vector3 TaillightOffsetRight { get; set; }

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

      if (HeadLightRight != null) {
        Object.Destroy(HeadLightLeft);
      }
      HeadLightRight = new GameObject();
      HeadLightRight.AddComponent<Light>();

      if (TailLightLeft != null) {
        Object.Destroy(HeadLightLeft);
      }
      TailLightLeft = new GameObject();
      TailLightLeft.AddComponent<Light>();

      if (TailLightRight != null) {
        Object.Destroy(HeadLightLeft);
      }
      TailLightRight = new GameObject();
      TailLightRight.AddComponent<Light>();
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

    public void Serialize(BinaryWriter writer) {
      writer.Write(CarId);
      writer.Write(IsNetworkCar);
      writer.Write(UserName);
      writer.Write(Pitch);
      writer.Write(PitchTail);
      writer.Write(HeadLightBrightness);
      writer.Write(HeadLightAngle);
      writer.Write(TailLightBrightness);
      writer.Write(TailLightAngle);

      writer.Write(IsHeadLightLeftEnabled);
      WriteVec3(writer, HeadlightOffsetLeft);

      writer.Write(IsHeadLightRightEnabled);
      WriteVec3(writer, HeadlightOffsetRight);

      writer.Write(IsTailLightLeftEnabled);
      WriteVec3(writer, TaillightOffsetLeft);

      writer.Write(IsTailLightRightEnabled);
      WriteVec3(writer, TaillightOffsetRight);
    }

    public void Deserialize(BinaryReader reader) {
      CarId = reader.ReadInt32();
      IsNetworkCar = reader.ReadBoolean();
      UserName = reader.ReadString();
      Pitch = reader.ReadSingle();
      PitchTail = reader.ReadSingle();
      HeadLightBrightness = reader.ReadSingle();
      HeadLightAngle = reader.ReadSingle();
      TailLightBrightness = reader.ReadSingle();
      TailLightAngle = reader.ReadSingle();

      IsHeadLightLeftEnabled = reader.ReadBoolean();
      HeadlightOffsetLeft = ReadVec3(reader);

      IsHeadLightRightEnabled = reader.ReadBoolean();
      HeadlightOffsetRight = ReadVec3(reader);

      IsTailLightLeftEnabled = reader.ReadBoolean();
      TaillightOffsetLeft = ReadVec3(reader);

      IsTailLightRightEnabled = reader.ReadBoolean();
      TaillightOffsetRight = ReadVec3(reader);
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