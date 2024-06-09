using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForPractik.Model
{
    public class GroupEnterprise
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int EnterpriseId { get; set; }
        public string EnterpriseName { get; set; }
        public string HeadOfEnterprise { get; set; }
    }
}
