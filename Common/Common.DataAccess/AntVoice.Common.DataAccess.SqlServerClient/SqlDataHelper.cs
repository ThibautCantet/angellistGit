using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntVoice.Common.DataAccess.SqlServerClient
{
    public class SqlDataHelper
    {
        private delegate bool ValueParser<ParserType>(string value, out ParserType result);
        private static ReturnType GetValue<ReturnType>(string field, IDataReader reader, ReturnType returnIfNull, ValueParser<ReturnType> parser)
        {
            //MT Attention : crash si colonne non présente
            if (reader[field] == DBNull.Value)
            {
                return returnIfNull;
            }
            else
            {
                ReturnType result;
                if (reader[field] is ReturnType)
                    return (ReturnType)reader[field];
                else if (parser(reader[field].ToString(), out result))
                    return result;
                else
                    return returnIfNull;
            }
        }

        public static Guid GetGuidValue(string name, IDataReader reader)
        {
            if (reader[name] == DBNull.Value)
            {
                return Guid.Empty;
            }
            else
            {
                return (Guid)reader[name];
            }
        }

        public static string GetStringValue(string name, IDataReader reader)
        {
            return GetStringValue(name, reader, string.Empty);
        }
        public static string GetStringValue(string name, IDataReader reader, string returnIfNull)
        {
            if (reader[name] == DBNull.Value)
                return returnIfNull;
            else
                return reader[name].ToString();
        }

        public static byte[] GetBytes(string name, IDataReader reader)
        {
            if (reader[name] == DBNull.Value)
                return null;

            return (byte[])reader[name];
        }

        public static short GetShortValue(string name, IDataReader reader)
        {
            return GetShortValue(name, reader, 0);
        }
        public static short GetShortValue(string name, IDataReader reader, short returnIfNull)
        {
            return GetValue<short>(name, reader, returnIfNull, short.TryParse);
        }
        public static short? GetNullableShort(string name, IDataReader reader)
        {
            if (reader[name] == DBNull.Value)
                return null;
            else
                return new short?((short)reader[name]);
        }

        public static int GetIntValue(string name, IDataReader reader)
        {
            return GetIntValue(name, reader, 0);
        }
        public static int GetIntValue(string name, IDataReader reader, int returnIfNull)
        {
            return GetValue<int>(name, reader, returnIfNull, int.TryParse);
        }
        public static int? GetNullableInt(string name, IDataReader reader)
        {
            if (reader[name] == DBNull.Value)
                return null;
            else
                return new int?((int)reader[name]);
        }

        public static uint GetUnsignedIntValue(string name, IDataReader reader)
        {
            return GetUnsignedIntValue(name, reader, 0);
        }
        public static uint GetUnsignedIntValue(string name, IDataReader reader, uint returnIfNull)
        {
            return GetValue<uint>(name, reader, returnIfNull, uint.TryParse);
        }
        public static uint? GetNullableUnsignedIntValue(string name, IDataReader reader)
        {
            if (reader[name] == DBNull.Value)
                return null;
            else
                return new uint?((uint)reader[name]);
        }

        public static long GetLongValue(string name, IDataReader reader)
        {
            return GetLongValue(name, reader, 0);
        }
        public static long GetLongValue(string name, IDataReader reader, long returnIfNull)
        {
            return GetValue<long>(name, reader, returnIfNull, long.TryParse);
        }
        public static long? GetNullableLong(string name, IDataReader reader)
        {
            if (reader[name] == DBNull.Value)
                return null;
            else
                return new long?((long)reader[name]);
        }

        public static byte GetByteValue(string name, IDataReader reader)
        {
            return GetByteValue(name, reader, 0);
        }
        public static byte GetByteValue(string name, IDataReader reader, byte returnIfNull)
        {
            return GetValue<byte>(name, reader, returnIfNull, byte.TryParse);
        }

        public static decimal GetDecimalValue(string name, IDataReader reader)
        {
            return GetDecimalValue(name, reader, 0);
        }
        public static decimal GetDecimalValue(string name, IDataReader reader, decimal returnIfNull)
        {
            return GetValue<decimal>(name, reader, returnIfNull, decimal.TryParse);
        }

        public static double GetDoubleValue(string name, IDataReader reader)
        {
            return GetDoubleValue(name, reader, 0.0);
        }
        public static double GetDoubleValue(string name, IDataReader reader, double returnIfNull)
        {
            return GetValue<double>(name, reader, returnIfNull, double.TryParse);
        }

        public static float GetFloatValue(string name, IDataReader reader)
        {
            return GetFloatValue(name, reader, 0.0f);
        }
        public static float GetFloatValue(string name, IDataReader reader, float returnIfNull)
        {
            return GetValue<float>(name, reader, returnIfNull, float.TryParse);
        }

        public static bool GetBoolValue(string name, IDataReader reader)
        {
            return GetBoolValue(name, reader, false);
        }
        public static bool GetBoolValue(string name, IDataReader reader, bool returnIfNull)
        {
            if (reader[name] == DBNull.Value)
            {
                return returnIfNull;
            }
            else
            {
                bool result;
                if (reader[name] is bool)
                    return (bool)reader[name];
                else if (reader[name] is int)
                    return ((int)reader[name]) == 1;
                else if (bool.TryParse(reader[name].ToString(), out result))
                    return result;
                else
                    return returnIfNull;
            }
        }

        public static DateTime GetDateTimeValue(string name, IDataReader reader)
        {
            return GetDateTimeValue(name, reader, DateTime.MinValue);
        }
        public static DateTime GetDateTimeValue(string name, IDataReader reader, DateTime returnIfNull)
        {
            return GetValue<DateTime>(name, reader, returnIfNull, DateTime.TryParse);
        }
        public static DateTime? GetNullableDateTime(string name, IDataReader reader)
        {
            if (reader[name] == DBNull.Value)
                return null;
            else
                return new DateTime?((DateTime)reader[name]);
        }
        public static int GetTimeStamp(DateTime value)
        {
            TimeSpan t = (value - new DateTime(1970, 1, 1).ToLocalTime());
            int timestamp = (int)t.TotalSeconds;

            return timestamp;
        }
    }
}
