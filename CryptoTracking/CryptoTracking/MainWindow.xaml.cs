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

namespace CryptoTracking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<CryptoCurrency> CryptoCurrenciesList = new List<CryptoCurrency>();
        public List<PhysicalCurrency> PhysicalCurrenciesList = new List<PhysicalCurrency>();
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

               

                Console.WriteLine("GAGE");
               
                foreach (var item in json_data.Keys)
                {
                    Console.WriteLine(item);
                }

                // do something with the json_data
            }        


            foreach (var item in PhysicalCurrenciesList)
            {
                Console.WriteLine(item.ToString());
            }

            foreach (var item in CryptoCurrenciesList)
            {
                Console.WriteLine(item.ToString());
            }


            InitializeComponent();
        }

        private void loadPhysicalCurrencies()
        {
            CsvParserOptions csvParserOptions = new CsvParserOptions(true, ',');
            CsvCryptoMapping csvCryptoMapper = new CsvCryptoMapping();
            CsvParser<CryptoCurrency> csvCryptoParser = new CsvParser<CryptoCurrency>(csvParserOptions, csvCryptoMapper);
            var resultCrypto = csvCryptoParser.ReadFromFile(@"../../data/digital_currency_list.csv", Encoding.UTF8).ToList();

            foreach (var item in resultCrypto)
            {
                CryptoCurrency temp = new CryptoCurrency(item.Result.Code, item.Result.Name);
                CryptoCurrenciesList.Add(temp);
            }
        }

        private void loadCryptoCurrencies()
        {
            CsvParserOptions csvPhysicalParserOptions = new CsvParserOptions(true, ',');
            CsvCryptoMapping csvPhysicalMapper = new CsvCryptoMapping();
            CsvParser<CryptoCurrency> csvPhysicalParser = new CsvParser<CryptoCurrency>(csvPhysicalParserOptions, csvPhysicalMapper);
            var resultPhysical = csvPhysicalParser.ReadFromFile(@"../../data/physical_currency_list.csv", Encoding.UTF8).ToList();
            foreach (var item in resultPhysical)
            {
                PhysicalCurrency temp = new PhysicalCurrency(item.Result.Code, item.Result.Name);
                PhysicalCurrenciesList.Add(temp);
            }
        }

        private void DisplayData(object sender, RoutedEventArgs e)

        {
            graph.Visibility = Visibility.Visible;
           

        }

        private void ClearData(object sender, RoutedEventArgs e)

        {
            graph.Visibility = Visibility.Hidden;
            

        }
    }
}
