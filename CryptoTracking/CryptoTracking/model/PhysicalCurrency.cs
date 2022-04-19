using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoTracking.model
{
    public class PhysicalCurrency
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public PhysicalCurrency() { }

        public PhysicalCurrency(string code, string name)
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
