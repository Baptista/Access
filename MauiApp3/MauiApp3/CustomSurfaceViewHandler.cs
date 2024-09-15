#if ANDROID
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Android.Views;
using Android.Content;

namespace MauiApp3
{
	// Definição do handler para o controle CustomSurfaceView
	public class CustomSurfaceViewHandler : ViewHandler<CustomSurfaceView, SurfaceView>
	{
		public CustomSurfaceViewHandler() : base(ViewHandler.ViewMapper)
		{
		}

		// Criação da visão de plataforma (SurfaceView)
		protected override SurfaceView CreatePlatformView()
		{
			// Obtenha o contexto Android
			var context = MauiApplication.Context;

			// Crie um novo SurfaceView Android
			var surfaceView = new SurfaceView(context);
			return surfaceView;
		}

		// Método opcional: pode conectar eventos e configurar a visualização de plataforma
		protected override void ConnectHandler(SurfaceView platformView)
		{
			base.ConnectHandler(platformView);
			// Conecte eventos aqui, se necessário
		}

		// Método opcional: pode desconectar eventos e liberar recursos
		protected override void DisconnectHandler(SurfaceView platformView)
		{
			base.DisconnectHandler(platformView);
			// Desconecte eventos e limpe recursos aqui, se necessário
		}
	}
}
#endif