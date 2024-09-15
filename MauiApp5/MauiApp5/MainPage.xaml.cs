#if WINDOWS
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;

namespace MauiApp5
{
	public partial class MainPage : ContentPage
	{
		private Process scrcpyProcess;

		public MainPage()
		{
			InitializeComponent();
		}

		private void StartButton_Clicked(object sender, EventArgs e)
		{
			StartScrcpy();
		}

		private void StopButton_Clicked(object sender, EventArgs e)
		{
			StopScrcpy();
		}

		private void StartScrcpy()
		{
			try
			{
				scrcpyProcess = new Process();
				scrcpyProcess.StartInfo.FileName = "C:\\Users\\bruno\\Downloads\\scrcpy-win64-v2.6.1\\scrcpy-win64-v2.6.1\\scrcpy.exe"; // Caminho para o executável scrcpy
				scrcpyProcess.StartInfo.Arguments = "--tcpip=192.168.1.117:33631"; // Argumentos do scrcpy, como usar TCP/IP
				scrcpyProcess.StartInfo.UseShellExecute = false;
				scrcpyProcess.StartInfo.RedirectStandardOutput = true;
				scrcpyProcess.StartInfo.RedirectStandardError = true;
				scrcpyProcess.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
				scrcpyProcess.ErrorDataReceived += (s, e) => Console.WriteLine("ERRO: " + e.Data);

				scrcpyProcess.Start();
				scrcpyProcess.BeginOutputReadLine();
				scrcpyProcess.BeginErrorReadLine();

				StatusLabel.Text = "Status: Espelhamento Iniciado";
			}
			catch (Exception ex)
			{
				StatusLabel.Text = $"Erro ao iniciar o scrcpy: {ex.Message}";
			}
		}

		private void StopScrcpy()
		{
			try
			{
				if (scrcpyProcess != null && !scrcpyProcess.HasExited)
				{
					scrcpyProcess.Kill();
					scrcpyProcess.Dispose();
					StatusLabel.Text = "Status: Espelhamento Parado";
				}
			}
			catch (Exception ex)
			{
				StatusLabel.Text = $"Erro ao parar o scrcpy: {ex.Message}";
			}
		}
	}

}
#endif