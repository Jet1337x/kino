using System.Collections.Generic;
using System.Linq;

namespace KN_Lights {

  public class LightsConfigBase {
    public List<CarLights> Lights { get; protected set; }
    protected LightsConfigBase() {
      Lights = new List<CarLights>();
    }

    public void AddLights(CarLights lights) {
      int id = Lights.FindIndex(cl => cl.Car == lights.Car);
      if (id != -1) {
        Lights[id] = lights;
        return;
      }
      Lights.Add(lights);
    }
  }

  public class LightsConfig : LightsConfigBase {
    public LightsConfig() { }

    public LightsConfig(List<CarLights> lights) {
      Lights = lights;
    }

    public CarLights GetLights(int carId) {
      return Lights.FirstOrDefault(light => light.CarId == carId);
    }
  }

  public class NwLightsConfig : LightsConfigBase {
    public NwLightsConfig() { }
    public NwLightsConfig(List<CarLights> lights) {
      Lights = lights;
    }
    public CarLights GetLights(int carId, string user) {
      return Lights.FirstOrDefault(cl => cl.CarId == carId && cl.UserName == user);
    }
  }
}