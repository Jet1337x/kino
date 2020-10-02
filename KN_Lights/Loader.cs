using BepInEx;
using KN_Core;
using KN_Loader;

namespace KN_Lights {
  [BepInPlugin("trbflxr.kn_lights", "KN_Lights", ModLoader.StringVersion)]
  public class Loader : BaseUnityPlugin {
    private const int Version = 125;
    private const int Patch = 1;
    private const int ClientVersion = 272;

    public Loader() {
      Core.CoreInstance.AddMod(new Lights(Core.CoreInstance, Version, Patch, ClientVersion));
    }
  }
}