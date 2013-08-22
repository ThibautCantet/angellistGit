using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DataAccess.MongoClient
{
    /// <summary>
    /// Int generator for _id (MongoDB)
    /// </summary>
    public class IntIdGenerator : IIdGenerator
    {
		private const string _sequenceIdName = "IDSequence";

        private static readonly IntIdGenerator __instance = new IntIdGenerator();

        public object GenerateId(object container, object document)
        {
			var idSequenceCollection = ((MongoCollection)container).Database.GetCollection(_sequenceIdName);
            var query = Query.EQ("_id", ((MongoCollection)container).Name);
            return idSequenceCollection
                .FindAndModify(query, null, Update.Inc("seq", 1), true, true)
                .ModifiedDocument["seq"]
                .AsInt32;
        }

        public object UpdateId(object container, int count)
        {
			var idSequenceCollection = ((MongoCollection)container).Database.GetCollection(_sequenceIdName);
            var query = Query.EQ("_id", ((MongoCollection)container).Name);
            return idSequenceCollection
                .FindAndModify(query, null, Update.Inc("seq", count), true, true)
                .ModifiedDocument["seq"]
                .AsInt32;
        }

        public bool IsEmpty(object id)
        {
            return (int)id == 0;
        }

        public static IntIdGenerator Instance
        {
            get
            {
                return __instance;
            }

        }
    }
}
