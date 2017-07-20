#region The Database Copyright & Version History
/*
 * ============================================================== 
 * 
 * Database, Version 1.0
 * 
 * Copyright (c) 2007-2008 上海地听计算机技术有限公司.  版权所有.
 * 
 * 张伟锋
 * 
 * 修改：增加了对SqlParameter方式的支持，并说明了采用此种方法的优点。此外明确采用实例化方式
 *       而不是采用静态类方式。
 * 张伟锋              2010年11月25日
 * ====================================================================
 * 
 * 功能说明：用于数据库连接，并且执行相关数据库操作，包括查询、插入、编辑、删除等功能，还有包括存储过程的调用
 *           以及有关数据集有效性等的判定。此类主要用于SQL Server数据库的操作，因此采用SqlConnection进行连接以
 *           提高性能。
 * 一、SQL语句中使用parameters的好处：
 * 1、提高SQL语句的性能
 * 每个不同的SQL语句在执行前都会进行相应的预解析等操作，这个过程是比较耗时的，而任何值的不
 * 同也是SQL不同，比如：SELECT * FROM USER WHERE USER_ID = 1与SELECT * FROM USER WHERE USER_ID = 2是不同的SQL语
 * 句。如果将where条件中的USER_ID的值通过参数传递的话，两个SQL内容就一样，数据库系统就只需要进行一次解析就可以了，
 * 然后缓存起来，以后可以直接使用，从而大大提高SQL语句的性能。
 *
 * 2、避免因为程序员的考虑不足引起的SQL注入安全问题
 * 比如：以 SELECT * FROM USER WHERE USER_NAME = '页面中的用户名'
 * 如果用户这样书写的话，用户名合法一般是没有什么问题的，但是，如果我输入的用户名为：1' or '0' = '0这样替换上面
 * 的内容后，变成了：
 * SELECT * FROM USER WHERE USER_NAME = '1' or '0' = '0'这样就可以查询出所有的用户数据了，造成信息泄露 
 * 
 * 3、类型匹配，更加清晰，对于二进制等操作直接可以通过参数实现
 * 
 * 二、SQL语句中使用parameters的缺点：
 * 1、不利于直接获取SQL语句进入数据库系统调试
 *
 */
#endregion


