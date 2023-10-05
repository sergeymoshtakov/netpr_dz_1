using System;
using System.Collections.Generic;
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

namespace NerProgramming
{
    /// <summary>
    /// Логика взаимодействия для ServerWindow.xaml
    /// </summary>
    public partial class ServerWindow : Window
    {
        private Socket listenSocket;
        private IPEndPoint endPoint;
        public ServerWindow()
        {
            InitializeComponent();
        }

        private void SwitchServer_Click(object sender, RoutedEventArgs e)
        {
            if(listenSocket == null)
            {
                try
                {
                    IPAddress ip = IPAddress.Parse(HostTextBox.Text);
                    int port = Convert.ToInt32(PortTextBox.Text);
                    endPoint = new IPEndPoint(ip, port);
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Wrong configuration parameters: "+ex.ToString());
                    return;
                }
                listenSocket = new Socket(
                        AddressFamily.InterNetwork,
                        SocketType.Stream,
                        ProtocolType.Tcp);
                new Thread(startServer).Start();    
            }
            else
            {
                listenSocket.Close();
            }
        }

        private void startServer()
        {
            if(listenSocket == null || endPoint == null) 
            {
                MessageBox.Show("Error");
                return;
            }
            else
            {
                try
                {
                    listenSocket.Bind(endPoint);
                    listenSocket.Listen(10);
                    Dispatcher.Invoke(() =>
                    {
                        ServerLog.Text += "Server started\n";
                        SwitchServer.Content = "Виключити";
                        StatusLabel.Content = "Включено";
                        StatusLabel.Background = Brushes.Green;
                    });
                    byte[] buffer = new byte[1024];
                    while(true)
                    {
                        Socket socket = listenSocket.Accept();
                        StringBuilder stringBuilder = new StringBuilder();
                        do
                        {
                            int n = socket.Receive(buffer);
                            stringBuilder.Append(Encoding.UTF8.GetString(buffer, 0, n));
                        } while (socket.Available > 0);
                        String str = stringBuilder.ToString();
                        Dispatcher.Invoke(() => ServerLog.Text += $"{DateTime.Now} {str}\n");
                    }
                }
                catch (Exception ex)
                {
                    listenSocket = null;
                    Dispatcher.Invoke(() =>
                    {
                        ServerLog.Text += "Served closed\n";
                        SwitchServer.Content = "Включити";
                        StatusLabel.Content = "Вимкнено";
                        StatusLabel.Background = Brushes.Pink;
                    });
                }
            }
        }

        private void Window_Closing(object sender, EventArgs e)
        {
            listenSocket?.Close();
        }
    }
}
