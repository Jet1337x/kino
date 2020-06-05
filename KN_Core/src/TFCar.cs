using CarX;
using UnityEngine;

namespace KN_Core {
  public class TFCar {
    public bool IsGhost { get; }
    public RaceCar Base { get; }
    public string Name { get; }

    public bool IsNetworkCar => Base.isNetworkCar;
    public Car CarX => Base.carX;
    public Transform CxTransform => Base.getTransform;
    public Transform Transform => Base.transform;

    public int Id => Base.metaInfo.id;

    public TFCar(RaceCar car) {
      if (car == null) {
        Base = null;
        Name = null;
      }

      Base = car;
      Name = car.isNetworkCar ? car.networkPlayer.FilteredNickName : "OWN_CAR";
      IsGhost = false;
    }

    public TFCar(string name, RaceCar car, bool ghost = true) {
      if (car == null) {
        Base = null;
        Name = null;
      }

      Base = car;
      Name = name;
      IsGhost = ghost;
    }

    public static bool IsNull(TFCar car) {
      return car == null || car.Base == null;
    }

    public static bool operator ==(TFCar car0, TFCar car1) {
      if (ReferenceEquals(car0, null)) {
        return ReferenceEquals(car1, null);
      }
      if (ReferenceEquals(car1, null)) {
        return false;
      }

      return car0.Name == car1.Name && car0.Id == car1.Id && car0.IsGhost == car1.IsGhost;
    }

    public static bool operator !=(TFCar car0, TFCar car1) {
      return !(car0 == car1);
    }

    private bool Equals(TFCar other) {
      return IsGhost == other.IsGhost && Equals(Base, other.Base) && Name == other.Name;
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != GetType()) return false;
      return Equals((TFCar) obj);
    }

    public override int GetHashCode() {
      unchecked {
        int hashCode = IsGhost.GetHashCode();
        hashCode = (hashCode * 397) ^ (Base != null ? Base.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
        return hashCode;
      }
    }
  }
}