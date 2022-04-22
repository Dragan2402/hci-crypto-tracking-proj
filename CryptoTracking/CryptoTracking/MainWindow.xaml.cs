using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using TinyCsvParser;
using System.Web.Script.Serialization;
using CryptoTracking.model;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;

namespace CryptoTracking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<CryptoCurrency> CryptoCurrencies { get; set; }
        public List<PhysicalCurrency> PhysicalCurrencies { get; set; }

        public string SelectedPhysicalCurrency { get; set; }
        public string SelectedCryptoCurrency { get; set; }

        public int btcIndex { get; set; }  

        public int usdIndex { get; set; }

        private const string QUERY_URL_ROOT = "https://www.alphavantage.co/query";
        private const string QUERY_URL_FUNCTION = "?function=";
        private const string QUERY_URL_SYMBOL = "&symbol=";
        private const string QUERY_URL_MARKET = "&market=";
        private const string QUERY_URL_INTERVAL = "&interval=";
        private const string QUERY_URL_API_KEY = "&apikey=2RORR06XPMBUTCS0";

        public MainWindow()
        {
            loadCryptoCurrencies();
            loadPhysicalCurrencies();

            KeyValuePair<List<string>, List<double>> kvp = GetData("BTC", "USD", Interval.OneMin, CandleValue.Low);

            InitializeComponent();
            DataContext = this;
        }

        private void loadCryptoCurrencies()
        {
            CryptoCurrencies =new List<CryptoCurrency>();
            CsvParserOptions csvParserOptions = new CsvParserOptions(true, ',');
            CsvCryptoMapping csvCryptoMapper = new CsvCryptoMapping();
            CsvParser<CryptoCurrency> csvCryptoParser = new CsvParser<CryptoCurrency>(csvParserOptions, csvCryptoMapper);
            var resultCrypto = csvCryptoParser.ReadFromFile(@"../../data/digital_currency_list.csv", Encoding.UTF8).ToList();

            //int i = 0;
            foreach (var item in resultCrypto)
            {
                
                //if (item.Result.Code.Equals("BTC"))
                //{
                //    btcIndex = i;
                //}
                //i++;
                
                CryptoCurrency temp = new CryptoCurrency(item.Result.Code, item.Result.Name);
                CryptoCurrencies.Add(temp);
            }
            SelectedCryptoCurrency = "BTC";
        }

        private void loadPhysicalCurrencies()
        {
            PhysicalCurrencies = new List<PhysicalCurrency>();
            CsvParserOptions csvPhysicalParserOptions = new CsvParserOptions(true, ',');
            CsvPhysicalMapping csvPhysicalMapper = new CsvPhysicalMapping();
            CsvParser<PhysicalCurrency> csvPhysicalParser = new CsvParser<PhysicalCurrency>(csvPhysicalParserOptions, csvPhysicalMapper);
            var resultPhysical = csvPhysicalParser.ReadFromFile(@"../../data/physical_currency_list.csv", Encoding.UTF8).ToList();
            //int i=0;
            foreach (var item in resultPhysical)
            {
                //if (item.Result.Code.Equals("USD"))
                //{
                //    usdIndex = i;
                    
                //}
                
                //i++;
                PhysicalCurrency temp = new PhysicalCurrency(item.Result.Code, item.Result.Name);
                PhysicalCurrencies.Add(temp);
            }
            SelectedPhysicalCurrency = "USD";
        }

        private void DisplayData(object sender, RoutedEventArgs e)
        {
            var intervalButton = (RadioButton)grid
                .Children
                .OfType<RadioButton>()
                .Where(rb => rb.GroupName.Equals("IntervalGroup") && rb.IsChecked.HasValue && rb.IsChecked.Value )
                .FirstOrDefault();
            Interval interval = (Interval)int.Parse(intervalButton.Uid);

            var candleButton = (RadioButton)grid
                .Children
                .OfType<RadioButton>()
                .Where(rb => rb.GroupName.Equals("CandleGroup") && rb.IsChecked.HasValue && rb.IsChecked.Value)
                .FirstOrDefault();
            CandleValue candleValue = (CandleValue)int.Parse(candleButton.Uid);


            KeyValuePair<List<string>, List<double>> timeSeries = GetData(SelectedCryptoCurrency, SelectedPhysicalCurrency, interval, candleValue);

            chart.AxisX.Clear();
            chart.AxisY.Clear();

            List<string> times = timeSeries.Key;
            List<double> values = timeSeries.Value;

            //for(int i = 0; i < 20; i++)
            //{
            //    labels.Add("Datum" + i.ToString());
            //}

            //double value = 48000;
            //Random random = new Random();

            //for(int i = 0; i < 20; i++)
            //{
            //    double randomMultiplier = random.Next(80, 120);
            //    value *= randomMultiplier/100;
            //    values.Add(value);
            //}

            chart.AxisX.Add(new LiveCharts.Wpf.Axis
            {
                Labels = times,

            });

            chart.AxisY.Add(new LiveCharts.Wpf.Axis
            {
                MinValue = values.Min()
            });

            chart.Series.Clear();
            SeriesCollection series = new SeriesCollection();
            series.Add(new LineSeries()
            {
                Values = new ChartValues<double>(values),
                LineSmoothness = 0,
                PointGeometrySize = 0
            });
            chart.Series = series;
        }

        private void ClearData(object sender, RoutedEventArgs e)
        {
            
        }

        private KeyValuePair<List<string>, List<double>> GetData(string symbol, string market, Interval interval, CandleValue candleValue)
        {
            string queryUrl = GetQueryUrl(symbol, market, interval);
            Uri queryUri = new Uri(queryUrl);

            List<string> times = new List<string>();
            List<double> values = new List<double>();

            using (WebClient client = new WebClient())
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                Dictionary<string, object> jsonData = (Dictionary<string, object>)js.Deserialize(client.DownloadString(queryUri), typeof(object));
                string keykey = interval.GetKey();
                Dictionary<string, object> timeSeries = (Dictionary<string, object>)jsonData[interval.GetKey()];

                string candleValueKey = candleValue.GetKey(market, interval.IsIntraday());

                foreach (string timeSeriesTime in timeSeries.Keys)
                {
                    Dictionary<string, object> timeSeriesValues = (Dictionary<string, object>)timeSeries[timeSeriesTime];
                    double timeSeriesValue = Double.Parse((string)timeSeriesValues[candleValueKey]);

                    times.Add(timeSeriesTime);
                    values.Add(timeSeriesValue);
                }
            }
            times.Reverse();
            values.Reverse();
            return new KeyValuePair<List<string>, List<double>>(times, values);
        }

        private string GetQueryUrl(string symbol, string market, Interval interval)
        {
            string queryUrl = QUERY_URL_ROOT +
                              QUERY_URL_FUNCTION + interval.GetFunction() +
                              QUERY_URL_SYMBOL + symbol +
                              QUERY_URL_MARKET + market;

            if (interval.IsIntraday())
                queryUrl += QUERY_URL_INTERVAL + interval.GetInterval();

            queryUrl += QUERY_URL_API_KEY;
            return queryUrl;
        }
    }
}
