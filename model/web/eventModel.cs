namespace userPanelOMR.model.web
{

    public class eventList
    {
        public int id { get; set; }
        public int eventTran { get; set; }
        public IFormFile imgPath { get; set; }
        public string date { get; set; } = "";  // Use string if you're directly binding raw date string like "2023-10-01"
        public string heading { get; set; } = "";
        public string pera { get; set; } = "";
        public string place { get; set; } = "";
        public string address { get; set; } = "";
        public string link { get; set; } = "";
        public string linkText { get; set; } = "";
    }
    public class eventDetails
    {
        public int Id { get; set; }
        public int eventTran {get; set;}
        public IFormFile img { get; set; }
        public string dadicat { get; set; } = "";
        public string head1 { get; set; } = "";
        public string pera11 { get; set; } = "";
        public string pera12 { get; set; }
        public string head2 { get; set; } = "";
        public string pera22 { get; set; } = "";
        public string li1 { get; set; } = "";
        public string li2 { get; set; } = "";
        public string li3 { get; set; } = "";
        public string li4 { get; set; } = "";
        public string li5 { get; set; } = "";
        public string otherHead { get; set; } = "";
        public string parti1 { get; set; } = "";
        public string parti12 { get; set; } = "";
        public string parti13 { get; set; } = "";
        public string parti14 { get; set; } = "";
        public string parti15 { get; set; } = "";
        public string parti16 { get; set; } = "";
        public string parti17 { get; set; } = "";
    }   
}
