using UnityEngine;

namespace KN_Lights {

  public class StaticLightData {
    private GameObject debugObject_;

    public GameObject Light { get; private set; }

    private bool enabled_;
    public bool Enabled {
      get => enabled_;
      set {
        //todo: enable light??
        enabled_ = value;
        Light.SetActive(value);
      }
    }

    public int Type { get; private set; }

    private Color color_;
    public Color Color {
      get => color_;
      set => color_ = value;
    }

    private float brightness_;
    public float Brightness {
      get => brightness_;
      set => brightness_ = value;
    }

    private float angle_;
    public float Angle {
      get => angle_;
      set => angle_ = value;
    }

    public Vector3 Position {
      get => Light.transform.position;
      set => Light.transform.position = value;
    }

    public Quaternion Rotation {
      get => Light.transform.rotation;
      set => Light.transform.rotation = value;
    }
  }
}