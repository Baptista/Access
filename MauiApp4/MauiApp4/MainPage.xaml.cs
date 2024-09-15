using System.Net.Sockets;

namespace MauiApp4
{
	public partial class MainPage : ContentPage
	{
		private TcpClient _client;
		private NetworkStream _networkStream;
		private bool _isConnected;

		public MainPage()
		{
			InitializeComponent();
		}

		private async void ConnectButton_Clicked(object sender, EventArgs e)
		{
			await ConnectToServerAsync();
		}

		// Conectar ao servidor TCP
		private async Task ConnectToServerAsync()
		{
			try
			{
				StatusLabel.Text = "Status: Connecting...";

				// Conectar ao servidor
				_client = new TcpClient();
				await _client.ConnectAsync("10.148.229.125", 5566); // Substitua pelo IP e porta do servidor

				_networkStream = _client.GetStream();
				_isConnected = true;
				StatusLabel.Text = "Status: Connected";

				// Iniciar recebimento de imagens
				await Task.Run(ReceiveImagesAsync);
			}
			catch (Exception ex)
			{
				StatusLabel.Text = $"Erro ao conectar: {ex.Message}";
			}
		}

		// Receber e processar imagens do servidor
		private async Task ReceiveImagesAsync()
		{
			try
			{
				byte[] buffer = new byte[1024 * 1024 * 4]; // Buffer de 4 MB para imagens

				while (_isConnected)
				{
					// Ler o tamanho da imagem (4 bytes)
					byte[] sizeBuffer = new byte[4];
					int bytesRead = await _networkStream.ReadAsync(sizeBuffer, 0, sizeBuffer.Length);

					if (bytesRead == 0)
					{
						_isConnected = false;
						StatusLabel.Text = "Status: Disconnected";
						break;
					}

					int imageSize = BitConverter.ToInt32(sizeBuffer, 0);

					// Ler a imagem inteira
					int totalBytesRead = 0;
					MemoryStream imageStream = new MemoryStream();

					while (totalBytesRead < imageSize)
					{
						int remainingBytes = imageSize - totalBytesRead;
						bytesRead = await _networkStream.ReadAsync(buffer, 0, Math.Min(remainingBytes, buffer.Length));
						if (bytesRead == 0)
						{
							_isConnected = false;
							StatusLabel.Text = "Status: Disconnected";
							break;
						}

						imageStream.Write(buffer, 0, bytesRead);
						totalBytesRead += bytesRead;
					}

					// Exibir a imagem na interface
					if (totalBytesRead == imageSize)
					{
						byte[] imageData = imageStream.ToArray();
						DisplayImage(imageData);
					}
				}
			}
			catch (Exception ex)
			{
				StatusLabel.Text = $"Erro ao receber imagem: {ex.Message}";
			}
		}

		// Exibir a imagem recebida na interface
		private void DisplayImage(byte[] imageData)
		{
			MainThread.BeginInvokeOnMainThread(() =>
			{
				try
				{
					ScreenImage.Source = ImageSource.FromStream(() => new MemoryStream(imageData));
				}
				catch (Exception ex)
				{
					StatusLabel.Text = $"Erro ao exibir imagem: {ex.Message}";
				}
			});
		}

		// Limpar recursos quando a página for destruída
		protected override void OnDisappearing()
		{
			base.OnDisappearing();

			_isConnected = false;
			_networkStream?.Close();
			_client?.Close();
		}
	}

}
