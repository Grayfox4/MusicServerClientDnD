using System;
using System.Windows.Forms;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace MusicServerClientDnD
{
    public class HostForm : Form
    {
        private Label infoLabel;
        private TextBox passwordBox;
        private Button startButton;
        private ListBox audioSourcesList;
        private Button backButton;
        private Label ipLabel;
        private Label publicIpLabel;
        private TcpListener? server;
        private CancellationTokenSource? cts;
        private List<string> audioSources;
        private MMDeviceEnumerator deviceEnumerator;
        private List<MMDevice> devices;
        private WasapiLoopbackCapture? capture;
        private List<TcpClient> clients = new();
        private string? encryptionKey;

        public HostForm()
        {
            this.Text = "Host - MusicServerClientDnD";
            this.Size = new Size(500, 500);
            this.MinimumSize = new Size(400, 400);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.AutoScroll = true;

            infoLabel = new Label() { Text = "Set a password and select an audio source to stream:\n(For per-app capture, use a tool like Virtual Audio Cable)", ForeColor = Color.White, Dock = DockStyle.Top, Height = 60, TextAlign = ContentAlignment.MiddleCenter };
            passwordBox = new TextBox() { PlaceholderText = "Password", UseSystemPasswordChar = true, Width = 200, Top = 70, Left = 100 };
            audioSourcesList = new ListBox() { Top = 120, Left = 50, Width = 300, Height = 120 };
            startButton = new Button() { Text = "Start Hosting", Top = 260, Left = 120, Width = 150, Height = 40, BackColor = Color.FromArgb(60, 120, 200), ForeColor = Color.White };
            backButton = new Button() { Text = "Back", Top = 320, Left = 10, Width = 80, Height = 30 };
            ipLabel = new Label() { Text = "Your Local IP: " + GetLocalIPAddress(), ForeColor = Color.LightGray, Top = 370, Left = 10, Width = 380, Height = 20 };
            publicIpLabel = new Label() { Text = "Your Public IP: Retrieving...", ForeColor = Color.LightGray, Top = 390, Left = 10, Width = 380, Height = 20 };

            startButton.Click += StartButton_Click;
            backButton.Click += (s, e) => { StopServer(); this.Close(); };

            this.Controls.Add(infoLabel);
            this.Controls.Add(passwordBox);
            this.Controls.Add(audioSourcesList);
            this.Controls.Add(startButton);
            this.Controls.Add(backButton);
            this.Controls.Add(ipLabel);
            this.Controls.Add(publicIpLabel);

            // List output devices (default and others)
            deviceEnumerator = new MMDeviceEnumerator();
            devices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToList();
            audioSourcesList.Items.Clear();
            foreach (var dev in devices) audioSourcesList.Items.Add(dev.FriendlyName);
            if (audioSourcesList.Items.Count == 0)
                audioSourcesList.Items.Add("No output devices found");

            _ = SetPublicIpAsync();
        }

        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            }
            return "127.0.0.1";
        }

        private async void StartButton_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(passwordBox.Text))
            {
                MessageBox.Show("Please enter a password.");
                return;
            }
            if (audioSourcesList.SelectedIndex < 0)
            {
                MessageBox.Show("Please select an output device.");
                return;
            }
            startButton.Enabled = false;
            encryptionKey = passwordBox.Text;
            cts = new CancellationTokenSource();
            server = new TcpListener(IPAddress.Any, 5000);
            server.Start();
            MessageBox.Show($"Hosting started on {GetLocalIPAddress()}:5000\nWaiting for clients...");
            StartAudioCapture(devices[audioSourcesList.SelectedIndex]);
            await AcceptClientsAsync(encryptionKey, cts.Token);
        }

        private void StartAudioCapture(MMDevice device)
        {
            // Fallback: Start loopback capture of selected output device
            // StartAudioCapture(devices[audioSourcesList.SelectedIndex]);
        }

        private byte[] EncryptAudio(byte[] data, string password)
        {
            using var aes = Aes.Create();
            var key = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            aes.Key = key;
            aes.IV = new byte[16]; // Simple IV for demo
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.FlushFinalBlock();
            return ms.ToArray();
        }

        private async Task AcceptClientsAsync(string password, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var client = await server!.AcceptTcpClientAsync(token);
                    lock (clients) clients.Add(client);
                }
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                    MessageBox.Show($"Server error: {ex.Message}");
            }
        }

        private void StopServer()
        {
            cts?.Cancel();
            server?.Stop();
            capture?.StopRecording();
            lock (clients) foreach (var c in clients) c.Close();
            clients.Clear();
        }

        private async Task SetPublicIpAsync()
        {
            try
            {
                using var client = new HttpClient();
                string ip = await client.GetStringAsync("https://api.ipify.org");
                publicIpLabel.Invoke(() => publicIpLabel.Text = $"Your Public IP: {ip.Trim()}");
            }
            catch
            {
                publicIpLabel.Invoke(() => publicIpLabel.Text = "Your Public IP: (unavailable)");
            }
        }
    }

    public class ClientForm : Form
    {
        private Label infoLabel;
        private TextBox ipBox;
        private TextBox passwordBox;
        private Button connectButton;
        private TrackBar volumeSlider;
        private Button backButton;
        private Label statusLabel;
        private TcpClient? client;
        private CancellationTokenSource? cts;
        private WaveOutEvent? waveOut;
        private BufferedWaveProvider? buffer;
        private string? encryptionKey;

        public ClientForm()
        {
            this.Text = "Client - MusicServerClientDnD";
            this.Size = new Size(500, 400);
            this.MinimumSize = new Size(400, 300);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.AutoScroll = true;

            infoLabel = new Label() { Text = "Connect to a host:", ForeColor = Color.White, Dock = DockStyle.Top, Height = 40, TextAlign = ContentAlignment.MiddleCenter };
            ipBox = new TextBox() { PlaceholderText = "Host IP", Width = 200, Top = 50, Left = 100 };
            passwordBox = new TextBox() { PlaceholderText = "Password", UseSystemPasswordChar = true, Width = 200, Top = 90, Left = 100 };
            connectButton = new Button() { Text = "Connect", Top = 130, Left = 120, Width = 150, Height = 40, BackColor = Color.FromArgb(60, 200, 120), ForeColor = Color.White };
            volumeSlider = new TrackBar() { Top = 200, Left = 50, Width = 300, Minimum = 0, Maximum = 100, Value = 50 };
            backButton = new Button() { Text = "Back", Top = 260, Left = 10, Width = 80, Height = 30 };
            statusLabel = new Label() { Text = "", ForeColor = Color.LightGray, Top = 180, Left = 50, Width = 300, Height = 20 };

            connectButton.Click += ConnectButton_Click;
            backButton.Click += (s, e) => { cts?.Cancel(); client?.Close(); this.Close(); };
            volumeSlider.Scroll += (s, e) => { if (waveOut != null) waveOut.Volume = volumeSlider.Value / 100f; };

            this.Controls.Add(infoLabel);
            this.Controls.Add(ipBox);
            this.Controls.Add(passwordBox);
            this.Controls.Add(connectButton);
            this.Controls.Add(volumeSlider);
            this.Controls.Add(backButton);
            this.Controls.Add(statusLabel);
        }

        private async void ConnectButton_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ipBox.Text) || string.IsNullOrWhiteSpace(passwordBox.Text))
            {
                MessageBox.Show("Please enter the host IP and password.");
                return;
            }
            connectButton.Enabled = false;
            statusLabel.Text = "Connecting...";
            cts = new CancellationTokenSource();
            try
            {
                client = new TcpClient();
                await client.ConnectAsync(ipBox.Text, 5000);
                statusLabel.Text = "Connected! Waiting for audio...";
                await ReceiveAudioAsync(client, passwordBox.Text, cts.Token);
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Error: {ex.Message}";
                connectButton.Enabled = true;
            }
        }

        private async Task ReceiveAudioAsync(TcpClient client, string password, CancellationToken token)
        {
            encryptionKey = password;
            waveOut = new WaveOutEvent();
            buffer = new BufferedWaveProvider(new WaveFormat(44100, 2));
            waveOut.Init(buffer);
            waveOut.Volume = volumeSlider.Value / 100f;
            waveOut.Play();
            using (client)
            {
                using var stream = client.GetStream();
                var audioBuffer = new byte[4096];
                while (!token.IsCancellationRequested)
                {
                    int read = await stream.ReadAsync(audioBuffer, 0, audioBuffer.Length, token);
                    if (read > 0)
                    {
                        byte[] decrypted = DecryptAudio(audioBuffer.Take(read).ToArray(), encryptionKey!);
                        buffer.AddSamples(decrypted, 0, decrypted.Length);
                    }
                }
            }
            connectButton.Enabled = true;
        }

        private byte[] DecryptAudio(byte[] data, string password)
        {
            using var aes = Aes.Create();
            var key = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            aes.Key = key;
            aes.IV = new byte[16];
            using var ms = new MemoryStream(data);
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var outMs = new MemoryStream();
            cs.CopyTo(outMs);
            return outMs.ToArray();
        }
    }
}
