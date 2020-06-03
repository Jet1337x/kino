using UnityEngine;

namespace KN_Core {
  public class Timeline {
    private const float MaxSpeed = 8.0f;
    private const int MaxSlow = 8;

    private string lowBoundString_ = "0.0";
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
        lowBoundString_ = $"{LowBound:F}";
        highBoundString_ = $"{highBound_:F}";
      }
    }

    private string highBoundString_ = "1.0";
    private float highBound_;
    public float HighBound {
      get => highBound_;
      set {
        if (value < LowBound) {
          highBound_ = LowBound;
        }
        else {
          var ghosts = core_.Replay?.Player.players;
          if (ghosts != null && ghosts.Count > 0) {
            highBound_ = value < MaxTime ? value : MaxTime;
          }
          else {
            highBound_ = value;
            MaxTime = value;
          }
          lowBoundString_ = $"{LowBound:F}";
          highBoundString_ = $"{highBound_:F}";
        }
      }
    }

    private float time_;
    public float CurrentTime {
      get => time_;
      set => time_ = value;
    }
    public float MaxTime { get; set; }
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
      float y = Screen.height - 100.0f;
      float x = 120.0f;
      const float boundsWidth = Gui.Width * 2.0f + Gui.OffsetSmall * 4.0f;
      float boxWidth = Screen.width - boundsWidth + 100.0f;
      const float boxHeight = 100.0f;
      float tlWidth = boxWidth - boundsWidth;

      float xBegin = x;

      gui.Box(x, y, boxWidth, boxHeight, Skin.MainContainer);

      x += boxWidth / 2.0f - Gui.Width / 2.0f;
      gui.Label(ref x, ref y, $"SPEED: {Speed:F}");
      y -= Gui.OffsetY;
      x = xBegin;

      y += Gui.OffsetY;
      x += Gui.OffsetSmall;


      if (gui.TextField(ref x, ref y, ref lowBoundString_, "LOW BOUND", 6, Config.FloatRegex)) {
        float.TryParse(lowBoundString_, out float value);
        LowBound = value;
        if (CurrentTime < LowBound) {
          CurrentTime = LowBound;
          if (core_.Replay.IsPlaying) {
            core_.Replay.Player.CurTimeOverride(CurrentTime);
          }
        }
      }
      y -= Gui.Height + Gui.OffsetY;
      x += Gui.Width + Gui.OffsetSmall;

      if (gui.SliderH(ref x, ref y, tlWidth, ref time_, LowBound, HighBound, $"TIME: {CurrentTime:F}  ")) {
        drag_ = true;
        OnDrag?.Invoke(CurrentTime);
      }
      y -= Gui.Height + Gui.OffsetY * 2.0f;
      x += tlWidth + Gui.OffsetSmall;

      if (gui.TextField(ref x, ref y, ref highBoundString_, "HIGH BOUND", 6, Config.FloatRegex)) {
        float.TryParse(highBoundString_, out float value);
        HighBound = value;
        if (CurrentTime > HighBound) {
          CurrentTime = HighBound - 0.001f;
          if (core_.Replay.IsPlaying) {
            core_.Replay.Player.CurTimeOverride(CurrentTime);
          }
        }
      }

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

      if (IsPlaying && !drag_) {
        CurrentTime += Time.deltaTime * (Slow != 1 ? 1.0f / Slow : Speed);
        if (CurrentTime > HighBound) {
          CurrentTime = HighBound;
          IsPlaying = Loop;
          if (IsPlaying) {
            CurrentTime = LowBound;
          }
          else {
            core_.Replay.Player.Pause();
          }
        }
        if (CurrentTime < LowBound) {
          CurrentTime = LowBound;
          IsPlaying = Loop;
          if (IsPlaying) {
            CurrentTime = HighBound;
          }
          else {
            core_.Replay.Player.Pause();
          }
        }
      }
    }
  }
}