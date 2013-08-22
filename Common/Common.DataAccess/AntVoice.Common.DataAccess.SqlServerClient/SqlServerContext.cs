using AntVoice.Platform.Tools.Logging;
using AntVoice.Platform.Tools.Monitoring;
using AntVoice.Platform.Tools.Monitoring.Interfaces;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntVoice.Common.DataAccess.SqlServerClient
{
    public class SqlServerContext
    {
        private readonly Database _db;

        public SqlServerContext(string databaseKey)
        {
            _db = DatabaseFactory.CreateDatabase(databaseKey);
        }

        public IDataReader ExecuteReader(string query)
        {
            DbCommand command = _db.GetSqlStringCommand(query);
            return ExecuteReader(command);
        }

        public IDataReader ExecuteReader(DbCommand command)
        {
            IStopwatch watch = MonitoringTimers.Current.GetNewStopwatch(true);
            watch.Start();
            try
            {
                IDataReader result = this._db.ExecuteReader(command);

                watch.Stop();
                MonitoringTimers.Current.AddTime(Counters.DataAccess_SqlClient_ExecuteReader, watch);

                return result;
            }
            catch (Exception e)
            {
                // Exception -- Monitoring then bubble the exception
                watch.Stop();
                MonitoringTimers.Current.AddTime(Counters.DataAccess_SqlClient_ExecuteReaderFailed, watch);
                throw e;
            }
        }

        public object ExecuteScalar(string query)
        {
            DbCommand command = _db.GetSqlStringCommand(query);
            return ExecuteScalar(command);
        }

        public object ExecuteScalar(DbCommand command)
        {
            IStopwatch watch = MonitoringTimers.Current.GetNewStopwatch(true);
            watch.Start();
            try
            {
                object result = this._db.ExecuteScalar(command);

                watch.Stop();
                MonitoringTimers.Current.AddTime(Counters.DataAccess_SqlClient_ExecuteScalar, watch);

                return result;
            }
            catch (Exception e)
            {
                // Exception -- Monitoring then bubble the exception
                watch.Stop();
                MonitoringTimers.Current.AddTime(Counters.DataAccess_SqlClient_ExecuteScalarFailed, watch);
                throw e;
            }
        }

        public void ExecuteNonQuery(string query)
        {
            DbCommand command = _db.GetSqlStringCommand(query);
            ExecuteNonQuery(command);
        }

        public void ExecuteNonQuery(DbCommand command)
        {
            IStopwatch watch = MonitoringTimers.Current.GetNewStopwatch(true);
            watch.Start();
            try
            {
                this._db.ExecuteNonQuery(command);

                watch.Stop();
                MonitoringTimers.Current.AddTime(Counters.DataAccess_SqlClient_ExecuteNonQuery, watch);
            }
            catch (Exception e)
            {
                // Exception -- Monitoring then bubble the exception
                watch.Stop();
                MonitoringTimers.Current.AddTime(Counters.DataAccess_SqlClient_ExecuteNonQueryFailed, watch);
                throw e;
            }
        }

        #region Query building

        public DbCommand GetStoredProcCommand(string storedProcName)
        {
            return this._db.GetStoredProcCommand(storedProcName);
        }

		public SqlServerContext AddInParameter(DbCommand command, string name, DbType dbType, object value)
		{
			this._db.AddInParameter(command, name, dbType, value);
			return this;
		}

		public SqlServerContext AddOutParameter(DbCommand command, string name, DbType dbType, int size)
		{
			this._db.AddOutParameter(command, name, dbType, size);
			return this;
		}
		
		public SqlServerContext AddInParameter(DbCommand command, string name, SqlDbType dbType, object value)
		{
			((SqlDatabase)this._db).AddInParameter(command, name, dbType, value);
			return this;
		}

		public SqlServerContext AddOutParameter(DbCommand command, string name, SqlDbType dbType, int size)
		{
			((SqlDatabase)this._db).AddOutParameter(command, name, dbType, size);
			return this;
		}

        public object GetOutParamValue(DbCommand command, string name)
        {
            return _db.GetParameterValue(command, name);
        }

        #endregion
    }
}
