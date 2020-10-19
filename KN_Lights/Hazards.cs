using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KN_Core;
using KN_Loader;
using SyncMultiplayer;
using UnityEngine;

namespace KN_Lights {
  public class Hazards {
    private const string DefaultConfigFile = "default_hazards.knl";
    private const string ConfigFile = "kn_hazards.knl";

    private HazardLights ownHazards_;
    private readonly List<HazardLights> hazards_;
    private readonly List<HazardLights> defaultHazards_;
    private readonly List<HazardLights> playerHazards_;

#if KN_DEV_TOOLS
    private bool forceEnable_;
    private readonly List<HazardLights> hazardsDump_;
#endif

    private readonly Core core_;

    public Hazards(Core core) {
      core_ = core;

      hazards_ = new List<HazardLights>();
      defaultHazards_ = new List<HazardLights>();
      playerHazards_ = new List<HazardLights>();

#if KN_DEV_TOOLS
      hazardsDump_ = new List<HazardLights>();
#endif
    }

    public void OnStart() {
#if KN_DEV_TOOLS
      if (DataSerializer.Deserialize<HazardLights>("KN_Lights::Hazards", KnConfig.BaseDir + "dev/" + DefaultConfigFile, out var data)) {
        hazardsDump_.AddRange(data.ConvertAll(d => (HazardLights) d));
      }
#endif

      var stream = Embedded.LoadEmbeddedFile(Assembly.GetExecutingAssembly(), $"KN_Lights.Resources.{DefaultConfigFile}");
      if (stream != null) {
        using (stream) {
          if (DataSerializer.Deserialize<HazardLights>("KN_Lights::Hazards", stream, out var defaultData)) {
            defaultHazards_.AddRange(defaultData.ConvertAll(d => (HazardLights) d));

#if KN_DEV_TOOLS
            foreach (var hz in defaultHazards_) {
              bool found = hazardsDump_.Any(h => h.Id == hz.Id);
              if (!found) {
                hazardsDump_.Add(hz.Copy());
              }
            }
#endif
          }
        }
      }

      if (DataSerializer.Deserialize<HazardLights>("KN_Lights::Hazards", KnConfig.BaseDir + ConfigFile, out var playerData)) {
        playerHazards_.AddRange(playerData.ConvertAll(d => (HazardLights) d));
      }
    }

    public void OnStop() {
#if KN_DEV_TOOLS
      DevFlushConfig();
#endif

      DataSerializer.Serialize("KN_Lights::Hazards", playerHazards_.ToList<ISerializable>(), KnConfig.BaseDir + ConfigFile, Loader.Version);
    }

    public void OnCarLoaded(KnCar car) {
      bool found = hazards_.Any(hz => hz.Car == car);
      if (!found) {
        var hz = GetOrCreateHazards(car.Id);
        hz.Attach(car);
        hazards_.Add(hz);

        if (car == core_.PlayerCar) {
          ownHazards_ = hz;
        }
      }

      SendData();
    }

    public void OnUdpData(SmartfoxDataPackage data) {
      int id = data.Data.GetInt("id");

      foreach (var hz in hazards_) {
        if (!KnCar.IsNull(hz.Car) && hz.Car.IsNetworkCar && hz.Car.Base.networkPlayer.NetworkID == id) {
          hz.OnUdpData(data);
          break;
        }
      }
    }

    public void Update(float discardDistance) {
      hazards_.RemoveAll(hz => {
        if (KnCar.IsNull(hz.Car)) {
          if (hz == ownHazards_) {
            ToggleHazards(true);
            ownHazards_ = null;
          }
          hz.Dispose();
          return true;
        }
        return false;
      });

      if ((core_.IsCarChanged || ownHazards_ == null) && !KnCar.IsNull(core_.PlayerCar)) {
        var hz = GetOrCreateHazards(core_.PlayerCar.Id);
        hz.Attach(core_.PlayerCar);
        hazards_.Add(hz);
        ownHazards_ = hz;
      }

      foreach (var hz in hazards_) {
        if (!hz.Hazard) {
          hz.Enabled = false;
        }
      }

#if KN_DEV_TOOLS
      if (forceEnable_ && ownHazards_ != null) {
        ownHazards_.Enabled = forceEnable_;
      }
#endif

      if (!core_.IsGuiEnabled && ownHazards_ != null && ownHazards_.Debug) {
        ownHazards_.Debug = false;
      }

      Optimize(discardDistance);

      if (Controls.KeyDown("toggle_hazards")) {
        ToggleHazards(false);
      }
    }

