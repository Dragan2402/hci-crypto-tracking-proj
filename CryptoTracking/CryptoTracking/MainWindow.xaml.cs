using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Net;
using TinyCsvParser;
using System.Web.Script.Serialization;
using CryptoTracking.model;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Configurations;
using System.Globalization;

namespace CryptoTracking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<CryptoCurrency> CryptoCurrencies { get; set; }
        public List<PhysicalCurrency> PhysicalCurrencies { get; set; }

        private Dictionary<string, object> LoadedData { get; set; }

        public string SelectedPhysicalCurrency { get; set; }
        public string SelectedCryptoCurrency { get; set; }

        private int MaxXValue { get; set; }

        private bool LimitMin = false, LimitMax = false;
        private delegate void PreviewRangeDelegate(LiveCharts.Events.PreviewRangeChangedEventArgs e);
        private delegate void RangeDelegate(LiveCharts.Events.RangeChangedEventArgs e);

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

            LoadedData = new Dictionary<string, object>();

            InitializeComponent();
            DataContext = this;

            DisplayChart();
        }

        public class Data
        {
            public Data(string time, double value)
            {
                this.Time = time;
                this.Value = value;
            }
            public Data() { }
            public string Time { get; set; }
            public double Value { get; set; }
        }

        private void loadCryptoCurrencies()
        {
            CryptoCurrencies =new List<CryptoCurrency>();
            CsvParserOptions csvParserOptions = new CsvParserOptions(true, ',');
            CsvCryptoMapping csvCryptoMapper = new CsvCryptoMapping();
            CsvParser<CryptoCurrency> csvCryptoParser = new CsvParser<CryptoCurrency>(csvParserOptions, csvCryptoMapper);
            var resultCrypto = csvCryptoParser.ReadFromFile(@"../../data/digital_currency_list.csv", Encoding.UTF8).ToList();

            foreach (var item in resultCrypto)
            {
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

            foreach (var item in resultPhysical)
            {
                PhysicalCurrency temp = new PhysicalCurrency(item.Result.Code, item.Result.Name);
                PhysicalCurrencies.Add(temp);
            }
            SelectedPhysicalCurrency = "USD";
        }

        private void DisplayChartClicked(object sender, RoutedEventArgs e)
        {
            DisplayChart();
        }

        private void DisplayChart()
        {
            var intervalButton = grid
                .Children
                .OfType<RadioButton>()
                .Where(rb => rb.GroupName.Equals("IntervalGroup") && rb.IsChecked.HasValue && rb.IsChecked.Value)
                .FirstOrDefault();
            Interval interval = (Interval)int.Parse(intervalButton.Uid);

            var candleButton = grid
                .Children
                .OfType<RadioButton>()
                .Where(rb => rb.GroupName.Equals("CandleGroup") && rb.IsChecked.HasValue && rb.IsChecked.Value)
                .FirstOrDefault();
            CandleValue candleValue = (CandleValue)int.Parse(candleButton.Uid);

            
            KeyValuePair<List<string>, List<double>> timeSeries = GetData(SelectedCryptoCurrency, SelectedPhysicalCurrency, interval, candleValue);

            List<string> times = timeSeries.Key;
            List<double> values = timeSeries.Value;

            if (times != null && values != null)
            {
                MaxXValue = times.Count - 1;

                ChartValues<Point> points = new ChartValues<Point>();
                for (int i = 0; i < values.Count; i++)
                {
                    points.Add(new Point() { X = i, Y = values[i] });
                }

                chart.AxisX.FirstOrDefault().LabelFormatter = value => times.ElementAtOrDefault((int)value);
                chart.AxisX.FirstOrDefault().MinValue = 0;
                chart.AxisX.FirstOrDefault().MaxValue = MaxXValue;

                chart.AxisY.Clear();
                double max = values.Max();
                double min = values.Min();
                double diff = (max - min) * 0.05;
                chart.AxisY.Add(new Axis
                {
                    MinValue = min - diff,
                    MaxValue = max + diff

                });

                chart.Series.Clear();
                SeriesCollection series = new SeriesCollection();
                series.Add(new LineSeries()
                {
                    Configuration = new CartesianMapper<Point>().X(point => point.X).Y(point => point.Y),
                    Values = points,
                    LineSmoothness = 0,
                    PointGeometrySize = 0
                });
                chart.Series = series;

                dataGrid.Items.Clear();
                if (times.Count() == values.Count())
                {
                    for (int i = 0; i < times.Count(); i++)
                    {
                        dataGrid.Items.Add(new Data(times[i], values[i]));
                    }
                }
            }
            else
            {
                MessageBoxResult result = MessageBox.Show(this,"Data is currently not available, try later","Info");
            }
        }

        private void Ax_PreviewRangeChanged(LiveCharts.Events.PreviewRangeChangedEventArgs e)
        {
            LimitMax = e.PreviewMaxValue > MaxXValue;
            LimitMin = e.PreviewMinValue < 0;
        }

        private void Ax_RangeChanged(LiveCharts.Events.RangeChangedEventArgs e)
        {
            Axis ax = (Axis)e.Axis;
            if (LimitMax) ax.MaxValue = MaxXValue;
            if (LimitMin) ax.MinValue = 0;
        }

        private KeyValuePair<List<string>, List<double>> GetData(string symbol, string market, Interval interval, CandleValue candleValue)
        {
            string queryUrl = GetQueryUrl(symbol, market, interval);
            
            Uri queryUri = new Uri(queryUrl);
            Dictionary<string, object> timeSeries = GetTimeSeries(interval.GetKey(), symbol, market, queryUri);
            
            List<string> times = new List<string>();
            List<double> values = new List<double>();
            string candleValueKey = candleValue.GetKey(market, interval.IsIntraday());
            if (timeSeries != null)
            {
                foreach (string timeSeriesTime in timeSeries.Keys)
                {
                    Dictionary<string, object> timeSeriesValues = (Dictionary<string, object>)timeSeries[timeSeriesTime];

                    string valueString = (string)timeSeriesValues[candleValueKey];
                    NumberFormatInfo provider = new NumberFormatInfo();
                    provider.NumberDecimalSeparator = ".";
                    double timeSeriesValue = Convert.ToDouble(valueString, provider);

                    times.Add(timeSeriesTime);

                    values.Add(timeSeriesValue);
                }

                times.Reverse();
                values.Reverse();

                return new KeyValuePair<List<string>, List<double>>(times, values);
            }
            else
            {
                return new KeyValuePair<List<string>, List<double>>();
            }
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

        private Dictionary<string, object> GetTimeSeries(string intervalKey, string symbol, string market, Uri queryUri)
        {
            Dictionary<string, object> timeSeries;
            string loadedDataKey = intervalKey + "(" + symbol + ")(" + market + ")";
       
            if (LoadedData.ContainsKey(loadedDataKey))
            {
                timeSeries = (Dictionary<string, object>)LoadedData[loadedDataKey];
            }
            else
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        Dictionary<string, object> jsonData = (Dictionary<string, object>)js.Deserialize(client.DownloadString(queryUri), typeof(object));
                        timeSeries = (Dictionary<string, object>)jsonData[intervalKey];

                        LoadedData[loadedDataKey] = timeSeries;
                    }
                }catch (Exception ex)
                {
                    return null;
                }
            }
            return timeSeries;
        }
    }
}
