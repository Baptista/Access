#if ANDROID 
using Android.AccessibilityServices;
using Android.Graphics;
using Android.Views.Accessibility;
using Android.App;


namespace MauiApp3
{
    [Service(Label = "Your Accessibility Service", Permission = "android.permission.BIND_ACCESSIBILITY_SERVICE")]
    [MetaData("android.accessibilityservice", Resource = "@xml/accessibility_service_config")]
    public class YourAccessibilityService : AccessibilityService
    {
        public override void OnAccessibilityEvent(AccessibilityEvent e) { }

        public override void OnInterrupt() { }

        // Simulate a click using Accessibility
        public void SimulateClick(float x, float y)
        {
            GestureDescription.Builder gestureBuilder = new GestureDescription.Builder();
            Android.Graphics.Path path = new Android.Graphics.Path();
            path.MoveTo(x, y);
            gestureBuilder.AddStroke(new GestureDescription.StrokeDescription(path, 0, 100));
            GestureDescription gesture = gestureBuilder.Build();
            DispatchGesture(gesture, null, null);
        }
    }
}
#endif