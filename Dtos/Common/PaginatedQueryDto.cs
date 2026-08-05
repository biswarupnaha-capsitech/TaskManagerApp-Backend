using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Projects.Dtos.Common
{
    public class PaginatedQueryDto
    {
        public string? Search { get; set; }

        [DefaultValue(1)]
        [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1")]
        public int Page { get; set; } = 1;

        /// <summary>
        /// Number of items per page. Use 0 or any negative number to fetch all items.
        /// </summary>
        [DefaultValue(10)]
        public int PageSize { get; set; } = 10;

        [JsonIgnore] // Ignore in JSON serialization (Swagger and API responses)
        [BindNever] // Ignore in model binding (request binding)
        public bool FetchAll => PageSize <= 0;
    }
}
