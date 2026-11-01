using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading;

namespace ApiControleFinanceiro
{
    public class TSQL
    {
        #region VARIAVEIS PRIVATE
        private string strConexao;
        private SqlConnection db;
        private SqlTransaction trans;
        private bool usarTransacao;
        private bool erro = false;
        private List<SqlCommand> cmds = new List<SqlCommand>();
        #endregion

        #region CONSTRUTOR
        public TSQL(string strConexao)
        {
            this.strConexao = strConexao;
            db = new SqlConnection(this.strConexao);
        }

        public TSQL(string strConexao, bool abrirConexao, bool usarTransacao)
        {
            this.strConexao = strConexao;
            db = new SqlConnection(this.strConexao);
            if (abrirConexao == true)
            {
                Open(usarTransacao);
            }
        }

        public TSQL(string strConexao, bool abrirConexao)
        {
            this.strConexao = strConexao;
            db = new SqlConnection(this.strConexao);
            if (abrirConexao == true)
            {
                Open(false);
            }
        }

        #endregion

        #region OPEN
        public void Open(bool usarTransacao)
        {
            db.Open();
            this.usarTransacao = usarTransacao;
            if (usarTransacao)
            {
                trans = db.BeginTransaction();
            }
        }
        #endregion

        #region CLOSE
        public void Close(bool forcaRollback = false)
        {

            if (usarTransacao)
            {
                if (erro || forcaRollback)
                {
                    trans.Rollback();
                }
                else
                {
                    trans.Commit();
                }
            }

            if (db.State != System.Data.ConnectionState.Closed)
            {
                db.Close();
            }
        }
        #endregion

        #region ExecuteNonQuery
        public void ExecuteNonQuery(SqlCommand cmd)
        {
            try
            {
                cmd.Connection = db;
                if (usarTransacao)
                {
                    cmd.Transaction = trans;
                }
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                erro = true;
                throw;
            }
        }

        public void AddCommand(SqlCommand cmd)
        {
            cmd.Connection = db;
            if (usarTransacao)
            {
                cmd.Transaction = trans;
            }
            cmds.Add(cmd);
        }
        public void ExecuteComands()
        {
            try
            {
                if (cmds.Count == 0)
                {
                    throw new Exception("A lista de comandos nao foi definida.");
                }

                foreach (SqlCommand cmd in cmds)
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                erro = true;
                throw;
            }
        }

        #endregion

        #region ExecuteReader
        public DataTable ExecuteReader(SqlCommand cmd, string nomeTabela)
        {
            DataTable tbRet = new DataTable(nomeTabela);
            try
            {
                cmd.Connection = db;
                if (usarTransacao)
                {
                    cmd.Transaction = trans;
                }
                SqlDataReader dr = cmd.ExecuteReader();
                tbRet.Load(dr);
                dr.Close();
            }
            catch (Exception)
            {
                erro = true;
                throw;
            }

            return tbRet;
        }
        #endregion
         
        //#region ExecuteReader
        //public DataTable ExecuteReader(string comandoSql, string nomeTabela)
        //{
        //    DataTable tbRet = new DataTable(nomeTabela);
        //    try
        //    {
        //        SqlCommand cmd = new SqlCommand();
        //        cmd.CommandText = comandoSql;
        //        cmd.CommandType = CommandType.Text;
        //        cmd.Connection = db;
        //        if (usarTransacao)
        //        {
        //            cmd.Transaction = trans;
        //        }
        //        SqlDataReader dr = cmd.ExecuteReader();
        //        tbRet.Load(dr);
        //        dr.Close();
        //    }
        //    catch (Exception)
        //    {
        //        erro = true;
        //        throw;
        //    }

        //    return tbRet;
        //}
        //#endregion

        #region ExecuteScalar
        public Object ExecuteScalar(SqlCommand cmd)
        {
            Object ret = null;
            try
            {
                cmd.Connection = db;
                if (usarTransacao)
                {
                    cmd.Transaction = trans;
                }
                ret = cmd.ExecuteScalar();
            }
            catch (Exception)
            {
                erro = true;
                throw;
            }

            return ret;
        }
        #endregion

        /*=====================================================
                        METODOS STATICOS
         =====================================================*/

        #region MEDOTO VALIDA CONEXAO
        public static void ValidaConexao(string conexao)
        {
            using (SqlConnection db = new SqlConnection(conexao))
            {
                try
                {
                    db.Open();
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    db.Close();
                }
            }
        }
        #endregion

