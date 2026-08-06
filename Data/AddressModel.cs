using Capsitech.Extensions;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TaskManager.Data
{

    /// <summary>
    /// Default model for address
    /// </summary>
    [BsonIgnoreExtraElements]
    public class AddLocationModel
    {
        /// <summary>
        /// Building name or Address line 1
        /// </summary>
        [Display(Name = "Building")]
        [StringLength(150, ErrorMessage = "The {0} must be at max {1} characters long.")]
        [BsonIgnoreIfNull]
        public string Building { get; set; }

        /// <summary>
        /// Street name or Address line 2
        /// </summary>
        [Display(Name = "Street")]
        [StringLength(150, ErrorMessage = "The {0} must be at max {1} characters long.")]
        [BsonIgnoreIfNull]
        public string Street { get; set; }

        /// <summary>
        /// City/town name or Address line 3
        /// </summary>
        [Display(Name = "City")]
        [StringLength(150, ErrorMessage = "The {0} must be at max {1} characters long.")]
        [BsonIgnoreIfNull]
        public string City { get; set; }
        
        /// <summary>
        /// State id
        /// </summary>
        [Display(Name = "State")]
        [BsonIgnoreIfNull]
        public string State { get; set; }
        
        /// <summary>
        /// Postal code
        /// </summary>
        [Display(Name = "Postcode")]
        [StringLength(150, ErrorMessage = "The {0} must be at max {1} characters long.")]
        [BsonIgnoreIfNull]
        public string Postcode { get; set; }

        /// <summary>
        /// Country id
        /// </summary>
        [Display(Name = "Country")]
        [BsonIgnoreIfNull]
        public string Country { get; set; }

        /// <summary>
        /// Get full address
        /// </summary>
        /// <param name="isHtmlBr">Get HTML output</param>
        /// <returns>String address</returns>
        public string GetAddress(bool isHtmlBr = false)
        {
            List<string> list = new List<string>
            {
                Building,
                Street,
                City,
                State,
                Postcode,
                Country
            };
            list.RemoveAll(s => s.IsEmpty());
            return string.Join(isHtmlBr ? "<br>" : ", ", list);
        }
    }
}
