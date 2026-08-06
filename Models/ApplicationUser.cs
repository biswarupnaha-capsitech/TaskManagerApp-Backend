using Capsitech.Data.Models;
using Capsitech.Data.MongoDB;
using Capsitech.Data.MongoDB.Identity;
using Capsitech.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using TaskManager.Services;
using TaskManager.Data;
using TaskManager.Common;

namespace TaskManager.Models
{
    [BsonIgnoreExtraElements]
    public class ApplicationUser : IdentityUser, IRecord
    {
        public ApplicationUser() : base()
        {
            Name = new Data.NameModel();
        }

        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public DateTime? OTPExpiry { get; set; }  // Nullable in case no OTP is set

        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public ApplicationUserStatus Status { get; set; }

        /// <summary>
        /// User first and last name
        /// </summary>
        public TaskManager.Data.NameModel Name { get; set; }
        /// <summary>
        /// User primary role
        /// </summary>
        /// 
        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string Otp { get; set; } = "";

        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string Role { get; set; }


        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public RoleTypes RoleType { get; set; }
        //public IdNameModel Employee { get; set; }
        [BsonIgnoreIfNull]
        public string Address { get; set; }
        public IdNameModel Company { get; set; }
        [BsonIgnoreIfNull]
        public RecordUpdateInfo CreatedBy { get; set; }
        [BsonIgnoreIfNull]
        public RecordUpdateInfo UpdatedBy { get; set; }


        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public bool IsDefaultPassword { get; set; } = true;


        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public ApplicationUserLastLoginDetail LastLogin { get; set; }

        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public ApplicationUserPicture Picture { get; set; }
      
        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public FileNameModel UserImage { get; set; }

        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string UpdateId { get; set; }
        [BsonIgnore]    
        public long Sno { get; set; }
        public string GetImageThumb(string defaultUrl = "")
        {
            return Picture?.GetImageThumb(defaultUrl);
        }
        [BsonIgnoreIfNull]
        public List<string> FireBaseTokens { get; set; }


        /// <summary>
        /// Check and assign new If (if required)
        /// </summary>
        public void AssignNewId()
        {
            if (!HasId())
                Id = ObjectId.GenerateNewId().ToString();
        }
        /// <summary>
        /// Check weather object has assigned Id
        /// </summary>
        /// <returns>bool</returns>
        public bool HasId() => !Id.IsEmpty();

        public override string ToString() => $"{Name?.ToString()} ({Role} user)";
        public ApplicationUserShort GetShort()
        {
            return new ApplicationUserShort
            {
                Email = this.Email,
                FullName = this.Name?.ToString(),
                Id = this.Id,
                Name = this.Name,
                Role = this.Role,
                Roles = this.Roles,
                Status = this.Status,
                UserName = this.UserName
            };
        }

        public string RefreshToken { get; set; }
        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public DateTime? RefreshTokenExpiry { get; set; }
    }

    public class SaveUserReq : IdentityUser, IRecord
    {
        public SaveUserReq() : base()
        {
            Name = new TaskManager.Data.NameModel();
        }
        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string UpdateId { get; set; }
        public TaskManager.Data.NameModel Name { get; set; }

        [BsonIgnoreIfDefault]
        public string Email { get; set; }
        public RoleTypes RoleType { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }

        [BsonIgnoreIfNull]
        public FileNameModel UserImage { get; set; }

        [BsonIgnoreIfDefault]
        public ApplicationUserStatus Status { get; set; }
        [BsonIgnoreIfNull]
        public RecordUpdateInfo CreatedBy { get; set; }
        [BsonIgnoreIfNull]
        public RecordUpdateInfo UpdatedBy { get; set; }


        /// <summary>
        /// Check and assign new If (if required)
        /// </summary>
        public void AssignNewId()
        {
            if (!HasId())
                Id = ObjectId.GenerateNewId().ToString();
        }
        /// <summary>
        /// Check weather object has assigned Id
        /// </summary>
        /// <returns>bool</returns>
        public bool HasId() => !Id.IsEmpty();

    }

