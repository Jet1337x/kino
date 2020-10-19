using BepInEx;
using KN_Core;

namespace KN_Visuals {
  [BepInPlugin("trbflxr.kn_visuals", "KN_Visuals", StringVersion)]
  public class Loader : BaseUnityPlugin {
    private const int Version = 200;
    private const int Patch = 1;
    private const int ClientVersion = 273;
    private const string StringVersion = "2.0.0";

    public Loader() {
      Core.CoreInstance.AddMod(new Visuals(Core.CoreInstance, Version, Patch, ClientVersion));
    }
  }
}