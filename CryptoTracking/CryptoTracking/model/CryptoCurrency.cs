using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyCsvParser;
using TinyCsvParser.Mapping;

namespace CryptoTracking.model
{
    public class CryptoCurrency
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public CryptoCurrency() { }

      

        public CryptoCurrency(string code,string name)
        {
            Code = code;
            Name = name;
        }

        public override string ToString()
        {
            return Code + ":" + Name;
        }
    }

    public class CsvCryptoMapping : CsvMapping<CryptoCurrency>
    {
       public CsvCryptoMapping(): base()
        {
            MapProperty(0, x => x.Code);
            MapProperty(1, x => x.Name);
        }
    }
}
