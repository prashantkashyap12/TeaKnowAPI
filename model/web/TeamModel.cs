namespace userPanelOMR.model.web
{
    public class TeamModel
    {
        public int Id { get; set; }
        public int TeamTran { get; set; }
        public IFormFile imgPath { get; set; }
        public string name { get; set; }
        public string position { get; set; }
    }
}
