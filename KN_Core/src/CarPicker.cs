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

    private float carsListHeight_;
    private float ghostsListHeight_;

    private readonly Core core_;

    public CarPicker(Core core) {
      core_ = core;

      Cars = new List<TFCar>(16);
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

      const float boxWidth = Gui.Width + Gui.OffsetGuiX * 2.0f;
      const float width = Gui.Width;
      const float height = Gui.Height;

      float yBegin = y;

      GuiCars(gui, ref x, ref y, boxWidth, width, height);
      carsListHeight_ = y - yBegin - Gui.Height;

      x += Gui.Width + Gui.OffsetGuiX;
      y = yBegin;
    }

    private void GuiCars(Gui gui, ref float x, ref float y, float boxWidth, float width, float height) {
      gui.Box(x, y, boxWidth, Gui.Height, "CARS", Skin.MainContainerDark);
      y += Gui.Height;

      gui.Box(x, y, boxWidth, carsListHeight_, Skin.MainContainer);
      y += Gui.OffsetY;
      x += Gui.OffsetGuiX;

      if (gui.Button(ref x, ref y, width, height, "PLAYER CAR", Skin.Button)) {
        PickedCar = PlayerCar;
      }

      if (Cars.Count > 0) {
        gui.Line(x, y, width, 1.0f, Skin.SeparatorColor);
        y += Gui.OffsetY;

        foreach (var c in Cars) {
          if (gui.Button(ref x, ref y, width, height, c.Name, Skin.Button)) {
            PickedCar = c;
          }
        }
      }
    }

    private void UpdateCars() {
      PlayerCar = null;
      PickedCar = null;

      Cars.Clear();

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
    }
  }
}