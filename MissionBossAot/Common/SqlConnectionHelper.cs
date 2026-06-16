using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionBossAot.Common
{
    /// <summary>
    /// SQL Server 数据库连接管理类
    /// </summary>
    public class SqlConnectionHelper : IDisposable
    {
        private SqlConnection _connection;
        private SqlTransaction _transaction;
        private string _connectionString;
        private bool _disposed = false;

        /// <summary>
        /// 获取当前连接状态
        /// </summary>
        public ConnectionState State => _connection?.State ?? ConnectionState.Closed;

        /// <summary>
        /// 获取是否在事务中
        /// </summary>
        public bool InTransaction => _transaction != null;

        /// <summary>
        /// 默认构造函数（使用配置文件中的默认连接字符串）
        /// </summary>
        public SqlConnectionHelper() : this("DefaultConnection")
        {
        }

        /// <summary>
        /// 使用配置文件中的连接字符串名称
        /// </summary>
        /// <param name="connectionStringName">配置文件中的连接字符串名称</param>
        public SqlConnectionHelper(string connectionStringName)
        {
            _connectionString = ConfigurationManager.ConnectionStrings[connectionStringName]?.ConnectionString;
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new ArgumentException($"未找到名为 '{connectionStringName}' 的连接字符串");
            }
            InitializeConnection();
        }

        /// <summary>
        /// 使用自定义连接字符串
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="isCustomString">是否自定义字符串（区分构造函数）</param>
        public SqlConnectionHelper(string connectionString, bool isCustomString)
        {
            _connectionString = connectionString;
            InitializeConnection();
        }

        /// <summary>
        /// 初始化连接
        /// </summary>
        private void InitializeConnection()
        {
            _connection = new SqlConnection(_connectionString);
        }

        /// <summary>
        /// 打开连接
        /// </summary>
        public void Open()
        {
            try
            {
                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"数据库连接打开失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            try
            {
                if (_connection != null && _connection.State != ConnectionState.Closed)
                {
                    _connection.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"数据库连接关闭失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取SqlConnection对象
        /// </summary>
        public SqlConnection GetConnection()
        {
            if (_connection.State != ConnectionState.Open)
            {
                Open();
            }
            return _connection;
        }

        /// <summary>
        /// 开始事务
        /// </summary>
        public void BeginTransaction()
        {
            if (_transaction != null)
            {
                throw new Exception("已有未完成的事务");
            }

            if (_connection.State != ConnectionState.Open)
            {
                Open();
            }

            _transaction = _connection.BeginTransaction();
        }

        /// <summary>
        /// 开始事务（指定隔离级别）
        /// </summary>
        /// <param name="isolationLevel">隔离级别</param>
        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            if (_transaction != null)
            {
                throw new Exception("已有未完成的事务");
            }

            if (_connection.State != ConnectionState.Open)
            {
                Open();
            }

            _transaction = _connection.BeginTransaction(isolationLevel);
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public void CommitTransaction()
        {
            if (_transaction == null)
            {
                throw new Exception("没有活动的事务");
            }

            try
            {
                _transaction.Commit();
                _transaction = null;
            }
            catch (Exception ex)
            {
                throw new Exception($"事务提交失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public void RollbackTransaction()
        {
            if (_transaction == null)
            {
                throw new Exception("没有活动的事务");
            }

            try
            {
                _transaction.Rollback();
                _transaction = null;
            }
            catch (Exception ex)
            {
                throw new Exception($"事务回滚失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 创建并返回SqlCommand对象
        /// </summary>
        /// <param name="sql">SQL语句或存储过程名称</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>SqlCommand对象</returns>
        public SqlCommand CreateCommand(string sql, CommandType commandType = CommandType.Text)
        {
            // 确保连接已打开
            if (_connection.State != ConnectionState.Open)
            {
                Open();
            }

            var command = new SqlCommand(sql, _connection);
            command.CommandType = commandType;

            if (_transaction != null)
            {
                command.Transaction = _transaction;
            }

            return command;
        }

        /// <summary>
        /// 执行非查询SQL（增、删、改）
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数数组</param>
        /// <returns>受影响的行数</returns>
        public int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using (var command = CreateCommand(sql))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 执行查询，返回DataTable
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数数组</param>
        /// <returns>DataTable</returns>
        public DataTable ExecuteDataTable(string sql, params SqlParameter[] parameters)
        {
            using (var adapter = new SqlDataAdapter())
            using (var command = CreateCommand(sql))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                adapter.SelectCommand = command;
                var dataTable = new DataTable();
                adapter.Fill(dataTable);
                return dataTable;
            }
        }

        /// <summary>
        /// 执行查询，返回DataSet
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数数组</param>
        /// <returns>DataSet</returns>
        public DataSet ExecuteDataSet(string sql, params SqlParameter[] parameters)
        {
            using (var adapter = new SqlDataAdapter())
            using (var command = CreateCommand(sql))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                adapter.SelectCommand = command;
                var dataSet = new DataSet();
                adapter.Fill(dataSet);
                return dataSet;
            }
        }

        /// <summary>
        /// 执行查询，返回第一行第一列的值
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数数组</param>
        /// <returns>第一行第一列的值</returns>
        public object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using (var command = CreateCommand(sql))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }
                return command.ExecuteScalar();
            }
        }

        /// <summary>
        /// 执行查询，返回SqlDataReader（需要手动关闭）
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数数组</param>
        /// <returns>SqlDataReader</returns>
        public SqlDataReader ExecuteReader(string sql, params SqlParameter[] parameters)
        {
            var command = CreateCommand(sql);
            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        /// <summary>
        /// 创建参数
        /// </summary>
        public static SqlParameter CreateParameter(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        /// <summary>
        /// 创建参数（指定数据类型）
        /// </summary>
        public static SqlParameter CreateParameter(string name, SqlDbType dbType, object value)
        {
            return new SqlParameter(name, dbType) { Value = value ?? DBNull.Value };
        }

        /// <summary>
        /// 创建输出参数
        /// </summary>
        public static SqlParameter CreateOutputParameter(string name, SqlDbType dbType, int size = 0)
        {
            var parameter = new SqlParameter(name, dbType, size)
            {
                Direction = ParameterDirection.Output
            };
            return parameter;
        }

        /// <summary>
        /// 测试连接是否正常
        /// </summary>
        public bool TestConnection()
        {
            try
            {
                Open();
                Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_transaction != null)
                    {
                        _transaction.Dispose();
                        _transaction = null;
                    }

                    if (_connection != null)
                    {
                        if (_connection.State != ConnectionState.Closed)
                        {
                            _connection.Close();
                        }
                        _connection.Dispose();
                        _connection = null;
                    }
                }
                _disposed = true;
            }
        }
    }
}
