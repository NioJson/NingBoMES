using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml.Linq;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace MissionBossAot.Common
{
    public class GlobalSqlExecute
    {
        private ConcurrentQueue<SqlItemObj> sqlWaitList = new ConcurrentQueue<SqlItemObj>();
        public static GlobalSqlExecute sGlobalSqlExecute = new GlobalSqlExecute();

        public GlobalSqlExecute()
        {
            Thread td = new Thread(new ThreadStart(ExecuteSql));
            td.IsBackground = true;
            td.Start();
        }
        public void AddExecSql(SqlItemObj sql)
        {
            sqlWaitList.Enqueue(sql);
        }



        private void ExecuteSql()
        {
            while (!GlobalProperty.sGlobalProperty.GetMainFormCloseStatus())
            {
                if (sqlWaitList.TryDequeue(out SqlItemObj sql))
                {
                    if (sql != null)
                    {
                        if (sql.IsProcedure)
                        {
                            if (sql.ProSqlString != null && sql.ProSqlString.Count >0)
                            {
                                using (var helper = new SqlConnectionHelper())
                                {
                                    try
                                    {
                                        helper.BeginTransaction();
                                        foreach (var item in sql.ProSqlString)
                                        {
                                            helper.ExecuteNonQuery(item);
                                        }
                                        helper.CommitTransaction();
                                    }
                                    catch (Exception ex)
                                    {
                                        helper.RollbackTransaction();
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(sql.Sql))
                            {
                                using (var helper = new SqlConnectionHelper())
                                {
                                    try
                                    {
                                        helper.ExecuteNonQuery(sql.Sql);
                                    }
                                    catch (Exception ex)
                                    {
                                    }
                                }
                            }
                        }
                    }
                }
                Thread.Sleep(1000);
            }
        }
    }
    public class SqlItemObj
    {
        public string Sql {  get; set; }
        public bool IsProcedure {  get; set; }
        public List<string> ProSqlString {  get; set; }
    }
}
