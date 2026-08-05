using Capsitech.Extensions;
using Projects.Data;
using Projects.Models;
using System.ComponentModel.DataAnnotations;

namespace Projects.Common;


public enum TimeFormat
{
    Hour,
    Minute,
    Second
}

public enum ApprovalStatus
{
    Rejected,
    OptionalOut,
    OnHold,
    Enrolled,
    InterestedOne,
    All
}

public enum ResponseType
{
    IdName,
    Complete,
    Filtered
}



//public enum IndianState
//{   
//    [Display(Name = "Andaman and Nicobar Islands")]
//    AndamanAndNicobarIslands,

//    [Display(Name = "Andhra Pradesh")]
//    AndhraPradesh,

//    [Display(Name = "Arunachal Pradesh")]
//    ArunachalPradesh,

//    [Display(Name = "Assam")]
//    Assam,

//    [Display(Name = "Bihar")]
//    Bihar,

//    [Display(Name = "Chandigarh")]
//    Chandigarh,

//    [Display(Name = "Chhattisgarh")]
//    Chhattisgarh,

//    [Display(Name = "Dadra and Nagar Haveli")]
//    DadraAndNagarHaveli,

//    [Display(Name = "Daman and Diu")]
//    DamanAndDiu,

//    [Display(Name = "Delhi")]
//    Delhi,

//    [Display(Name = "Goa")]
//    Goa,

//    [Display(Name = "Gujarat")]
//    Gujarat,

//    [Display(Name = "Haryana")]
//    Haryana,

//    [Display(Name = "Himachal Pradesh")]
//    HimachalPradesh,

//    [Display(Name = "Jammu and Kashmir")]
//    JammuAndKashmir,

//    [Display(Name = "Jharkhand")]
//    Jharkhand,

//    [Display(Name = "Karnataka")]
//    Karnataka,

//    [Display(Name = "Kerala")]
//    Kerala,

//    [Display(Name = "Lakshadweep")]
//    Lakshadweep,

//    [Display(Name = "Madhya Pradesh")]
//    MadhyaPradesh,

//    [Display(Name = "Maharashtra")]
//    Maharashtra,

//    [Display(Name = "Manipur")]
//    Manipur,

//    [Display(Name = "Meghalaya")]
//    Meghalaya,

//    [Display(Name = "Mizoram")]
//    Mizoram,

//    [Display(Name = "Nagaland")]
//    Nagaland,

//    [Display(Name = "Odisha")]
//    Odisha,

//    [Display(Name = "Puducherry")]
//    Puducherry,

//    [Display(Name = "Punjab")]
//    Punjab,

//    [Display(Name = "Rajasthan")]
//    Rajasthan,

//    [Display(Name = "Sikkim")]
//    Sikkim,

//    [Display(Name = "Tamil Nadu")]
//    TamilNadu,

//    [Display(Name = "Telangana")]
//    Telangana,

//    [Display(Name = "Tripura")]
//    Tripura,

//    [Display(Name = "Uttar Pradesh")]
//    UttarPradesh,

//    [Display(Name = "Uttarakhand")]
//    Uttarakhand,

//    [Display(Name = "West Bengal")]
//    WestBengal
//}

public enum IndianState
{
    [Display(Name = "None")]
    None = 0,

    [Display(Name = "Andaman and Nicobar Islands")]
    AndamanAndNicobarIslands = 1,

    [Display(Name = "Andhra Pradesh")]
    AndhraPradesh = 2,

    [Display(Name = "Arunachal Pradesh")]
    ArunachalPradesh = 3,

    [Display(Name = "Assam")]
    Assam = 4,

    [Display(Name = "Bihar")]
    Bihar = 5,

    [Display(Name = "Chandigarh")]
    Chandigarh = 6,

    [Display(Name = "Chhattisgarh")]
    Chhattisgarh = 7,

    [Display(Name = "Dadra and Nagar Haveli")]
    DadraAndNagarHaveli = 8,

    [Display(Name = "Daman and Diu")]
    DamanAndDiu = 9,

    [Display(Name = "Delhi")]
    Delhi = 10,

    [Display(Name = "Goa")]
    Goa = 11,

    [Display(Name = "Gujarat")]
    Gujarat = 12,

    [Display(Name = "Haryana")]
    Haryana = 13,

    [Display(Name = "Himachal Pradesh")]
    HimachalPradesh = 14,

    [Display(Name = "Jammu and Kashmir")]
    JammuAndKashmir = 15,

    [Display(Name = "Jharkhand")]
    Jharkhand = 16,

    [Display(Name = "Karnataka")]
    Karnataka = 17,

    [Display(Name = "Kerala")]
    Kerala = 18,

    [Display(Name = "Lakshadweep")]
    Lakshadweep = 19,

    [Display(Name = "Madhya Pradesh")]
    MadhyaPradesh = 20,

    [Display(Name = "Maharashtra")]
    Maharashtra = 21,

    [Display(Name = "Manipur")]
    Manipur = 22,

    [Display(Name = "Meghalaya")]
    Meghalaya = 23,

    [Display(Name = "Mizoram")]
    Mizoram = 24,

    [Display(Name = "Nagaland")]
    Nagaland = 25,

    [Display(Name = "Odisha")]
    Odisha = 26,

    [Display(Name = "Puducherry")]
    Puducherry = 27,

    [Display(Name = "Punjab")]
    Punjab = 28,

    [Display(Name = "Rajasthan")]
    Rajasthan = 29,

    [Display(Name = "Sikkim")]
    Sikkim = 30,

    [Display(Name = "Tamil Nadu")]
    TamilNadu = 31,

    [Display(Name = "Telangana")]
    Telangana = 32,

    [Display(Name = "Tripura")]
    Tripura = 33,

    [Display(Name = "Uttar Pradesh")]
    UttarPradesh = 34,

    [Display(Name = "Uttarakhand")]
    Uttarakhand = 35,

    [Display(Name = "West Bengal")]
    WestBengal = 36
}

public enum RoleTypes
{
    Undefined,
    Admin,
    Student,
    Counsellor,
    Mentor,
    Accountant,
    Teacher
}

public enum StatusEnum
{
    [Display(Name = "Undefined")]
    Undefined,
    [Display(Name = "Pending")]
    Pending,
    [Display(Name = "Approve")]
    Approved,
    [Display(Name = "Cancel")]
    Cancelled,
    [Display(Name = "Reject")]
    Rejected
}

public enum ActionBioType
{
    Undefined,
    AssignAccessOnly,
    AssignAccess,
    RemoveAccess,
    EnableAttSync,
    DisableAttSync
}

public enum BioDeviceOptionsEnum
{
    all,
    accessOnly,
    accessAndAttendance,
}

public enum UserType
{
    [Display(Name = "All")]
    All = 0,
    [Display(Name = "Admin")]
    Admin = 1,
    [Display(Name = "Student")]
    Student = 2,
    [Display(Name ="Counsellor")]
    Counsellor,
    [Display(Name ="Mentor")]
    Mentor,
    [Display(Name ="Accountant")]
    Accountant,
    [Display(Name ="Teacher")]
    Teacher
}

public enum QuestionsType
{
    Undefined, //default for question Type
    MCQ, // MCQ for radio option
    LongAnswer ,//For textBox
    Program //For Programming languages
}

public enum MarksStatus
{
    Undefined, 
    Pending,
    Marked, 
}

public enum AttRequestType
{
    Undefined,
    CorrectionCard,
    MissingCard
}

public enum ExamType
{
    Undefined,
    Practical,
    Viva,
    Theory
}


public enum TaskStatus
{
    Pending,
    InProgress,
    Completed,
}