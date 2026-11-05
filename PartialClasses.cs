using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace proekt
{
    public partial class Vehicles
    {
        public bool NeedService
        {
            get { return Mileage.HasValue && Mileage >= 100000; }
        }
    }
}
