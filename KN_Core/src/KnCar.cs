using CarX;
using GameOverlay;
using UnityEngine;
using NetworkPlayer = SyncMultiplayer.NetworkPlayer;

namespace KN_Core {
  public class KnCar {
    public RaceCar Base { get; }
    public string Name { get; }

    public bool IsNetworkCar => Base.isNetworkCar;
    public Car CarX => Base.carX;
    public Transform CxTransform => Base.getTransform;
    public Transform Transform => Base.transform;

    public int Id => Base.metaInfo.id;

    public bool IsConsole { get; }

    public KnCar(RaceCar car) {
      if (car == null) {
        Base = null;
        Name = null;
      }

      Base = car;
      Name = car.isNetworkCar ? car.networkPlayer.FilteredNickName : "OWN_CAR";
      IsConsole = car.isNetworkCar && car.networkPlayer.PlayerId.platform != UserPlatform.Id.Steam;
    }

    public static bool IsNull(KnCar car) {
      return car == null || car.Base == null;
    }

    public static bool operator ==(KnCar car0, KnCar car1) {
      if (ReferenceEquals(car0, null)) {
        return ReferenceEquals(car1, null);
      }
      if (ReferenceEquals(car1, null)) {
        return false;
      }

      return car0.Name == car1.Name && car0.Id == car1.Id;
    }

    public static bool operator !=(KnCar car0, KnCar car1) {
      return !(car0 == car1);
    }

    private bool Equals(KnCar other) {
      return Equals(Base, other.Base) && Name == other.Name;
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != GetType()) return false;
      return Equals((KnCar) obj);
    }

    public override int GetHashCode() {
      unchecked {
        int hashCode = IsConsole.GetHashCode();
        hashCode = (hashCode * 397) ^ (Base != null ? Base.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
        return hashCode;
      }
    }
  }

  public class DisabledCar {
    public bool Collision = true;
    public bool CollisionPlayer = true;
    public bool Hide = false;
    public bool AutoDisable = false;

    public KnCar Car;

    public static bool IsNull(DisabledCar car) {
      return car == null || car.Car == null || car.Car.Base == null;
    }
  }

  public class LoadingCar {
    public bool Loaded = false;
    public bool Loading = false;
    public NetworkPlayer Player = null;
  }
}