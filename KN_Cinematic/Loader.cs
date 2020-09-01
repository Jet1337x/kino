#if false
using BepInEx;
using KN_Core;

namespace KN_Cinematic {
  [BepInPlugin("trbflxr.kn_cinematic", "KN_Cinematic", KnConfig.StringVersion)]
  public class Loader : BaseUnityPlugin {
    private const int Version = 120;

    public Loader() {
      Core.CoreInstance.AddMod(new Cinematic(Core.CoreInstance, Version));
    }
  }
}
#endif