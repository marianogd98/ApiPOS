using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfPos_Rios.Class
{
   
    public class ClassPaymentMethods
    {
        string name { get; set; }
        double amount { get; set; }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }
        public double Amount
        {
            get
            {
                return amount;
            }
        }

    }
}