    public void LateUpdate() {
      foreach (var hz in hazards_) {
        hz.LateUpdate();
      }
    }

    public void GuiButton(Gui gui, ref float x, ref float y, float width, float height) {
      bool hazard = ownHazards_?.Hazard ?? false;
      if (gui.TextButton(ref x, ref y, width, height, Locale.Get("hazard_lights"), hazard ? Skin.ButtonSkin.Active : Skin.ButtonSkin.Normal)) {
        ToggleHazards(false);
      }
    }

    public bool OnGui(Gui gui, float x, float y) {
      const float width = Gui.Width * 2.0f;
      const float height = Gui.Height;
      float widthPos = width / 3.0f - Gui.OffsetSmall / 2.0f - 1.0f;


      GuiButton(gui, ref x, ref y, width, height);

      bool debugObjects = ownHazards_?.Debug ?? false;
      if (gui.TextButton(ref x, ref y, width, height, Locale.Get("debug_obj"), debugObjects ? Skin.ButtonSkin.Active : Skin.ButtonSkin.Normal)) {
        if (ownHazards_ != null) {
          ownHazards_.Debug = !ownHazards_.Debug;
        }
      }

      float tx = x;
      var offset = ownHazards_?.OffsetFront ?? Vector3.zero;
      if (gui.SliderH(ref x, ref y, widthPos, ref offset.x, Lights.MinPosBound, Lights.MaxPosBound, $"{Locale.Get("front")} X: {offset.x:F}")) {
        if (ownHazards_ != null) {
          ownHazards_.OffsetFront = offset;
          AddToPlayersConfig();
        }
      }
      x += widthPos + Gui.OffsetSmall;
      y -= Gui.Height + Gui.Offset;

      if (gui.SliderH(ref x, ref y, widthPos, ref offset.y, Lights.MinPosBound, Lights.MaxPosBound, $"{Locale.Get("front")} Y: {offset.y:F}")) {
        if (ownHazards_ != null) {
          ownHazards_.OffsetFront = offset;
          AddToPlayersConfig();
        }
      }
      x += widthPos + Gui.OffsetSmall;
      y -= Gui.Height + Gui.Offset;

      // it's because the width is odd
      widthPos += 1.0f;
      if (gui.SliderH(ref x, ref y, widthPos, ref offset.z, Lights.MinPosBoundZ, Lights.MaxPosBoundZ, $"{Locale.Get("front")} Z: {offset.z:F}")) {
        if (ownHazards_ != null) {
          ownHazards_.OffsetFront = offset;
          AddToPlayersConfig();
        }
      }

      widthPos -= 1.0f;
      x = tx;
      offset = ownHazards_?.OffsetRear ?? Vector3.zero;
      if (gui.SliderH(ref x, ref y, widthPos, ref offset.x, Lights.MinPosBound, Lights.MaxPosBound, $"{Locale.Get("rear")} X: {offset.x:F}")) {
        if (ownHazards_ != null) {
          ownHazards_.OffsetRear = offset;
          AddToPlayersConfig();
        }
      }
      x += widthPos + Gui.OffsetSmall;
      y -= Gui.Height + Gui.Offset;

      if (gui.SliderH(ref x, ref y, widthPos, ref offset.y, Lights.MinPosBound, Lights.MaxPosBound, $"{Locale.Get("rear")} Y: {offset.y:F}")) {
        if (ownHazards_ != null) {
          ownHazards_.OffsetRear = offset;
          AddToPlayersConfig();
        }
      }
      x += widthPos + Gui.OffsetSmall;
      y -= Gui.Height + Gui.Offset;

      widthPos += 1.0f;
      if (gui.SliderH(ref x, ref y, widthPos, ref offset.z, -Lights.MinPosBoundZ, -Lights.MaxPosBoundZ, $"{Locale.Get("rear")} Z: {offset.z:F}")) {
        if (ownHazards_ != null) {
          ownHazards_.OffsetRear = offset;
          AddToPlayersConfig();
        }
      }

#if KN_DEV_TOOLS
      x = tx;

      gui.Line(x, y, width, 1.0f, Skin.SeparatorColor);
      y += Gui.Offset;

      if (gui.TextButton(ref x, ref y, width, height, $"FORCE ENABLE {hazards_.Count}", forceEnable_ ? Skin.ButtonSkin.Active : Skin.ButtonSkin.Normal)) {
        forceEnable_ = !forceEnable_;
        if (ownHazards_ != null) {
          ownHazards_.Enabled = forceEnable_;
        }
      }

      float brightnessFront = ownHazards_?.BrightnessFront ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref brightnessFront, 1.0f, 20.0f, $"HAZARD LIGHT BRIGHTNESS FRONT: {brightnessFront:F1}")) {
        if (ownHazards_ != null) {
          ownHazards_.BrightnessFront = brightnessFront;
          AddToPlayersConfig();
        }
      }

