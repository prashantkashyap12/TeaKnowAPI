namespace userPanelOMR.model
{
    public class Pg_model
    {
        public int id { get; set; }
        public string? PaymentId { get; set; } 
        public string dateTime { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        public string name { get; set; } = "";


        public string UserId { get; set; } ="";
        public string Currency { get; set; } = "";
        public int amount { get; set; } = 0;
        public string CustomNote { get; set; } = "";
        public string Package { get; set; } = "";
    }
}
