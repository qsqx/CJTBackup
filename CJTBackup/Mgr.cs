using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace CJTBackup
{
    class Mgr
    {
        public delegate void Logger(string text);

        private static string defaultConnectCfg = "data source=.;user id=sa;pwd=sa";

        private Logger logger;

        public Mgr(Logger logger)
        {
            this.logger = logger;
        }
        private string GetTempDir()
        {
            string tmp = Path.GetTempPath();

            return (tmp.EndsWith(@"\") ? tmp : tmp + @"\") + @"CJTBackup\" + Guid.NewGuid().ToString();
        }

        private void createDir(string dir)
        {
            DirectorySecurity rules = new DirectorySecurity();
            rules.AddAccessRule(new FileSystemAccessRule("Authenticated Users", FileSystemRights.Modify, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
            rules.AddAccessRule(new FileSystemAccessRule("Users", FileSystemRights.ReadAndExecute, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
            Directory.CreateDirectory(dir, rules);
        }
        private void createTmpDir(string tmpdir)
        {
            logger("Create tmpdir " + tmpdir + " ... ");
            createDir(tmpdir);
            logger("Success\n");
        }

        private void deleteTmpDir(string tmpdir)
        {
            logger("Clear tmpdir ... ");
            Directory.Delete(tmpdir, true);
            logger("Success\n");
        }

        private string[] QueryDatabases(SqlConnection conn)
        {
            List<string> dbs = new List<string>();

            using (SqlCommand stmt = new SqlCommand("select name from sys.databases where database_id > 4", conn))
            {
                SqlDataReader rows = stmt.ExecuteReader();
                while (rows.Read())
                {
                    dbs.Add(rows[0].ToString());
                }
                rows.Close();
            }
            return dbs.ToArray();
        }
        private void doBackup(ZipArchive archive, string tmpdir)
        {
            using (SqlConnection conn = new SqlConnection(defaultConnectCfg))
            {
                conn.Open();
                string[] dbs = QueryDatabases(conn);


                foreach (string db in dbs)
                {
                    if (db == "UFData_998_2012" || db == "UFData_999_2011")
                        continue;

                    logger("Backup Database " + db + " ... ");
                    string path = tmpdir + @"\" + db + ".bak";

                    using (SqlCommand stmt = conn.CreateCommand())
                    {
                        stmt.CommandText = "BACKUP DATABASE [" + db + "] TO DISK = @path";
  
                        stmt.Parameters.AddWithValue("@path", path);
                        stmt.ExecuteNonQuery();

                        archive.CreateEntryFromFile(path, db + ".bak");
                    }
                    logger("Success\n");
                }
            }
        }

        public int Backup(string path)
        {
            string tmpdir = GetTempDir();

            try
            {
                using (ZipArchive archive = ZipFile.Open(path, ZipArchiveMode.Create))
                {
                    createTmpDir(tmpdir);
                    doBackup(archive, tmpdir);
                    logger("Pack ZIP File ...");
                }
                logger("success\n");
                deleteTmpDir(tmpdir);
                logger("备份成功\n");
            } catch (Exception e)
            {
                logger(e.ToString());
                logger("\n备份失败\n");
            }
            return 0;
        }

        private void doRestore(string tmpdir)
        {
            string[] files = Directory.GetFiles(tmpdir, "*.bak");

            using (SqlConnection conn = new SqlConnection(defaultConnectCfg))
            {
                conn.Open();
                string[] dbs = QueryDatabases(conn);

                foreach (string filepath in files)
                {
                    string filename = Path.GetFileName(filepath);
                    string db = filename.Split('.')[0];

                    if (dbs.Contains(db))
                    {
                        logger("Restore Database " + db + " ... ");
                        using (SqlCommand stmt = conn.CreateCommand())
                        {
                            stmt.CommandText = "RESTORE DATABASE [" + db + "] FROM DISK = @path WITH RECOVERY";
                            stmt.Parameters.AddWithValue("@path", filepath);
                            stmt.ExecuteNonQuery();
                        }
                        logger(" Success\n");
                    } else
                    {
                        logger("Create Database " + db + " From Backup ... ");
                        string[] splited = db.Split('_');
                        if (splited.Length != 3)
                            throw new Exception("Invalid Database Name");
                        string dbdir = @"C:\UFSMART\Admin\ZT" + splited[1] + @"\" + splited[2];
                        createDir(dbdir);
                        string dataname = "", logname = "";
                        using (SqlCommand stmt = conn.CreateCommand())
                        {
                            stmt.CommandText = "RESTORE FILELISTONLY FROM DISK = @path";
                            stmt.Parameters.AddWithValue("@path", filepath);
                            SqlDataReader rows = stmt.ExecuteReader();
                            while (rows.Read()){
                                string logicalName = rows[0].ToString();
                                string typ = rows[2].ToString();
                                if (typ == "D")
                                    dataname = logicalName;
                                else if (typ == "L")
                                    logname = logicalName;
                            }
                            rows.Close();
                        }
                        if (logname == "" || dataname == "")
                            throw new Exception("invalid backup file");
                        using (SqlCommand stmt = conn.CreateCommand())
                        {
                            stmt.CommandText = "RESTORE DATABASE " + db + " FROM DISK = @path WITH RECOVERY, MOVE @dataname TO @datafile, MOVE @logname TO @logfile";
                            stmt.Parameters.AddWithValue("@path", filepath);
                            stmt.Parameters.AddWithValue("@dataname", dataname);
                            stmt.Parameters.AddWithValue("@datafile", dbdir + @"\UFDATA.mdf");
                            stmt.Parameters.AddWithValue("@logname", logname);
                            stmt.Parameters.AddWithValue("@logfile", dbdir + @"\UFDATA.ldf");
                            stmt.ExecuteNonQuery();
                        }
                        logger(" Success\n");
                    }
                }
            }
        }


        public int Restore(string path)
        {
            string tmpdir = GetTempDir();

            try
            {
                using (ZipArchive archive = ZipFile.Open(path, ZipArchiveMode.Read))
                {
                    createTmpDir(tmpdir);
                    logger("Extact Files ... ");
                    archive.ExtractToDirectory(tmpdir);
                    logger("Success\n");
                }
                doRestore(tmpdir);
                deleteTmpDir(tmpdir);
                logger("恢复成功\n");
            }
            catch (Exception e)
            {
                logger(e.ToString());
                logger("\n恢复失败\n");
            }

            return 0;
        }
    }
}
