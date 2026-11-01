namespace Api.DTO
{
    public class Register
    {
        public string Name { get; set; }
        public int DocumentType { get; set; }
        public int DocumentNumber { get; set; }
        public List<string> SeatsTaken { get; set; }
    }
}
