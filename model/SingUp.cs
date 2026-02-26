using System.ComponentModel.DataAnnotations;

namespace userPanelOMR.model
{
    public class SingUps
    {
        [Key]
        public int userId { get; set; } = 0;
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Contact { get; set; } = "";
        public string Otp { get; set; } = "";
        public string role { get; set; } = "";
        public DateTime ExpiryDate { get; set; }
        public bool IsVerified { get; set; } 
        
    }
}
