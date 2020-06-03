using System.Collections.Generic;
using UnityEngine;

namespace KN_Core {
  public class CarPicker {
    private bool isPicking_;
    public bool IsPicking {
      get => isPicking_;
      set {
        isPicking_ = value;
        if (isPicking_) {
          UpdateCars();
        }
      }
    }

    public TFCar PickedCar { get; private set; }
    public TFCar PlayerCar { get; private set; }

    public List<TFCar> Cars { get; }
    public List<TFCar> Ghosts { get; }

    private float carsListHeight_;

    private readonly Core core_;

    public CarPicker(Core core) {
      core_ = core;

      Cars = new List<TFCar>(16);
      Ghosts = new List<TFCar>(16);
    }

    public void Reset() {
      IsPicking = false;
      PickedCar = null;
    }

    public void Toggle() {
      IsPicking = !IsPicking;
      if (!IsPicking) {
        Reset();
      }
    }

    public void OnGUI(Gui gui, ref float x, ref float y) {
      if (!IsPicking) {
        return;
      }

      float yBegin = y;

      gui.Box(x, y, Gui.Width + Gui.OffsetGuiX * 2.0f, Gui.Height, "CARS", Skin.MainContainerDark);
      y += Gui.Height;

      gui.Box(x, y, Gui.Width + Gui.OffsetGuiX * 2.0f, carsListHeight_, Skin.MainContainer);
      y += Gui.OffsetY;
      x += Gui.OffsetGuiX;

      if (gui.Button(ref x, ref y, "PLAYER CAR", Skin.Button)) {
        PickedCar = PlayerCar;
      }

      if (Cars.Count > 0) {
        gui.Line(x, y, Gui.Width, 1.0f, Skin.SeparatorColor);
        y += Gui.OffsetY;

        foreach (var c in Cars) {
          if (gui.Button(ref x, ref y, c.Name, Skin.Button)) {
            PickedCar = c;
          }
        }
      }

      if (Ghosts.Count > 0) {
        gui.Line(x, y, Gui.Width, 1.0f, Skin.SeparatorColor);
        y += Gui.OffsetY;

        gui.Box(x, y, Gui.Width, Gui.Height, "GHOSTS", Skin.MainContainerDark);
        y += Gui.Height + Gui.OffsetY;

        foreach (var c in Ghosts) {
          if (gui.Button(ref x, ref y, c.Name, Skin.Button)) {
            PickedCar = c;
          }
        }
      }

      carsListHeight_ = y - yBegin - Gui.Height;
      x += Gui.Width + Gui.OffsetGuiX;
      y = yBegin;
    }

    private void UpdateCars() {
      PlayerCar = null;
      PickedCar = null;

      Cars.Clear();
      Ghosts.Clear();

      var cars = Object.FindObjectsOfType<RaceCar>();
      if (cars != null && cars.Length > 0) {
        foreach (var c in cars) {
          if (!c.isNetworkCar) {
            PlayerCar = new TFCar(c);
          }
          else {
            Cars.Add(new TFCar(c));
          }
        }
      }

      var ghosts = core_.Replay?.Player.players;
      if (ghosts != null && ghosts.Count > 0) {
        foreach (var car in ghosts) {
          Ghosts.Add(new TFCar(car.NickName, car.ghostCar));
        }
      }
    }
  }
}