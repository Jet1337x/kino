namespace KN_Loader {
  public interface ICore {
      void OnInit();
      void OnDeinit();

      void FixedUpdate();
      void Update();
      void LateUpdate();

      void OnGui();
  }
}