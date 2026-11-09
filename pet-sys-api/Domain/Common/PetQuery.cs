namespace Domain.Common
{
    public class PetQuery : PaginationQuery
    {
        public int? SpeciesId { get; set; }
        public int? BreedId { get; set; }
    }
}
