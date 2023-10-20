using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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
    /// Логика взаимодействия для CryptoWindow.xaml
    /// </summary>
    public partial class CryptoWindow : Window
    {
        private readonly HttpClient _httpClient;
        private ListViewItem lastSelectedItem = null;
        public ObservableCollection<CoinData> CoinsData { get; set; }
        public CryptoWindow()
        {
            InitializeComponent();
            CoinsData = new ObservableCollection<CoinData>();
            this.DataContext = this;
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri("https://api.coincap.io/")
            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAssestsAsync();
        }

        private async Task LoadAssestsAsync()
        {
            var response = JsonSerializer.Deserialize<CoincapResponse>(
                await _httpClient.GetStringAsync("/v2/assets?limit=10")
            );
            if(response == null)
            {
                MessageBox.Show("Error desirializing");
                return;
            }
            CoinsData.Clear();
            foreach(var coinData in response.data)
            {
                CoinsData.Add(coinData);
            }
        }

        private void FrameworkElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(sender is ListViewItem item)
            {
                if (lastSelectedItem != null && lastSelectedItem != item)
                {
                    lastSelectedItem.Background = Brushes.Transparent;
                }

                item.Background = Brushes.Aqua;
                lastSelectedItem = item;

                if (item.DataContext is CoinData data)
                {
                    MessageBox.Show($"ID ассета: {data.id}");
                }
            }
        }
    }

    public class CoincapResponse
    {
        public List<CoinData> data { get; set; }
        public long timestamp { get; set; }
    }
    public class CoinData
    {
        public string id { get; set; }
        public string rank { get; set; }
        public string symbol { get; set; }
        public string name { get; set; }
        public string supply { get; set; }
        public string maxSupply { get; set; }
        public string marketCapUsd { get; set; }
        public string volumeUsd24Hr { get; set; }
        public string priceUsd { get; set; }
        public string changePercent24Hr { get; set; }
        public string vwap24Hr { get; set; }
        public string explorer { get; set; }
    }
}