    public class FcmToken
    {
        public string Token { get; set; }
        public LoginMedium Medium { get; set; }
    }
    //User profile picture
    public class ApplicationUserPicture
    {
        [BsonElement("Image")]
        public string ImagePath { get; set; }
        [BsonElement("Thumb")]
        public string ThumbPath { get; set; }
        public string GetImageThumb(string defaultUrl = "")
        {
            return ThumbPath.IsEmpty() ? defaultUrl : ThumbPath;
        }
    }
    //user last login detail
    [BsonIgnoreExtraElements]
    public class ApplicationUserLastLoginDetail
    {
        [BsonIgnoreIfDefault,BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Time { get; set; }
        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string IP { get; set; }
        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string Browser { get; set; }

        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string CountryCode { get; set; }
        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string CountryName { get; set; }
        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string CityName { get; set; }

        public static async Task<ApplicationUserLastLoginDetail> GetLoginDetail(string ip, string browser)
        {
            var ipDetail = await IPLocationService.GetGeoLocation(ip);
            var detail = new ApplicationUserLastLoginDetail
            {
                Time = DateTime.Now,
                Browser = browser,
                IP = ip,
                CityName = ipDetail.CityName,
                CountryCode = ipDetail.CountryCode,
                CountryName = ipDetail.CountryName
            };

            return detail;
        }
    }
    [BsonIgnoreExtraElements]
    public class ApplicationUserShort : IdNameModel<TaskManager.Data.NameModel>
    {
        string fullName;
        public string FullName
        {
            get => fullName.IsEmpty() ? Name?.ToString() : fullName;
            set => fullName = value;
        }
        public IdNameModel Employee { get; set; }
        public string Role { get; set; }
        public List<string> Roles { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string ThumbUrl => Picture?.GetImageThumb();
        public ApplicationUserPicture Picture { get; set; }
        public ApplicationUserStatus Status { get; set; }
        public string GetFirstName()
        {
            if (Name != null)
                return Name.First;
            if (!FullName.IsEmpty())
                return FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries)?.Length > 0 ? FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0] : "";
            return "";
        }
        public bool IsEmail(string email)
        {
            return Email?.Equals(email, StringComparison.CurrentCultureIgnoreCase) == true || UserName?.Equals(email, StringComparison.CurrentCultureIgnoreCase) == true;
        }
    }

