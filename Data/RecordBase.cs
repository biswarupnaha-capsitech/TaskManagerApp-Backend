using Capsitech.Data.MongoDB;
using Capsitech.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Linq.Expressions;
using System.Security.Claims;
using TaskManager.Models;
using System.Reflection;
using System.Text.RegularExpressions;
using TaskManager.Util;
using TaskManager.CollectionMetaInformation;
using System.Threading.Channels;

namespace TaskManager.Data
{
    [BsonIgnoreExtraElements(Inherited = true)]
    public abstract class Record : RecordBase
    {
        protected Record() : base() { }
    }

    public abstract class RecordDB<TDoc, TId> : RecordBaseDB<TDoc, TId> where TDoc : IRecord<TId>
    {
        protected IMongoCollection<UserLog> _collectionLog = null;

        protected RecordDB(DBConfiguration DBConfig) : base(DBConfig)
        {
            if (_database != null)
                _collectionLog = _database.GetCollection<UserLog>(TaskManagerCollectionName.Logs);
        }

        protected RecordDB(DBConfiguration DBConfig, ClaimsPrincipal user) : base(DBConfig, user) { }

        #region Override methods to save log


        #region GetAsync

        /// <summary>
        /// Get document from the collection
        /// </summary>
        /// <param name="id">Document id</param>
        /// <returns>Document type</returns>
        public override async Task<TDoc> GetAsync(TId id)
        {
            TDoc doc = await base.GetAsync(id);

            if (doc == null && LastError != null)
                await Services.EmailSender.SendException(LastError, $"{typeof(TDoc).FullName}.GetAsync({id}), DB:{_dbConfig?.Database}");

            return doc;
        }
        /// <summary>
        /// Get document from the collection
        /// </summary>
        /// <param name="expression">Document search expression</param>
        /// <returns>Document type</returns>
        public override async Task<TDoc> GetAsync(Expression<Func<TDoc, bool>> expression)
        {
            var doc = await base.GetAsync(expression);
            if (doc == null && LastError != null)
                await Services.EmailSender.SendException(LastError, $"{typeof(TDoc).FullName}.GetAsync({expression?.ToString()}), DB:{_dbConfig?.Database}");
            return doc;
        }
        /// <summary>
        /// Get document from the collection
        /// </summary>
        /// <param name="expression">Document search expression</param>
        /// <param name="keySelector">Order by key selector</param>
        /// <param name="decendingOrder">If order by decending required</param>
        /// <returns>Document type</returns>
        public async Task<TDoc> GetAsync<TKey>(Expression<Func<TDoc, bool>> expression, Expression<Func<TDoc, TKey>> keySelector, bool decendingOrder = false)
        {
            var docs = await GetAllAsync(expression, keySelector, length: 1);
            if (docs?.Count > 0)
                return docs[0];
            return default;
        }

