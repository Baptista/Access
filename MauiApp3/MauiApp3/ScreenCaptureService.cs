#if ANDROID 
using Android.App; using Android.Content; using Android.OS; using AndroidX.Core.App;

namespace MauiApp3
{
	[Service(Name = "com.example.remotescreenserver.ScreenCaptureService", Exported = false, ForegroundServiceType = Android.Content.PM.ForegroundService.TypeMediaProjection)]
	public class ScreenCaptureService : Service
	{
		public const int SERVICE_ID = 1;
		private const string CHANNEL_ID = "media_projection_channel";

		public override IBinder OnBind(Intent intent) => null;

		public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
		{
			// Iniciar o serviço em primeiro plano com uma notificação
			StartForeground(SERVICE_ID, CreateNotification());

			// Continue executando o serviço
			return StartCommandResult.Sticky;
		}

		private Notification CreateNotification()
		{
			// Criar o canal de notificação (obrigatório para Android 8.0 e superior)
			var notificationManager = (NotificationManager)GetSystemService(NotificationService);

			if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
			{
				// Certifique-se de que o canal de notificação é criado corretamente
				var channel = new NotificationChannel(CHANNEL_ID, "Media Projection", NotificationImportance.Default)
				{
					Description = "Notificação para captura de tela em andamento"
				};

				notificationManager.CreateNotificationChannel(channel);
			}

			// Criar a notificação que manterá o serviço em execução
			var notificationBuilder = new NotificationCompat.Builder(this, CHANNEL_ID)
				.SetContentTitle("Espelhamento de Tela") // Título da notificação
				.SetContentText("Capturando a tela do dispositivo.") // Texto da notificação
				.SetSmallIcon(Resource.Drawable.ic_call_decline) // Ícone pequeno
				.SetOngoing(true) // Torna a notificação contínua (não pode ser removida pelo usuário)
				.SetPriority(NotificationCompat.PriorityHigh); // Define a prioridade como alta

			return notificationBuilder.Build();
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			// Pare a captura de tela ou outros processos se necessário
		}
	}
}
#endif