    /// <summary>
    /// User search result item
    /// </summary>
    [BsonIgnoreExtraElements]
    public class UserSearchResultItem : SearchResultItemBase
    {
        /// <summary>
        /// User's name
        /// </summary>
        public TaskManager.Data.NameModel Name { get; set; }
        /// <summary>
        /// User role
        /// </summary>
        public string Role { get; set; }
        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; }
    }
    public class SelectListItemEqualityComparer : IEqualityComparer<SelectListItem>
    {
        public bool Equals(SelectListItem x, SelectListItem y)
        {
            // Two items are equal if their keys are equal.
            return x.Value == y.Value;
        }

        public int GetHashCode(SelectListItem obj)
        {
            return obj.Value?.GetHashCode() ?? 0;
        }
    }

    /// <summary>
    /// Temporary data handler, to handel only read requests.
    /// <para>Please don't use this class to create/update or delete user</para>
    /// </summary>
    public class ApplicationUserDB : RecordDB<ApplicationUser>
    {
        public ApplicationUserDB(DBConfiguration DBConfig) : base(DBConfig) { }
        public ApplicationUserDB(DBConfiguration DBConfig, ClaimsPrincipal user) : base(DBConfig, user) { }

        public override string CollectionName => "Users";
        public override EntitiesEnum LogEntity => EntitiesEnum.ApplicationUser;

        /// <summary>
        /// Get all 
        /// </summary>
        /// <returns><see cref="List{ApplicationUser}"/></returns>
        public async Task<List<ApplicationUser>> GetAll() => await GetAllAsync(p => true);
        protected override string LogMessage(ApplicationUser Record, UserLogActions Action)
        {
            return Action switch
            {
                UserLogActions.Insert => $"User '{Record.UserName}' with role '{Record.Role}' added",
                UserLogActions.Update => $"User '{Record.UserName}' updated",
                UserLogActions.Delete => $"User '{Record.UserName}' deleted",
                _ => "",
            };

        }
        public async Task<bool> RecordLoginDetail(string id, string ip, string browser)
        {
            if (id.IsEmpty())
                return false;
            var user = await GetAsync(id);
            return await RecordLoginDetail(user, ip, browser);
        }
        public async Task<bool> RecordLoginDetail(ApplicationUser user, string ip, string browser)
        {
            bool result = false;
            try
            {
                if (user != null)
                {
                    user.LastLogin = await ApplicationUserLastLoginDetail.GetLoginDetail(ip, browser);
                    var ur = await _collection.UpdateOneAsync(Builders<ApplicationUser>.Filter.Eq("Id", user.Id), Builders<ApplicationUser>.Update.Set("LastLogin", user.LastLogin));
                    if (ur != null && ur.ModifiedCount > 0)
                        result = true;
                }
            }
            catch (Exception ex)
            {
                LastError = ex;
            }
            return result;
        }

        public async Task<string> GetName(string id)
        {
            var qry = GetCollectionBson().Aggregate()
                .Match(Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(id)))
                .Project(new BsonDocument
                {
                    {"_id", 1},
                    {"Name", BsonDocument.Parse("{'$concat': [ '$Name.First', ' ', { $ifNull: [ '$Name.Last', '' ] } ]}")}
                });
            var name = await qry.FirstOrDefaultAsync();
            if (name != null)
                return name["Name"].ToString();

            return "";
        }


        public async Task<PagedData<UserSearchResultItem>> GetSearchResult(string term)
        {
            LastError = null;
            if (term.IsEmpty())
                return null;
            try
            {
                PagedData<UserSearchResultItem> pagedData = new PagedData<UserSearchResultItem>();

                var builders = Builders<BsonDocument>.Filter;
                var terms = GetBsonRegEx(term);
                var qry = GetCollectionBson().Aggregate()
                    .AppendStage<BsonDocument>(BsonDocument.Parse("{ $addFields: { FullName: {'$concat': [ '$Name.First', ' ', { $ifNull: [ '$Name.Last', '' ] } ]} } }"))
                    .Match(builders.Or(
                            builders.Regex("FullName", terms),
                            builders.Regex("Email", terms)
                        ));

                var count = await qry.Group(new BsonDocument
                {
                    { "_id", "_id" },
                    {"count", new BsonDocument("$sum", 1)}
                }).FirstOrDefaultAsync();
                pagedData.TotalRecords = count != null ? count["count"].ToInt32() : 0;
                if (pagedData.TotalRecords > 0)
                {
                    pagedData.Items = await qry.Project<UserSearchResultItem>(new BsonDocument
                    {
                        {"_id", 1},
                        {"Name", 1},
                        {"Email", 1},
                        {"PhoneNumber", 1},
                        {"RegDate", "$CreatedBy.Date"},
                        {"Role", 1},
                    }).ToListAsync();
                }
                return pagedData;
            }
            catch (Exception ex)
            {
                LastError = ex;
            }
            return null;
        }


        /// <summary>
        /// Get all active users by role
        /// </summary>
        /// <param name="roles">Roles to check for the user</param>
        /// <returns><see cref="List{ApplicationUserShort}"/></returns>
        public async Task<List<ApplicationUserShort>> GetByRole(params string[] roles) => await GetByRole(false, roles);
        /// <summary>
        /// Get all active users by role
        /// </summary>
        /// <param name="withConfig">With user config settings</param>
        /// <param name="roles">Roles to check for the user</param>
        /// <returns><see cref="List{ApplicationUserShort}"/></returns>
        public async Task<List<ApplicationUserShort>> GetByRole(bool withConfig, params string[] roles) => await GetByRole(withConfig, null, roles);
        /// <summary>
        /// Get all active users by role
        /// </summary>
        /// <param name="withConfig">With user config settings</param>
        /// <param name="roles">Roles to check for the user</param>
        /// <param name="extraUserIdsToSelect">User ids to select other then role</param>
        /// <returns><see cref="List{ApplicationUserShort}"/></returns>
        public async Task<List<ApplicationUserShort>> GetByRole(bool withConfig, IEnumerable<string> extraUserIdsToSelect, params string[] roles) => await GetByRole(withConfig, extraUserIdsToSelect, ObjectId.Empty, roles);
        /// <summary>
        /// Get all active users by role
        /// </summary>
        /// <param name="withConfig">With user config settings</param>
        /// <param name="roles">Roles to check for the user</param>
        /// <param name="extraUserIdsToSelect">User ids to select other then role</param>
        /// <param name="forTeamId">Team id to filter users</param>
        /// <returns><see cref="List{ApplicationUserShort}"/></returns>
        public async Task<List<ApplicationUserShort>> GetByRole(bool withConfig, IEnumerable<string> extraUserIdsToSelect, ObjectId forTeamId, params string[] roles)
        {
            if (roles == null || roles.Length <= 0)
                return null;

            List<ApplicationUserShort> users = null;
            try
            {
                var projectDoc = new BsonDocument
                {
                    {"_id", 1},
                    {"FullName", 1},
                    {"Email", 1},
                    {"Role", 1},
                    {"Roles", 1},
                    {"Status", 1}
                };
                if (withConfig)
                {
                    projectDoc.Add("Picture", 1);
                }
                var fb = Builders<BsonDocument>.Filter;
                var filter = fb.Nin("Status", new int[] { (int)ApplicationUserStatus.Inactive, (int)ApplicationUserStatus.Deleted }) & fb.AnyIn("Roles", roles);
                if (extraUserIdsToSelect?.Count() > 0)
                    filter = fb.Or(filter, fb.In("_id", extraUserIdsToSelect.Where(i => !i.IsEmpty()).Select(i => ObjectId.Parse(i))));
                if (forTeamId != ObjectId.Empty)
                    filter &= fb.Eq("Teams._id", forTeamId);
                var qry = GetCollectionBson().Aggregate()
                    .Match(filter)
                    .AppendStage<BsonDocument>(BsonDocument.Parse("{ $addFields: { FullName: {'$concat': [ '$Name.First', ' ', { $ifNull: [ '$Name.Last', '' ] } ]} } }"))
                    .Project<ApplicationUserShort>(projectDoc);
                users = await qry.ToListAsync();
            }
            catch (Exception ex)
            {
                LastError = ex;
            }
            return users;
        }

        /// <summary>
        /// Get users by email addresses
        /// </summary>
        /// <param name="emails">Email address to search the user</param>
        /// <returns><see cref="List{ApplicationUserShort}"/></returns>
        public async Task<List<ApplicationUserShort>> GetByEmail(params string[] emails)
        {
            if (emails == null || emails.Length <= 0)
                return null;

            List<ApplicationUserShort> users = null;
            try
            {
                LastError = null;

                var fb = Builders<BsonDocument>.Filter;
                var fd = fb.Nin("Status", new int[] { (int)ApplicationUserStatus.Inactive, (int)ApplicationUserStatus.Deleted });

                //filter contacts for user access
                if (CurrentUser?.Identity?.IsAuthenticated == true && CurrentUser?.IsInRole(ApplicationRoleNames.Admin) == false && CurrentUser?.IsInRole(ApplicationRoleNames.HrManager) == false)
                {
                    List<string> teamIds = new List<string>();
                    if (CurrentUser.Identity?.IsAuthenticated == true)
                    {
                        Claim c = ((ClaimsIdentity)CurrentUser.Identity).FindFirst("Teams");
                        if (c != null && !c.Value.IsEmpty())
                            teamIds = new List<string>(c.Value.Split(","));
                    }

                    var ids = teamIds.Select(t => new ObjectId(t));
                    fd &= fb.ElemMatch("Teams", fb.In("_id", ids));
                }
                fd &= fb.Or(fb.In("UserName", emails), fb.In("Email", emails));

                //get collection
                var coll = this.GetCollectionBson();
                //prepare query object
                var qry = coll.Aggregate(new AggregateOptions() { Collation = new Collation("en", strength: CollationStrength.Secondary) })
                    .Match(fd)
                    .Sort(Builders<BsonDocument>.Sort.Ascending("Name.First"));
                //get the result
                users = await qry
                    .Project<ApplicationUserShort>(new BsonDocument
                    {
                        {"_id", 1},
                        {"Name", 1},
                        {"Role", 1},
                        {"Email", 1},
                        {"UserName", 1}
                    }).ToListAsync();
            }
            catch (Exception ex)
            {
                LastError = ex;
            }
            return users;
        }

        /// <summary>
        /// Get user employeeId
        /// </summary>
        /// <param name="userId">userId</param>
        /// <returns><see cref="employeeId"/></returns>
        //public async Task<string> GetUserEmployeeId(string userId)
        //{
        //    string employeeId = null;
        //    if(!userId.IsEmpty() && userId != "null")
        //    {
        //        var qry = GetCollectionBson().Aggregate()
        //            .Match(Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(userId)))
        //            .Project<ApplicationUser>(new BsonDocument
        //            {
        //                { "_id", 0 },
        //                { "EmployeeId", 1 },
        //            });
        //        var userData = await qry.FirstOrDefaultAsync();
        //        if(userData != null && userData?.EmployeeId != null)
        //            employeeId = userData.EmployeeId;
        //    }            
        //    return employeeId;
        //}

        /// <summary>
        /// Get user by employeeId
        /// </summary>
        /// <param name="empId">empId</param>
        /// <returns><see cref="ApplicationUser"/></returns>
        public async Task<ApplicationUser> GetUserByEmployeeId(string empId)
        {
            ApplicationUser response = null;
            if (!empId.IsEmpty() && empId != "null")
            {
                var qry = GetCollectionBson().Aggregate()
                    .Match(Builders<BsonDocument>.Filter.Eq("EmployeeId", ObjectId.Parse(empId)))
                    .As<ApplicationUser>();

                var userData = await qry.FirstOrDefaultAsync();
                if (userData != null)
                    response = userData;
            }
            return response;
        }

        /// <summary>
        /// Get user by userName
        /// </summary>
        /// <param name="UserName">UserName</param>
        /// <returns><see cref="ApplicationUser"/></returns>
        public async Task<ApplicationUser> GetUserByUserName(string userName)
        {
            ApplicationUser response = null;
            if (!userName.IsEmpty() && userName != "null")
            {
                var qry = GetCollectionBson().Aggregate()
                    .Match(Builders<BsonDocument>.Filter.Eq("UserName", userName))
                    .As<ApplicationUser>();

                var userData = await qry.FirstOrDefaultAsync();
                if (userData != null)
                    response = userData;
            }
            return response;
        }

    }

    public static class ApplicationRoleNames
    {
        public const string SuperAdmin = "SUPERADMIN";
        public const string Admin = "ADMIN";
        public const string HrManager = "HRMANAGER";
        public const string HrExecutive = "HREXECUTIVE";
        public const string Employee = "EMPLOYEE";
        public const string Candidate = "CANDIDATE";
        public const string TeamLeader = "TEAMLEADER";
        public const string Interviwer = "INTERVIEWER";              
    }
    public enum ApplicationUserStatus
    {
        [Display(Name = "Active")]
        Active = 0,

        [Display(Name = "Inactive")]
        Inactive = 1,

        [Display(Name = "Deleted")]
        Deleted = 2,
    }
    public enum LoginMedium
    {
        [Display(Name = "Android")]
        Android = 1,
        [Display(Name = "Ios")]
        Ios = 2
    }

    /// <summary>
    /// refresh fcm token
    /// </summary>
    public class RefreshFcmTokenResponse
    {
        public string UserId { get; set; }
        public FcmToken Fcm_Token { get; set; }
    }
}