        /// <summary>
        /// Get documents from the collection
        /// </summary>
        /// <param name="predicate">Condition to filter documents</param>
        /// <param name="start">Records start from</param>
        /// <param name="length">Records count</param>
        /// <returns><see cref="List{TDoc}"/></returns>
        public override async Task<List<TDoc>> GetAllAsync(Expression<Func<TDoc, bool>> predicate, int start = 0, int length = 0)
        {
            var doc = await base.GetAllAsync(predicate, start, length);
            if (doc == null && LastError != null)
                await Services.EmailSender.SendException(LastError, $"{typeof(TDoc).FullName}.GetAllAsync({predicate?.ToString()}), DB:{_dbConfig?.Database}");
            return doc;
        }
        /// <summary>
        /// Get documents sorted by a selector
        /// </summary>
        /// <typeparam name="TKey">Placeholder key (temp use)</typeparam>
        /// <typeparam name="TResult">Placeholder for output</typeparam>
        /// <param name="predicate">Record selector</param>
        /// <param name="keySelector">Order by key selector</param>
        /// <param name="selector">Output record selector</param>
        /// <param name="decendingOrder">If order by decending required</param>
        /// <param name="start">Records start from</param>
        /// <param name="length">Records count</param>
        /// <returns><see cref="List{TDoc}"/></returns>
        public override async Task<List<TResult>> GetAllAsync<TKey, TResult>(Expression<Func<TDoc, bool>> predicate, Expression<Func<TDoc, TKey>> keySelector, Expression<Func<TDoc, TResult>> selector, bool decendingOrder = false, int start = 0, int length = 0)
        {
            var doc = await base.GetAllAsync(predicate, keySelector, selector, decendingOrder, start, length);
            if (doc == null && LastError != null)
                await Services.EmailSender.SendException(LastError, $"{typeof(TDoc).FullName}.GetAllAsync({predicate?.ToString()}, {selector?.ToString()}), DB:{_dbConfig?.Database}");
            return doc;
        }
        /// <summary>
        /// Get documents sorted by a selector
        /// </summary>
        /// <typeparam name="TKey">Placeholder key (temp use)</typeparam>
        /// <param name="predicate">Record selector</param>
        /// <param name="keySelector">Order by key selector</param>
        /// <param name="decendingOrder">If order by decending required</param>
        /// <param name="start">Records start from</param>
        /// <param name="length">Records count</param>
        /// <returns><see cref="List{TDoc}"/></returns>
        public override async Task<List<TDoc>> GetAllAsync<TKey>(Expression<Func<TDoc, bool>> predicate, Expression<Func<TDoc, TKey>> keySelector, bool decendingOrder = false, int start = 0, int length = 0)
        {
            var doc = await base.GetAllAsync(predicate, keySelector, decendingOrder, start, length);
            if (doc == null && LastError != null)
                await Services.EmailSender.SendException(LastError, $"{typeof(TDoc).FullName}.GetAllAsync({predicate?.ToString()}, {keySelector?.ToString()}), DB:{_dbConfig?.Database}");
            return doc;
        }

        #endregion

        #region AddAsync

