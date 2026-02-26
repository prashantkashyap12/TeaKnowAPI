using System.ComponentModel.DataAnnotations;

namespace userPanelOMR.model
{
    public class DynamicForm
    {
        [Key]
        public int id { get; set; } = 0;
        public string UName { get; set; } = string.Empty;
        public string UCont { get; set; } = string.Empty;
        public string UAdd { get; set; } = string.Empty;
        public string Furl { get; set; } = string.Empty;
    }
}
