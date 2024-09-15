using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace MauiApp3
{
	[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
	public class MainActivity : MauiAppCompatActivity
	{
		// Define um evento para permitir que outras partes do aplicativo ouçam o resultado da permissão
		public static event Action<int, Result, Intent> ActivityResult;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			// Dispara o evento quando o resultado é recebido
			ActivityResult?.Invoke(requestCode, resultCode, data);
		}
	}
}
