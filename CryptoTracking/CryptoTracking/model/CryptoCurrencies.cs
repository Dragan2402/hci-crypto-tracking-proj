using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoTracking.model
{
    public class CryptoCurrencies
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public CryptoCurrencies() { }

        public CryptoCurrencies(string code,string name)
        {
            Code = code;
            Name = name;
        }

        public override string ToString()
        {
            return Code + ":" + Name;
        }
    }
}
