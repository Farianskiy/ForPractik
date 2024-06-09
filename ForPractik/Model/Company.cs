using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ForPractik.Model
{
    internal class Company
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string FullNameOfTheHead { get; set; }
        public string JobTitle { get; set; }
        public string Tin { get; set; }
        public string Kpp { get; set; }
        public string Oprn { get; set; }
        public string Bic { get; set; }
        public string CheckingAccount { get; set; }
        public string CorporateAccount { get; set; }
        public string Bank { get; set; }
        public string Telephone { get; set; }
        public string Address { get; set; }
    }
}
