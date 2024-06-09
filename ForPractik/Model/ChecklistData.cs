using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForPractik.Model
{
    public class ChecklistData
    {
        public int Id { get; set; } 
        public bool Listokpk { get; set; }
        public bool Questionnaire { get; set; }
        public bool Diary { get; set; }
        public bool Agreement { get; set; }
        public bool Review { get; set; }
        public bool Protection { get; set; }
        public bool Report { get; set; }
        public int? Grade { get; set; }
    }

}
