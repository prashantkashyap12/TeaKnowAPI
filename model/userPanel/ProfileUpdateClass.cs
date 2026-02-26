namespace userPanelOMR.model.userPanel
{
    public class ProfileUpdate
    {
        public int Id { get; set; }
        public int userId { get; set; }
        public IFormFile ProfileImg { get; set; } 
        public string Father_Name { get; set; } = "";
        public string DOB { get; set; } = "";
        public string Gender { get; set; } = "";
        public string MaritalStatus { get; set; } = "";
        public string Education { get; set; } = "";
        public string Profession { get; set; } = "";
        public string whatApp { get; set; } = "ok";
        public string AddressFull { get; set; } = "";
        public string Member_Type { get; set; } = "";
        public string join_cat { get; set; } = "";
        public string Already_Join { get; set; } = "";
        public string designation { get; set; } = "";
    }

    public class ProfIsEdit
    {
        public int uid { get; set; }
        public string isDocView { get; set; } = "";
        public string designation { get; set; } = "";

    }
}