namespace SI24004.Models.Requests
{
    public class SvhcSubstanceRequest
    {
        public string SubstanceName { get; set; }
        public string CasNo { get; set; }
        public string EcNo { get; set; }
        public string ReasonForInclusion { get; set; }
        public string Uses { get; set; }
        public DateOnly? SvhcCandidate { get; set; }
    }

}
