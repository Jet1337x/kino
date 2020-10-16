using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KN_Core;
using KN_Loader;
using SyncMultiplayer;
using UnityEngine;

namespace KN_Lights {
  public class Lights : BaseMod {
    public static Texture2D LightMask;

    private const string HelpLink = "https://github.com/trbflxr/kino/blob/master/Help/CarLights.md";

    private const float MinPosBound = 0.0f;
    private const float MaxPosBound = 1.0f;

    private const float MinPosBoundZ = 1.5f;
    private const float MaxPosBoundZ = 3.0f;

    private const string LightsConfigFile = "kn_lights.knl";
    private const string NwLightsConfigFile = "kn_nwlights.knl";
    private const string LightsConfigDefault = "kn_lights_default.knl";
#if KN_DEV_TOOLS
    private const string LightsDevConfigFile = "dev/kn_lights_dev.knl";
#endif

    private LightsConfig lightsConfig_;
    private NwLightsConfig nwLightsConfig_;
    private LightsConfig lightsConfigDefault_;
#if KN_DEV_TOOLS
    private LightsConfig carLightsDev_;
#endif

    private CarLights activeLights_;
    private CarLights ownLights_;
    private readonly List<CarLights> carLights_;
    private readonly List<CarLights> carLightsToRemove_;

    private float clListScrollH_;
    private Vector2 clListScroll_;

    private float carLightsDiscard_;

    private bool autoAddLights_;

    private bool shouldSync_;
    private readonly Timer syncTimer_;
    private readonly Timer joinTimer_;

    private bool pickHeadLightsColor_;

    private readonly Settings settings_;

    public Lights(Core core, int version, int patch, int clientVersion) : base(core, "lights", 0, version, patch, clientVersion) {
      SetIcon(Skin.CarLightsSkin);
      AddTab("car_lights", OnGui);
      SetInfoLink(HelpLink);

      settings_ = core.Settings;

      carLights_ = new List<CarLights>();
      carLightsToRemove_ = new List<CarLights>();

      syncTimer_ = new Timer(0.5f);
      syncTimer_.Callback += SendLightsData;

      joinTimer_ = new Timer(5.0f, true);
      joinTimer_.Callback += SendLightsData;

      pickHeadLightsColor_ = true;
    }

    public override void OnStart() {
      carLightsDiscard_ = Core.KnConfig.Get<float>("cl_discard_distance");

      var assembly = Assembly.GetExecutingAssembly();
      LightMask = Embedded.LoadEmbeddedTexture(assembly, "KN_Lights.Resources.HeadLightMask.png");

      LoadDefaultLights(assembly);
#if KN_DEV_TOOLS
      carLightsDev_ = DataSerializer.Deserialize<CarLights>("KN_CarLights", KnConfig.BaseDir + LightsDevConfigFile, out var devLights)
        ? new LightsConfig(devLights.ConvertAll(l => (CarLights) l))
        : new LightsConfig();
#endif

      nwLightsConfig_ = DataSerializer.Deserialize<CarLights>("KN_CarLights", KnConfig.BaseDir + NwLightsConfigFile, out var nwLights)
        ? new NwLightsConfig(nwLights.ConvertAll(l => (CarLights) l))
        : new NwLightsConfig();

      lightsConfig_ = DataSerializer.Deserialize<CarLights>("KN_CarLights", KnConfig.BaseDir + LightsConfigFile, out var lights)
        ? new LightsConfig(lights.ConvertAll(l => (CarLights) l))
        : new LightsConfig();
    }

    private void LoadDefaultLights(Assembly assembly) {
      var stream = Embedded.LoadEmbeddedFile(assembly, $"KN_Lights.Resources.{LightsConfigDefault}");
      if (stream != null) {
        using (stream) {
          // lightsConfigDefault_ = DataSerializer.Deserialize<CarLights>("KN_CarLights", stream, out var lights)
          //   ? new LightsConfig(lights.ConvertAll(l => (CarLights) l))
          //   : new LightsConfig();

          lightsConfigDefault_ = new LightsConfig();

#if false
          foreach (var l in lightsConfigDefault_.Lights) { }
          LightsConfigSerializer.Serialize(lightsConfigDefault_, "dump.knl");
#endif
        }
      }
    }