      float rangeFront = ownHazards_?.RangeFront ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref rangeFront, 0.05f, 1.0f, $"HAZARD LIGHT RANGE FRONT: {rangeFront:F}")) {
        if (ownHazards_ != null) {
          ownHazards_.RangeFront = rangeFront;
          AddToPlayersConfig();
        }
      }

      float brightnessRear = ownHazards_?.BrightnessRear ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref brightnessRear, 1.0f, 20.0f, $"HAZARD LIGHT BRIGHTNESS REAR: {brightnessRear:F1}")) {
        if (ownHazards_ != null) {
          ownHazards_.BrightnessRear = brightnessRear;
          AddToPlayersConfig();
        }
      }

      float rangeRear = ownHazards_?.RangeRear ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref rangeRear, 0.05f, 1.0f, $"HAZARD LIGHT RANGE REAR: {rangeRear:F}")) {
        if (ownHazards_ != null) {
          ownHazards_.RangeRear = rangeRear;
          AddToPlayersConfig();
        }
      }

      if (gui.TextButton(ref x, ref y, width, height, "DEV SAVE", Skin.ButtonSkin.Normal)) {
        if (ownHazards_ != null) {
          bool found = false;
          for (int i = 0; i < hazardsDump_.Count; ++i) {
            if (hazardsDump_[i].Id == ownHazards_.Id) {
              found = true;
              hazardsDump_[i] = ownHazards_.Copy();
              break;
            }
          }
          if (!found) {
            hazardsDump_.Add(ownHazards_.Copy());
          }
        }
        Log.Write($"[KN_Lights::Hazards]: Dev save / saved for '{ownHazards_?.Id ?? 0}'");
        DevFlushConfig();
      }
#endif

      return false;
    }

    private HazardLights GetOrCreateHazards(int id) {
      foreach (var hz in playerHazards_) {
        if (hz.Id == id) {
          return hz.Copy();
        }
      }

#if KN_DEV_TOOLS
      foreach (var hz in hazardsDump_) {
#else
      foreach (var hz in defaultHazards_) {
#endif
        if (hz.Id == id) {
          return hz.Copy();
        }
      }
      Log.Write($"[KN_Lights::Hazards]: Unable to found config for '{id}', creating default");
      return new HazardLights(id);
    }

    private void Optimize(float distance) {
      var camera = core_.ActiveCamera;
      if (camera != null) {
        foreach (var hz in hazards_) {
          if (!KnCar.IsNull(hz.Car)) {
            hz.Discarded = Vector3.Distance(camera.transform.position, hz.Car.Transform.position) > distance;
          }
        }
      }
    }

    private void ToggleHazards(bool disable) {
      if (ownHazards_ != null) {
        if (disable) {
          ownHazards_.Hazard = false;
        }
        else {
          ownHazards_.Hazard = !ownHazards_.Hazard;
        }
        SendData();
      }
    }

    private void SendData() {
      int id = NetworkController.InstanceGame?.LocalPlayer?.NetworkID ?? -1;
      if (id == -1 || ownHazards_ == null) {
        return;
      }

      ownHazards_.Send(id, core_.Udp);
    }

    private void AddToPlayersConfig() {
      if (ownHazards_ == null) {
        return;
      }
      bool found = false;
      for (int i = 0; i < playerHazards_.Count; ++i) {
        if (playerHazards_[i].Id == ownHazards_.Id) {
          found = true;
          playerHazards_[i] = ownHazards_.Copy();
          break;
        }
      }
      if (!found) {
        playerHazards_.Add(ownHazards_.Copy());
      }
    }

#if KN_DEV_TOOLS
    private void DevFlushConfig() {
      DataSerializer.Serialize("KN_Lights::Hazards", hazardsDump_.ToList<ISerializable>(), KnConfig.BaseDir + "dev/" + DefaultConfigFile, Loader.Version);
    }
#endif
  }
}