using System.ComponentModel.DataAnnotations;

namespace userPanelOMR.model.web
{
    public class blogList
    {
        [Key]
        public int Id { get; set; }
        public int blogid { get; set; }
        public IFormFile imgPath { get; set; }
        public string date { get; set; } = "";
        public string rights { get; set; } = "";
        public string type { get; set; } = "";
        public string link { get; set; } = "";
        public string linkText1 { get; set; } = "";
        public string linkText2 { get; set; } = "";
    }

    public class blogDetails
    {
        [Key]
        public int Id { get; set; } 
        public string blogTran { get; set; } = "";
        public IFormFile img { get; set; }
        public string rigths { get; set; } = "";
        public string type { get; set; } = "";
        public string head1 { get; set; } = "";
        public string pera1 { get; set; } = "";
        public string pera2 { get; set; } = "";
        public string blockquate { get; set; } = "";
        public string pera3 { get; set; } = "";
        public IFormFile blogImg1 { get; set; }
        public IFormFile blogImg2   { get; set; }
        public string pera4 { get; set; } = "";
        public string fbLink { get; set; } = "";
        public string twLink { get; set; } = "";
        public string linkLink { get; set; } = "";
        public string instLink { get; set; } = "";

    }
}
