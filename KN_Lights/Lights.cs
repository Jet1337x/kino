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

    private const string HelpLink = "https://github.com/trbflxr/kino";

    private const string LightsConfigFile = "kn_lights.knl";
    private const string NwLightsConfigFile = "kn_nwlights.knl";
    private const string LightsConfigDefault = "kn_lights_default.knl";
#if KN_DEV_TOOLS
    private const string LightsDevConfigFile = "kn_lights_dev.knl";
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
    }

    public override void OnStart() {
      var assembly = Assembly.GetExecutingAssembly();

      LightMask = Embedded.LoadEmbeddedTexture(assembly, "KN_Lights.Resources.HeadLightMask.png");

      LoadDefaultLights(assembly);
#if KN_DEV_TOOLS
      carLightsDev_ = DataSerializer.Deserialize<CarLights>("KN_CarLights", KnConfig.BaseDir + LightsDevConfigFile, out var devLights)
        ? new LightsConfig(devLights.ConvertAll(l => (CarLights) l))
        : new LightsConfig();
#endif

      carLightsDiscard_ = Core.KnConfig.Get<float>("cl_discard_distance");

      nwLightsConfig_ = DataSerializer.Deserialize<CarLights>("KN_CarLights", KnConfig.BaseDir + NwLightsConfigFile, out var nwLights)
        ? new NwLightsConfig(nwLights.ConvertAll(l => (CarLights) l))
        : new NwLightsConfig();

      lightsConfig_ = DataSerializer.Deserialize<CarLights>("KN_CarLights", KnConfig.BaseDir + LightsConfigFile, out var lights)
        ? new LightsConfig(lights.ConvertAll(l => (CarLights) l))
        : new LightsConfig();
    }

    public override void OnStop() {
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
            if (cl.CarId == car.Id && cl.UserName == car.Name) {
              cl.ModifyFrom(data);
              if (autoAddLights_) {
                EnableLightsOn(car, false);
              }
              found = true;
              break;
            }
          }

          if (!found) {
            var lights = new CarLights();
            lights.FromNwData(data, car.Id, car.Name);
            if (autoAddLights_) {
              EnableLightsOn(car, false);
            }
            nwLightsConfig_.AddLights(lights);
          }
          break;
        }
      }
    }

    public override void Update(int id) {
      if (!Core.IsGuiEnabled && activeLights_ != null && activeLights_.IsDebugObjectsEnabled) {
        activeLights_.IsDebugObjectsEnabled = false;
      }

      OptimizeLights();

      ToggleOwnLights();

      if (Core.IsInGarageChanged) {
        joinTimer_.Reset();
      }

      if (id != Id) {
        return;
      }

      if (Core.CarPicker.IsPicking && !KnCar.IsNull(Core.CarPicker.PickedCar)) {
        if (Core.CarPicker.PickedCar != Core.PlayerCar) {
          EnableLightsOn(Core.CarPicker.PickedCar);
        }
        else {
          EnableLightsOn(Core.PlayerCar);
        }
        Core.CarPicker.Reset();
      }

      if (Core.ColorPicker.IsPicking) {
        if (activeLights_ != null && Core.ColorPicker.PickedColor != activeLights_.HeadLightsColor) {
          activeLights_.HeadLightsColor = Core.ColorPicker.PickedColor;
        }
      }

      if (settings_.SyncLights && shouldSync_) {
        syncTimer_.Update();
      }

      joinTimer_.Update();
    }

    public override void LateUpdate(int id) {
      RemoveAllNullLights();
    }

    private bool OnGui(Gui gui, float x, float y) {
      float yBegin = y;

      const float width = Gui.Width * 2.0f;
      const float height = Gui.Height;

      bool guiEnabled = GUI.enabled;
      GUI.enabled = !KnCar.IsNull(Core.PlayerCar);

      if (gui.TextButton(ref x, ref y, width, height, Locale.Get("lights_enable_own"), Skin.ButtonSkin.Normal)) {
        Core.ColorPicker.Reset();
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

      bool debugObjects = activeLights_?.IsDebugObjectsEnabled ?? false;
      if (gui.TextButton(ref x, ref y, width, height, Locale.Get("debug_obj"), debugObjects ? Skin.ButtonSkin.Active : Skin.ButtonSkin.Normal)) {
        if (activeLights_ != null) {
          activeLights_.IsDebugObjectsEnabled = !activeLights_.IsDebugObjectsEnabled;
        }
      }

      bool hlIllumination = activeLights_?.IsLightsEnabledIl ?? false;
      if (gui.TextButton(ref x, ref y, width, height, Locale.Get("hl_illumination"), hlIllumination ? Skin.ButtonSkin.Active : Skin.ButtonSkin.Normal)) {
        if (activeLights_ != null) {
          activeLights_.IsLightsEnabledIl = !activeLights_.IsLightsEnabledIl;
        }
        shouldSync_ = activeLights_ == ownLights_;
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

      bool lh = activeLights_?.IsHeadLightLeftEnabled ?? false;
      if (gui.TextButton(ref x, ref y, widthLight, height, Locale.Get("hl_left"), lh ? Skin.ButtonSkin.Active : Skin.ButtonSkin.Normal)) {
        if (activeLights_ != null) {
          activeLights_.IsHeadLightLeftEnabled = !activeLights_.IsHeadLightLeftEnabled;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }
      y -= Gui.Offset + height;
      x += widthLight + Gui.OffsetSmall * 2.0f;

      bool rh = activeLights_?.IsHeadLightRightEnabled ?? false;
      if (gui.TextButton(ref x, ref y, widthLight, height, Locale.Get("hl_right"), rh ? Skin.ButtonSkin.Active : Skin.ButtonSkin.Normal)) {
        if (activeLights_ != null) {
          activeLights_.IsHeadLightRightEnabled = !activeLights_.IsHeadLightRightEnabled;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }
      x = xBegin;

      if (gui.TextButton(ref x, ref y, width, height, Locale.Get("color"), Skin.ButtonSkin.Normal)) {
        if (activeLights_ != null) {
          Core.CarPicker.IsPicking = false;
          Core.ColorPicker.Toggle(activeLights_.HeadLightsColor, false);
          shouldSync_ = activeLights_ == ownLights_;
        }
      }

      float brightness = activeLights_?.HeadLightBrightness ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref brightness, 100.0f, 10000.0f, $"{Locale.Get("hl_brightness")}: {brightness:F1}")) {
        if (activeLights_ != null) {
          activeLights_.HeadLightBrightness = brightness;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }

      float angle = activeLights_?.HeadLightAngle ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref angle, 50.0f, 160.0f, $"{Locale.Get("hl_angle")}: {angle:F1}")) {
        if (activeLights_ != null) {
          activeLights_.HeadLightAngle = angle;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }

      float hlPitch = activeLights_?.Pitch ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref hlPitch, -20.0f, 20.0f, $"{Locale.Get("hl_pitch")}: {hlPitch:F}")) {
        if (activeLights_ != null) {
          activeLights_.Pitch = hlPitch;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }

      var offset = activeLights_?.HeadlightOffset ?? Vector3.zero;
      if (gui.SliderH(ref x, ref y, width, ref offset.x, 0.0f, 3.0f, $"X: {offset.x:F}")) {
        if (activeLights_ != null) {
          activeLights_.HeadlightOffset = offset;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }

      if (gui.SliderH(ref x, ref y, width, ref offset.y, 0.0f, 3.0f, $"Y: {offset.y:F}")) {
        if (activeLights_ != null) {
          activeLights_.HeadlightOffset = offset;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }

      if (gui.SliderH(ref x, ref y, width, ref offset.z, 0.0f, 3.0f, $"Z: {offset.z:F}")) {
        if (activeLights_ != null) {
          activeLights_.HeadlightOffset = offset;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }
    }

    private void GuiTailLights(Gui gui, ref float x, ref float y, float width, float height) {
      float xBegin = x;
      float widthLight = width / 2.0f - Gui.OffsetSmall;

      bool lt = activeLights_?.IsTailLightLeftEnabled ?? false;
      if (gui.TextButton(ref x, ref y, widthLight, height, Locale.Get("tl_left"), lt ? Skin.ButtonSkin.Active : Skin.ButtonSkin.Normal)) {
        if (activeLights_ != null) {
          activeLights_.IsTailLightLeftEnabled = !activeLights_.IsTailLightLeftEnabled;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }
      y -= Gui.Offset + height;
      x += widthLight + Gui.OffsetSmall * 2.0f;

      bool rt = activeLights_?.IsTailLightRightEnabled ?? false;
      if (gui.TextButton(ref x, ref y, widthLight, height, Locale.Get("tl_right"), rt ? Skin.ButtonSkin.Active : Skin.ButtonSkin.Normal)) {
        if (activeLights_ != null) {
          activeLights_.IsTailLightRightEnabled = !activeLights_.IsTailLightRightEnabled;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }
      x = xBegin;

      float brightness = activeLights_?.TailLightBrightness ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref brightness, 15.0f, 80.0f, $"{Locale.Get("tl_brightness")}: {brightness:F1}")) {
        if (activeLights_ != null) {
          activeLights_.TailLightBrightness = brightness;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }

      float angle = activeLights_?.TailLightAngle ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref angle, 50.0f, 170.0f, $"{Locale.Get("tl_angle")}: {angle:F1}")) {
        if (activeLights_ != null) {
          activeLights_.TailLightAngle = angle;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }

      float tlPitch = activeLights_?.PitchTail ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref tlPitch, -20.0f, 20.0f, $"{Locale.Get("tl_pitch")}: {tlPitch:F1}")) {
        if (activeLights_ != null) {
          activeLights_.PitchTail = tlPitch;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }

      var offset = activeLights_?.TailLightOffset ?? Vector3.zero;
      if (gui.SliderH(ref x, ref y, width, ref offset.x, 0.0f, 3.0f, $"X: {offset.x:F}")) {
        if (activeLights_ != null) {
          activeLights_.TailLightOffset = offset;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }

      if (gui.SliderH(ref x, ref y, width, ref offset.y, 0.0f, 3.0f, $"Y: {offset.y:F}")) {
        if (activeLights_ != null) {
          activeLights_.TailLightOffset = offset;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }

      if (gui.SliderH(ref x, ref y, width, ref offset.z, 0.0f, -3.0f, $"Z: {offset.z:F}")) {
        if (activeLights_ != null) {
          activeLights_.TailLightOffset = offset;
          shouldSync_ = activeLights_ == ownLights_;
        }
      }
    }

    private void GuiLightsList(Gui gui, ref float x, ref float y) {
      const float listHeight = 490.0f;
      const float widthScale = 1.2f;
      const float buttonWidth = Gui.Width * widthScale;

      bool guiEnabled = GUI.enabled;
      GUI.enabled = !Core.IsInGarage;

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
      }
      GUI.enabled = guiEnabled;

      gui.BeginScrollV(ref x, ref y, buttonWidth, listHeight, clListScrollH_, ref clListScroll_, $"{Locale.Get("lights")} {carLights_.Count}");

      float sx = x;
      float sy = y;
      const float offset = Gui.ScrollBarWidth;
      bool scrollVisible = clListScrollH_ > listHeight;
      float width = scrollVisible ? Gui.WidthScroll * widthScale - offset : Gui.WidthScroll * widthScale + offset;
      foreach (var cl in carLights_) {
        if (cl != null) {
          bool active = activeLights_ == cl;
          if (gui.ScrollViewButton(ref sx, ref sy, width, Gui.Height, $"{cl.UserName}", out bool delPressed,
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
              Core.ColorPicker.Pick(activeLights_.HeadLightsColor, false);
            }
          }
        }
      }

      clListScrollH_ = gui.EndScrollV(ref x, ref y, sy);

      gui.Dummy(x, y, buttonWidth + Gui.Offset, 0.0f);
    }

    private void EnableLightsOn(KnCar car, bool select = true) {
      bool player = car == Core.PlayerCar;

      var lights = player ? lightsConfig_.GetLights(car.Id) : nwLightsConfig_.GetLights(car.Id, car.Name);
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
      else {
        lights.Attach(car);
      }

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

    private CarLights CreateLights(KnCar car, LightsConfigBase config) {
      var l = lightsConfigDefault_.GetLights(car.Id);
      if (l == null) {
        l = new CarLights {
          HeadLightsColor = Color.white,
          Pitch = 0.0f,
          PitchTail = 0.0f,
          HeadLightBrightness = 1500.0f,
          HeadLightAngle = 100.0f,
          TailLightBrightness = 80.0f,
          TailLightAngle = 170.0f,
          IsHeadLightLeftEnabled = true,
          IsHeadLightRightEnabled = true,
          HeadlightOffset = new Vector3(0.6f, 0.6f, 1.9f),
          IsTailLightLeftEnabled = true,
          IsTailLightRightEnabled = true,
          TailLightOffset = new Vector3(0.6f, 0.6f, -1.6f)
        };
        Log.Write($"[KN_CarLights]: Lights for car '{car.Id}' not found. Creating default.");
      }

      var light = l.Copy();
      light.Attach(car);
      config.AddLights(light);
      Log.Write($"[KN_CarLights]: Car lights attached to '{car.Id}'");

      return light;
    }

    private void ToggleOwnLights() {
      if (Controls.KeyDown("toggle_lights")) {
        if (ownLights_ == null) {
          Core.ColorPicker.Reset();
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
            if (Vector3.Distance(cam.transform.position, cl.Car.Transform.position) > carLightsDiscard_) {
              if (cl.IsLightsEnabled) {
                cl.IsLightsEnabled = false;
              }
            }
            else {
              if (!cl.IsLightsEnabled) {
                cl.IsLightsEnabled = true;
              }
            }
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

    private void RemoveAllNullLights() {
      foreach (var cl in carLights_) {
        if (KnCar.IsNull(cl.Car)) {
          carLightsToRemove_.Add(cl);
          continue;
        }
        cl.LateUpdate();
      }

      if (carLightsToRemove_.Count > 0) {
        foreach (var cl in carLightsToRemove_) {
          if (activeLights_ == cl) {
            activeLights_ = null;
          }
          if (ownLights_ == cl) {
            ownLights_ = null;
          }
          carLights_.Remove(cl);
        }
        carLightsToRemove_.Clear();
      }
    }

    private void LoadDefaultLights(Assembly assembly) {
      var stream = Embedded.LoadEmbeddedFile(assembly, $"KN_Lights.Resources.{LightsConfigDefault}");
      if (stream != null) {
        using (stream) {
          if (DataSerializer.Deserialize<CarLights>("KN_CarLights", stream, out var lights)) {
            lightsConfigDefault_ = new LightsConfig(lights.ConvertAll(l => (CarLights) l));
#if false
          foreach (var l in lightsConfigDefault_.Lights) { }
          LightsConfigSerializer.Serialize(lightsConfigDefault_, "dump.knl");
#endif
          }
        }
      }
    }

    private void SendLightsData() {
      int id = NetworkController.InstanceGame?.LocalPlayer?.NetworkID ?? -1;
      if (id == -1) {
        return;
      }

      if (ownLights_ == null) {
        return;
      }

      shouldSync_ = false;

      var data = new SmartfoxDataPackage(PacketId.Subroom);
      data.Add("1", (byte) 25);
      data.Add("type", Udp.TypeLights);
      data.Add("id", id);
      data.Add("color", KnUtils.EncodeColor(ownLights_.HeadLightsColor));
      data.Add("pitch", ownLights_.Pitch);
      data.Add("pitchTail", ownLights_.PitchTail);
      data.Add("hlBrightness", ownLights_.HeadLightBrightness);
      data.Add("hlAngle", ownLights_.HeadLightAngle);
      data.Add("tlBrightness", ownLights_.TailLightBrightness);
      data.Add("tlAngle", ownLights_.TailLightAngle);

      data.Add("hlLEnabled", ownLights_.IsHeadLightLeftEnabled);
      data.Add("hlREnabled", ownLights_.IsHeadLightRightEnabled);
      data.Add("hlOffsetX", ownLights_.HeadlightOffset.x);
      data.Add("hlOffsetY", ownLights_.HeadlightOffset.y);
      data.Add("hlOffsetZ", ownLights_.HeadlightOffset.z);

      data.Add("tlLEnabled", ownLights_.IsTailLightLeftEnabled);
      data.Add("tlREnabled", ownLights_.IsTailLightRightEnabled);
      data.Add("tlOffsetX", ownLights_.TailLightOffset.x);
      data.Add("tlOffsetY", ownLights_.TailLightOffset.y);
      data.Add("tlOffsetZ", ownLights_.TailLightOffset.z);

      Core.Udp.Send(data);
    }
  }
}