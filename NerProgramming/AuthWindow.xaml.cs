﻿using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
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
using System.Data;

namespace NerProgramming
{
    /// <summary>
    /// Логика взаимодействия для AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        private String connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=NewProject;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";
        public AuthWindow()
        {
            InitializeComponent();
        }

        private void SigninButton_Click(object sender, RoutedEventArgs e)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM [Table] WHERE " +
                $" [Email]=N'{textboxEmail.Text}' " +
                $" AND [Password]=N'{textboxPassword.Password}' ";
            SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())  // Користувач знайдений
            {
                if (!reader.IsDBNull(reader.GetOrdinal("ConfirmCode")))   // код не NULL - не підтверджений
                {
                    String code = reader.GetString(reader.GetOrdinal("ConfirmCode"));
                    ConfirmContainer.Visibility = Visibility.Visible;
                    ConfirmContainer.Tag = code;
                    textboxCode.Focus();
                    logBlock.Text += "Welcome, Email needs confirmation\n";
                }
                else
                {
                    logBlock.Text += "Welcome, Email confirmed\n";
                }
            }
            else  // Неправильні логін/пароль
            {
                MessageBox.Show("Credentials incorrect");
            }
        }

        private void SignupButton_Click(object sender, RoutedEventArgs e)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            String code = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
            command.CommandText = "INSERT INTO [Table](Email, Password, ConfirmCode)" +
                $" VALUES (N'{textboxEmail.Text}', N'{textboxPassword.Password}', '{code}')";

            command.ExecuteNonQuery();

            SmtpClient smtpClient = GetSmtpClient();
            smtpClient?.Send(
                App.getConfiguration("smtp:email"),
                textboxEmail.Text,
                "Signup successfull",
                $"Congratulations! To confirm Email use code: {code}"
            );
            MessageBox.Show("Chek Email");
        }

        private SmtpClient GetSmtpClient()
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
            int port;
            try { port = int.Parse(portString); }
            catch
            {
                MessageBox.Show("Error parsing port");
                return null;
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

            return new SmtpClient(host, port)
            {
                EnableSsl = ssl,
                Credentials = new NetworkCredential(email, password)
            };
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (ConfirmContainer.Tag is String savedCode)
            {
                if (textboxCode.Text.Equals(savedCode))
                {
                    logBlock.Text += "Email  confirmed\n";
                    /* Д.З. При правильному вводі коду підтвердження пошти
                     * виконати SQL запит, який змінить значення ConfirmCode
                     * на NULL, а також приховає "вікно" введення коду.
                     * Перевірити, що повторному вході пошта вважається
                     * підтвердженою
                     */
                    SqlConnection connection = new SqlConnection(connectionString);
                    connection.Open();
                    SqlCommand command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO [Table](ConfirmCode)" +
                        " VALUES (NULL)";
                    command.ExecuteNonQuery();
                    ConfirmContainer.Visibility = Visibility.Hidden;
                }
                else
                {
                    logBlock.Text += "Email not confirmed\n";
                }
            }
        }
    }
}
