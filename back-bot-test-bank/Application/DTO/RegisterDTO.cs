using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO
{
    public class RegisterDTO
    {
        public string Name { get; set; }
        public int DocumentType { get; set; }
        public int DocumentNumber { get; set; }
        public List<string> SeatsTaken { get; set; }
        public int QuantityOfPeopleConected { get; set; }

        public RegisterDTO(string name, int documentType, int documentNumber, List<string> seatsTaken, int quantity)
        {
            Name = name;
            DocumentType = documentType;
            DocumentNumber = documentNumber;
            SeatsTaken = seatsTaken;
            QuantityOfPeopleConected = quantity;
        }
    }
}
