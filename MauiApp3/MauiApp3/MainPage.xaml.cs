#if ANDROID
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media.Projection;
using Android.Media;
using Android.Views;
using Android.Util;
using Android.Hardware.Display;
using static Microsoft.Maui.LifecycleEvents.AndroidLifecycle;
using Android.Runtime;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Android.Content.PM;
using Android.OS;
#endif
using Microsoft.Maui.Controls;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;



namespace MauiApp3
{
	public partial class MainPage : ContentPage
	{

#if ANDROID
		private MediaProjectionManager _projectionManager;
		private MediaProjection _mediaProjection;
		private VirtualDisplay _virtualDisplay;
		private ImageReader _imageReader;
		private const int PORT = 5566;
		private bool _isStreaming;
		private const int REQUEST_CODE_PERMISSIONS = 1001;
		
#endif
		private TcpListener _tcpListener;
		private Socket _clientSocket;

		public MainPage()
		{
			InitializeComponent();
#if ANDROID
			MainActivity.ActivityResult += OnActivityResult;
			RequestPermissions();
			
#endif
		}
#if ANDROID
		private void RequestPermissions()
		{
			string[] permissions = {
				"android.permission.FOREGROUND_SERVICE_MEDIA_PROJECTION",
				"android.permission.CAPTURE_VIDEO_OUTPUT"
			};

			// Verifique se todas as permissões necessárias foram concedidas
			foreach (var permission in permissions)
			{
				if (ContextCompat.CheckSelfPermission(Android.App.Application.Context, permission) != Permission.Granted)
				{
					ActivityCompat.RequestPermissions(Microsoft.Maui.ApplicationModel.Platform.CurrentActivity, permissions, REQUEST_CODE_PERMISSIONS);
					return;
				}
			}
		}
		private void StartButton_Clicked(object sender, EventArgs e)
		{
			StartServer();
		}

		private void StopButton_Clicked(object sender, EventArgs e)
		{
			StopServer();
		}

		private void StartServer()
		{
			try
			{
				System.Net.IPAddress ipaddress = System.Net.IPAddress.Parse("10.148.229.125");
				_tcpListener = new TcpListener(IPAddress.Any, PORT);
				_tcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				_tcpListener.Start();
				_tcpListener.BeginAcceptSocket(OnClientAccepted, null);

				// Iniciar o serviço em primeiro plano
				var serviceIntent = new Intent(Android.App.Application.Context, typeof(ScreenCaptureService));
				Microsoft.Maui.ApplicationModel.Platform.CurrentActivity.StartForegroundService(serviceIntent);

				_projectionManager = (MediaProjectionManager)Android.App.Application.Context.GetSystemService(Context.MediaProjectionService);
				var intent = _projectionManager.CreateScreenCaptureIntent();
				Microsoft.Maui.ApplicationModel.Platform.CurrentActivity.StartActivityForResult(intent, 1000);

				StatusLabel.Text = "Status: Aguardando permissão de captura de tela.";
			}
			catch (Exception ex)
			{
				StatusLabel.Text = $"Erro ao iniciar o servidor: {ex.Message}";
			}
		}

		private void StopServer()
		{
			try
			{
				_isStreaming = false;
				if (_virtualDisplay != null)
				{
					_virtualDisplay.Release();
					_virtualDisplay = null;
				}
				if (_mediaProjection != null)
				{
					_mediaProjection.Stop();
					_mediaProjection = null;
				}
				_clientSocket?.Close();
				_tcpListener?.Stop();

				// Parar o serviço em primeiro plano
				var serviceIntent = new Intent(Android.App.Application.Context, typeof(ScreenCaptureService));
				Microsoft.Maui.ApplicationModel.Platform.CurrentActivity.StopService(serviceIntent);

				StatusLabel.Text = "Status: Espelhamento Parado.";
			}
			catch (Exception ex)
			{
				StatusLabel.Text = $"Erro ao parar o servidor: {ex.Message}";
			}
		}

		private void OnClientAccepted(IAsyncResult ar)
		{
			_clientSocket = _tcpListener.EndAcceptSocket(ar);
			//StartScreenCapture();
		}

