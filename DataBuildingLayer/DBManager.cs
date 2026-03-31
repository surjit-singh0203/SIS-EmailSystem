using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBuildingLayer
{
    public sealed class DBManager : IDBManager, IDisposable
    {

        private IDbConnection idbConnection;
        private IDataReader idataReader;
        private IDbCommand idbCommand;
        private DataProvider providerType;
        private IDbTransaction idbTransaction = null;
        private IDbDataParameter[] idbParameters = null;
        private string strConnection;

        public DBManager()
        {
            this.providerType = DataProvider.SQLServer;
            this.strConnection = ConfigurationManager.ConnectionStrings["SISContext"].ToString();
        }

        public DBManager(DataProvider providerType)
        {
            this.providerType = providerType;
        }

        public DBManager(DataProvider providerType, string connectionString)
        {
            this.providerType = providerType;
            this.strConnection = connectionString;
        }

        public IDbCommand Command
        {
            get
            {
                return idbCommand;
            }
        }

        public IDbConnection Connection
        {
            get
            {
                return idbConnection;
            }
        }

        public string ConnectionString
        {
            get
            {
                return strConnection;
            }
            set
            {
                strConnection = value;
            }
        }

        public IDataReader DataReader
        {
            get
            {
                return idataReader;
            }
            set
            {
                idataReader = value;
            }
        }

        public IDbDataParameter[] Parameters
        {
            get
            {
                return idbParameters;
            }
        }

        public DataProvider ProviderType
        {
            get
            {
                return providerType;
            }
            set
            {
                providerType = value;
            }
        }

        public IDbTransaction Transaction
        {
            get
            {
                return idbTransaction;
            }
        }

        public void AddParameters(int index, string paramName, object objValue, ParameterDirection paramDirection = ParameterDirection.Input)
        {
            if (index < idbParameters.Length)
            {
                idbParameters[index].ParameterName = paramName;
                idbParameters[index].Value = objValue;
            }
        }

        public void BeginTransaction()
        {
            if (this.idbTransaction == null)
                idbTransaction = idbConnection.BeginTransaction(); ;
            this.idbCommand.Transaction = idbTransaction;
        }

        public void Close()
        {
            if (idbConnection.State != ConnectionState.Closed)
                idbConnection.Close();
        }

        public void CloseReader()
        {
            if (this.DataReader != null)
                this.DataReader.Close();
        }

        public void CommitTransaction()
        {
            if (this.idbTransaction != null)
                this.idbTransaction.Commit();
            idbTransaction = null;
        }

        public void CreateParameters(int paramsCount)
        {
            idbParameters = new IDbDataParameter[paramsCount];
            idbParameters = DBManagerFactory.GetParameters(this.ProviderType,
              paramsCount);
        }

        public void RollbackTransaction()
        {
            if (this.idbTransaction != null)
                this.idbTransaction.Rollback();
            idbTransaction = null;
        }

        public DataSet ExecuteDataSet(CommandType commandType, string commandText)
        {
            this.idbCommand = DBManagerFactory.GetCommand(this.ProviderType);
            PrepareCommand(idbCommand, this.Connection, this.Transaction, commandType, commandText, this.Parameters);
            IDbDataAdapter dataAdapter = DBManagerFactory.GetDataAdapter
              (this.ProviderType);
            dataAdapter.SelectCommand = idbCommand;
            DataSet dataSet = new DataSet();
            dataAdapter.Fill(dataSet);
            idbCommand.Parameters.Clear();
            return dataSet;
        }

        //public DataTable ExecuteDataTable(CommandType commandType, string commandText)
        //{
        //    this.idbCommand = DBManagerFactory.GetCommand(this.ProviderType);
        //    PrepareCommand(idbCommand, this.Connection, this.Transaction, commandType, commandText, this.Parameters);
        //    IDbDataAdapter dataAdapter = DBManagerFactory.GetDataAdapter
        //      (this.ProviderType);
        //    dataAdapter.SelectCommand = idbCommand;
        //    DataTable dataTable = new DataTable();
        //    dataAdapter.Fill(dataTable);
        //    idbCommand.Parameters.Clear();
        //    return dataTable;
        //}

        public object ExecuteNonQuery(CommandType commandType, string commandText)
        {
            this.idbCommand = DBManagerFactory.GetCommand(this.ProviderType);
            PrepareCommand(idbCommand, this.Connection, this.Transaction,
            commandType, commandText, this.Parameters);
            object returnValue = idbCommand.ExecuteNonQuery();
            idbCommand.Parameters.Clear();
            return returnValue;
        }

        public IDataReader ExecuteReader(CommandType commandType, string commandText)
        {
            this.idbCommand = DBManagerFactory.GetCommand(this.ProviderType);
            idbCommand.Connection = this.Connection;
            PrepareCommand(idbCommand, this.Connection, this.Transaction,
             commandType, commandText, this.Parameters);
            this.DataReader = idbCommand.ExecuteReader();
            idbCommand.Parameters.Clear();
            return this.DataReader;
        }

        public object ExecuteScalar(CommandType commandType, string commandText)
        {
            this.idbCommand = DBManagerFactory.GetCommand(this.ProviderType);
            PrepareCommand(idbCommand, this.Connection, this.Transaction, commandType, commandText, this.Parameters);
            object returnValue = idbCommand.ExecuteScalar();
            idbCommand.Parameters.Clear();
            return returnValue;
        }

        private void PrepareCommand(IDbCommand command, IDbConnection connection, IDbTransaction transaction, CommandType commandType, string commandText, IDbDataParameter[] commandParameters)
        {
            command.Connection = connection;
            command.CommandText = commandText;
            command.CommandType = commandType;

            if (transaction != null)
            {
                command.Transaction = transaction;
            }

            if (commandParameters != null)
            {
                AttachParameters(command, commandParameters);
            }
        }

        private void AttachParameters(IDbCommand command, IDbDataParameter[] commandParameters)
        {
            foreach (IDbDataParameter idbParameter in commandParameters)
            {
                if ((idbParameter.Direction == ParameterDirection.InputOutput)
                &&
                  (idbParameter.Value == null))
                {
                    idbParameter.Value = DBNull.Value;
                }
                command.Parameters.Add(idbParameter);
            }
        }
        public void Open()
        {
            idbConnection = DBManagerFactory.GetConnection(this.providerType);
            idbConnection.ConnectionString = this.ConnectionString;
            if (idbConnection.State != ConnectionState.Open)
                idbConnection.Open();
            this.idbCommand = DBManagerFactory.GetCommand(this.ProviderType);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.Close();
            this.idbCommand = null;
            this.idbTransaction = null;
            this.idbConnection = null;
        }

    }
}
