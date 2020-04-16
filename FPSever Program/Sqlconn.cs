using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPSever_Program
{
    class Sqlconn
    {
        public static string Str_sqlcon = "Server=AMD-R9-3900x;Database=FPServerDB;User id=sa;PWD=CYJjzr2014*";
        public static SqlConnection Conn;
        public static SqlConnection Getcon()
        {
            Conn = new SqlConnection(Str_sqlcon);
            Conn.Open();
            return Conn;
        }
        public static void con_close()
        {
            if (Conn.State==ConnectionState.Open)
            {
                Conn.Close();
                Conn.Dispose();
            }
        }
        public SqlDataReader getcom(string Sqlstr)
        {
            Getcon();
            //创建一个sqlconnection以执行查询语句
            SqlCommand sqlCommand = Conn.CreateCommand();
            sqlCommand.CommandText = Sqlstr;
            SqlDataReader Myread = sqlCommand.ExecuteReader();
            return Myread;
        }
        public void getsqlcom(string Sqlstr)
        {
            Getcon();
            SqlCommand sqlcom = new SqlCommand(Sqlstr, Conn);
            sqlcom.ExecuteNonQuery();
            sqlcom.Dispose();
            con_close();
        }
        public DataSet GetDataSet(string Sqlstr,string tablename)
        {
            Getcon();
            SqlDataAdapter sqlData = new SqlDataAdapter(Sqlstr, Conn);
            DataSet mydataset = new DataSet();
            sqlData.Fill(mydataset,tablename);
            con_close();
            return mydataset;
        }
    }
}
