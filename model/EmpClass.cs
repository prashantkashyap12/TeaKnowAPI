using System.ComponentModel.DataAnnotations;

namespace userPanelOMR.model
{
    public class EmpClass
    {
        [Key]
        public int EmpId { get; set; } = 0;
        public string EmpName { get; set; } = string.Empty;
        public string EmpEmail { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string contact { get; set; } = string.Empty;
        public string bankName { get; set; } = string.Empty;
    }
}