    public override void OnStop() {
      Core.KnConfig.Set("cl_discard_distance", carLightsDiscard_);

      if (!DataSerializer.Serialize("KN_CarLights", lightsConfig_.Lights.ToList<ISerializable>(), KnConfig.BaseDir + LightsConfigFile, Loader.Version)) { }
      if (!DataSerializer.Serialize("KN_CarLights", nwLightsConfig_.Lights.ToList<ISerializable>(), KnConfig.BaseDir + NwLightsConfigFile, Loader.Version)) { }

#if KN_DEV_TOOLS
      if (!DataSerializer.Serialize("KN_CarLights", carLightsDev_.Lights.ToList<ISerializable>(), KnConfig.BaseDir + LightsDevConfigFile, Loader.Version)) { }
#endif
    }

    public override void OnCarLoaded(KnCar car) {
      AutoAddLights(false);
      shouldSync_ = true;
    }

    public override void OnGuiToggle() {
      if (Core.ColorPicker.IsPicking) {
        shouldSync_ = activeLights_ == ownLights_;
      }
    }

    public override void OnUdpData(SmartfoxDataPackage data) {
      int type = data.Data.GetInt("type");
      if (type != Udp.TypeLights) {
        return;
      }

      int id = data.Data.GetInt("id");

      foreach (var car in Core.Cars) {
        if (car.IsNetworkCar && car.Base.networkPlayer.NetworkID == id) {
          bool found = false;
          foreach (var cl in nwLightsConfig_.Lights) {
            if (cl.CarId == car.Id && cl.Sid == car.Base.networkPlayer.PlayerId.uid) {
              cl.ModifyFrom(data);
              if (autoAddLights_) {
                EnableLightsOn(car, false);
              }
              found = true;
              break;
            }
          }

          if (!found) {
            var lights = CreateLights(car, nwLightsConfig_, false);
            lights.ModifyFrom(data);
            if (autoAddLights_) {
              EnableLightsOn(car, false);
            }
          }
          break;
        }
      }
    }

    public override void Update(int id) {
      if (!Core.IsGuiEnabled && activeLights_ != null && activeLights_.Debug) {
        activeLights_.Debug = false;
      }

      OptimizeLights();

      ToggleOwnLights();

      if (Core.IsInGarageChanged) {
        joinTimer_.Reset();
      }

      if (settings_.SyncLights && shouldSync_) {
        syncTimer_.Update();
      }

      joinTimer_.Update();

      if (id != Id) {
        return;
      }

      if (Core.CarPicker.IsPicking && !KnCar.IsNull(Core.CarPicker.PickedCar)) {
        EnableLightsOn(Core.CarPicker.PickedCar);
        Core.CarPicker.Reset();
      }

      if (Core.ColorPicker.IsPicking) {
        if (activeLights_ != null) {
          if (pickHeadLightsColor_) {
            if (Core.ColorPicker.PickedColor != activeLights_.HeadLights.Color) {
              activeLights_.HeadLights.Color = Core.ColorPicker.PickedColor;
              shouldSync_ = activeLights_ == ownLights_;
            }
          }
          else {
            if (Core.ColorPicker.PickedColor != activeLights_.DashLight.Color) {
              activeLights_.DashLight.Color = Core.ColorPicker.PickedColor;
              shouldSync_ = activeLights_ == ownLights_;
            }
          }
        }
      }
    }

    public override void LateUpdate(int id) {
      RemoveAllNullLights();

      foreach (var cl in carLights_) {
        cl.LateUpdate();
      }
    }