using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace Readearth.Data
{
    /// <summary>
    /// Database 的摘要说明
    /// </summary>
    public class MySQLDatabase
    {
        private string m_ConnectionString;//数据库连接字符串

        public MySQLDatabase()
        {

            //获取连接数据库的参数
            ConnectionStringSettings settings;
            settings =
                ConfigurationManager.ConnectionStrings["MYSQLDBCONFIG"];

            this.m_ConnectionString = settings.ConnectionString;
        }


        public MySQLDatabase(string connectionString)
        {
            this.m_ConnectionString = connectionString;
        }

        public string ConnectionString
        {
            get
            {
                return this.m_ConnectionString;
            }
            set
            {
                this.m_ConnectionString = value;
            }
        }



        /// <summary>
        /// 根据输入的查询语句，获取符合条件的数据集，数据集与DataReader的区别
        /// 是DataReader是向前只读的，用于快速只读访问因此占用的资源少。Dataset
        /// 提供数据存储空间。
        /// </summary>
        /// <param name="queryString">查询条件，当queryString为安全SQL语句的情况</param>
        /// <returns>数据集</returns>
        public DataSet GetDataset(string queryString)
        {
            using (MySqlConnection connection = new MySqlConnection(m_ConnectionString))
            {
                //创建Command对象
                MySqlCommand dbCommand = new MySqlCommand(queryString, connection);
                try
                {
                    connection.Open();

                    //创建数据适配器
                    MySqlDataAdapter dataAdapter = new MySqlDataAdapter();
                    dataAdapter.SelectCommand = dbCommand;

                    //创建数据集，提取数据
                    DataSet dataset = new DataSet();
                    dataAdapter.Fill(dataset);

                    return dataset;
                }
                catch (Exception ex)
                {
                    connection.Close();
                    connection.Dispose();
                    throw ex;
                }
            }
        }

        //public DataSet GetDataset(string queryString, params OracleParameter[] values)
        //{
        //    using (MySqlConnection connection = new MySqlConnection(m_ConnectionString))
        //    {
        //        //创建Command对象
        //        OracleCommand dbCommand = new OracleCommand(queryString, connection);
        //        try
        //        {
        //            connection.Open();

        //            //创建数据适配器
        //            OracleDataAdapter dataAdapter = new OracleDataAdapter();
        //            dbCommand.Parameters.AddRange(values);
        //            dataAdapter.SelectCommand = dbCommand;

        //            //创建数据集，提取数据
        //            DataSet dataset = new DataSet();
        //            dataAdapter.Fill(dataset);

        //            return dataset;
        //        }
        //        catch (Exception ex)
        //        {
        //            connection.Close();
        //            connection.Dispose();
        //            throw ex;
        //        }
        //    }
        //}
        /// <summary>
        /// 获取DataReader对象，用于获取只读数据
        /// </summary>
        /// <param name="queryString">查询条件</param>
        /// <returns></returns>
        //public OracleDataReader GetDataReader(string queryString)
        //{
        //    OracleConnection connection = new OracleConnection(m_ConnectionString);
        //    try
        //    {
        //        //创建Command对象
        //        OracleCommand dbCommand = new OracleCommand(queryString, connection);
        //        connection.Open();

        //        //创建DataReader对象，用于获取只读数据
        //        OracleDataReader dataReader = dbCommand.ExecuteReader(CommandBehavior.CloseConnection);
        //        return dataReader;
        //    }
        //    catch (Exception ex)
        //    {
        //        connection.Close();
        //        connection.Dispose();
        //        throw ex;
        //    }

        //}
        //public OracleDataReader GetDataReader(string queryString, params OracleParameter[] values)
        //{
        //    OracleConnection connection = new OracleConnection(m_ConnectionString);
        //    try
        //    {
        //        //创建Command对象
        //        OracleCommand dbCommand = new OracleCommand(queryString, connection);
        //        connection.Open();

        //        //创建DataReader对象，用于获取只读数据
        //        dbCommand.Parameters.AddRange(values);
        //        OracleDataReader dataReader = dbCommand.ExecuteReader(CommandBehavior.CloseConnection);
        //        return dataReader;
        //    }
        //    catch (Exception ex)
        //    {
        //        connection.Close();
        //        connection.Dispose();
        //        throw ex;
        //    }

        //}



        /// <summary>
        /// 根据输入的查询符合条件记录集个数的语句返回记录集的个数
        /// </summary>
        /// <param name="countString">符合条件记录集个数的SQL语句。此sql语句如果写成如下形式，可以返回当前插入的记录的主键
        /// string sql =
		///		"INSERT Card (CardTypeId, CardNo, CardPassword, CardDesc, CardTime, CardState)" +
		///		"VALUES (@CardTypeId, @CardNo, @CardPassword, @CardDesc, @CardTime, @CardState)";
		///		
        ///	sql += " ; SELECT @@IDENTITY";
        /// </param>
        /// <returns>记录集个数</returns>
        public string GetFirstValue(string countString)
        {
            string value = "";
            using (MySqlConnection connection = new MySqlConnection(m_ConnectionString))
            {
                MySqlCommand dbCommand = new MySqlCommand(countString, connection);
                try
                {
                    connection.Open();
                    object objFirstValue = dbCommand.ExecuteScalar();
                    if (objFirstValue != null)
                        value = objFirstValue.ToString();//ExecuteScalar方法，它从相关的查询中返回第一行和第一列的值

                }
                catch (Exception ex)
                {
                    connection.Close();
                    connection.Dispose();
                    throw ex;
                }
            }
            return value;
        }
        //public string GetFirstValue(string countString, params OracleParameter[] values)
        //{
        //    string value = "";
        //    using (OracleConnection connection = new OracleConnection(m_ConnectionString))
        //    {
        //        OracleCommand dbCommand = new OracleCommand(countString, connection);
        //        try
        //        {
        //            connection.Open();
        //            dbCommand.Parameters.AddRange(values);
        //            object objFirstValue = dbCommand.ExecuteScalar();
        //            if (objFirstValue != null)
        //                value = objFirstValue.ToString();//ExecuteScalar方法，它从相关的查询中返回第一行和第一列的值

        //        }
        //        catch (Exception ex)
        //        {
        //            connection.Close();
        //            connection.Dispose();
        //            throw ex;
        //        }
        //    }
        //    return value;
        //}

        /// <summary>
        /// 对连接对象执行 SQL 语句
        /// </summary>
        /// <param name="editSQL">被执行的SQL语句，一般是编辑类型的SQL语句，当然以可以执行其它类型的SQL语句</param>
        /// <returns>对于 UPDATE、INSERT 和 DELETE 语句，返回值为该命令所影响的行数。对于其他所有类型的语句，返回值为 -1</returns>
        public int Execute(string editSQL)
        {
            using (MySqlConnection connection = new MySqlConnection(m_ConnectionString))
            {
                MySqlCommand dbCommand = new MySqlCommand(editSQL, connection);
                try
                {
                    connection.Open();
                    return dbCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    connection.Close();
                    connection.Dispose();
                    throw ex;
                }
            }
        }
        //public int Execute(string editSQL, params OracleParameter[] values)
        //{
        //    using (OracleConnection connection = new OracleConnection(m_ConnectionString))
        //    {
        //        OracleCommand dbCommand = new OracleCommand(editSQL, connection);
        //        try
        //        {
        //            connection.Open();
        //            dbCommand.Parameters.AddRange(values);
        //            return dbCommand.ExecuteNonQuery();
        //        }
        //        catch (Exception ex)
        //        {
        //            connection.Close();
        //            connection.Dispose();
        //            throw ex;
        //        }
        //    }
        //}
        /// <summary>
        /// 进行数据库连接，如果不能连接则返回False
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            using (MySqlConnection connection = new MySqlConnection(m_ConnectionString))
            {
                try
                {
                    connection.Open();
                    return true;
                }
                catch (Exception ex)
                {
                    connection.Close();
                    connection.Dispose();
                    throw ex;
                }
            }
        }
    }
}