        #region METODO MONTA STRING CONEXAO 
        public static string MontaStringConexao(string servidor, string bancoDados, string usuario, string senha, int porta = 1433, int timeout = 30)
        {

            var builder = new SqlConnectionStringBuilder
            {
                DataSource = porta == 1433 ? servidor : $"{servidor},{porta}",
                InitialCatalog = bancoDados,
                UserID = usuario,
                Password = senha,
                ConnectTimeout = timeout,
                TrustServerCertificate = true // Útil para ambientes de desenvolvimento local
            };

            return builder.ConnectionString;
        }
        #endregion 

        #region METODOS ExecuteNonQuery 

        public static void ExecuteNonQuery(string conexao, SqlCommand cmd)
        {
            SqlConnection db = new SqlConnection(conexao);
            try
            {
                db.Open();
                cmd.Connection = db;
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                db.Close();
            }
        }

        public static void ExecuteNonQuery(string conexao, string comandoSql, params SqlParameter[] par)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = comandoSql;
            if (par != null)
            {
                cmd.Parameters.AddRange(par);
            }
            ExecuteNonQuery(conexao, cmd);
        }

        #endregion

        #region METODO SP TO TABLE 
        public static DataTable SPToTable(string conexao, string nomeProcedure, params SqlParameter[] par)
        {
            DataTable tbRet = new DataTable();

            SqlConnection db = new SqlConnection(conexao);
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = db;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = nomeProcedure;
                cmd.Parameters.AddRange(par);
                db.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                tbRet.Load(dr);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                db.Close();
            }
            return tbRet;
        }

        public static DataTable SPToTable(string conexao, string nomeProcedure, Int32 timeOut, params SqlParameter[] par)
        {
            DataTable tbRet = new DataTable();

            SqlConnection db = new SqlConnection(conexao);
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = db;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = timeOut;
                db.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                tbRet.Load(dr);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                db.Close();
            }

            return tbRet;
        }
        #endregion

        #region METODO TO TABLE
        public static DataTable ToTable(string conexao, SqlCommand cmd, string nomeTabela)
        {
            DataTable tbRet = new DataTable();
            SqlConnection db = new SqlConnection(conexao);
            SqlDataReader dr = null;
            try
            {
                cmd.Connection = db;
                db.Open();
                dr = cmd.ExecuteReader();
                tbRet = new DataTable();
                tbRet.Load(dr);

                tbRet.TableName = nomeTabela;
            }
            catch
            {
                throw;
            }
            finally
            {
                dr.Close();
                db.Close();
            }

            return tbRet;
        }

        public static DataTable ToTable(string conexao, string comandoSql, string nomeTabela, params SqlParameter[] par)
        {

            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = comandoSql;
            if (par != null)
            {
                cmd.Parameters.AddRange(par);
            }
            return ToTable(conexao, cmd, nomeTabela);
        }

        public static DataTable ToTable(string conexao, string comandoSql, string nomeTabela, Int32 timeOut, params SqlParameter[] par)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = comandoSql;
            cmd.CommandTimeout = timeOut;
            if (par != null)
            {
                cmd.Parameters.AddRange(par);
            }
            return ToTable(conexao, cmd, nomeTabela);
        }

        #endregion

        #region METODO SCALAR
        public static Object ExecuteScalar(string conexao, SqlCommand cmd)
        {
            object ret = null;

            SqlConnection db = new SqlConnection(conexao);
            try
            {
                cmd.Connection = db;
                db.Open();
                ret = cmd.ExecuteScalar();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                db.Close();
            }

            return ret;
        }

        public static Object ExecuteScalar(string conexao, string comandoSql, params SqlParameter[] par)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = comandoSql;
            if (par != null)
            {
                cmd.Parameters.AddRange(par);
            }
            return ExecuteScalar(conexao, cmd);
        }

        public static Object ExecuteScalar(string conexao, string comandoSql, Int32 timeOut, params SqlParameter[] par)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = comandoSql;
            cmd.CommandTimeout = timeOut;
            if (par != null)
            {
                cmd.Parameters.AddRange(par);
            }
            return ExecuteScalar(conexao, cmd);
        }

        #endregion

        #region GET DATABASES
        public static List<string> GetDataBases(string conexao)
        {
            List<string> ret = new List<string>();

            string comandoSql = @"  
                    select name
                      from sysdatabases
                     where dbid > 4
                       and name not like 'report%'
                     order by name";

            DataTable tbret = ToTable(conexao, comandoSql, null);

            if (tbret != null && tbret.Rows.Count > 0)
            {
                foreach (DataRow row in tbret.Rows)
                {
                    ret.Add(row["name"].ToString());
                }
            }

            return ret;
        }
        #endregion

    }
}
