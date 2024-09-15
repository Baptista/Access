using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TcpServerApp
{
	public partial class MainPage : ContentPage
	{
		private TcpListener? _tcpListener;
		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		public MainPage()
		{
			InitializeComponent();
		}

		private async void StartServerButton_Clicked(object sender, EventArgs e)
		{
			await StartTcpServerAsync();
		}

		private async Task StartTcpServerAsync()
		{
			try
			{
				// Inicialize o TcpListener na porta 5555
				_tcpListener = new TcpListener(IPAddress.Any, 5555);
				_tcpListener.Start();

				StatusLabel.Text = "Servidor TCP iniciado na porta 5555.";

				while (!_cancellationTokenSource.Token.IsCancellationRequested)
				{
					// Aguarde por um cliente conectar
					TcpClient client = await _tcpListener.AcceptTcpClientAsync();
					StatusLabel.Text = "Cliente conectado!";

					_ = HandleClientAsync(client);
				}
			}
			catch (Exception ex)
			{
				StatusLabel.Text = $"Erro: {ex.Message}";
			}
		}

		private async Task HandleClientAsync(TcpClient client)
		{
			using (NetworkStream stream = client.GetStream())
			{
				byte[] buffer = new byte[1024];
				int bytesRead;

				while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
				{
					string receivedData = Encoding.ASCII.GetString(buffer, 0, bytesRead);
					StatusLabel.Text = $"Recebido: {receivedData}";

					// Responda ao cliente
					string response = "Resposta do servidor: " + receivedData;
					byte[] responseBytes = Encoding.ASCII.GetBytes(response);
					await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
				}
			}

			StatusLabel.Text = "Cliente desconectado.";
		}

		private void StopServerButton_Clicked(object sender, EventArgs e)
		{
			StopTcpServer();
		}

		private void StopTcpServer()
		{
			_cancellationTokenSource.Cancel();
			_tcpListener?.Stop();
			StatusLabel.Text = "Servidor TCP parado.";
		}
	}
}