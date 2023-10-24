using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
            if (response == null)
            {
                MessageBox.Show("Error desirializing");
                return;
            }
            CoinsData.Clear();
            foreach (var coinData in response.data)
            {
                CoinsData.Add(coinData);
            }
        }

        private async void FrameworkElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item)
            {
                // if (lastSelectedItem != null && lastSelectedItem != item)
                // {
                // lastSelectedItem.Background = Brushes.Transparent;
                // }

                // item.Background = Brushes.Aqua;
                // lastSelectedItem = item;

                if (item.DataContext is CoinData coinData)
                {
                    // MessageBox.Show($"ID ассета: {data.id}");
                    await showHistory(coinData);
                    item.Background = Brushes.Aqua;
                }

            }
        }

        private async Task showHistory(CoinData coinData)
        {
            String body = await _httpClient.GetStringAsync(
                $"/v2/assets/{coinData.id}/history?interval=d1"
            );
            var response = JsonSerializer.Deserialize<HistoryResponse>(body);
            if (response == null || response.data == null)
            {
                MessageBox.Show("Error");
                return;
            }
            long minTime, maxTime;
            double minPrice, maxPrice;
            minPrice = maxPrice = response.data[0].price;
            minTime = maxTime = response.data[0].time;
            foreach(HistoryItem item in response.data)
            {
                if(item.time < minTime) { minTime =  item.time; }
                if (item.time > maxTime) { maxTime = item.time; }
                if (item.price < minPrice) { minPrice = item.price; }
                if (item.price > maxPrice) { maxPrice = item.price; }
            }
            double yOffset = 30.0;
            double grafH = Graph.ActualHeight - yOffset;
            double x0 = (response.data[0].time - minTime) * Graph.ActualWidth / (maxTime - minTime);
            double y0 = grafH - (response.data[0].price - minPrice) * Graph.ActualHeight / (maxPrice - minPrice);
            Graph.Children.Clear();

            foreach (HistoryItem item in response.data)
            {
                double x = (item.time - minTime) * Graph.ActualWidth / (maxTime - minTime);
                double y = grafH - (item.price - minPrice) * Graph.ActualHeight / (maxPrice - minPrice);
                Dispatcher.Invoke(() => DrawLine(x0,y0,x,y));
                if(item.time == maxTime)
                {
                    Graph.Children.Add(new TextBlock
                    {
                        FontSize = 18,
                        Foreground = Brushes.Red,
                        FontStyle = FontStyles.Italic,
                        FontWeight = FontWeights.Bold,
                        Text = DateTime.Parse(item.date, null, System.Globalization.DateTimeStyles.RoundtripKind).ToString("dd.MM.yyyy"),
                        Margin = new Thickness(x, y, 0, 0)
                    });
                }
                if (item.time == minTime)
                {
                    Graph.Children.Add(new TextBlock
                    {
                        FontSize = 18,
                        Foreground = Brushes.Green,
                        FontStyle = FontStyles.Italic,
                        FontWeight = FontWeights.Bold,
                        Text = DateTime.Parse(item.date, null, System.Globalization.DateTimeStyles.RoundtripKind).ToString("dd.MM.yyyy"),
                        Margin = new Thickness(x, y, 0, 0)
                    });
                }
                if (item.price == minPrice)
                {
                    Graph.Children.Add(new TextBlock
                    {
                        FontSize = 18,
                        Foreground = Brushes.Green,
                        FontStyle = FontStyles.Italic,
                        FontWeight = FontWeights.Bold,
                        Text = item.price.ToString(),
                        Margin = new Thickness(x, y, 0, 0)
                    });
                }
                if (item.price == maxPrice)
                {
                    Graph.Children.Add(new TextBlock
                    {
                        FontSize = 18,
                        Foreground = Brushes.Red,
                        FontStyle = FontStyles.Italic,
                        FontWeight = FontWeights.Bold,
                        Text = item.price.ToString(),
                        Margin = new Thickness(x, y, 0, 0)
                    });
                }
                x0 = x;
                y0 = y;
            }
            DrawLine(0, grafH, Graph.ActualWidth, grafH, new SolidColorBrush(Colors.Purple));
        }

        private void DrawLine(double x1, double y1, double x2, double y2, Brush brush = null)
        {
            brush = brush == null ? new SolidColorBrush(Colors.Black) : brush;
            Graph.Children.Add(new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = brush,
                StrokeThickness = 2
            });
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

    public class HistoryItem
    {
        public string priceUsd { get; set;}
        public long time { get; set; }
        public string date {  get; set; }
        public double price => Convert.ToDouble(priceUsd, CultureInfo.InvariantCulture);
    }

    public class HistoryResponse
    {
        public List<HistoryItem> data { get; set; }
        public long timestamp { get; set; }
    }
}
