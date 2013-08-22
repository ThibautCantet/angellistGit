using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntVoice.Common.DataAccess.MongoClient;
using AntVoice.Common.DataAccess.RedisClient;
using AntVoice.Common.Entities.Enums;
using AntVoice.Common.Entities.Filters;
using AntVoice.Common.Entities.Recommendation;
using AntVoice.Common.Entities.Settings.Enums;
using AntVoice.Common.Entities.Settings.Recommendation;
using AntVoice.Platform.Tools.Logging;
using AntVoice.Platform.Tools.Monitoring;
using AntVoice.Platform.Tools.Monitoring.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace AntVoice.Common.DataAccess
{
    public class ProductDataAccess
    {

        /// <summary>
        /// Number of items to get from each collection and place in Redis cache
        /// </summary>
        private const int Limit = 500;

        // name of the columns in mongo db
        private const string ProductIdField = "_id";
        private const string MongoTypeField = "ItemType";
        private const string DateCreatedField = "CreationDate";
        private const string WebSiteIdField = "WebSiteId";

        // we use these numbers to generate unique key for redis
        private static readonly int[] Fibonacci = new int[] { 0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233, 377, 610, 987, 1597, 2584 };

        private static readonly MongoDBContext _mongoContext = new MongoDBContext(new DataAccessSettings().MongoDBRecommendationConnectionString);

        /// <summary>
        /// Redis set name
        /// </summary>
        private const string RedisSetWorkspace = "ProductDataAccess";

        private const string _collectionName = "Product";
        
        public static Product GetProduct(int id)
        {
            var query = Query.EQ("_id", new BsonInt32(id));
            return _mongoContext.GetDocument<Product>(query, _collectionName);
        }

        public static List<Product> GetProducts(List<int> ids)
        {
            var query = Query.In("_id", ids.Select(product => new BsonInt32(product)).ToList());
            return _mongoContext.GetDocuments<Product>(query, _collectionName).ToList();
        }

        public static List<Product> GetProducts(List<string> clientProductIds)
        {
            var query = Query.In("ClientProductId", clientProductIds.Select(product => new BsonString(product)).ToList());
            return _mongoContext.GetDocuments<Product>(query, _collectionName).ToList();
        }

        public static Product GetProduct(string clientProductId, WebSites webSiteId)
        {
            var query = Query.EQ("ClientProductId", new BsonString(clientProductId));
            return _mongoContext.GetDocuments<Product>(query, _collectionName).SingleOrDefault(p => p.WebSiteId == webSiteId);
        }

        public static List<Product> GetProductsByWebSite(WebSites webSiteId)
        {
            var query = Query.EQ("WebSiteId", webSiteId.ToString());
            return _mongoContext.GetDocuments<Product>(query, _collectionName).ToList();
        }

        public static bool AddProduct(Product product)
        {
            if (product != null)
            {
                product.Id = _mongoContext.GenerateId(product, _collectionName);
                _mongoContext.InsertDocument(product, _collectionName);
                var query = Query.EQ("_id", product.Id);
                return _mongoContext.IsDocumentExist<Product>(query, _collectionName);
            }
            {
                Log.Warn("ProductDataAccess AddProduct", "Product null");
                return false;
            }
        }

        public static List<Product> AddProducts(List<Product> products)
        {
            if (products != null && products.Any())
            {
                var ids = _mongoContext.GenerateIds(_collectionName, products.Count);
                for (var i = 0; i < products.Count; i++)
                {
                    products[i].Id = ids[i];
                }
                _mongoContext.InsertDocuments(products, _collectionName);
                return products;
            }
            {
                Log.Error("ProductDataAccess AddProducts", "Products list null or empty");
                throw (new Exception("ProductDataAccess.AddProducts : Products list null or empty"));
            }

        }

        public static void DeleteProduct(int id)
        {
            var query = Query.EQ("_id", new BsonInt32(id));
            _mongoContext.RemoveDocument<Product>(query, _collectionName);
        }

        public static void DeleteProducts(List<Product> products)
        {
            var queries = Query.In("_id", products.Select(product => new BsonInt32(product.Id)).ToList());
            _mongoContext.RemoveDocument<Product>(queries, _collectionName);
        }

        public static Product UpdateProduct(Product product)
        {
            if (product != null)
            {
                _mongoContext.SaveDocument(product, _collectionName);
                return product;
            }
            {
                Log.Error("ProductDataAccess UpdateProduct", "Products list null or empty");
                throw (new Exception("ProductDataAccess.UpdateProduct : Products list null or empty"));
            }
        }

        //public static void UpdateProducts(List<Product> products)
        //{
        //    if (products != null && products.Any())
        //    {
        //        _mongoContext.SaveDocument(products, _collectionName);
        //    }
        //    {
        //        Log.Error("ProductDataAccess UpdateProducts", "Products list null or empty");
        //        throw (new Exception("ProductDataAccess.UpdateProducts : Products list null or empty"));
        //    }
        //}

        #region Recommendation

        public static List<ScoringFunctionResult> GetRandomProducts(int count, WebSites webSite, AggregateFilter filters)
        {
            return GetRandomProducts(count, webSite, filters, null);
        }

        public static List<ScoringFunctionResult> GetRecentProducts(int count, WebSites webSite, AggregateFilter filters)
        {
            return GetRecentProducts(count, webSite, filters, null);
        }

        public static List<ScoringFunctionResult> GetRandomProducts(int count, WebSites webSite, AggregateFilter filters, ContentTypeQuota quota)
        {
            Log.Debug("ProductDataAccess.GetRandomItems", "parameters", count, webSite);
            string queryUniquestring;

            IMongoQuery query = GetQuery(webSite, quota, filters, out queryUniquestring);
            RedisSet<string> cache = GetRedisSet(query, queryUniquestring);
            var set = cache.GetRandomItems(count, x => x);

            return quota != null ? set.Select(sfr => new ScoringFunctionResult() { ItemId = int.Parse(sfr), QuotaId = quota.Id }).ToList() : set.Select(sfr => new ScoringFunctionResult() { ItemId = int.Parse(sfr) }).ToList();
        }

        public static List<ScoringFunctionResult> GetRecentProducts(int count, WebSites webSite, AggregateFilter filters, ContentTypeQuota quota)
        {
            string queryUniquestring;
            IMongoQuery query = GetQuery(webSite, quota, filters, out queryUniquestring);

            return quota != null ? GetRecentProductsWithQuery(query, quota.Id, count) : GetRecentProductsWithQuery(query, null, count);
        }

        public static ProductInfoCollection GetProductInfos(List<ScoringFunctionResult> items, AggregateFilter filters, List<string> fieldsToAdd)
        {
            IStopwatch sw = MonitoringTimers.Current.GetNewStopwatch(true);
            try
            {
                ProductInfoCollection result = new ProductInfoCollection();
                Dictionary<ItemTypes, List<IMongoQuery>> queries = new Dictionary<ItemTypes, List<IMongoQuery>>();
                fieldsToAdd = new List<string>(fieldsToAdd);
                fieldsToAdd.Add(ProductIdField);
                foreach (var item in items)
                {
                    ItemTypes type = item.GetItemType();
                    int itemId = item.GetId();
                    if (queries.ContainsKey(type))
                    {
                        queries[type].Add(Query.EQ(ProductIdField, itemId));
                    }
                    else
                    {
                        queries.Add(type, new List<IMongoQuery>());
                        queries[type].Add(Query.EQ(ProductIdField, itemId));
                    }
                }

                // adding filters
                IMongoQuery filterQuery = null;
                var queryFilters = GetFilterQuery(filters);
                if (queryFilters != null)
                {
                    if (queryFilters.Count > 1)
                    {
                        filterQuery = Query.And(queryFilters);
                    }
                    else if (queryFilters.Count == 1)
                    {
                        filterQuery = queryFilters[0];
                    }
                }

                foreach (var query in queries)
                {
                    var idQuery = Query.Or(query.Value);
                    IMongoQuery finalQuery = null;
                    if (filterQuery != null)
                    {
                        finalQuery = Query.And(new IMongoQuery[] { idQuery, filterQuery });
                    }
                    else
                    {
                        finalQuery = idQuery;
                    }

                    Log.Debug("ProductDataAccess.GetProductInfos", "Final query", finalQuery.ToJson(), query.Key);
                    ItemTypes itemType = query.Key;
                    var docs = _mongoContext.GetDocuments<BsonDocument>(finalQuery, _collectionName, fieldsToAdd.ToArray(), items.Count());
                    foreach (var doc in docs)
                    {
                        int itemId = doc.GetValue(ProductIdField).AsInt32;
                        ProductInfo ProductInfo = new ProductInfo() { ItemId = itemId, Type = itemType, Fields = new Dictionary<string, string>() };
                        foreach (var field in fieldsToAdd)
                        {
                            ProductInfo.Fields.Add(field, doc.GetValue(field).ToString());
                        }

                        result.AddProductInfo(ProductInfo);
                    }
                }

                return result;
            }
            finally
            {
                sw.Stop();
                MonitoringTimers.Current.AddTime(Counters.ProductDataAccess_GetProductInfos, sw);
            }
        }

        private static List<ScoringFunctionResult> GetRecentProductsWithQuery(IMongoQuery query, int? quotaId, int count)
        {
            try
            {
                List<BsonDocument> products = _mongoContext.GetDocuments<BsonDocument>(query, _collectionName, new string[] { ProductIdField, MongoTypeField, DateCreatedField }, SortBy.Descending(DateCreatedField), count);
                Log.Debug("ProductDataAccess.GetRecentItemsWithQuery", "Nb of documents found", "Collection Name", query, products.Count, _collectionName);

                return products
                    .OrderByDescending(p => p.GetValue(DateCreatedField).AsDateTime)
                    .Take(count)
                    .Select(p => new ScoringFunctionResult { ItemId = p.GetValue(ProductIdField).AsInt32, Type = (ItemTypes)p.GetValue(MongoTypeField).AsInt32, QuotaId = quotaId, TimeCreated = p.GetValue(DateCreatedField).AsDateTime })
                    .ToList();
            }
            catch (Exception ex)
            {
                Log.Error("ProductDataAccess.GetRecentItemsWithQuery", "Exception when getting from mongo", query, ex);
                throw;
            }
        }

        private static RedisSet<string> GetRedisSet(IMongoQuery query, string queryUniqueString)
        {
            try
            {

                Log.Debug("ProductDataAccess.GetRedisSet", "Accessing redis with key", queryUniqueString);
                RedisSet<string> cache = new RedisSet<string>(RedisSetWorkspace, queryUniqueString);

                if (!cache.IsSetExists())
                {
                    // ids of items
                    var ids = new List<string>();

                    var products = _mongoContext.GetDocuments<BsonDocument>(query, _collectionName, new string[] { ProductIdField, MongoTypeField }, Limit);
                    Log.Debug("ProductDataAccess.GetRedisSet", "Get query items", queryUniqueString, _collectionName, query, _collectionName, ProductIdField, MongoTypeField, Limit);
                    if (products != null && products.Any())
                    {
                        ids.AddRange(products.Select(doc => doc.GetValue(ProductIdField).ToString()));
                        if (Logger.Current.IsDebugEnabled)
                        {
                            Log.Debug("ProductDataAccess.GetRedisSet", "ids found", _collectionName, query, queryUniqueString, string.Join(",", ids.Select(id => id)));
                        }
                    }
                    else
                    {
                        Log.Debug("ProductDataAccess.GetRedisSet", "No products in mongo db", _collectionName, query, queryUniqueString);
                    }

                    // filling cache
                    cache.AddRange(ids);
                }

                return cache;
            }
            catch (Exception ex)
            {
                Log.Error("ProductDataAccess.GetRedisSet", "Exception when getting from mongo", query, ex);
                throw;
            }
        }

        private static IMongoQuery GetQuery(WebSites webSite, ContentTypeQuota quota, AggregateFilter filters, out string queryUniqueString)
        {
            IMongoQuery query = null;
            List<IMongoQuery> queries = new List<IMongoQuery>();
            StringBuilder sb = new StringBuilder("w" + ((int)webSite).ToString());

            if (quota != null)
            {
                sb.Append("q" + quota.Id.ToString());
                foreach (var filter in quota.Values.Values)
                {
                    queries.Add(Query.EQ(filter.MongoName, GetFilterValue(filter.Value, filter.IsString)));
                    Log.Debug("ProductDataAccess.GetQuery", "quota filter", queries.Last());
                }
            }

            if (filters != null && filters.Filters != null)
            {
                //TODO Replace GetHashCode and Fibonacci approach by other way
                string filterStr = filters.ToString();
                sb.Append(filterStr.GetHashCode());
                for (int i = 0; i < Fibonacci.Length; i++)
                {
                    if (filterStr.Length < Fibonacci[i])
                    {
                        sb.Append(filterStr[Fibonacci[i]]);
                    }
                    else
                    {
                        break;
                    }
                }

                queries.AddRange(GetFilterQuery(filters));
            }

            queryUniqueString = sb.ToString();
            Log.Debug("ProductDataAccess.GetQuery", "queryUniqueString", queryUniqueString);

            // only active products
            queries.Add(Query.EQ("ItemStatus", 1));

            // final query
            var webSiteId = (int)webSite;
            queries.Add(Query.EQ(WebSiteIdField, new BsonInt32(webSiteId)));
            query = Query.And(queries);
            Log.Debug("ProductDataAccess.GetQuery", "Final query", query.ToJson());
            return query;
        }

        private static List<IMongoQuery> GetFilterQuery(AggregateFilter filters)
        {
            var queries = new List<IMongoQuery>();
            if (filters != null && filters.Filters != null)
            {
                foreach (var filter in filters.Filters)
                {
                    List<IMongoQuery> fqs = new List<IMongoQuery>();

                    if (filter.Operator == FilterOperator.Equals)
                    {
                        foreach (var value in filter.Values)
                        {
                            Log.Debug("ProductDataAccess.GetFilterQuery", "Filter Equals added", filter.ColumnNameMongo, GetFilterValue(value, filter.IsString));
                            fqs.Add(Query.EQ(filter.ColumnNameMongo, GetFilterValue(value, filter.IsString)));
                        }

                        if (fqs.Count > 1)
                        {
                            queries.Add(Query.Or(fqs));
                        }
                        else
                        {
                            if (fqs.Count == 1)
                            {
                                queries.Add(fqs[0]);
                            }
                        }
                    }
                    else if (filter.Operator == FilterOperator.Different)
                    {
                        foreach (var value in filter.Values)
                        {
                            Log.Debug("ProductDataAccess.GetFilterQuery", "Filter Not Equels added", filter.ColumnNameMongo, GetFilterValue(value, filter.IsString));
                            fqs.Add(Query.NE(filter.ColumnNameMongo, GetFilterValue(value, filter.IsString)));
                        }

                        if (fqs.Count > 1)
                        {
                            queries.Add(Query.And(fqs));
                        }
                        else
                        {
                            if (fqs.Count == 1)
                            {
                                queries.Add(fqs[0]);
                            }
                        }
                    }
                }
            }

            return queries;
        }

        private static dynamic GetFilterValue(string value, bool isString)
        {
            if (isString)
            {
                return value;
            }

            int intValue = 0;
            if (Int32.TryParse(value, out intValue))
            {
                return intValue;
            }

            double doubleValue = 0;
            if (Double.TryParse(value, out doubleValue))
            {
                return doubleValue;
            }

            bool boolValue;
            if (Boolean.TryParse(value, out boolValue))
            {
                return boolValue;
            }

            DateTimeOffset dtValue;
            if (DateTimeOffset.TryParse(value, out dtValue))
            {
                return dtValue;
            }

            Log.Warn("ProductDataAccess.GetFilterValue", "Filter value is not parsed");
            return value;

        }

        #endregion

        /// <summary>
        /// Add or edit the products in the context for the update of the catalog
        /// </summary>
        /// <param name="products">products to insert</param>
        /// <returns>new inserted and updated items</returns>
        public static List<Product> BulkAdd(List<Product> products, WebSites webSiteId)
        {
            try
            {
                var convertedproducts = products;

                List<string> existingproductsItemRefId = new List<string>();
                // get from the converted products those that are already in the context
                List<Product> existingproducts = new List<Product>();
                // keep only the new products
                List<Product> productsToInsert = new List<Product>();
                //Get all products from DB to check if the new product exist
                List<Product> existingproductsInDB = GetProducts(convertedproducts.Select(p => p.ClientProductId).ToList());


                foreach (var convertedProduct in convertedproducts)
                {
                    var existingItem =
                        existingproductsInDB.SingleOrDefault(
                            p =>
                            p.ClientProductId == convertedProduct.ClientProductId &&
                            p.WebSiteId == convertedProduct.WebSiteId);

                    if (existingItem != null)
                    {
                        existingproductsItemRefId.Add(existingItem.ClientProductId);
                        // set the id of the item in the xml
                        convertedProduct.Id = existingItem.Id;
                        existingproducts.Add(convertedProduct);
                    }
                    else
                    {
                        productsToInsert.Add(convertedProduct);
                    }
                }

                var nbExistingInMongoDb = existingproductsItemRefId.Count();

                // get from the converted products those that are already in the context
                //   existingproducts = convertedproducts.Where(e => existingproductsItemRefId.Contains(e.ItemRefId)).ToList();

                var nbExisting = existingproducts.Count();

                if (nbExistingInMongoDb != nbExisting)
                {
                    throw new Exception("The nb of existing products in mongodb is not the same as in new converted products");
                }

                // keep only the new products
                //productsToInsert = convertedproducts.Where(e => !existingproductsItemRefId.Contains(e.ItemRefId)).ToList();

                var nbNew = productsToInsert.Count();

                if (nbNew + nbExisting != products.Count())
                {
                    throw new Exception("The nb of existing and new products in mongodb is not the same as in the products as parameter");
                }

                List<Product> newOrEditedItems = new List<Product>();

                if (productsToInsert.Any())
                {
                    // only add new products
                    newOrEditedItems.AddRange(AddProducts(productsToInsert));
                }

                if (Logger.Current.IsDebugEnabled)
                {
                    Logger.Current.Debug("BulkAdd", "Items added",
                                         string.Join(";", productsToInsert.Select(e => e.ClientProductId)),
                                         string.Join(";", productsToInsert.Select(e => e.Id)));
                }

                if (existingproducts.Any())
                {
                    // update all the existing products
                    newOrEditedItems.AddRange(existingproducts.Select(UpdateProduct));
                }

                if (Logger.Current.IsDebugEnabled)
                {
                    Logger.Current.Debug("BulkAdd", "Items udpated",
                                         string.Join(";", existingproducts.Select(e => e.ClientProductId)),
                                         string.Join(";", existingproducts.Select(e => e.Id)));
                }

                return newOrEditedItems;
            }
            catch (Exception ex)
            {
                Log.Debug("ProductDataAccess.BulkAdd", "general exception", ex);
                throw;
            }
        }
    }


}
