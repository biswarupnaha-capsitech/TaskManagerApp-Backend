using MongoDB.Bson.Serialization.Attributes;

namespace Projects.Models
{
    [BsonIgnoreExtraElements]
    public class UserFiles : Data.Record
    {
    }
    public class FileNameModel
    {
        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string Id { get; set; }

        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string Name { get; set; }

        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        [BsonElement("CType")]
        public string ContentType { get; set; }

        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public long? Length { get; set; }

        //[Newtonsoft.Json.JsonIgnore]
        public string Path { get; set; }
    }

}
