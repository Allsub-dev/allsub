namespace AllSub.Common.Models
{
    public record ServiceData
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public ServiceType Type { get; set; }
        public string? Url { get; set; }
        public string? ImageUrl { get; set;}
        public int Relevance { get; set; }
    }
}