namespace Domain.Common
{
    public class PetQuery : PaginationQuery
    {
        public string? Specie { get; set; }
        public string? Breed { get; set; }
    }
}
