using System.Collections;

namespace WebApplication.Core.Common.Models
{
    public class PaginatedDto<T> where T : IEnumerable
    {
        public T Data { get; set; } = default!;
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public bool HasNextPage { get; set; }
    }
}
