using CarX;
using SyncMultiplayer;

namespace KN_Core {
  public static class Suspension {
    public static void Apply(SmartfoxDataPackage data) {
      int id = data.Data.GetInt("id");
      float fl = data.Data.GetFloat("fl");
      float fr = data.Data.GetFloat("fr");
      float rl = data.Data.GetFloat("rl");
      float rr = data.Data.GetFloat("rr");

      foreach (var player in NetworkController.InstanceGame.Players) {
        if (player.NetworkID == id) {
          Adjust(player.userCar.carX, fl, fr, rl, rr);
          break;
        }
      }
    }

    private static void Adjust(Car car, float fl, float fr, float rl, float rr) {
      var flW = car.GetWheel(WheelIndex.FrontLeft);
      var frW = car.GetWheel(WheelIndex.FrontRight);
      var rlW = car.GetWheel(WheelIndex.RearLeft);
      var rrW = car.GetWheel(WheelIndex.RearRight);

      flW.maxSpringLen = fl;
      frW.maxSpringLen = fr;
      rlW.maxSpringLen = rl;
      rrW.maxSpringLen = rr;
    }
  }
}