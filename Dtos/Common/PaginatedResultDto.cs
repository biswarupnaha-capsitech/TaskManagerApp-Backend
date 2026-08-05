using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projects.Dtos.Common
{
    public class PaginatedResultDto<T>
    {
        public List<T> Results { get; set; } = [];
        public long Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
