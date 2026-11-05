using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common
{
    public class PaginatedList<T>(List<T> items, int pageIndex, int totalPage)
    {
        public List<T> Items { get; set; } = items;
        public int PageIndex { get; set; } = pageIndex;
        public int TotalPages { get; set; } = totalPage;
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

    }
}