    private bool OnGui(Gui gui, float x, float y) {
      float yBegin = y;

      const float width = Gui.Width * 2.0f;
      const float height = Gui.Height;

      bool guiEnabled = GUI.enabled;
      GUI.enabled = !KnCar.IsNull(Core.PlayerCar);

      if (gui.TextButton(ref x, ref y, width, height, Locale.Get("lights_enable_own"), Skin.ButtonSkin.Normal)) {
        Core.ColorPicker.Reset();
        pickHeadLightsColor_ = true;
        EnableLightsOn(Core.PlayerCar);
      }

      gui.Line(x, y, width, 1.0f, Skin.SeparatorColor);
      y += Gui.Offset;

      GUI.enabled = activeLights_ != null && !activeLights_.IsNwCar;

#if KN_DEV_TOOLS
      if (gui.TextButton(ref x, ref y, width, height, "DEV SAVE", Skin.ButtonSkin.Normal)) {
        carLightsDev_.AddLights(activeLights_);
        Log.Write($"[KN_CarLights]: Dev save / saved for '{activeLights_?.CarId ?? 0}'");
      }
#endif

      bool debugObjects = activeLights_?.Debug ?? false;
      if (gui.TextButton(ref x, ref y, width, height, Locale.Get("debug_obj"), debugObjects ? Skin.ButtonSkin.Active : Skin.ButtonSkin.Normal)) {
        if (activeLights_ != null) {
          activeLights_.Debug = !activeLights_.Debug;
        }
      }

      if (gui.SliderH(ref x, ref y, width, ref carLightsDiscard_, 50.0f, 500.0f, $"{Locale.Get("hide_lights_after")}: {carLightsDiscard_:F1}")) {
        Core.KnConfig.Set("cl_discard_distance", carLightsDiscard_);
        shouldSync_ = activeLights_ == ownLights_;
      }

      gui.Line(x, y, width, 1.0f, Skin.SeparatorColor);
      y += Gui.Offset;

      GuiHeadLights(gui, ref x, ref y, width, height);

      gui.Line(x, y, width, 1.0f, Skin.SeparatorColor);
      y += Gui.Offset;

      GuiTailLights(gui, ref x, ref y, width, height);

#if KN_DEV_TOOLS
      gui.Line(x, y, width, 1.0f, Skin.SeparatorColor);
      y += Gui.Offset;

      GuiDashLight(gui, ref x, ref y, width, height);
#endif

      y = yBegin;
      x += width;

      x += Gui.Offset;
      gui.Line(x, y, 1.0f, gui.MaxContentHeight - Gui.Offset * 2.0f, Skin.SeparatorColor);
      x += Gui.Offset;

      GUI.enabled = guiEnabled;

      GuiLightsList(gui, ref x, ref y);

      return false;
    }

