using UnityEngine;

namespace KN_Core {
  public class Timeline {
    private const float MaxSpeed = 8.0f;
    private const int MaxSlow = 8;

    private float lowBound_;
    public float LowBound {
      get => lowBound_;
      set {
        if (value > HighBound) {
          lowBound_ = HighBound;
        }
        else {
          lowBound_ = value > 0.0f ? value : 0.0f;
        }
      }
    }

    private float highBound_;
    public float HighBound {
      get => highBound_;
      set {
        if (value < lowBound_) {
          highBound_ = LowBound;
        }
        else {
          highBound_ = value < MaxTime ? value : MaxTime;
        }
      }
    }

    private float time_;
    public float CurrentTime {
      get => time_;
      set => time_ = value;
    }

    private float maxTime_;
    public float MaxTime {
      get => maxTime_;
      set {
        maxTime_ = value;
        if (value < HighBound) {
          HighBound = value;
        }
        if (value < LowBound) {
          maxTime_ = LowBound;
        }
      }
    }
    public float Speed { get; private set; }
    public int Slow { get; private set; }

    public bool Loop { get; set; }

    private bool drag_;
    public bool IsPlaying { get; set; }

    public bool IsKeyframeEditing { get; set; }

    private Core core_;

    public delegate void BoolCallback(bool value);
    public delegate void TimeCallback(float time);

    public event TimeCallback OnStop;
    public event BoolCallback OnPlay;
    public event TimeCallback OnDrag;
    public event TimeCallback OnKeyframe;
    public event BoolCallback OnKeyframeEdit;

    public Timeline(Core core) {
      core_ = core;
      Reset();
    }

    public void Reset() {
      Loop = true;
      IsPlaying = false;
      Speed = 1.0f;
      MaxTime = 1.0f;
      HighBound = MaxTime;
      CurrentTime = 0.0f;
      IsKeyframeEditing = false;
      Slow = 1;
    }

    public void OnGUI(Gui gui) {
      const float boxHeight = 120.0f;
      float y = Screen.height - boxHeight;
      float x = 120.0f;
      float boundsWidth = x * 2.0f;
      float boxWidth = Screen.width - boundsWidth;
      float tlWidth = boxWidth - Gui.OffsetSmall * 2.0f;

      float xBegin = x;

      gui.Box(x, y, boxWidth, boxHeight, Skin.MainContainer);

      x += boxWidth / 2.0f - Gui.Width / 2.0f;
      gui.Label(ref x, ref y, $"SPEED: {(Slow != 1 ? 1.0f / Slow : Speed):F}");

      x = xBegin;
      y += Gui.OffsetY * 2.0f;
      x += Gui.OffsetSmall;

      if (gui.SliderH(ref x, ref y, tlWidth, ref time_, 0.0f, MaxTime, $"LOW: {lowBound_:F}s | TIME: {CurrentTime:F}s | HIGH: {highBound_:F}s", Skin.TimelineSliderMid)) {
        drag_ = true;
        if (CurrentTime > LowBound && CurrentTime < HighBound) {
          OnDrag?.Invoke(CurrentTime);
        }
      }
      y -= Gui.Height + Gui.OffsetY * 2.0f;

      if (gui.SliderH(ref x, ref y, tlWidth, ref highBound_, 0.0f, MaxTime, "", Skin.TimelineSliderHigh)) {
        HighBound = highBound_;
        OnDrag?.Invoke(CurrentTime);
      }
      y -= Gui.Height;

      if (gui.SliderH(ref x, ref y, tlWidth, ref lowBound_, 0.0f, MaxTime, "", Skin.TimelineSliderLow)) {
        LowBound = lowBound_;
        OnDrag?.Invoke(CurrentTime);
      }
      y += Gui.OffsetY;

      const float buttonOffset = 20.0f;
      float barCenter = Screen.width / 2.0f;

      x = barCenter;
      gui.Line(x, y, 1.0f, Gui.IconSize, Skin.SeparatorColor);

      x = barCenter;
      x -= Gui.IconSize + Gui.OffsetSmall;
      GuiLeftBar(gui, ref x, ref y, buttonOffset);

      x = barCenter;
      x += Gui.OffsetSmall;
      GuiRightBar(gui, ref x, ref y, buttonOffset);
    }

