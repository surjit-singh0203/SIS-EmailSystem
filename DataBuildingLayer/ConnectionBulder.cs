using System;
using System.Configuration;
using System.Data.SqlClient;

namespace DataBuildingLayer
{
    public class ConnectionBulder
    {
        //public readonly static SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SISContext"].ConnectionString);
        public readonly static SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SISContext"].ConnectionString);
    }
}
