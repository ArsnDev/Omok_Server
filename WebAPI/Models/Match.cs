namespace WebAPI.Models
{
    public class Match
    {
        public int MatchId { get; set; }
        public int WinnerId { get; set; }
        public int LoserId { get; set; }
        public DateTime MatchDate { get; set; }
    }
}