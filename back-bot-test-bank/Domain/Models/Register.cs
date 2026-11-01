using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Register
    {
        public string Name { get; set; }
        public int DocumentType { get; set; }
        public int DocumentNumber { get; set; }
        public List<string> SeatsTaken { get; set; }
    }
}
