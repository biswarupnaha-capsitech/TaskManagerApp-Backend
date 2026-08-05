using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Projects.Models.Auth
{
    public static class AccountRoles
    {
        public const string Staff = "staff";
        public const string User = "user";
    }

    public static class AccountStatus
    {
        public const string Active = "active";
        public const string Inactive = "inactive";
        public const string Suspended = "suspended";
    }

    public class Account
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Email { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public string Role { get; set; } = AccountRoles.User;

        public string Status { get; set; } = "active";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