    private void GuiHeadLights(Gui gui, ref float x, ref float y, float width, float height) {
      float xBegin = x;
      float widthLight = width / 2.0f - Gui.OffsetSmall;
      float widthPos = width / 3.0f - Gui.OffsetSmall / 2.0f - 1.0f;

      bool lh = activeLights_?.HeadLights.EnabledLeft ?? false;
      if (gui.TextButton(ref x, ref y, widthLight, height, Locale.Get("hl_left"), lh ? Skin.ButtonSkin.Active : Skin.ButtonSkin.Normal)) {
        if (activeLights_ != null) {
          activeLights_.HeadLights.EnabledLeft = !activeLights_.HeadLights.EnabledLeft;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }
      y -= Gui.Offset + height;
      x += widthLight + Gui.OffsetSmall * 2.0f;

      bool rh = activeLights_?.HeadLights.EnabledRight ?? false;
      if (gui.TextButton(ref x, ref y, widthLight, height, Locale.Get("hl_right"), rh ? Skin.ButtonSkin.Active : Skin.ButtonSkin.Normal)) {
        if (activeLights_ != null) {
          activeLights_.HeadLights.EnabledRight = !activeLights_.HeadLights.EnabledRight;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }
      x = xBegin;

      if (gui.TextButton(ref x, ref y, width, height, Locale.Get("color"), Skin.ButtonSkin.Normal)) {
        if (activeLights_ != null) {
          Core.CarPicker.IsPicking = false;
          Core.ColorPicker.Toggle(activeLights_.HeadLights.Color, false);
          pickHeadLightsColor_ = true;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }

      float brightness = activeLights_?.HeadLights.Brightness ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref brightness, 100.0f, 10000.0f, $"{Locale.Get("hl_brightness")}: {brightness:F1}")) {
        if (activeLights_ != null) {
          activeLights_.HeadLights.Brightness = brightness;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }

      float angle = activeLights_?.HeadLights.Angle ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref angle, 50.0f, 160.0f, $"{Locale.Get("hl_angle")}: {angle:F1}")) {
        if (activeLights_ != null) {
          activeLights_.HeadLights.Angle = angle;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }

      float hlPitch = activeLights_?.HeadLights.Pitch ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref hlPitch, -20.0f, 20.0f, $"{Locale.Get("hl_pitch")}: {hlPitch:F}")) {
        if (activeLights_ != null) {
          activeLights_.HeadLights.Pitch = hlPitch;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }

      float tx = x;
      var offset = activeLights_?.HeadLights.Offset ?? Vector3.zero;
      if (gui.SliderH(ref x, ref y, widthPos, ref offset.x, MinPosBound, MaxPosBound, $"X: {offset.x:F}")) {
        if (activeLights_ != null) {
          activeLights_.HeadLights.Offset = offset;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }
      x += widthPos + Gui.OffsetSmall;
      y -= Gui.Height + Gui.Offset;

      if (gui.SliderH(ref x, ref y, widthPos, ref offset.y, MinPosBound, MaxPosBound, $"Y: {offset.y:F}")) {
        if (activeLights_ != null) {
          activeLights_.HeadLights.Offset = offset;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }
      x += widthPos + Gui.OffsetSmall;
      y -= Gui.Height + Gui.Offset;

      // it's because the width is odd
      widthPos += 1.0f;
      if (gui.SliderH(ref x, ref y, widthPos, ref offset.z, MinPosBoundZ, MaxPosBoundZ, $"Z: {offset.z:F}")) {
        if (activeLights_ != null) {
          activeLights_.HeadLights.Offset = offset;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }

      x = tx;
    }

    private void GuiTailLights(Gui gui, ref float x, ref float y, float width, float height) {
      float xBegin = x;
      float widthLight = width / 2.0f - Gui.OffsetSmall;
      float widthPos = width / 3.0f - Gui.OffsetSmall / 2.0f - 1.0f;

      bool lt = activeLights_?.TailLights.EnabledLeft ?? false;
      if (gui.TextButton(ref x, ref y, widthLight, height, Locale.Get("tl_left"), lt ? Skin.ButtonSkin.Active : Skin.ButtonSkin.Normal)) {
        if (activeLights_ != null) {
          activeLights_.TailLights.EnabledLeft = !activeLights_.TailLights.EnabledLeft;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }
      y -= Gui.Offset + height;
      x += widthLight + Gui.OffsetSmall * 2.0f;

      bool rt = activeLights_?.TailLights.EnabledRight ?? false;
      if (gui.TextButton(ref x, ref y, widthLight, height, Locale.Get("tl_right"), rt ? Skin.ButtonSkin.Active : Skin.ButtonSkin.Normal)) {
        if (activeLights_ != null) {
          activeLights_.TailLights.EnabledRight = !activeLights_.TailLights.EnabledRight;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }
      x = xBegin;

      float brightness = activeLights_?.TailLights.Brightness ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref brightness, 15.0f, 80.0f, $"{Locale.Get("tl_brightness")}: {brightness:F1}")) {
        if (activeLights_ != null) {
          activeLights_.TailLights.Brightness = brightness;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }

      float angle = activeLights_?.TailLights.Angle ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref angle, 50.0f, 170.0f, $"{Locale.Get("tl_angle")}: {angle:F1}")) {
        if (activeLights_ != null) {
          activeLights_.TailLights.Angle = angle;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }

      float tlPitch = activeLights_?.TailLights.Pitch ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref tlPitch, -20.0f, 20.0f, $"{Locale.Get("tl_pitch")}: {tlPitch:F1}")) {
        if (activeLights_ != null) {
          activeLights_.TailLights.Pitch = tlPitch;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }

      float tx = x;
      var offset = activeLights_?.TailLights.Offset ?? Vector3.zero;
      if (gui.SliderH(ref x, ref y, widthPos, ref offset.x, MinPosBound, MaxPosBound, $"X: {offset.x:F}")) {
        if (activeLights_ != null) {
          activeLights_.TailLights.Offset = offset;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }
      x += widthPos + Gui.OffsetSmall;
      y -= Gui.Height + Gui.Offset;

      if (gui.SliderH(ref x, ref y, widthPos, ref offset.y, MinPosBound, MaxPosBound, $"Y: {offset.y:F}")) {
        if (activeLights_ != null) {
          activeLights_.TailLights.Offset = offset;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }
      x += widthPos + Gui.OffsetSmall;
      y -= Gui.Height + Gui.Offset;

      // it's because the width is odd
      widthPos += 1.0f;
      if (gui.SliderH(ref x, ref y, widthPos, ref offset.z, MinPosBoundZ, -MaxPosBoundZ, $"Z: {offset.z:F}")) {
        if (activeLights_ != null) {
          activeLights_.TailLights.Offset = offset;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }
      x = tx;
    }

    private void GuiLightsList(Gui gui, ref float x, ref float y) {
      const float listHeight = 385.0f;
      const float widthScale = 1.2f;
      const float buttonWidth = Gui.Width * widthScale;

      bool guiEnabled = GUI.enabled;
      GUI.enabled = !Core.IsInGarage && !KnCar.IsNull(Core.PlayerCar);

      if (gui.TextButton(ref x, ref y, buttonWidth, Gui.Height, Locale.Get("add_lights_all"), autoAddLights_ ? Skin.ButtonSkin.Active : Skin.ButtonSkin.Normal)) {
        autoAddLights_ = !autoAddLights_;

        if (autoAddLights_) {
          AddLightsToEveryone();
          EnableLightsOn(Core.PlayerCar);
          shouldSync_ = activeLights_ == ownLights_;
        }
      }

      if (gui.TextButton(ref x, ref y, buttonWidth, Gui.Height, Locale.Get("add_lights_to"), Skin.ButtonSkin.Normal)) {
        Core.CarPicker.Toggle();
        Core.ColorPicker.Reset();
        pickHeadLightsColor_ = true;
      }

      GUI.enabled = carLights_.Count > 0;
      if (gui.TextButton(ref x, ref y, buttonWidth, Gui.Height, Locale.Get("remove_all_lights"), Skin.ButtonSkin.Normal)) {
        autoAddLights_ = false;
        RemoveAllLights();
      }
      GUI.enabled = guiEnabled;

      gui.BeginScrollV(ref x, ref y, buttonWidth, listHeight, clListScrollH_, ref clListScroll_, $"{Locale.Get("lights")} {carLights_.Count}");

      float sx = x;
      float sy = y;
      bool scrollVisible = clListScrollH_ > listHeight;
      float width = scrollVisible ? Gui.WidthScroll * widthScale : Gui.WidthScroll * widthScale + Gui.ScrollBarWidth;
      foreach (var cl in carLights_) {
        if (cl != null) {
          bool active = activeLights_ == cl;
          if (gui.ScrollViewButton(ref sx, ref sy, width, Gui.Height, $"{cl.Name}", out bool delPressed,
            active ? Skin.ListButtonSkin.Active : Skin.ListButtonSkin.Normal, Skin.RedButtonSkin.Normal)) {

            if (delPressed) {
              if (cl == activeLights_) {
                activeLights_ = null;
              }
              if (cl == ownLights_) {
                ownLights_ = null;
              }
              cl.Dispose();
              carLights_.Remove(cl);
              break;
            }
            activeLights_ = cl;
            if (Core.ColorPicker.IsPicking) {
              Core.ColorPicker.Pick(activeLights_.HeadLights.Color, false);
            }
          }
        }
      }

      clListScrollH_ = gui.EndScrollV(ref x, ref y, sy);

      y -= Gui.Offset;
      gui.Dummy(x, y, buttonWidth + Gui.Offset, 0.0f);
    }

#if KN_DEV_TOOLS
    private void GuiDashLight(Gui gui, ref float x, ref float y, float width, float height) {
      float widthPos = width / 3.0f - Gui.OffsetSmall / 2.0f - 1.0f;

      bool enabled = activeLights_?.DashLight.Enabled ?? false;
      if (gui.TextButton(ref x, ref y, width, height, "DASH LIGHT ENABLED", enabled ? Skin.ButtonSkin.Active : Skin.ButtonSkin.Normal)) {
        if (activeLights_ != null) {
          activeLights_.DashLight.Enabled = !activeLights_.DashLight.Enabled;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }

      if (gui.TextButton(ref x, ref y, width, height, Locale.Get("color"), Skin.ButtonSkin.Normal)) {
        if (activeLights_ != null) {
          Core.CarPicker.IsPicking = false;
          Core.ColorPicker.Toggle(activeLights_.DashLight.Color, false);
          pickHeadLightsColor_ = false;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }

      float brightness = activeLights_?.DashLight.Brightness ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref brightness, 0.1f, 10.0f, $"DASH LIGHT BRIGHTNESS: {brightness:F1}")) {
        if (activeLights_ != null) {
          activeLights_.DashLight.Brightness = brightness;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }

      float range = activeLights_?.DashLight.Range ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref range, 0.1f, 0.5f, $"DASH LIGHT RANGE: {range:F1}")) {
        if (activeLights_ != null) {
          activeLights_.DashLight.Range = range;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }

      float tx = x;
      var offset = activeLights_?.DashLight.Offset ?? Vector3.zero;
      if (gui.SliderH(ref x, ref y, widthPos, ref offset.x, -0.8f, 0.8f, $"X: {offset.x:F}")) {
        if (activeLights_ != null) {
          activeLights_.DashLight.Offset = offset;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }
      x += widthPos + Gui.OffsetSmall;
      y -= Gui.Height + Gui.Offset;

      if (gui.SliderH(ref x, ref y, widthPos, ref offset.y, 0.4f, 1.5f, $"Y: {offset.y:F}")) {
        if (activeLights_ != null) {
          activeLights_.DashLight.Offset = offset;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }
      x += widthPos + Gui.OffsetSmall;
      y -= Gui.Height + Gui.Offset;

      // it's because the width is odd
      widthPos += 1.0f;
      if (gui.SliderH(ref x, ref y, widthPos, ref offset.z, 1.5f, -0.5f, $"Z: {offset.z:F}")) {
        if (activeLights_ != null) {
          activeLights_.DashLight.Offset = offset;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }
      x = tx;
    }
#endif

    private void EnableLightsOn(KnCar car, bool select = true) {
      bool player = car == Core.PlayerCar;

      CarLights lights = null;
      if (player) {
        lights = lightsConfig_.GetLights(car.Id);
      }
      else if (car.Base.networkPlayer != null) {
        lights = nwLightsConfig_.GetLights(car.Id, car.Base.networkPlayer.PlayerId.uid);
      }

      if (lights == null) {
        if (player) {
          lights = CreateLights(car, lightsConfig_);
          lightsConfig_.AddLights(lights);
        }
        else {
          lights = CreateLights(car, nwLightsConfig_);
          nwLightsConfig_.AddLights(lights);
        }
      }
      lights.Attach(car);

      int index = carLights_.FindIndex(cl => cl.Car == car);
      if (index != -1) {
        carLights_[index] = lights;
      }
      else {
        carLights_.Add(lights);
      }

      if (select) {
        activeLights_ = lights;
      }
      if (player) {
        ownLights_ = lights;
      }
    }

    private CarLights CreateLights(KnCar car, LightsConfigBase config, bool attach = true) {
      var l = lightsConfigDefault_.GetLights(car.Id);
      if (l == null) {
        l = new CarLights();
        Log.Write($"[KN_CarLights]: Lights for car '{car.Id}' not found. Creating default.");
      }

      var light = l.Copy();
      if (attach) {
        light.Attach(car);
      }
      config.AddLights(light);
      Log.Write($"[KN_CarLights]: Car lights attached to '{car.Id}'");

      return light;
    }

    private void ToggleOwnLights() {
      if (Controls.KeyDown("toggle_lights")) {
        if (ownLights_ == null) {
          Core.ColorPicker.Reset();
          pickHeadLightsColor_ = true;
          EnableLightsOn(Core.PlayerCar);
        }
        else {
          if (ownLights_ == activeLights_) {
            activeLights_ = null;
          }
          ownLights_.Dispose();
          carLights_.Remove(ownLights_);
          ownLights_ = null;
        }
      }
    }

    private void OptimizeLights() {
      var cam = Core.ActiveCamera;
      if (cam != null) {
        foreach (var cl in carLights_) {
          if (!KnCar.IsNull(cl.Car)) {
            cl.Discarded = cl != activeLights_ && Vector3.Distance(cam.transform.position, cl.Car.Transform.position) > carLightsDiscard_;
          }
        }
      }
    }

    private void AutoAddLights(bool select = true) {
      if ((Core.IsInGarageChanged || KnCar.IsNull(Core.PlayerCar)) && carLights_.Count > 0) {
        autoAddLights_ = false;
        RemoveAllNullLights();
      }

      if (autoAddLights_) {
        if (!carLights_.Contains(ownLights_)) {
          EnableLightsOn(Core.PlayerCar, select);
        }
        AddLightsToEveryone(select);
      }
    }

    private void AddLightsToEveryone(bool select = true) {
      foreach (var car in Core.Cars) {
        bool found = false;
        foreach (var cl in carLights_) {
          if (cl.Car == car) {
            found = true;
          }
        }
        if (!found) {
          EnableLightsOn(car, select);
        }
      }
    }

    private void RemoveAllLights() {
      foreach (var l in carLights_) {
        if (l == activeLights_) {
          activeLights_ = null;
        }
        if (l == ownLights_) {
          ownLights_ = null;
        }
        l.Dispose();
      }
      carLights_.Clear();
      carLightsToRemove_.Clear();
    }

    private void RemoveAllNullLights() {
      foreach (var cl in carLights_) {
        if (KnCar.IsNull(cl.Car)) {
          carLightsToRemove_.Add(cl);
        }
      }

      if (carLightsToRemove_.Count > 0) {
        foreach (var cl in carLightsToRemove_) {
          if (activeLights_ == cl) {
            activeLights_ = null;
          }
          if (ownLights_ == cl) {
            ownLights_ = null;
          }
          cl.Dispose();
          carLights_.Remove(cl);
        }
        carLightsToRemove_.Clear();
      }
    }

    private void SendLightsData() {
      int id = NetworkController.InstanceGame?.LocalPlayer?.NetworkID ?? -1;
      if (id == -1 || ownLights_ == null) {
        return;
      }

      shouldSync_ = false;
      ownLights_.Send(id, Core.Udp);
    }
  }
}