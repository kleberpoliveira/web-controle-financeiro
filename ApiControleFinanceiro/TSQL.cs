using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiControleFinanceiro
{
    public class TSQL : IDisposable
    {
        #region VARIAVEIS PRIVATE
        private string strConexao;
        private SqlConnection db;
        private SqlTransaction? trans;
        private bool usarTransacao;
        private bool erro = false;
        private bool disposed = false;
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
                try
                {
                    Open(usarTransacao);
                }
                catch
                {
                    // Se não conseguiu abrir, não deixa a conexão pendurada
                    db.Dispose();
                    throw;
                }
            }
        }

        public TSQL(string strConexao, bool abrirConexao)
        {
            this.strConexao = strConexao;
            db = new SqlConnection(this.strConexao);
            if (abrirConexao == true)
            {
                try
                {
                    Open(false);
                }
                catch
                {
                    db.Dispose();
                    throw;
                }
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
        // Confirma a transação (commit), a menos que tenha ocorrido erro ou forcaRollback = true,
        // e fecha a conexão.
        public void Close(bool forcaRollback = false)
        {
            if (usarTransacao && trans != null)
            {
                if (erro || forcaRollback)
                {
                    trans.Rollback();
                }
                else
                {
                    trans.Commit();
                }

                trans.Dispose();
                trans = null;
                usarTransacao = false;
            }

            if (db.State != System.Data.ConnectionState.Closed)
            {
                db.Close();
            }
        }
        #endregion

        #region DISPOSE
        // Permite usar: using (var tsql = new TSQL(conexao, true, true)) { ... tsql.Close(); }
        // Se o bloco terminar sem chamar Close() (por exemplo, por causa de uma exceção),
        // a transação pendente é DESFEITA (rollback) e a conexão é fechada.
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            disposed = true;

            try
            {
                if (usarTransacao && trans != null)
                {
                    try
                    {
                        trans.Rollback();
                    }
                    catch (Exception)
                    {
                        // Conexão já quebrada: o erro que importa é o original, não este
                    }

                    trans.Dispose();
                    trans = null;
                    usarTransacao = false;
                }
            }
            finally
            {
                foreach (SqlCommand cmd in cmds)
                {
                    cmd.Dispose();
                }
                cmds.Clear();

                db.Dispose();
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
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    tbRet.Load(dr);
                }
            }
            catch (Exception)
            {
                erro = true;
                throw;
            }

            return tbRet;
        }
        #endregion

        #region ExecuteScalar
        public Object? ExecuteScalar(SqlCommand cmd)
        {
            Object? ret = null;
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

        // Versão tipada: ex.: int novoId = tsql.ExecuteScalar<int>(cmd);
        public T ExecuteScalar<T>(SqlCommand cmd)
        {
            return ConverterValor<T>(ExecuteScalar(cmd));
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
                db.Open();
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

        #region METODO PAR (CRIA PARAMETRO)
        // Cria um SqlParameter e troca null por DBNull.Value
        // Uso: TSQL.Par("@id_conta_bancaria", jsonPar.IdContaBancaria)
        public static SqlParameter Par(string nome, object? valor)
        {
            return new SqlParameter(nome, valor ?? DBNull.Value);
        }
        #endregion

        #region METODO CONVERTER VALOR (PRIVADO)
        // Converte o retorno de um ExecuteScalar para o tipo pedido.
        // Se vier null/DBNull, devolve o valor padrão do tipo (0, null, etc.)
        private static T ConverterValor<T>(object? valor)
        {
            if (valor == null || valor == DBNull.Value)
            {
                return default!;
            }

            Type tipo = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            return (T)System.Convert.ChangeType(valor, tipo);
        }
        #endregion

        #region METODOS ExecuteNonQuery 

        public static void ExecuteNonQuery(string conexao, SqlCommand cmd)
        {
            using (SqlConnection db = new SqlConnection(conexao))
            {
                db.Open();
                cmd.Connection = db;
                cmd.ExecuteNonQuery();
            }
        }

        public static void ExecuteNonQuery(string conexao, string comandoSql, params SqlParameter[] par)
        {
            using (SqlCommand cmd = new SqlCommand(comandoSql))
            {
                cmd.CommandType = CommandType.Text;
                if (par != null)
                {
                    cmd.Parameters.AddRange(par);
                }
                ExecuteNonQuery(conexao, cmd);
            }
        }

        #endregion

        #region METODO SP TO TABLE 
        public static DataTable SPToTable(string conexao, string nomeProcedure, params SqlParameter[] par)
        {
            return SPToTable(conexao, nomeProcedure, 30, par);
        }

        public static DataTable SPToTable(string conexao, string nomeProcedure, Int32 timeOut, params SqlParameter[] par)
        {
            using (SqlConnection db = new SqlConnection(conexao))
            using (SqlCommand cmd = new SqlCommand(nomeProcedure, db))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = timeOut;
                if (par != null)
                {
                    cmd.Parameters.AddRange(par);
                }

                db.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    DataTable tbRet = new DataTable();
                    tbRet.Load(dr);
                    return tbRet;
                }
            }
        }
        #endregion

        #region METODO TO TABLE
        public static DataTable ToTable(string conexao, SqlCommand cmd, string nomeTabela)
        {
            using (SqlConnection db = new SqlConnection(conexao))
            {
                cmd.Connection = db;
                db.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    DataTable tbRet = new DataTable(nomeTabela);
                    tbRet.Load(dr);
                    return tbRet;
                }
            }
        }

        public static DataTable ToTable(string conexao, string comandoSql, string nomeTabela, params SqlParameter[] par)
        {
            return ToTable(conexao, comandoSql, nomeTabela, 30, par);
        }

        public static DataTable ToTable(string conexao, string comandoSql, string nomeTabela, Int32 timeOut, params SqlParameter[] par)
        {
            using (SqlCommand cmd = new SqlCommand(comandoSql))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = timeOut;
                if (par != null)
                {
                    cmd.Parameters.AddRange(par);
                }
                return ToTable(conexao, cmd, nomeTabela);
            }
        }

        #endregion

        #region METODO SCALAR
        public static Object? ExecuteScalar(string conexao, SqlCommand cmd)
        {
            using (SqlConnection db = new SqlConnection(conexao))
            {
                cmd.Connection = db;
                db.Open();
                return cmd.ExecuteScalar();
            }
        }

        public static Object? ExecuteScalar(string conexao, string comandoSql, params SqlParameter[] par)
        {
            return ExecuteScalar(conexao, comandoSql, 30, par);
        }

        public static Object? ExecuteScalar(string conexao, string comandoSql, Int32 timeOut, params SqlParameter[] par)
        {
            using (SqlCommand cmd = new SqlCommand(comandoSql))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = timeOut;
                if (par != null)
                {
                    cmd.Parameters.AddRange(par);
                }
                return ExecuteScalar(conexao, cmd);
            }
        }

        // Versões tipadas: ex.: int total = TSQL.ExecuteScalar<int>(conexao, "select count(*) from cartao");
        public static T ExecuteScalar<T>(string conexao, SqlCommand cmd)
        {
            return ConverterValor<T>(ExecuteScalar(conexao, cmd));
        }

        public static T ExecuteScalar<T>(string conexao, string comandoSql, params SqlParameter[] par)
        {
            return ConverterValor<T>(ExecuteScalar(conexao, comandoSql, par));
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

            DataTable tbret = ToTable(conexao, comandoSql, "databases");

            if (tbret != null && tbret.Rows.Count > 0)
            {
                foreach (DataRow row in tbret.Rows)
                {
                    ret.Add(row["name"].ToString()!);
                }
            }

            return ret;
        }
        #endregion

    }
}