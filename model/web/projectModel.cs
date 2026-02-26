namespace userPanelOMR.model.web
{
    public class projectList
    {
        public int id { get; set; }
        public int projectTran { get; set; }
        public string head { get; set; } = "";
        public string pera { get; set; } = "";
        public IFormFile Imgpath { get; set; } 
        public string path { get; set; } = "";
    }



    // ProjectList
    public class ProjectDetailsImages
    {
        public int projectTran { get; set; }
        public IFormFile img1 { get; set; }
        public IFormFile img2 { get; set; }
        public IFormFile img3 { get; set; }
    }

    public class ProjectDetailsContent
    {
        public int projectTran { get; set; } = 0;
        public string head1 { get; set; } = "";
        public string pera1 { get; set; } = "";
        public string pera11 { get; set; } = "";
        public string head2 { get; set; } = "";
        public string pera2 { get; set; } = "";
        public string head3 { get; set; } = "";
        public string pera3 { get; set; } = "";
    }

    public class ProjectInfo
    {
        public int projectTran { get; set; }
        public string cat { get; set; }
        public string auth { get; set; }
        public string tag { get; set; }
        public string cost { get; set; }
        public string date { get; set; }
    }
    
    
    public class priojectDetails
    {
        public List<ProjectDetailsImages> projDetails1 { get; set; }
        public List<ProjectDetailsContent> projDetails2 { get; set; }
        public List<ProjectInfo> projDetails3 { get; set; }
    }
}
