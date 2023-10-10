using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;
using System.Data;

namespace NerProgramming
{
    /// <summary>
    /// Логика взаимодействия для ClientWindow.xaml
    /// </summary>
    public partial class ClientWindow : Window
    {
        private Random random = new Random();
        private IPEndPoint endPoint;
        private DateTime lastSyncMoment;
        private bool isServerOn;
        public ClientWindow()
        {
            InitializeComponent();
            lastSyncMoment = default;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoginTextBox.Text = "User" + random.Next(100);
            MessageTextBox.Text = "Hello, all";
            isServerOn = true;
            Sync();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            String[] address = HostTextBox.Text.Split(':');
            try
            {
                endPoint = new IPEndPoint(
                    IPAddress.Parse(address[0]),
                    Convert.ToInt32(address[1]));

                new Thread(SendMessage).Start(
                    new ClientRequest
                    {
                        Command = "Message",
                        Message = new ChatMessage()
                        {
                            Login = LoginTextBox.Text,
                            Text = MessageTextBox.Text
                        }
                    }
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SendMessage(Object args)
        {
            var clientRequest = args as ClientRequest;
            if (endPoint == null || clientRequest == null)
            {
                return;
            }
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                clientSocket.Connect(endPoint);
                clientSocket.Send(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(clientRequest)));
                MemoryStream memoryStream = new MemoryStream();
                byte[] buffer = new byte[1024];
                do
                {
                    int n = clientSocket.Receive(buffer);
                    memoryStream.Write(buffer, 0, n);
                } while (clientSocket.Available > 1);
                String str = Encoding.UTF8.GetString(memoryStream.ToArray());
                ServerResponse response = null;
                try
                {
                    response = JsonSerializer.Deserialize<ServerResponse>(str);
                }
                catch { }
                if (str == null)
                {
                    str = "JSON error: " + str;
                    new Task(() => {
                        Dispatcher.Invoke(() =>
                        {
                            StatusLabel.Background = Brushes.Red;
                            StatusLabel.Content = "ERROR";

                        });
                        Thread.Sleep(3000);
                        Dispatcher.Invoke(() =>
                        {
                            StatusLabel.Background = Brushes.LightGray;
                            StatusLabel.Content = "OFF";
                        });
                    }).Start();
                }
                else
                {
                    str = response.Status;
                    if (response.Massages != null)
                    {
                        foreach(var massage in response.Massages)
                        {
                            str += " " + massage + " " + massage.getDate() + "\n";
                            if(massage.Moment > lastSyncMoment)
                            {
                                lastSyncMoment = massage.Moment;
                            }
                        }
                    }
                    new Task(() => {
                        Dispatcher.Invoke(() =>
                        {
                            StatusLabel.Background = Brushes.Green;
                            StatusLabel.Content = "ON";

                        });
                        Thread.Sleep(3000);
                        Dispatcher.Invoke(() =>
                        {
                            StatusLabel.Background = Brushes.LightGray;
                            StatusLabel.Content = "OFF";
                        });
                    }).Start();
                }
                Dispatcher.Invoke(() => ClientLog.Text += str + "\n");
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
            catch(Exception ex)
            {
                if (isServerOn)  // клієнт намагається підключитись тричі
                {  // і тричі виводить повідомлення. Реагуємо тільки на перше
                    isServerOn = false;
                    clientSocket.Dispose();
                    MessageBox.Show(ex.Message);
                    isServerOn = true;
                }

            }
        }

        private async void Sync()
        {
            if (isServerOn)
            {
                String[] address = HostTextBox.Text.Split(':');
                try
                {
                    endPoint = new IPEndPoint(
                        IPAddress.Parse(address[0]),
                        Convert.ToInt32(address[1]));

                    new Thread(SendMessage).Start(
                        new ClientRequest
                        {
                            Command = "Check",
                            Message = new ChatMessage()
                            {
                                Moment = lastSyncMoment
                            }
                        }
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            await Task.Delay(10000);
            Sync();
        }
    }
}
