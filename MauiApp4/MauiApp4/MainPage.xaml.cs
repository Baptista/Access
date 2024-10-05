using System.Net.Sockets;
using System.Text;

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
				await _client.ConnectAsync("192.168.1.117", 5566); // Substitua pelo IP e porta do servidor

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
        
        private async void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (_networkStream != null && _networkStream.CanWrite)
            {
                try
                {
                    // Enviar evento MOVE (movimento do mouse/toque)
                    string message = $"MOVE {e.TotalX} {e.TotalY}";
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    await _networkStream.WriteAsync(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    StatusLabel.Text = $"Erro ao enviar comando: {ex.Message}";
                }
            }
        }

        // Capturar eventos de clique (tap) e enviar para o Android
        private async void OnTapped(object sender, EventArgs e)
        {
            if (_networkStream != null && _networkStream.CanWrite)
            {
                try
                {
                    // Obter as coordenadas do toque em relação à imagem
                    var image = (Image)sender;
                    var touchPosition = GetTouchPosition(image, (TappedEventArgs)e);

                    // Enviar a posição X e Y do clique
                    string message = $"CLICK {touchPosition.X} {touchPosition.Y}";
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    await _networkStream.WriteAsync(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    StatusLabel.Text = $"Erro ao enviar comando: {ex.Message}";
                }
            }
        }

        // Função para calcular a posição do toque em relação ao controle Image
        private Point GetTouchPosition(Image image, TappedEventArgs e)
        {
            // Pegar as dimensões da Image
            var imageWidth = image.Width;
            var imageHeight = image.Height;

            // Posição absoluta do toque na página (usando coordenação relativa ao controle)
            var touchPosition = e.GetPosition(image);

            // Calcular a posição do clique relativa ao tamanho da imagem
            var relativeX = touchPosition.Value.X / imageWidth;
            var relativeY = touchPosition.Value.Y / imageHeight;

            // Retornar a posição em relação ao tamanho da Image
            return new Point(relativeX * imageWidth, relativeY * imageHeight);
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
