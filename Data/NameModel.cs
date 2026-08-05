using Capsitech.Extensions;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;


namespace Projects.Data
{
    /// <summary>
    /// Default model for name
    /// </summary>
    [BsonIgnoreExtraElements]
    public class NameModel
    {
        public NameModel() { }
        public NameModel(string first) => First = first;
        public NameModel(string first, string last) : this(first) => Last = last;

        /// <summary>
        /// Title
        /// </summary>
        [Display(Name = "Title")]
        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string Title { get; set; }

        /// <summary>
        /// First name
        /// </summary>
        [Display(Name = "First name")]
        [StringLength(150, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public string First { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        [Display(Name = "Last name")]
        [StringLength(150, ErrorMessage = "The {0} must be at max {1} characters long.")]
        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string Last { get; set; }

        /// <summary>
        /// Alias name
        /// </summary>
        [Display(Name = "Alias name")]
        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string Alias { get; set; }

        ///// <summary>
        ///// Get full name
        ///// </summary>
        ///// <returns>Full name</returns>
        //public string GetFullName()
        //{
        //    return ToString();
        //}

        /// <summary>
        /// Get full name
        /// </summary>
        /// <returns>Name in string type</returns>
        public override string ToString()
        {
            try
            {
                return string.Join(" ", First, Last, Alias?.Length > 0 ? " (" + Alias + ")" : "").Trim();
            }
            catch { }
            return "";
        }
        public void Trim()
        {
            if (!Title.IsEmpty())
                Title = Title.Trim();
            if (!First.IsEmpty())
                First = First.Trim();
            if (!Last.IsEmpty())
                Last = Last.Trim();
            if (!Alias.IsEmpty())
                Alias = Alias.Trim();
        }
    }
}
