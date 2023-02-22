using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfPos_Rios.ViewModels
{
    public class VmClient
    {
        private int id;
        private string name;
        private string lastName;
        private string rif;
        private string cellnumber;
        private string address;
        private string email;
        private double saldo;
        public void resetData()
        {
            this.id = 0;
            this.name = "";
            this.rif = "";
            this.lastName = "";
            this.rif = "";
            this.cellnumber = "";
            this.address = "";
            this.email = "";
            this.saldo = 0;
        }

        public string Name { get => name; set => name = value; }
        public string LastName { get => lastName; set => lastName = value; }
        public string Rif { get => rif; set => rif = value; }
        public string Cellnumber { get => cellnumber; set => cellnumber = value; }
        public string Address { get => address; set => address = value; }
        public string Email { get => email; set => email = value; }
        public int Id { get => id; set => id = value; }
        public double Saldo { get => saldo; set => saldo = value; }
    }



}