        /// <summary>
        /// Add new document to the collection
        /// </summary>
        /// <param name="document">Document to be inserted</param>
        /// <param name="ignoreDuplicateCheck">Ignore validation for duplicate check</param>
        /// <returns><see cref="bool"/></returns>
        public override async Task<bool> AddAsync(TDoc document, bool ignoreDuplicateCheck)
        {
            if (await AddAsyncInternal(document, ignoreDuplicateCheck))
            {
                await LogInsertAsync(document);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Add new document to the collection
        /// </summary>
        /// <param name="document">Document to be inserted</param>
        /// <param name="logMessage">Message for log records</param>
        /// <returns><see cref="bool"/></returns>
        public async Task<bool> AddAsync(TDoc document, string logMessage)
        {
            if (await AddAsyncInternal(document, false))
            {
                await LogActivityAsync(document.Id, logMessage, UserLogActions.Insert);
                return true;
            }
            return false;
        }
        private async Task<bool> AddAsyncInternal(TDoc document, bool ignoreDuplicateCheck)
        {
            bool result = await base.AddAsync(document, ignoreDuplicateCheck);
            if (LastError != null)
                await Services.EmailSender.SendException(LastError, $"{typeof(TDoc).FullName}.AddAsync({document}), DB:{_dbConfig?.Database}");
            return result;
        }

        #endregion

        #region UpdateAsync
        /// <summary>
        /// Update an existing document with the new one
        /// </summary>
        /// <param name="document">Document to be updated</param>
        /// <param name="ignoreDuplicateCheck">Ignore validation for duplicate check</param>
        /// <returns><see cref="bool"/></returns>
        public override async Task<bool> UpdateAsync(TDoc document, bool ignoreDuplicateCheck)
        {
            if (await UpdateAsyncInternal(document, ignoreDuplicateCheck))
            {
                await LogUpdateAsync(document);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Update an existing document with the new one
        /// </summary>
        /// <param name="document">Document to be updated</param>
        /// <param name="logMessage">Message for log records</param>
        /// <returns><see cref="bool"/></returns>
        public async Task<bool> UpdateAsync(TDoc document, string logMessage)
        {
            if (await UpdateAsyncInternal(document, false))
            {
                await LogActivityAsync(document.Id, logMessage, UserLogActions.Update);
                return true;
            }
            return false;
        }
        private async Task<bool> UpdateAsyncInternal(TDoc document, bool ignoreDuplicateCheck)
        {
            bool result = await base.UpdateAsync(document, ignoreDuplicateCheck);
            if (LastError != null)
                await Services.EmailSender.SendException(LastError, $"{typeof(TDoc).FullName}.UpdateAsync({document}), DB:{_dbConfig?.Database}");
            return result;
        }

        /// <summary>
        /// Update an existing document's elements with the new one
        /// </summary>
        /// <param name="id">Document id</param>
        /// <param name="update">Document update definition (to be updated)</param>
        /// <param name="logMessage">Message for log records</param>
        /// <returns><see cref="bool"/></returns>
        public async Task<bool> UpdateAsync(TId id, UpdateDefinition<TDoc> update, string logMessage)
        {
            bool result = await base.UpdateAsync(id, update);
            if (result)
                await LogActivityAsync(id, logMessage, UserLogActions.Update);
            else if (LastError != null)
                await Services.EmailSender.SendException(LastError, $"{typeof(TDoc).FullName}.UpdateAsync({id},{update}), DB:{_dbConfig?.Database}");

            return result;
        }

        /// <summary>
        /// Update an existing document'
        /// </summary>
        /// <param name="id">Document id</param>
        /// <param name="previousValue">Document previous value</param>
        /// <param name="updatedValue">Document update definition (to be updated)</param>
        /// <returns><see cref="bool"/></returns>
        public async Task<bool> UpdateAsync(TId id, TDoc previousValue, TDoc updatedValue)
        {
            bool result = await base.UpdateAsync(updatedValue, false);

            var logMessage = CompareObjects(previousValue, updatedValue);

            if (result && logMessage != "")
            {
                await LogActivityAsync(id, logMessage, UserLogActions.Update);
            }
            else if (LastError != null)
                await Services.EmailSender.SendException(LastError, $"{typeof(TDoc).FullName}.UpdateAsync({updatedValue}), DB:{_dbConfig?.Database}");

            return result;
        }

        #endregion

        #region CompareObjects
        /// <summary>
        /// Compare before update
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns><see cref="string"/></returns>
        private static string CompareObjects<T>(T obj1, T obj2)
        {
            var changes = "";
            List<string> fieldsToExclude = new() { "CreatedBy", "UpdatedBy", "Id", "RecordStatus", "IsEditAllowed", "ItemId", "AutoNumber", "Variants", "Sno" };

            Type objectType = typeof(T);
            PropertyInfo[] properties = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                if (fieldsToExclude != null && fieldsToExclude.Contains(property.Name))
                {
                    continue; // Skip the excluded field
                }
                object value1 = property.GetValue(obj1);
                object value2 = property.GetValue(obj2);

                if (!Equals(value1, value2))
                {
                    string fieldName = Regex.Replace(property.Name, "[a-z][A-Z]", m => $"{m.Value[0]} {char.ToLower(m.Value[1])}"); //camalcase to normal string
                    string oldValue = "";
                    string newValue = "";

                    // Handle DateTime fields
                    if (value1 is DateTime date1 && value2 is DateTime date2)
                    {
                        oldValue = value1?.ToString() ?? "-";
                        newValue = value2?.ToString() ?? "-";

                        if (!DateTime.Equals(date1, date2))
                        {
                            changes += $"\r\n{fieldName}: {oldValue} to {newValue}";
                        }
                    }

                    // Handle list of objects
                    else if (value1 is List<IdNameModel> list1 && value2 is List<IdNameModel> list2)
                    {
                        string listChanges = "";
                        int length = Math.Min(list1.Count, list2.Count);


                        if (list1.Count > length)
                        {
                            for (int i = length; i < list1.Count; i++)
                            {
                                object item1 = list1[i];
                                listChanges += $"\r\n{fieldName}: {item1?.ToString().Split(':').Skip(1).FirstOrDefault() ?? "-"} is removed";
                            }
                        }
                        if (list2.Count > length)
                        {
                            for (int i = length; i < list2.Count; i++)
                            {
                                object item2 = list2[i];
                                listChanges += $"\r\n{fieldName}: {item2?.ToString().Split(':').Skip(1).FirstOrDefault() ?? "-"} is added";
                            }
                        }

                        changes += listChanges;
                    }

                    // for custom Object
                    #region For Custom Object
                    //else if (value1 is List<Purchase> lst1 && value2 is List<Purchase> lst2)
                    //{
                    //    string listChanges = "";
                    //    List<Tuple<Purchase, long>> removedItems = null;
                    //    List<Tuple<Purchase, long>> addedItems = null;
                    //    List<Tuple<Purchase, Purchase, long>> remaingItems = null;
                    //    ComparePurchase.GetRemovedAddedRemaining(lst1, lst2, out removedItems, out addedItems, out remaingItems);
                    //    if (removedItems != null)
                    //    {
                    //        if (remaingItems != null)
                    //        {
                    //            foreach (var item in remaingItems)
                    //            {
                    //                changes = changes + listChanges + CompareObjects(item.Item1, item.Item2, null, "CPurchase", item.Item3);
                    //            }
                    //        }
                    //        foreach (var item in removedItems)
                    //        {
                    //            changes = changes + listChanges + $"\nInvoice[{item.Item2 + 1}] row is removed from invoice no.  {purchases.BillingNo} with\r\n[\r\n    Category: {item.Item1.MaterialType.Name},\r\n    Subcategory: {item.Item1.Item.Name},\r\n    Measurement: {item.Item1.Measurement.Name},\r\n    Qty: {item.Item1.Quantity},\r\n    Rate: {item.Item1.Rate},\r\n    Amount: {item.Item1.Amount},\r\n    Description: {item.Item1.Description},  \r\n    Gst: {item.Item1.Gst?.Name}%,\r\n   Gst Amount: {item.Item1.GstAmount},\r\n   Total Amount: {item.Item1.TotalAmount}\r\n]";
                    //        }
                    //    }
                    //    if (addedItems != null)
                    //    {
                    //        if (remaingItems != null)
                    //        {
                    //            foreach (var item in remaingItems)
                    //            {
                    //                changes = changes + listChanges + CompareObjects(item.Item1, item.Item2, null, "CPurchase", item.Item3);
                    //            }
                    //        }
                    //        foreach (var item in addedItems)
                    //        {
                    //            changes = changes + listChanges + $"\nNew row added in invoice no. {purchases.BillingNo} with\r\n[\r\n    Category: {item.Item1.MaterialType.Name},\r\n    Subcategory: {item.Item1.Item.Name},\r\n    Measurement: {item.Item1.Measurement.Name},\r\n    Qty: {item.Item1.Quantity},\r\n    Rate: {item.Item1.Rate},\r\n    Amount: {item.Item1.Amount},\r\n    Description: {item.Item1.Description},  \r\n    Gst: {item.Item1.Gst?.Name}%,\r\n   Gst Amount: {item.Item1.GstAmount},\r\n   Total Amount: {item.Item1.TotalAmount}\r\n]";
                    //        }

                    //    }

                    //    else
                    //    {
                    //        if (remaingItems != null && removedItems == null && addedItems == null)
                    //        {
                    //            foreach (var item in remaingItems)
                    //            {
                    //                changes = changes + listChanges + CompareObjects(item.Item1, item.Item2, null, "CPurchase", item.Item3);
                    //            }
                    //        }
                    //    }
                    //}
                    #endregion

                    else
                    {
                        oldValue = value1?.ToString() == ":" || value1?.ToString() == "" || value1?.ToString() == null ? "-" : value1?.ToString();
                        newValue = value2?.ToString() == ":" || value2?.ToString() == "" || value2?.ToString() == null ? "-" : value2?.ToString();

                        if (!oldValue.Equals(newValue))
                        {
                            // To remove id from IdNameModel when changes message is set
                            if (oldValue.Contains(":"))
                                oldValue = oldValue.Split(':').Skip(1).FirstOrDefault();
                            if (newValue.Contains(":"))
                                newValue = newValue.Split(':').Skip(1).FirstOrDefault();

                            changes += $"\r\n{fieldName}: {oldValue} to {newValue}";
                        }
                    }

                }
            }

            return changes;
        }

        #endregion

        #region DeleteAsync
        /// <summary>
        /// Delete existing document
        /// </summary>
        /// <param name="id">Document id</param>
        /// <returns><see cref="bool"/></returns>
        public override async Task<bool> DeleteAsync(TId id)
        {
            TDoc doc = await GetAsync(id);

            bool result = await base.DeleteAsync(id);
            if (result)
            {
                await LogDeleteAsync(doc);
            }
            else if (LastError != null)
                await Services.EmailSender.SendException(LastError, $"{typeof(TDoc).FullName}.DeleteAsync({id}), DB:{_dbConfig?.Database}");

            return result;
        }

        /// <summary>
        /// Delete multiple documents
        /// </summary>
        /// <param name="filter">Filter definition to update</param>
        /// <returns><see cref="bool"/></returns>
        public override async Task<DeleteResult> DeleteManyAsync(FilterDefinition<TDoc> filter)
        {
            var result = await base.DeleteManyAsync(filter);
            if (result?.DeletedCount > 0)
                await LogDeleteManyAsync();
            return result;
        }

        #endregion

        #endregion

        #region Log actions

        public virtual EntitiesEnum LogEntity => EntitiesEnum.Unknown;

        /// <summary>
        /// Get log message for actions
        /// </summary>
        /// <param name="Record">For record</param>
        /// <param name="Action">On action</param>
        /// <returns></returns>
        protected virtual string LogMessage(TDoc Record, UserLogActions Action) { return ""; }

        /// <summary>
        /// Record current user activity for insert event
        /// </summary>
        /// <param name="Value">Record/Value</param>
        public async System.Threading.Tasks.Task LogInsertAsync(TDoc Value)
        {
            if (Value == null || CurrentUser == null)
                return;

            string msg = LogMessage(Value, UserLogActions.Insert);
            if (msg != null)
                await LogActivityAsync(Value.Id, msg == "" ? $"New {LogEntity.GetDisplayName()} created with description '{Value}'" : msg, UserLogActions.Insert);
        }

        /// <summary>
        /// Record current user activity for edit/update event
        /// </summary>
        /// <param name="Value">Record/Value</param>
        public async System.Threading.Tasks.Task LogUpdateAsync(TDoc Value)
        {
            if (Value == null || CurrentUser == null)
                return;

            string msg = LogMessage(Value, UserLogActions.Update);
            if (msg != null)
                await LogActivityAsync(Value.Id ?? default, msg == "" ? $"{LogEntity.GetDisplayName()} updated with description '{Value?.ToString()}'" : msg, UserLogActions.Update);
        }

        /// <summary>
        /// Record current user activity for delete event
        /// </summary>
        /// <param name="Value">Record/Value</param>
        public async System.Threading.Tasks.Task LogDeleteAsync(TDoc Value)
        {
            if (Value == null || CurrentUser == null)
                return;

            string msg = LogMessage(Value, UserLogActions.Delete);
            if (msg != null)
                await LogActivityAsync(Value.Id, msg == "" ? $"{LogEntity.GetDisplayName()} deleted with description '{Value.ToString()}'" : msg, UserLogActions.Delete);
        }

        /// <summary>
        /// Record current user activity for delete many event
        /// </summary>
        public async System.Threading.Tasks.Task LogDeleteManyAsync()
        {
            if (CurrentUser == null)
                return;

            string msg = LogMessage(default, UserLogActions.DeleteMany);
            if (msg != null)
                await LogActivityAsync(default, msg == "" ? $"{LogEntity.GetDisplayName()} deleted multiple documents" : msg, UserLogActions.DeleteMany);
        }

        /// <summary>
        /// Record current user activity
        /// </summary>
        /// <param name="Id">Entity id</param>
        /// <param name="Description">Log detail</param>
        /// <param name="Action">Action to log activity</param>
        /// <returns>bool</returns>
        public async Task<bool> LogActivityAsync(TId Id, string Description, UserLogActions Action)
        {
            if (CurrentUser == null)
                return false;

            return await LogActivityAsync(Id, Description, Action, CurrentUser?.GetUserId(), CurrentUser?.Identity?.GetUserName());
        }
        /// <summary>
        /// Record current user activity for custom event
        /// </summary>
        /// <param name="Action">Action to log activity</param>
        /// <param name="Description">Log description</param>
        /// <param name="Id">Record Id</param>
        /// <param name="UserId">User Id</param>
        /// <param name="UserName">User name</param>
        public async Task<bool> LogActivityAsync(TId Id, string Description, UserLogActions Action, string UserId, string UserName = "")
        {
            //skip log for this description
            if (UserId.IsEmpty() || Description == "~")
                return false;

            try
            {
                var log = new UserLog
                {
                    Action = Action,
                    CreatedBy = new RecordUpdateInfo
                    {
                        Date = DateTime.UtcNow,
                        UserId = UserId,
                        UserName = UserName
                    },
                    Entity = LogEntity,
                    EntityRef = new MongoDBRef(CollectionName, Id as string),
                    Message = Description,
                    Id = ObjectId.GenerateNewId().ToString()
                };

                if (_collectionLog == null && _database != null)
                    _collectionLog = _database.GetCollection<UserLog>(TaskManagerCollectionName.Logs);

                await _collectionLog.InsertOneAsync(log);
                return true;
            }
            catch
            {
                //CurrentUser.SendError(ex, string.Format("MasterDbBase<{0}> - {1}", EntityName, Action));
            }
            return false;
        }

        #endregion

        #region Utility methods

        public async Task<T> FetchDBRef<T>(MongoDBRef reference) where T : IRecord
        {
            if (reference == null || reference.Id.ToString().IsEmpty())
                return default(T);

            var filter = Builders<T>.Filter.Eq(e => e.Id, reference.Id.AsString);
            return await _database.GetCollection<T>(reference.CollectionName).Find(filter).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get all documents by custom query
        /// </summary>
        /// <param name="start">Records/page start from</param>
        /// <param name="length">Page length/size</param>
        /// <param name="filterDefinition">Record filter definition</param>
        /// <param name="sortDefinition">Record sort definition</param>
        /// <param name="projectionDefinition">Record project definition</param>
        /// <returns></returns>
        public new async Task<List<T>> GetAllAsync<T>(FilterDefinition<BsonDocument> filterDefinition, ProjectionDefinition<BsonDocument> projectionDefinition, SortDefinition<BsonDocument> sortDefinition = null, int start = 0, int length = 0)
        {
            List<T> response = new List<T>();
            try
            {
                var query = _database.GetCollection<BsonDocument>(CollectionName)
                    .Aggregate()
                    .Match(filterDefinition);

                if (sortDefinition != null)
                    query = query.Sort(sortDefinition);

                if (start > 0)
                    query = query.Skip(start);
                if (length > 0)
                    query = query.Limit(length);

                response = await query
                    .Project<T>(projectionDefinition)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                LastError = ex;
            }
            return response;
        }

        #endregion

        //#endregion

        #region Get Icon

        public string GetIconName(UserLogActions action)
        {
            return action switch
            {
                UserLogActions.Undefined => "",
                UserLogActions.Insert=> "<PlusOutlined />",
                UserLogActions.Update=> "<EditOutlined />",
                UserLogActions.Delete=> "<DeleteOutlined />",
                UserLogActions.EnquiryReject=> "<StopOutlined />",
                UserLogActions.ProcessStepChanged=> "<InfoCircleOutlined />",
                _ => ""
            };
        }

        #endregion

    }
    public abstract class RecordDB<TDoc> : RecordDB<TDoc, string> where TDoc : IRecord
    {
        protected RecordDB(DBConfiguration DBConfig) : base(DBConfig) { }

        protected RecordDB(DBConfiguration DBConfig, ClaimsPrincipal user) : base(DBConfig, user) { }
    }
}