		private async  void StartScreenCapture()
		{
			
			if (_virtualDisplay != null)
			{
				_virtualDisplay.Release();
				_virtualDisplay = null;
			}

			var platformSurfaceView = (SurfaceView)SurfaceView?.Handler?.PlatformView;

			if (platformSurfaceView?.Holder?.Surface != null && platformSurfaceView.Holder.Surface.IsValid)
			{
				var surface = platformSurfaceView.Holder.Surface;

				var displayManager = (DisplayManager)Android.App.Application.Context.GetSystemService(Context.DisplayService);
				Display display = displayManager.GetDisplay(Display.DefaultDisplay); // Obter o display padrão

				if (display != null)
				{
					var windowManager = Android.App.Application.Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();

					// Instanciar DisplayMetrics para armazenar as dimensões da tela
					DisplayMetrics metrics = new DisplayMetrics();
					windowManager.DefaultDisplay.GetMetrics(metrics);

					int width = metrics.WidthPixels;
					int height = metrics.HeightPixels;
					int density = (int)metrics.DensityDpi;

					int minDimension = Math.Min(width, height);
					width = minDimension;
					height = minDimension;

					var callback = new ScreenCaptureCallback(StopServer);
					_mediaProjection.RegisterCallback(callback, null);
										
					// Use o SurfaceView como o destino para o VirtualDisplay
					_virtualDisplay = _mediaProjection.CreateVirtualDisplay(
						"ScreenCapture",
						width,
						height,
						density,
						DisplayFlags.Presentation,
						surface,
						null,
						null
					);

					_isStreaming = true;

					StatusLabel.Text = "Status: Capturando a tela.";

					//await Task.Run(async () => await CaptureAndSendSurfaceImage());
					await Task.Run(async () => await CaptureAndSendSurfaceViewContent(platformSurfaceView));

				}
				else
				{
					StatusLabel.Text = "Erro: Display padrão não encontrado.";
				}
			}
			else
			{
				StatusLabel.Text = "Erro: SurfaceView inválido.";
			}
		}
		private void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			try
			{
				if (requestCode == 1000 && resultCode == Result.Ok)
				{
					_mediaProjection = _projectionManager.GetMediaProjection((int)resultCode, data);
					StatusLabel.Text = "Permissão de captura de tela concedida.";
					StartScreenCapture();
				}
				else
				{
					StatusLabel.Text = "Permissão de captura de tela negada.";
				}
			}catch(Exception ex)
			{
				StatusLabel.Text = ex.Message;
			}
		}

		private async Task CaptureAndSendSurfaceImage()
		{
			try
			{
				while (_isStreaming)
				{
					// Captura a imagem diretamente da SurfaceView
					Bitmap bitmap = GetBitmapFromSurfaceView((SurfaceView)SurfaceView?.Handler?.PlatformView);
					if (bitmap != null)
					{
						using (var stream = new MemoryStream())
						{
							bitmap.Compress(Bitmap.CompressFormat.Png, 100, stream);
							var imageData = stream.ToArray();

							// Enviar os dados da imagem via TCP
							await SendImageData(imageData);
						}
					}

					await Task.Delay(100);  // Delay para evitar captura muito rápida
				}
			}
			catch (Exception ex)
			{
				StatusLabel.Text = $"Erro ao capturar ou enviar imagem: {ex.Message}";
			}
		}

		// Função para capturar Bitmap do SurfaceView
		private Bitmap GetBitmapFromSurfaceView(SurfaceView surfaceView)
		{
			try
			{
				Bitmap bitmap = Bitmap.CreateBitmap(surfaceView.Width, surfaceView.Height, Bitmap.Config.Argb8888);
				Canvas canvas = new Canvas(bitmap);
				surfaceView.Draw(canvas);
				return bitmap;
			}
			catch (Exception ex)
			{
				StatusLabel.Text = $"Erro ao capturar imagem: {ex.Message}";
				return null;
			}
		}

		private async Task SendImageData(byte[] imageData)
		{
			try
			{
				if (_clientSocket != null && _clientSocket.Connected)
				{
					// Enviar o tamanho da imagem (4 bytes)
					var sizeBuffer = BitConverter.GetBytes(imageData.Length);
					await _clientSocket.SendAsync(sizeBuffer, SocketFlags.None);

					// Enviar os dados da imagem
					await _clientSocket.SendAsync(imageData, SocketFlags.None);
				}
				else
				{
					StatusLabel.Text = "Erro: Cliente desconectado.";
				}
			}
			catch (Exception ex)
			{
				StatusLabel.Text = $"Erro ao enviar imagem: {ex.Message}";
			}
		}

		private async Task CaptureAndSendSurfaceViewContent(SurfaceView surfaceView)
		{
			try
			{
				while (true)
				{
					// Criar um bitmap para receber o conteúdo da captura
					var bitmap = Bitmap.CreateBitmap(surfaceView.Width, surfaceView.Height, Bitmap.Config.Argb8888);
					MainThread.BeginInvokeOnMainThread(() =>
					{
						// Usar PixelCopy para capturar o conteúdo do SurfaceView
						PixelCopy.Request(surfaceView, bitmap, new PixelCopyFinishedListener(bitmap,this), new Handler());
					});
					await Task.Delay(100);  // Aguarde um curto período antes de capturar novamente
				}
			}
			catch (Exception ex)
			{
				StatusLabel.Text = $"Erro ao capturar imagem: {ex.Message}";
			}
		}
		private class PixelCopyFinishedListener : Java.Lang.Object, PixelCopy.IOnPixelCopyFinishedListener
		{
			private readonly Bitmap _bitmap;
			private readonly MainPage _mainPage;

			public PixelCopyFinishedListener(Bitmap bitmap, MainPage mainPage)
			{
				_bitmap = bitmap;
				_mainPage = mainPage;
			}

			public void OnPixelCopyFinished(int copyResult)
			{
				if (copyResult == (int)PixelCopyResult.Success)
				{
					// Converter o Bitmap em bytes
					using (var stream = new MemoryStream())
					{
						_bitmap.Compress(Bitmap.CompressFormat.Png, 100, stream);
						var imageData = stream.ToArray();

						// Enviar os bytes da imagem para o cliente via TCP
						Task.Run(() => _mainPage.SendImageData(imageData));
					}
				}
				else
				{
					// Tratar erros de cópia
					MainThread.BeginInvokeOnMainThread(() =>
					{
						_mainPage.StatusLabel.Text = "Erro ao copiar conteúdo do SurfaceView.";
					});
				}
			}
		}
#endif

	}

}