    private void GuiLeftBar(Gui gui, ref float x, ref float y, float offset) {
      //right to left direction
      x -= offset;

      if (gui.ImageButton(ref x, ref y, Skin.IconSpeedRight)) {
        IsPlaying = true;
        if (Speed < 0.0f) {
          Speed = 1.0f;
        }

        Speed *= 2.0f;
        Slow = 1;
        if (Speed > MaxSpeed) {
          Speed = MaxSpeed;
        }
        OnPlay?.Invoke(IsPlaying);
      }
      x -= Gui.IconSize + offset;
      y -= Gui.IconSize + Gui.OffsetY;

      if (gui.ImageButton(ref x, ref y, IsPlaying ? Skin.IconPlayPauseActive : Skin.IconPlayPause)) {
        IsPlaying = !IsPlaying;
        Speed = 1.0f;
        if (IsPlaying) {
          IsKeyframeEditing = false;
        }
        OnPlay?.Invoke(IsPlaying);
      }
      x -= Gui.IconSize + offset;
      y -= Gui.IconSize + Gui.OffsetY;

      if (gui.ImageButton(ref x, ref y, Skin.IconStop)) {
        IsPlaying = false;
        IsKeyframeEditing = false;

        CurrentTime = LowBound;
        OnStop?.Invoke(CurrentTime);
      }
      x -= Gui.IconSize + offset;
      y -= Gui.IconSize + Gui.OffsetY;

      if (gui.ImageButton(ref x, ref y, Skin.IconSpeedLeft)) {
        IsPlaying = true;
        if (Speed > 0.0f) {
          Speed = -1.0f;
        }

        Speed *= 2.0f;
        Slow = 1;
        if (Speed < -MaxSpeed) {
          Speed = -MaxSpeed;
        }
        OnPlay?.Invoke(IsPlaying);
      }
      x -= Gui.IconSize + offset;
      y -= Gui.IconSize + Gui.OffsetY;
    }

    private void GuiRightBar(Gui gui, ref float x, ref float y, float offset) {
      x += offset;

      const float size = 30.0f;
      const float boxOffset = 5.0f;
      const float boxWidth = size * 3.0f + boxOffset * 4.0f;

      gui.Box(x, y, boxWidth, Gui.IconSize, Skin.MainContainer);
      y += boxOffset;
      x += boxOffset;

      if (gui.ImageButton(ref x, ref y, size, size, Skin.IconMinus)) {
        Speed = 1.0f;
        Slow *= 2;
        if (Slow > MaxSlow) {
          Slow = MaxSlow;
        }
      }
      x += size + boxOffset;
      y -= size + Gui.OffsetY;

      gui.Box(x, y, size, size, $"x{Slow:D}", Skin.MainContainerDark);
      x += size + boxOffset;

      if (gui.ImageButton(ref x, ref y, size, size, Skin.IconPlus)) {
        Speed = 1.0f;
        Slow /= 2;
        if (Slow < 1) {
          Slow = 1;
        }
      }
      x += size + boxOffset;
      y -= size + Gui.OffsetY;

      x += offset + boxOffset;
      y -= boxOffset;

      if (gui.ImageButton(ref x, ref y, Loop ? Skin.IconLoopActive : Skin.IconLoop)) {
        Loop = !Loop;
      }
      x += Gui.IconSize + offset;
      y -= Gui.IconSize + Gui.OffsetY;

      if (gui.ImageButton(ref x, ref y, Skin.IconKeyframe)) {
        OnKeyframe?.Invoke(CurrentTime);
      }
      x += Gui.IconSize + offset;
      y -= Gui.IconSize + Gui.OffsetY;

      if (gui.ImageButton(ref x, ref y, IsKeyframeEditing ? Skin.IconGearActive : Skin.IconGear)) {
        IsKeyframeEditing = !IsKeyframeEditing;
        OnKeyframeEdit?.Invoke(IsKeyframeEditing);

        if (IsKeyframeEditing) {
          IsPlaying = false;
        }
      }
      x += Gui.IconSize + offset;
      y -= Gui.IconSize + Gui.OffsetY;
    }

    public void Update() {
      if (drag_) {
        if (Input.GetKeyUp(KeyCode.Mouse0)) {
          drag_ = false;
        }
      }

      if (CurrentTime < LowBound) {
        CurrentTime = LowBound;
      }
      else if (CurrentTime > HighBound) {
        CurrentTime = HighBound;
      }

      if (IsPlaying && !drag_) {
        CurrentTime += Time.deltaTime * (Slow != 1 ? 1.0f / Slow : Speed);
        if (CurrentTime > HighBound) {
          CurrentTime = HighBound;
          IsPlaying = Loop;
          if (IsPlaying) {
            CurrentTime = LowBound;
          }
        }
        if (CurrentTime < LowBound) {
          CurrentTime = LowBound;
          IsPlaying = Loop;
          if (IsPlaying) {
            CurrentTime = HighBound;
          }
        }
      }
    }
  }
}