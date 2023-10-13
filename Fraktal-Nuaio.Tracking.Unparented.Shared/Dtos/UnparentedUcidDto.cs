namespace Fraktal_Nuaio.Tracking.Unparented.Shared.Dtos
{
    public class UnparentedUcidDto
    {
        public Guid Id { get; set; }
        public string Origin { get; set; } = null!;
        public string Ucid { get; set; } = null!;
        public string RequestJson { get; set; } = null!;
        public string JsonToQueue { get; set; } = null!;
        public DateTime GenerationDate { get; set; }

    }
}