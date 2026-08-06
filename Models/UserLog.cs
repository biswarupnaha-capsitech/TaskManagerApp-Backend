using Capsitech.Data.MongoDB;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using TaskManager.CollectionMetaInformation;
using TaskManager.Data;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace TaskManager.Models
{
    [BsonIgnoreExtraElements]
    public class UserLog : Record, IRecord
    {
        [BsonIgnoreIfNull, BsonIgnoreIfDefault]
        public MongoDBRef EntityRef { get; set; }

        [BsonIgnoreIfNull, BsonIgnoreIfDefault]
        public EntitiesEnum Entity { get; set; }


        [BsonIgnoreIfNull, BsonIgnoreIfDefault]
        public string Message { get; set; }

        [BsonIgnoreIfNull, BsonIgnoreIfDefault]
        public string Icon { get; set; }

        [BsonIgnoreIfNull, BsonIgnoreIfDefault]
        public UserLogActions Action { get; set; }

        [BsonIgnoreIfNull, BsonIgnoreIfDefault]
        public IdNameModel LogEntity { get; set; }

        [BsonIgnoreIfNull, BsonIgnoreIfDefault]
        public List<Changes> Changes { get; set; }

        #region Add & Delete

        [BsonIgnoreIfNull, BsonIgnoreIfDefault]
        public string EntityName { get; set; }

        [BsonIgnoreIfNull, BsonIgnoreIfDefault]
        public string IncrementalSeries { get; set; }

        #endregion

    }

    public class Changes
    {
        #region For Update Operation

        [BsonIgnoreIfNull, BsonIgnoreIfDefault]
        public string From { get; set; }
        [BsonIgnoreIfNull, BsonIgnoreIfDefault]
        public string To { get; set; }
        [BsonIgnoreIfNull, BsonIgnoreIfDefault]
        public string FieldName { get; set; }

        [BsonIgnoreIfNull, BsonIgnoreIfDefault]
        public DataTypeEnum DataType { get; set; }

        [BsonIgnoreIfNull, BsonIgnoreIfDefault]
        public string Path { get; set; }

        #endregion
    }
    public enum Change
    {
        Added,
        Updated,
        Deleted
    }
    public class UserLogDB : RecordBaseDB<UserLog, string>
    {
        public UserLogDB(DBConfiguration DBConfig) : base(DBConfig) { }
        public UserLogDB(DBConfiguration DBConfig, ClaimsPrincipal user) : base(DBConfig, user) { }

        public override string CollectionName => TaskManagerCollectionName.Logs;
    }

    public enum EntitiesEnum
    {
        [Display(Name = "")]
        Unknown = 0,

        [Display(Name = "User")]
        ApplicationUser = 1,

        [Display(Name = "Company")]
        Company = 2,

        [Display(Name = "Master")]
        Master = 3,

        [Display(Name = "WebsiteEnquires")]
        WebsiteEnquiries = 4,

        [Display(Name = "Enquiry")]
        Enquiry = 5,

        [Display(Name = "Setting Process Steps")]
        ProcessSteps = 6,

        //[Display(Name = "Edit Enquiry Information")]
        //EditEnquiryInformation = 6,

        //[Display(Name = "Enquiry Stages")]
        //EnquiryStages = 7,
    }
    public enum UserLogActions
    {
        [Display(Name = "Undefined")]
        Undefined = 0,
        [Display(Name = "Insert")]
        Insert = 1,
        [Display(Name = "Update")]
        Update = 2,
        [Display(Name = "Delete")]
        Delete = 3,
        [Display(Name = "DeleteMany")]
        DeleteMany = 4,
        [Display(Name = "Approved")]
        Approved = 5,
        [Display(Name = "Enquriy Reject")]
        EnquiryReject = 6,
        [Display(Name = "Process Step Change")]
        ProcessStepChanged = 7,
    }

    public enum DataTypeEnum
    {
        [Display(Name = "Undefined")]
        Undefined,
        [Display(Name = "DateTime")]
        DateTime,
        [Display(Name = "String")]
        String,
        [Display(Name = "Number")]
        Number,
        [Display(Name = "Boolean")]
        Boolean,
    }
}
