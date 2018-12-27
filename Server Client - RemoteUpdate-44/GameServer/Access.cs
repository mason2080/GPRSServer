using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;

namespace GameServer
{
    class Access
    {
        public void ConnectAccess()
        {
            string   strDSN   = "Provider=Microsoft.Jet.OLEDB.4.0;DataSource=c:\\mcTest.MDB";       
            string   strSQL   = "INSERT   INTO   Developer(Name,   Address   )   VALUES( 'NewName',   'NewAddress')";
            //   create   Objects   of   ADOConnection   and   ADOCommand         
            OleDbConnection myConn = new OleDbConnection(strDSN);
            OleDbCommand myCmd = new OleDbCommand(strSQL, myConn);
            try
            {
                myConn.Open();
                myCmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
            }
            finally
            {
                myConn.Close();
            }   
        }
    }
}
