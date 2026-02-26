namespace userPanelOMR.model.web
{
    public class donationList
    {
        public int id;
        public int donationTran { get; set; }
        public IFormFile imgPath { get; set; }
        public string categ { get; set; }
        public string rais { get; set; }
        public string goal { get; set; }
        public string heading { get; set; }
        public string para { get; set; }
        public string progress { get; set; }
        public string link { get; set; }
    }
    public class donationDetails
    {
        public int id;
        public int donationTran { get; set; }
        public IFormFile img { get; set; }

        public string colAmt { get; set; }
        public string raised { get; set; }
        public string progres { get; set; }

        public string notes { get; set; }

        public string fix_amt1 { get; set; }
        public string fix_amt2 { get; set; }
        public string fix_amt3 { get; set; }
        public string fix_amt4 { get; set; }
        public string fix_amt5 { get; set; }
        public string sumryPera { get; set; }
        public string sumryLi1 { get; set; }
        public string sumryLi2 { get; set; }
        public string sumryLi3 { get; set; }
        public string sumryLi4 { get; set; }
        public string sumryLi5 { get; set; }
        public string sumryLi6 { get; set; }
        public string sumryLi7 { get; set; }
        public string sumryLi8 { get; set; }
        public IFormFile smr_img1 { get; set; }
        public IFormFile smr_img2 { get; set; }
        public string smr_pra1 { get; set; }
        public string smr_blockqt { get; set; }
        public string smr_pra2 { get; set; }
    }
}
