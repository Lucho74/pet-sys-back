using System.ComponentModel.DataAnnotations;

namespace Domain.Common
{
    public class PaginationQuery
    {
        [Range(1, int.MaxValue)]
        public int PageIndex { get; set; } = 1;

        [Range(1, 50)]
        public int PageSize { get; set; } = 10;

        public string? Search { get; set; }
        public bool IncludeSuspended { get; set; } = false;
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }
}
