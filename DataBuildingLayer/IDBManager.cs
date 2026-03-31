using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBuildingLayer
{
    public enum DataProvider
    {
        OleDb,
        Odbc,
        SQLServer
        //Oracle
        // any other data source type
    }
    public interface IDBManager
    {
        DataProvider ProviderType
        {
            get;
            set;
        }

        string ConnectionString
        {
            get;
            set;
        }

        IDbConnection Connection
        {
            get;
        }
        IDbTransaction Transaction
        {
            get;
        }

        IDataReader DataReader
        {
            get;
        }
        IDbCommand Command
        {
            get;
        }

        IDbDataParameter[] Parameters
        {
            get;
        }

        void Open();
        void BeginTransaction();
        void CommitTransaction();
        void CreateParameters(int paramsCount);
        void AddParameters(int index, string paramName, object objValue, ParameterDirection paramDirection = ParameterDirection.Input);
        IDataReader ExecuteReader(CommandType commandType, string
        commandText);
        DataSet ExecuteDataSet(CommandType commandType, string
        commandText);
        object ExecuteScalar(CommandType commandType, string commandText);
        object ExecuteNonQuery(CommandType commandType, string commandText);
        void CloseReader();
        void Close();
        void Dispose();
    }
}
