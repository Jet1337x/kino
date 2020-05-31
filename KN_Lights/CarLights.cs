using System;
using System.Xml.Serialization;
using KN_Core;
using UnityEngine;

namespace KN_Lights {

  [Serializable]
  public struct Vector3S {
    [XmlAttribute("x")] public float x;
    [XmlAttribute("y")] public float y;
    [XmlAttribute("z")] public float z;

    public Vector3S(float x, float y, float z) {
      this.x = x;
      this.y = y;
      this.z = z;
    }

    public override bool Equals(object obj) {
      if (!(obj is Vector3S)) {
        return false;
      }

      var s = (Vector3S) obj;
      return Math.Abs(x - s.x) < 0.0001f &&
             Math.Abs(y - s.y) < 0.0001f &&
             Math.Abs(z - s.z) < 0.0001f;
    }

    public override int GetHashCode() {
      int hashCode = 373119288;
      hashCode = hashCode * -1521134295 + x.GetHashCode();
      hashCode = hashCode * -1521134295 + y.GetHashCode();
      hashCode = hashCode * -1521134295 + z.GetHashCode();
      return hashCode;
    }

    public Vector3 ToVector3() {
      return new Vector3(x, y, z);
    }

    public static bool operator ==(Vector3S a, Vector3S b) {
      return Math.Abs(a.x - b.x) < 0.0001f && Math.Abs(a.y - b.y) < 0.0001f && Math.Abs(a.z - b.z) < 0.0001f;
    }

    public static bool operator !=(Vector3S a, Vector3S b) {
      return Math.Abs(a.x - b.x) > 0.0001f && Math.Abs(a.y - b.y) > 0.0001f && Math.Abs(a.z - b.z) > 0.0001f;
    }

    public static implicit operator Vector3(Vector3S x) {
      return new Vector3(x.x, x.y, x.z);
    }

    public static implicit operator Vector3S(Vector3 x) {
      return new Vector3S(x.x, x.y, x.z);
    }
  }

  [Serializable]
  public class CarLights {
    [XmlIgnore] public GameObject HeadLightLeft { get; private set; }
    [XmlIgnore] public GameObject HeadLightRight { get; private set; }

    [XmlIgnore] public GameObject TailLightLeft { get; private set; }
    [XmlIgnore] public GameObject TailLightRight { get; private set; }

    [XmlIgnore] public TFCar Car { get; private set; }

    [XmlAttribute("id")] public int CarId { get; set; }
    [XmlAttribute("carName")] public string CarName { get; set; }
    [XmlAttribute("nwCar")] public bool IsNetworkCar { get; set; }
    [XmlAttribute("userName")] public string UserName { get; set; }

    public float Pitch { get; set; }

    public bool IsHeadLightLeftEnabled { get; set; }
    public Vector3S HeadlightOffsetLeft { get; set; }

    public bool IsHeadLightRightEnabled { get; set; }
    public Vector3S HeadlightOffsetRight { get; set; }

    public bool IsTailLightLeftEnabled { get; set; }
    public Vector3S TaillightOffsetLeft { get; set; }

    public bool IsTailLightRightEnabled { get; set; }
    public Vector3S TaillightOffsetRight { get; set; }
  }
}