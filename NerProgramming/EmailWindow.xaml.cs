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
using System.Net.Mime;

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
            int randomCode = GenerateRandomCode();
            string textBoxText = $"<h2>Добрий день!</h2> Ваш код <b style='font-weight:bold'>{randomCode}</b> для підтвердження";
            textBoxApprove.Text = textBoxText;
        }
        private SmtpClient getSmtp()
        {
            #region get and check config
            String host = App.getConfiguration("smtp:host");
            if (host == null)
            {
                MessageBox.Show("Error getting host");
                return null;
            }
            String portString = App.getConfiguration("smtp:port");
            if (portString == null)
            {
                MessageBox.Show("Error getting port");
                return null;
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
                return null;
            }
            String password = App.getConfiguration("smtp:password");
            if (password == null)
            {
                MessageBox.Show("Error getting password");
                return null;
            }
            String sslString = App.getConfiguration("smtp:ssl");
            if (sslString == null)
            {
                MessageBox.Show("Error getting ssl");
                return null;
            }
            bool ssl;
            try { ssl = bool.Parse(sslString); }
            catch
            {
                MessageBox.Show("Error parsing ssl");
                return null;
            }
            #endregion
            if (!textBoxTo.Text.Contains("@"))
            {
                MessageBox.Show("Enter right adress");
                return null;
            }
            SmtpClient smtpClient = new SmtpClient(host, port)
            {
                EnableSsl = ssl,
                Credentials = new NetworkCredential(email, password)
            };
            return smtpClient;
        }
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SmtpClient smtpClient = getSmtp();
            if (smtpClient == null) return;
            smtpClient.Send(App.getConfiguration("smtp:email"), textBoxTo.Text, textBoxThema.Text, textBoxMessage.Text);
            MessageBox.Show("Sent!");
        }

        private void SendButton2_Click(object sender, RoutedEventArgs e)
        {
            SmtpClient smtpClient = getSmtp();
            if (smtpClient == null) return;
            MailMessage mailMessage = new MailMessage(App.getConfiguration("smtp:email"), textBoxTo.Text, textBoxThema.Text, textBoxHtml.Text)
            {
                IsBodyHtml = true,
            };
            ContentType pngType = new ContentType("image/png");
            ContentType mp3Type = new ContentType("audio/mpeg");
            mailMessage.Attachments.Add(new Attachment("monetka.png", pngType));
            mailMessage.Attachments.Add(new Attachment("Jump_01.mp3", mp3Type));
            smtpClient.Send(mailMessage);
            MessageBox.Show("Sent!!");
        }
        private int GenerateRandomCode()
        {
            Random random = new Random();
            return random.Next(10000000, 99999999);
        }
        private void SendButton3_Click(object sender, RoutedEventArgs e)
        {
            SmtpClient smtpClient = getSmtp();
            if (smtpClient == null) return;
            MailMessage mailMessage = new MailMessage(App.getConfiguration("smtp:email"), textBoxTo.Text, textBoxThema.Text, textBoxApprove.Text)
            {
                IsBodyHtml = true,
            };
            ContentType txtType = new ContentType("text/plain");
            ContentType docType = new ContentType("application/msword");
            mailMessage.Attachments.Add(new Attachment("privacy.txt", txtType));
            mailMessage.Attachments.Add(new Attachment("privacy.doc", docType));
            smtpClient.Send(mailMessage);
            MessageBox.Show("Sent!!");
        }
    }
}
