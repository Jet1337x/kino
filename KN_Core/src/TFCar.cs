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
      Name = car.networkPlayer != null ? car.networkPlayer.FilteredNickName : "OWN_CAR";
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
  }
}