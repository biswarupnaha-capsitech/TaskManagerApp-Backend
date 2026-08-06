using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManager.Models
{
    public class PersonName
    {
        public string First { get; set; } = string.Empty;
        public string? Middle { get; set; }
        public string Last { get; set; } = string.Empty;

        public string GetFullName(bool? includeMiddle = true)
        {
            if (includeMiddle == true && !string.IsNullOrWhiteSpace(Middle))
                return $"{First} {Middle} {Last}";
            return $"{First} {Last}";
        }
    }
}
