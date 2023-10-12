using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
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

namespace NerProgramming
{
    /// <summary>
    /// Логика взаимодействия для EmailWindow.xaml
    /// </summary>
    public partial class EmailWindow : Window
    {
        public EmailWindow()
        {
            InitializeComponent();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            #region get and check config
            String host = App.getConfiguration("smtp:host");
            if(host == null)
            {
                MessageBox.Show("Error getting host");
                return;
            }
            String portString = App.getConfiguration("smtp:port");
            if (portString == null)
            {
                MessageBox.Show("Error getting port");
                return;
            }
            int port = 0;
            try
            {
                port = int.Parse(portString);
            }
            catch
            {
                MessageBox.Show("Error parsing");
            }
            String email = App.getConfiguration("smtp:email");
            if (email == null)
            {
                MessageBox.Show("Error getting email");
                return;
            }
            String password = App.getConfiguration("smtp:password");
            if (password == null)
            {
                MessageBox.Show("Error getting password");
                return;
            }
            String sslString = App.getConfiguration("smtp:ssl");
            if (sslString == null)
            {
                MessageBox.Show("Error getting ssl");
                return;
            }
            bool ssl;
            try { ssl = bool.Parse(sslString); }
            catch
            {
                MessageBox.Show("Error parsing ssl");
                return;
            }
            #endregion
            if (!textBoxTo.Text.Contains("@"))
            {
                MessageBox.Show("Enter right adress");
                return;
            }
            SmtpClient smtpClient = new SmtpClient(host, port)
            {
                EnableSsl = ssl,
                Credentials = new NetworkCredential(email, password)
            };
            smtpClient.Send(email, textBoxTo.Text, textBoxThema.Text, textBoxMessage.Text);
            MessageBox.Show("Sent!");
        }
    }
}
