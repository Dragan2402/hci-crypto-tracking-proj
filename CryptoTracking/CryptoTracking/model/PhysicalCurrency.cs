using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyCsvParser.Mapping;

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

    public class CsvPhysicalMapping : CsvMapping<PhysicalCurrency>
    {
        public CsvPhysicalMapping() : base()
        {
            MapProperty(0, x => x.Code);
            MapProperty(1, x => x.Name);
        }
    }
}
