using System.Collections.Generic;
using System.Linq;

namespace KN_Lights {

  public interface ILightsConfig {
    void AddLights(CarLights lights);
    CarLights GetLights(int carId, ulong sid);
  }

  public class LightsConfig : ILightsConfig {
    public List<CarLights> Lights { get; }

    public LightsConfig() {
      Lights = new List<CarLights>();
    }

    public LightsConfig(List<CarLights> lights) {
      Lights = lights;
    }

    public void AddLights(CarLights lights) {
      int id = Lights.FindIndex(cl => cl.CarId == lights.CarId);
      if (id != -1) {
        Lights[id] = lights;
        return;
      }
      Lights.Add(lights);
    }

    public CarLights GetLights(int carId, ulong sid) {
      return Lights.FirstOrDefault(light => light.CarId == carId);
    }
  }

  public class NwLightsConfig : ILightsConfig {
    public List<CarLights> Lights { get; }

    public NwLightsConfig() {
      Lights = new List<CarLights>();
    }

    public NwLightsConfig(List<CarLights> lights) {
      Lights = lights;
    }

    public void AddLights(CarLights lights) {
      int id = Lights.FindIndex(cl => cl.CarId == lights.CarId && cl.Sid == lights.Sid);
      if (id != -1) {
        Lights[id] = lights;
        return;
      }
      Lights.Add(lights);
    }

    public CarLights GetLights(int carId, ulong sid) {
      return Lights.FirstOrDefault(cl => cl.CarId == carId && cl.Sid == sid);
    }
  }
}