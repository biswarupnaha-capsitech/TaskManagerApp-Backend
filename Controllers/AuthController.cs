using System.Security.Claims;
using Capsitech;
using Capsitech.Data.Models;
using Capsitech.Data.MongoDB;
using Capsitech.Extensions;
using Capsitech.Storage;
using Capsitech.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using TaskManager.Common;
using TaskManager.Identity;
using TaskManager.Models;
using TaskManager.Models.Auth;
using TaskManager.Services;
using static TaskManager.Identity.ApplicationSignInManager;

namespace TaskManager.Controllers
{
    [Route("/API/Auth")]
    public class AuthController : ApiControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationSignInManager _signInManager;
        private readonly IEmailSender _emailSender;


        public AuthController(ILogger<AuthController> logger, IEmailSender emailSender, UserManager<ApplicationUser> userManager,
          ApplicationSignInManager signInManager, DBConfiguration dbConfig) : base(dbConfig)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            //_blobClient = blobClient;
            //_azureBlobClient = azureBlobClient;
        }


        #region UploadImage

        private async Task<FileNameModel> UploadImage(ApplicationUser applicationUser, bool operation, ApplicationUser existingData)
        {
            FileNameModel ba = null;
            try
            {
                if (applicationUser.UserImage != null && applicationUser.UserImage.Id.IsEmpty())
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        string[] array = null;
                        string contentType = null;
                        string name = null;
                        string bNameDelete = "";

                        array = applicationUser.UserImage.Path.Split(';');
                        contentType = applicationUser.UserImage.ContentType;
                        name = applicationUser.UserImage.Name;


                        if (array.Length > 1)
                        {
                            var file = new FileNameModel
                            {
                                ContentType = contentType,//User.file.ContentType,
                                Name = name, //User.file.Name,
                                Id = Guid.NewGuid().ToString(),
                                Path = ""
                            };

                            byte[] bytes = Convert.FromBase64String(array[1].Split("base64,")?[1]);
                            ms.Seek(0, SeekOrigin.Begin);
                            string bName = $"Profile/Images/{file.Id}{Path.GetExtension(file.Name)}";

                            if (existingData != null)
                            {
                                bNameDelete = $"Profile/Images/{existingData.UserImage.Id}{"."}{(existingData.UserImage.Name.Length > 0 ? existingData.UserImage.Name.Split(".")?[1] : "")}";
                            }

                            if (operation)
                                file.Path = await _azureBlobClient.UploadFileAsync(AppConfig.Current.Container_Profile, bName, bytes, file.ContentType);
                            else
                            {
                                if (existingData != null && bNameDelete.Split(".").Length > 1)
                                    await _azureBlobClient.DeleteFileWithSnapshotsAsync(AppConfig.Current.Container_Profile, bNameDelete);
                                file.Path = await _azureBlobClient.UploadFileAsync(AppConfig.Current.Container_Profile, bName, bytes, file.ContentType);
                            }
                            ba = file;

                        }
                    }
                }
                else
                {
                    ba = applicationUser.UserImage;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception in document upload", ex);
            }
            return ba;
        }
        #endregion

        #region Login
        /// <summary>
        /// Get user's access token
        /// </summary>
        /// <param name="model">Login model</param>
        /// <returns><see cref="ApiResponse{UserLogInResponse}"/></returns>
        [HttpPost("Login")]
        [AllowAnonymous]
        [RequireHttps]
        public async Task<ApiResponse<UserLogInResponse>> Login([FromBody] UserLogInRequest model)
        {
            ApiResponse<UserLogInResponse> response = new ApiResponse<UserLogInResponse>();
            try
            {
                if (ValidateModel(response))
                {
                    ApplicationUser user = await _userManager.FindByNameAsync(model.UserName);
                    if (user != null && user.Status == ApplicationUserStatus.Active) //&& !isClient 
                    {

                        // This doesn't count login failures towards account lockout
                        // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                        Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                        //result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
                        if (result.Succeeded)
                        {
                            //if (user.RoleType == RoleTypes.student)
                            //{
                            //    throw new AppModelException("User not allowed to sign-in");
                            //}
                            //string token = ((ApplicationSignInManager)_signInManager).GenerateJwtToken(user);

                            var tokens = await _signInManager.GenerateTokensAsync(user);

                            SetRefreshTokenCookie(tokens.RefreshToken);

                            response.Result = new UserLogInResponse
                            {
                                Name = user.Name.ToString(),
                                Token = tokens.AccessToken,
                                TokenExpiry = 15,
                                Id = user.Id,
                                Email = user.Email,
                                UserName = user.UserName,
                                Role = user.Role,
                                Roles = user.Roles,
                                RoleType = user.RoleType,
                                IsDefaultPassword = user.IsDefaultPassword,
                            };

                            if (user.UserImage != null)
                            {
                                response.Result.UserImage = user.UserImage;
                            }
                            string ip = GetIpAddress(), agent = GetUserAgent();
                            await new ApplicationUserDB(_dbConfig).RecordLoginDetail(user, ip, agent);

                            _logger.LogDebug($"User '{user.UserName}' token generated");
                        }
                        else
                        {
                            if (result.IsLockedOut)
                                throw new AppModelException("User account is locked out");
                            else if (result.IsNotAllowed)
                                throw new AppModelException("User not allowed to sign-in");
                            else
                                throw new AppModelException("Invalid user name or password");
                        }
                    }
                    else
                    {
                        if (user != null && user?.Status == ApplicationUserStatus.Inactive || user?.Status == ApplicationUserStatus.Deleted)
                            throw new AppModelException("Inactive User");

                        throw new AppModelException("Invalid user name or password");
                    }

                }
            }
            catch (AppModelException ex)
            {
                response.AddError(ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sign-in");
                response.AddError(ex);
            }
            return response;
        }

        [HttpPost("Refresh")]
        [AllowAnonymous]
        public async Task<ApiResponse<UserLogInResponse>> Refresh()
        {
            ApiResponse<UserLogInResponse> response = new();
            var refreshTokenFromCookie = Request.Cookies["tm-refresh"];

            try
            {
                if (string.IsNullOrEmpty(refreshTokenFromCookie))
                    throw new AppModelException("Refresh token is missing.");

                var db = new ApplicationUserDB(_dbConfig);
                var user = await db.GetAsync(u => u.RefreshToken == refreshTokenFromCookie);

                if (user == null || user.RefreshTokenExpiry <= DateTime.UtcNow)
                    throw new AppModelException("Invalid or expired refresh token.");


                var newTokens = await _signInManager.GenerateTokensAsync(user);

                user.RefreshToken = newTokens.RefreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

                await _userManager.UpdateAsync(user);

                SetRefreshTokenCookie(newTokens.RefreshToken);

                response.Result = new UserLogInResponse
                {
                    Name = user.Name.ToString(),
                    Token = newTokens.AccessToken,
                    TokenExpiry = 15,
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    Role = user.Role,
                    Roles = user.Roles,
                    RoleType = user.RoleType,
                    IsDefaultPassword = user.IsDefaultPassword,
                };
            }
            catch (AppModelException ex)
            {
                _logger.LogError(ex, "Error refreshing tokens");
                response.AddError(ex);
            }

            return response;
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("tm-refresh", refreshToken, cookieOptions);
        }
        #endregion

        #region Change password
        // update password welcome to other password for user
        [Authorize(AuthenticationSchemes = "Bearer")]
        /*       [Authorize(Roles = "ADMIN, HRMANAGER, HREXECUTIVE, EMPLOYEE, CANDIDATE")]*/
        [HttpPost("ChangePassword/{userId}")]
        public async Task<ApiResponse<bool>> ChangePassword(string userId, [FromBody] ChangePasswordViewModel item)
        {
            ApiResponse<bool> response = new ApiResponse<bool>();
            try
            {
                if (item == null)
                    throw new Exception("Please provide valid data");

                if (userId.IsEmpty() || userId == "null")
                    throw new Exception("Something went wrong, please try after some time");

                ApplicationUser currentUser = await _userManager.FindByIdAsync(userId);

                if (currentUser == null)
                    throw new Exception("Unfortunately, the requested user details could not be found. Please try again later");

                if (item.CurrentPassword == item.NewPassword)
                    throw new Exception("The old password and new password must be distinct. Kindly choose a different password");

                if (item.ConfirmPassword != item.NewPassword)
                    throw new Exception("The password you entered does not match the confirm password. Please make sure to provide a valid password");

                var result = await _userManager.ChangePasswordAsync(currentUser, item.CurrentPassword, item.NewPassword);

                if (result.Succeeded)
                {
                    response.Status = true;
                    response.Result = true;

                    if (!await new ApplicationUserDB(_dbConfig, User).UpdateAsync(currentUser.Id, Builders<ApplicationUser>.Update.Set("InitPasswordChanged", true).Set("IsDefaultPassword", false)))
                        throw new Exception("Something went wrong, please try again");
                }
                else
                {
                    response.Status = false;
                    response.Errors = new List<ApiError>();
                    foreach (var itm in result.Errors)
                    {
                        response.Errors.Add(new ApiError(itm.Description));
                    }
                }

            }
            catch (Exception ex)
            {
                response.AddError(ex.Message);
            }
            return response;
        }

        #endregion

        #region Post User
        [HttpPost("SaveUser")]
        public async Task<ApiResponse<ApplicationUser>> SaveUser([FromBody] SaveUserReq user)
        {
            ApiResponse<ApplicationUser> response = new ApiResponse<ApplicationUser>();
            try
            {
                if (user == null) throw new Exception("User is null");
                if (user.Id.IsEmpty())
                {

                    //user.UserImage = await UploadImage(user, true, null);
                }
                var db = new ApplicationUserDB(_dbConfig, User);

                IdentityResult result = new IdentityResult();

                if (user.UpdateId != null && !user.UpdateId.IsEmpty()) // Update User Information
                {

                    var userRcd = await db.GetAsync(u => u.Id == user.Id);
                    if (userRcd != null)
                    {
                        if (user.UserImage != null && !user.UserImage.Path.IsEmpty()) { }
                        //userRcd.UserImage = await UploadImage(user, true, null);
                        else { }
                        //userRcd.UserImage = await UploadImage(user, false, userRcd);
                    }
                    //userRcd.Role = user?.Role?.Trim()?.ToUpper();
                    userRcd.Role = "Admin";
                    userRcd.Roles = new List<string>() { user?.Role?.Trim()?.ToUpper() };
                    userRcd.Status = user.Status;
                    userRcd.RoleType = RoleTypes.Admin;
                    userRcd.Name = new Data.NameModel(user?.Name?.First, user?.Name?.Last);
                    //userRcd.Address = user?.Address;
                    userRcd.PhoneNumber = user?.PhoneNumber;

                    userRcd.UpdatedBy = new RecordUpdateInfo
                    {
                        Date = DateTime.UtcNow,
                        UserId = User.GetUserId(),
                        UserName = User.Identity.GetUserName()
                    };
                    result = await _userManager.UpdateAsync(userRcd);
                }
                else
                {
                    var userRcd = new ApplicationUser
                    {
                        UserName = user?.Email,
                        Roles = new List<string>() { user?.Role?.Trim()?.ToUpper() },
                        Email = user?.Email,
                        Status = ApplicationUserStatus.Active,
                        Name = new Data.NameModel(user?.Name?.First, user?.Name?.Last),
                        EmailConfirmed = true,
                        Role = user?.Role?.Trim()?.ToUpper(),
                        //Company = user?.Company,
                        RoleType = user.RoleType,
                        //Address = user?.Address,
                        CreatedBy = new RecordUpdateInfo
                        {
                            Date = DateTime.UtcNow,
                            UserId = User.GetUserId(),
                            UserName = User.Identity.GetUserName()
                        },
                        UserImage = user.UserImage,
                    };

                    var ba = await db.GetAsync(u => u.Email.ToUpper() == user.Email.ToUpper());
                    if (ba != null)
                        throw new Exception("User mail Already Exist");

                    result = await _userManager.CreateAsync(userRcd, "welcome"); // Create without password.
                    if (result.Succeeded)
                    {
                        response.Result = userRcd;
                    }

                }

            }
            catch (Exception ex)
            {
                response.AddError(ex.Message);
            }
            return response;
        }

        #endregion

        #region Get List
        /// <summary>
        /// Get Users List
        /// </summary>
        //[Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("UserList")]
        public async Task<ApiResponse<PagedData<ApplicationUser>>> GetUserList(UserType userType = UserType.All, string nameSearch = "", string roleSearch = "", string mailSearch = "", string phoneNumberSearch = "", int status = 0, int start = 0, int length = 5000)
        {
            ApiResponse<PagedData<ApplicationUser>> response = new() { Result = new PagedData<ApplicationUser>() };
            try
            {
                var filterBuilder = Builders<BsonDocument>.Filter;
                var filter = filterBuilder.Empty;

                if (status == 0) // User Active
                    filter &= filterBuilder.Eq("Status", ApplicationUserStatus.Active);
                else if (status == 1) // User Inactive
                    filter &= filterBuilder.Eq("Status", ApplicationUserStatus.Inactive);

                //filter for userNameSearch
                if (!nameSearch.IsEmpty())
                {
                    var regexName = filterBuilder.Regex("Name.First", new BsonRegularExpression(nameSearch, "i"));
                    var regexEmail = filterBuilder.Regex("Email", new BsonRegularExpression(nameSearch, "i"));

                    filter &= filterBuilder.Or(regexName, regexEmail);
                }

                if (userType != UserType.All)
                {
                    filter &= filterBuilder.Eq("RoleType", userType);
                }
                //else if(userType== UserType.Student)
                //{
                //    filter &= filterBuilder.Eq("RoleType", RoleTypes.student);
                //}

                //filter for roleSearch
                if (!roleSearch.IsEmpty())
                    filter &= filterBuilder.Or(filterBuilder.Regex("Role", new BsonRegularExpression(roleSearch, "i")));

                //filter for mailSearch
                if (!mailSearch.IsEmpty())

                    //filter for phoneNumberSearch
                    if (!phoneNumberSearch.IsEmpty())
                        filter &= filterBuilder.Or(filterBuilder.Regex("PhoneNumber", new BsonRegularExpression(phoneNumberSearch, "i")));

                var db = new ApplicationUserDB(_dbConfig);
                var qry = db.GetCollectionBson()
                    .Aggregate();

                qry = qry
                 .Sort(Builders<BsonDocument>.Sort.Descending("CreatedBy.Date"))
                 .Match(filter)
                 .Skip(start)
                 .Limit(length);

                IAggregateFluent<ApplicationUser> qryb = null;

                qryb = qry.Project<ApplicationUser>(new BsonDocument
                    {

                           {"Name", "$Name"},
                           {"Role","$Role" },
                           {"Email","$Email" },
                           {"UserName","$UserName" },
                           //{"Company","$Company" },
                           {"Status","$Status" },
                           {"PhoneNumber","$PhoneNumber" },
                           {"CreatedBy","$CreatedBy" },
                           {"LastLogin","$LastLogin" },
                           {"Address","$Address" },
                           {"RoleType","$RoleType" }
                    });


                var res = await qryb.ToListAsync();
                long idx = start + 1;
                res?.Foreach(x => x.Sno = idx++);
                response.Result = new PagedData<ApplicationUser>
                {
                    Items = res,
                    TotalRecords = ConvertUtility.ToInt32(await db.GetCollectionBson().CountDocumentsAsync(filter))
                };
            }
            catch (Exception ex)
            {
                response.AddError(ex);
            }
            return response;
        }

        #endregion


        #region Get User By Id

        [HttpGet("GetUser")]
        public async Task<ApiResponse<ApplicationUser>> GetUser(string id)
        {
            ApiResponse<ApplicationUser> apiResponse = new ApiResponse<ApplicationUser>();
            try
            {
                if (id.IsEmpty())
                    throw new Exception("User ID is Empty");

                var filterBuilder = Builders<BsonDocument>.Filter;
                var filter = filterBuilder.Eq("_id", ObjectId.Parse(id));

                var db = new ApplicationUserDB(_dbConfig);
                var qry = db.GetCollectionBson();

                var qryb = qry.Aggregate()
                           .Match(filter)
                           .Sort(Builders<BsonDocument>.Sort.Descending("CreatedBy"))

                     .Project<ApplicationUser>(new BsonDocument()
                     {
                        {"Id","$Id" },
                        {"Name","$Name" },
                        {"Role","$Role" },
                        {"Status","$Status" },
                        {"Company","$Company" },
                        {"CreatedBy","$CreatedBy" },
                        {"Email","$Email" },
                        {"PhoneNumber","$PhoneNumber" },
                        {"LastLogin","$LastLogin" },
                        { "Picture","$Picture"},
                         {"Address","$Address" },
                         {"UserImage","$UserImage"},
                         {"Rights","$Rights" },
                         { "RoleType","$RoleType"},
                         {"Departments","$Departments" },
                         {"AgentId","$AgentId" },
                         {"CallerId","$CallerId" }
                    });

                apiResponse.Result = await qryb.FirstOrDefaultAsync();

            }
            catch (Exception ex)
            {
                apiResponse.AddError(ex);
            }
            return apiResponse;
        }

        #endregion

        #region Not Use
        ///// <summary>
        ///// Forgot password
        //[HttpPost("ForgotPassword")]
        //[AllowAnonymous]
        //[RequireHttps]
        //public async Task<ActionResult<ApiResponse<bool>>> ForgotPassword([FromBody] ForgotPasswordModel model)
        //{
        //    ApiResponse<bool> response = new ApiResponse<bool>();
        //    try
        //    {
        //        if (ValidateModel(response))
        //        {
        //            var user = await _userManager.FindByNameAsync(model.Email);
        //            if (user == null)
        //            {
        //                response.Status = false;
        //                response.Result = false;
        //                throw new Exception("User Not Found");
        //            }

        //            else
        //            {
        //                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        //                await _emailSender.SendEmailWithReplyAsync(model.Email, "", "Reset Password",
        //                    $"Please reset your password by clicking here : <a href='{AppConfig.Current.AppUrl}reset-password/{user.Id}/{code}'>Reset Password</a>", "Total Time Pay");
        //                response.Status = true;
        //                response.Result = true;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error while getting-config");
        //        response.AddError(ex.Message);
        //    }
        //    return response;
        //}

        ///// <summary>
        ///// Forgot password
        //[HttpPost("ResetPassword")]
        //[AllowAnonymous]
        //[RequireHttps]
        //public async Task<ActionResult<ApiResponse<bool>>> ResetPassword([FromBody] ResetPasswordViewModel model)
        //{
        //    ApiResponse<bool> response = new ApiResponse<bool>();
        //    try
        //    {
        //        if (ValidateModel(response))
        //        {
        //            var user = await _userManager.FindByIdAsync(model.UserId);

        //            if (model.ConfirmPassword != model.Password)
        //                throw new Exception("The password you entered does not match the confirm password. Please make sure to provide a valid password");

        //            if (user == null)
        //            {
        //                response.Status = false;
        //                response.Result = false;
        //                throw new Exception("User Not Found");
        //            }

        //            else
        //            {
        //                var result = await _userManager.ResetPasswordAsync(user, model.Code, model.ConfirmPassword);
        //                await _emailSender.SendEmailWithReplyAsync(user.Email, "", "Password Changed",
        //                    $"Password Successfully changed click to login : <a href='{AppConfig.Current.AppUrl}login'>Login</a>", "Total Time Pay");
        //                response.Status = true;
        //                response.Result = true;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error while getting-config");
        //        response.AddError(ex.Message);
        //    }
        //    return response;
        //}


        //[HttpGet]
        //[AllowAnonymous]
        //public IActionResult ForgotPasswordConfirmation()
        //{
        //    return null;
        //}

        //[HttpGet]
        //[AllowAnonymous]
        //public IActionResult ResetPassword(string code = null)
        //{
        //    if (code == null)
        //    {
        //        throw new ApplicationException("A code must be supplied for password reset.");
        //    }
        //    //var model = new ResetPasswordModel { Code = code };
        //    return null;
        //}
        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<IActionResult> ConfirmEmail(string userId, string code)
        //{
        //    if (userId == null || code == null)
        //    {
        //       // return RedirectToAction(nameof(HomeController.Index), "Home");
        //        return null;
        //    }
        //    var user = await _userManager.FindByIdAsync(userId);
        //    if (user == null)
        //    {
        //        throw new ApplicationException($"Unable to load user with ID '{userId}'.");
        //    }
        //    var result = await _userManager.ConfirmEmailAsync(user, code);
        //    return null;
        //}

        #endregion

        #region to convert all student to user, not used now
        //[Authorize(AuthenticationSchemes = "Bearer")]
        //[HttpPost("StudentToUser")]
        //[RequireHttps]
        //public async Task<ApiResponse<dynamic>> StudentToUser()
        //{
        //    ApiResponse<dynamic> response = new ApiResponse<dynamic>();

        //    if (true)
        //    {
        //        var result = new
        //        {
        //            createdUser = new List<object>(),
        //            failedUser = new List<object>(),
        //            createdUserCount = 0,  // Count for created users
        //            failedUserCount = 0     // Count for failed users
        //        };

        //        var filterBuilder = Builders<Enquiry>.Filter;
        //        var enrollFilter = filterBuilder.And(
        //            filterBuilder.Eq(x => x.IsEnrolled, true),
        //            filterBuilder.Eq(x => x.RecordStatus, RecordStatusType.Undefined)
        //        );

        //        var enquiryData = await new EnquiryDB(_dbConfig, User)
        //            .GetCollection()
        //            .Aggregate()
        //            .Match(enrollFilter)
        //            .ToListAsync();

        //        try
        //        {
        //            foreach (var student in enquiryData)
        //            {
        //                if (!string.IsNullOrEmpty(student.OfficialEmail))
        //                {
        //                    var newUser = new ApplicationUser
        //                    {
        //                        UserName = student.OfficialEmail,
        //                        NormalizedUserName = student.OfficialEmail.ToUpper(),
        //                        Email = student.OfficialEmail,
        //                        NormalizedEmail = student.OfficialEmail.ToUpper(),
        //                        Name = new()
        //                        {
        //                            First = student.FullName,
        //                            Last = student.LastName
        //                        },
        //                        Role = "STUDENT",
        //                        RoleType = RoleTypes.student,
        //                        PhoneNumber = student.ContactDetail?.MobileNumber.ToString(),
        //                        LastLogin = null,
        //                        IsDefaultPassword = true
        //                    };

        //                    // Uncomment these lines to actually create users and update the database:
        //                    await _userManager.CreateAsync(newUser, "welcome");
        //                    var updateFilter = Builders<Enquiry>.Filter.Eq(e => e.Id, student.Id);
        //                    var updateDefinition = Builders<Enquiry>.Update.Set("UserId", ObjectId.Parse(newUser.Id));
        //                    await new EnquiryDB(_dbConfig, User).GetCollection().UpdateOneAsync(updateFilter, updateDefinition);

        //                    result.createdUser.Add(new
        //                    {
        //                        student.Id,
        //                        student.Name,
        //                    });
        //                }
        //                else
        //                {
        //                    result.failedUser.Add(new
        //                    {
        //                        student.Id,
        //                        student.Name,
        //                    });
        //                }
        //            }

        //            var finalResult = new
        //            {
        //                createdUserCount = result.createdUser.Count,
        //                failedUserCount = result.failedUser.Count,
        //                result.createdUser,
        //                result.failedUser,
        //            };

        //            // Set the processed result
        //            response.Result = finalResult;
        //            response.Status = true;  // Indicate success

        //        }
        //        catch (Exception ex)
        //        {
        //            response.Message = $"An error occurred: {ex.Message}";
        //            response.Status = false;  // Indicate failure
        //        }
        //    }
        //    return response;
        //}
        #endregion

        #region reset user pass to welcome
        [HttpPost("ResetUserPassword")]
        public async Task<ActionResult<ApiResponse<bool>>> ResetUserPassword(string userId)
        {
            ApiResponse<bool> response = new();
            try
            {
                if (ValidateModel(response))
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    //var res = await GetUser(userId);
                    //var user = res.Result;
                    if (user == null)
                    {
                        response.Status = false;
                        response.Result = false;
                        throw new Exception("User Not Found");
                    }

                    else
                    {
                        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                        await _userManager.ResetPasswordAsync(user, code, "welcome");
                        response.Status = true;
                        response.Result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting-config");
                response.AddError(ex.Message);
            }
            return response;
        }
        #endregion
    }
}
