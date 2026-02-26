namespace userPanelOMR.model.userPanel
{
    public class CstLiveEvent
    {
        public int IdEvent { get; set; }
        public IFormFile EventImg { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string ProjectCat { get; set; } = string.Empty;
        public string EventDescription { get; set; } = string.Empty; 
        public string EventCategory { get; set; } = string.Empty;
        public string LandMark { get; set; } = string.Empty;
        public string DatTim { get; set; } = string.Empty;
        public string Locat { get; set; } = string.Empty;
        public string OrganizerName { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string EventDiscription { get; set; } = string.Empty;
        public string EventPurpose { get; set; } = string.Empty;
        public string ParticipantsNo { get; set; } = string.Empty;
        public string PartnerOrganizations { get; set; } = string.Empty;
        public string Resources { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public List<userInfo> userId { get; set; } = new List<userInfo>();
    }
    
    public class userInfo
    {
        public int id { set; get; }
        public string Name { set; get; }
    }
}
