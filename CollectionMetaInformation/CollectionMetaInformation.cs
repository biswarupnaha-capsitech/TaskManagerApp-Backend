using Capsitech.Data.MongoDB;
using Capsitech.Extensions;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
namespace TaskManager.CollectionMetaInformation
{
    public class YearlyRecord
    {
        public string Year { get; set; }
        public ulong AutoNumber { get; set; }  
        public ulong TotalRecord { get; set; }
    }

    public class CollectionMetaInformation : RecordBase
    {
        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string CollectionName { get; set; } = "";
        public ulong TotalRecord { get; set; }
        public ulong SoftDelete { get; set;}
        public ulong HardDelete { get; set;}
        public ulong AutoNumber { get; set; }
        public short WidthOfSeries { get; set; }
        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string Prefix { get; set; } = "";
        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string Postfix { get; set; } = "";
        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string IncrementalSeries { get; set; } = "";

        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public List<YearlyRecord> YearlyRecords { get; set; } = new List<YearlyRecord>();
    }

    public class TaskManagerCollectionName
    {
        public readonly static string Masters = "CITMasters";
        public readonly static string CollectionMetaInformation = "CITCollectionMetaInformation";
        public readonly static string Logs = "CITUserLogs";
        public readonly static string Users = "Users";
    }

    public class CollectionConfig
    {
        public readonly static ConfigDetails UserNumberConfig = new()
        {
            Prefix = "USR-",
            CollectionName = TaskManagerCollectionName.Users,
            WidthOfSeries = 4
        };
    }

    public class ConfigDetails
    { 
        public string Prefix="";
        public string Postfix="";
        public short WidthOfSeries = 7;
        public ulong AutoNumber = 1;
        public string CollectionName = "";
    }
    public class CollectionMetaInformationDB:RecordBaseDB<CollectionMetaInformation,string>
    {
        public  CollectionMetaInformationDB(DBConfiguration dBConfiguration) : base(dBConfiguration) { }
        public CollectionMetaInformationDB(DBConfiguration dBConfiguration, ClaimsPrincipal User):base(dBConfiguration,User) { }
        public override string CollectionName => TaskManagerCollectionName.CollectionMetaInformation;

        public async Task<CollectionMetaInformation> GetByCollectionName (string collectionName)
        {
            CollectionMetaInformation information = null;
            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter = filterBuilder.Empty;
            try
            {
                if (collectionName == null || collectionName.IsEmpty())
                    throw new Exception("Collection name is empty or null.");

                filter &= filterBuilder.Eq("CollectionName", collectionName);

                var qry = GetCollectionBson().Aggregate().Match(filter);

                var qryb = qry.Project<CollectionMetaInformation>(new BsonDocument()
                {
                    { "CollectionName", 1 },
                    { "TotalRecord", 1 },
                    { "SoftDelete", 1 },
                    { "HardDelete", 1 },
                    { "AutoNumber", 1 },
                    { "WidthOfSeries", 1 },
                    { "Prefix", 1 },
                    { "Postfix", 1 },
                    { "IncrementalSeries", 1 },
                    { "YearlyRecords",1}
                });

                information = await qryb.FirstOrDefaultAsync()??throw new Exception("Record not found.");
            }
            catch (Exception ex)
            {

            }
            return information;
        }

        public string GenerateIncrementalSeries(ulong autoNumber,short width,string prefix)
        {
            string result = prefix+autoNumber.ToString("".PadLeft(width, '0'));
            return result;
        }   
        public async Task<CollectionMetaInformation> SaveMeta(ConfigDetails config)
        {
            CollectionMetaInformation information = null;
            try
            {
                var filterBuilder = Builders<BsonDocument>.Filter;
                var filter = filterBuilder.Empty;
                information = await GetByCollectionName(config.CollectionName);
                if (information==null)
                {
                    information = new()
                    {
                        CollectionName = config.CollectionName,
                        TotalRecord = 1,
                        AutoNumber = config.AutoNumber,
                        Prefix = config.Prefix,
                        Postfix = config.Postfix,
                        WidthOfSeries = config.WidthOfSeries,
                        IncrementalSeries = GenerateIncrementalSeries(config.AutoNumber,config.WidthOfSeries,config.Prefix),
                    };
                    if (!await AddAsync(information))
                        throw new Exception("Operation is failed during add Meta information");
                }
                else
                {
                    information.AutoNumber += 1;
                    information.TotalRecord += 1;
                    information.IncrementalSeries = GenerateIncrementalSeries(information.AutoNumber,config.WidthOfSeries,config.Prefix);
                    if (!await UpdateAsync(information))
                        throw new Exception("Operation is failed during update Meta information");
                }
            }
            catch(Exception ex)
            {

            }
            return information;
        }

        public async Task<CollectionMetaInformation> SaveMeta(ConfigDetails config, string year)
        {
            CollectionMetaInformation information = null;
            try
            {
                // Retrieve existing meta information for the collection
                information = await GetByCollectionName(config.CollectionName);

                // Find the existing record for the current year
                var yearlyRecord = information?.YearlyRecords.FirstOrDefault(y => y.Year == year);

                if (yearlyRecord == null)
                {
                    // If the year is different or not found, start a new sequence for this year
                    yearlyRecord = new YearlyRecord
                    {
                        Year = year,
                        AutoNumber = 1,  // Start from 1
                        TotalRecord = 1  // First record for this year
                    };

                    if (information == null)
                    {
                        // Create a new CollectionMetaInformation record if it doesn't exist
                        information = new CollectionMetaInformation
                        {
                            CollectionName = config.CollectionName,
                            Prefix = config.Prefix,
                            Postfix = config.Postfix,
                            WidthOfSeries = config.WidthOfSeries,
                            IncrementalSeries = GenerateIncrementalSeries(1, config.WidthOfSeries, config.Prefix),
                            YearlyRecords = new List<YearlyRecord> { yearlyRecord }
                        };

                        if (!await AddAsync(information))
                        {
                            throw new Exception("Operation failed during adding Meta information");
                        }
                    }
                    else
                    {
                        // If CollectionMetaInformation exists, add the new year record
                        information.YearlyRecords.Add(yearlyRecord);
                        information.IncrementalSeries = GenerateIncrementalSeries(1, config.WidthOfSeries, config.Prefix);

                        if (!await UpdateAsync(information))
                        {
                            throw new Exception("Operation failed during updating Meta information");
                        }
                    }
                }
                else
                {
                    // If the year is the same, continue incrementing the sequence
                    yearlyRecord.AutoNumber += 1;
                    yearlyRecord.TotalRecord += 1;

                    information.IncrementalSeries = GenerateIncrementalSeries(yearlyRecord.AutoNumber, config.WidthOfSeries, config.Prefix);

                    if (!await UpdateAsync(information))
                    {
                        throw new Exception("Operation failed during updating Meta information");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while saving data", ex);
            }

            return information;
        }
    }
}
