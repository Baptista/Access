#if ANDROID
using Android.Media.Projection;
using Android.Util;

namespace MauiApp3
{
	
		public class ScreenCaptureCallback : MediaProjection.Callback
		{
			private readonly Action _onStopCallback;

			public ScreenCaptureCallback(Action onStopCallback)
			{
				_onStopCallback = onStopCallback;
			}

			public override void OnStop()
			{
				base.OnStop();
				Log.Debug("ScreenCaptureCallback", "Captura de tela interrompida.");

				// Chamar o callback de parada para liberar recursos
				_onStopCallback?.Invoke();
			}
		}
	
}
#endif