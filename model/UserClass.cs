using System.ComponentModel.DataAnnotations;

namespace userPanelOMR.model
{
    public class UserClass
    {
        [Key]
        public int CstId { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string customerName { get; set; }
        public string contact { get; set; }
        public string Address { get; set; }
        public int donateAmt { get; set; }

    }
}
