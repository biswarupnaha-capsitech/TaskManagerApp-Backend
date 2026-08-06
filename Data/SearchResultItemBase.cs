using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TaskManager.Data
{

    public class SearchResultItemBase
    {
        /// <summary>
        /// Serial number
        /// </summary>
        public int Sno { get; set; }
        /// <summary>
        /// Record id
        /// </summary>
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
    }
}
