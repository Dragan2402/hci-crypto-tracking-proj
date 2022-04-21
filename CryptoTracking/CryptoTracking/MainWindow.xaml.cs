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

namespace CryptoTracking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<CryptoCurrency> CryptoCurrencies { get; set; }
        public List<PhysicalCurrency> PhysicalCurrencies { get; set; }

        public int btcIndex { get; set; }  

        public int usdIndex { get; set; }

        public MainWindow()
        {
            loadCryptoCurrencies();
            loadPhysicalCurrencies();
            string QUERY_URL = "https://www.alphavantage.co/query?function=DIGITAL_CURRENCY_WEEKLY&symbol=BTC&market=CNY&apikey=2RORR06XPMBUTCS0";
            Uri queryUri = new Uri(QUERY_URL);

            using (WebClient client = new WebClient())
            {
                // -------------------------------------------------------------------------
                // if using .NET Framework (System.Web.Script.Serialization)

                JavaScriptSerializer js = new JavaScriptSerializer();
                dynamic json_data = js.Deserialize(client.DownloadString(queryUri), typeof(object));

               

                
     

                // do something with the json_data
            }        


           


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

            int i = 0;
            foreach (var item in resultCrypto)
            {
                
                if (item.Result.Code.Equals("BTC"))
                {
                    btcIndex = i;
                }
                i++;
                
                CryptoCurrency temp = new CryptoCurrency(item.Result.Code, item.Result.Name);
                CryptoCurrencies.Add(temp);
            }
        }

        private void loadPhysicalCurrencies()
        {
            PhysicalCurrencies = new List<PhysicalCurrency>();
            CsvParserOptions csvPhysicalParserOptions = new CsvParserOptions(true, ',');
            CsvPhysicalMapping csvPhysicalMapper = new CsvPhysicalMapping();
            CsvParser<PhysicalCurrency> csvPhysicalParser = new CsvParser<PhysicalCurrency>(csvPhysicalParserOptions, csvPhysicalMapper);
            var resultPhysical = csvPhysicalParser.ReadFromFile(@"../../data/physical_currency_list.csv", Encoding.UTF8).ToList();
            int i=0;
            foreach (var item in resultPhysical)
            {
                if (item.Result.Code.Equals("USD"))
                {
                    usdIndex = i;
                    
                }
                
                i++;
                PhysicalCurrency temp = new PhysicalCurrency(item.Result.Code, item.Result.Name);
                PhysicalCurrencies.Add(temp);
            }
        }

        private void DisplayData(object sender, RoutedEventArgs e)

        {
            chart.AxisX.Clear();
            chart.AxisY.Clear();

            chart.AxisX.Add(new LiveCharts.Wpf.Axis
            {
                Labels = new List<string> { "1.1.2001.", "2.1.2001.", "3.1.2001.", "4.1.2001.", "5.1.2001.", "6.1.2001.", "7.1.2001.", "8.1.2001.", "9.1.2001." },

            });

            chart.Series.Clear();
            SeriesCollection series = new SeriesCollection();
            List<double> values = new List<double>();
            values.AddRange( new List<double> { 48000, 51000, 59000, 42000, 35000, 31000, 44000, 48000, 55000});
            series.Add(new LineSeries() { Values = new ChartValues<double>(values) });
            chart.Series = series;
        }

        private void ClearData(object sender, RoutedEventArgs e)

        {
            chart.AxisX.Clear();
            chart.AxisY.Clear();
            chart.AxisX.Add(new LiveCharts.Wpf.Axis
            {
                Labels = new List<string> { "1.1.2002.", "2.1.2002.", "3.1.2002.", "4.1.2002.", "5.1.2002.", "6.1.2002.", "7.1.2002." },

            });

            chart.Series.Clear();
            SeriesCollection series = new SeriesCollection();
            List<double> values = new List<double>();
            values.AddRange(new List<double> { 12000, 22000, 25000, 17000, 14000, 19000, 22000 });
            series.Add(new LineSeries() { Values = new ChartValues<double>(values) });
            chart.Series = series;

        }
    }
}
