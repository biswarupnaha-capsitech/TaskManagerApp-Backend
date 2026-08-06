using Capsitech.Data.MongoDB;
using TaskManager.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
    public class GenerateOTPModel
    {

        [Required]
       [EmailAddress]
    public string Email { get; set; }
    }
    /// <summary>
    /// Signin response with Jwt token
    /// </summary>
    public class UserLogInResponse
    {
        /// <summary>
        /// Sequrity Token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// User's name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Expiry Token
        /// </summary>
        public int TokenExpiry { get; set; }
        public string Id { get; set; }
        public string Role { get; set; }        
        public string Email { get; set; }        
        public string UserName   { get; set; }        
        public string EmployeeId   { get; set; }        
        public IdNameModel Company { get; set; }
        public IdNameModel Branch { get; set; }

        public bool InitPasswordChanged { get; set; }
        [BsonIgnore]
        public bool IsWFHAllowed { get; set; }    
        public ApplicationUserStatus Status { get; set; }
        public List<string> Roles { get; set; }
        //public Rights Rights { get; set; }
        public string OTP { get; set; }
        [BsonIgnoreIfDefault,BsonIgnoreIfNull]
        public ObjectId? EnquiryId { get; set; }

        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public bool IsDefaultPassword { get; set; }
        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public FileNameModel UserImage { get; set; }
        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public RoleTypes RoleType { get; set; }
    }

    /// <summary>
    /// Request to get user's access token
    /// </summary>
    public class UserLogInRequest
    {
        /// <summary>
        /// User name
        /// </summary>
        [Required]
        [EmailAddress]
        public string UserName { get; set; }

        /// <summary>
        /// User's password
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [Display(Name = "Current password")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required]
        [Display(Name = "New pasword")]
        [DataType(DataType.Password)]
        [StringLength(50, ErrorMessage = "The password must be at least 6 characters long.", MinimumLength = 6)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
    public class ChangePasswordForApp
    {
        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        [Display(Name = "Current password")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } 

        [Required]
        [Display(Name = "New pasword")]
        [DataType(DataType.Password)]
        [StringLength(50, ErrorMessage = "The password must be at least 6 characters long.", MinimumLength = 6)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string UserId { get; set; }
        [Required]
        [Display(Name = "New pasword")]
        [DataType(DataType.Password)]
        [StringLength(50, ErrorMessage = "The password must be at least 6 characters long.", MinimumLength = 6)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        [Required]
        public string Code { get; set; }
        
    }

    //model for app login request
    public class AppUserLogInRequest
    {
        /// <summary>
        /// User name
        /// </summary>
        [Required]
        [EmailAddress]
        public string UserName { get; set; }

        /// <summary>
        /// User's password
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public FcmToken Fcm_Token { get; set; }
    }






    public class UserWithTasks : ApplicationUser
    {
        public List<Models.Task> Tasks { get; set; } = new List<Models.Task>();
    }
}
