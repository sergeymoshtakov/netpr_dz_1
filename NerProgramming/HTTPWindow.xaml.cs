using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Net.Http;
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
using System.Xml;
using System.Text.Json;

namespace NerProgramming
{
    /// <summary>
    /// Логика взаимодействия для HTTPWindow.xaml
    /// </summary>
    public partial class HTTPWindow : Window
    {
        private List<NBURate> rates;
        private string[] popularCc = { "XAU", "USD", "EUR" };
        public HTTPWindow()
        {
            InitializeComponent();
        }

        private async void Button1_Click(object sender, RoutedEventArgs e)
        {
            HttpClient httpClient = new HttpClient();
            var response = await httpClient.GetAsync("https://itstep.org/");
            textBlock1.Text = "";
            textBlock1.Text += (int)response.StatusCode + " " + response.ReasonPhrase + "\r\n";
            foreach (var header in response.Headers)
            {
               textBlock1.Text += $"{header.Key,-20}: " + String.Join(",",header.Value).Elipsis(30) + "\r\n";
            }
            String body = await response.Content.ReadAsStringAsync();
            textBlock1.Text = $"\r\n{body}";
        }

        private async void CurrencyCourseXML_Click(object sender, RoutedEventArgs e)
        {
            HttpClient httpClient = new HttpClient();
            var response = await httpClient.GetAsync("https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange");
            textBlock1.Text = "";
            String body = await response.Content.ReadAsStringAsync();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(body);
            XmlNodeList currencyNodes = xmlDoc.SelectNodes("/exchange/currency");
            foreach (XmlNode currencyNode in currencyNodes)
            {
                textBlock1.Text += $"\r\n 1 {currencyNode.SelectSingleNode("txt").InnerText} = {currencyNode.SelectSingleNode("rate").InnerText} Гривень";
            }
            
        }

        private async void CurrencyCourseJSON_Click(object sender, RoutedEventArgs e)
        {
            HttpClient httpClient = new HttpClient();
            var response = await httpClient.GetAsync("https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?json");
            textBlock1.Text = "";
            String body = await response.Content.ReadAsStringAsync();
            List<Currency> currencies = JsonSerializer.Deserialize<List<Currency>>(body);
            foreach(Currency currency in currencies)
            {
                textBlock1.Text += $"1 {currency.txt} = {currency.rate} Гривень\r\n";
            }
        }

        private async void ratesButton_Click(object sender, RoutedEventArgs e)
        {
            if(rates == null)
            {
                await loadRatesAsync();
            }
            if(rates == null)
            {
                return;
            }
            foreach (var rate in rates)
            {
                textBlock1.Text += $"{rate.cc} {rate.txt} {rate.rate}\n";
            }
        }
        private async Task loadRatesAsync()
        {
            HttpClient httpClient = new HttpClient();
            String body = await httpClient.GetStringAsync("https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?json");
            rates = JsonSerializer.Deserialize<List<NBURate>>(body);
            if (rates == null)
            {
                MessageBox.Show("Error desirializing");
                return;
            }
        }

        private async void popular_Click(object sender, RoutedEventArgs e)
        {
            if (rates == null)
            {
                await loadRatesAsync();
            }
            if (rates == null)
            {
                return;
            }
            foreach (var rate in rates)
            {
                if (popularCc.Contains(rate.cc))
                {
                    textBlock1.Text += $"{rate.cc} {rate.txt} {rate.rate}\n";
                }
            }
        }
    }

    public static class MyExtensions { 
        public static string Elipsis(this string str, int maxLength) 
        { 
            return str.Length > maxLength ? str.Substring(0,maxLength - 3) + "..." : str; 
        } 
    }

    public class Currency
    {
        public int r030 { get; set; }
        public string txt { get; set; }
        public double rate { get; set; }
        public string cc { get; set; }
        public string exchangedate { get; set; }
    }

    public class NBURate
    {
        public int r030 { get; set; }
        public string txt { get; set; }
        public double rate { get; set; }
        public string cc { get; set; }
        public string exchangedate { get; set; }
    }
}
