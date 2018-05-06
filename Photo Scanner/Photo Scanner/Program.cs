using System;
using System.Configuration;
using System.Data.SqlClient;

namespace Photo_Scanner
{
    internal static class Program
    {
        private static readonly string ConnString = ConfigurationManager.ConnectionStrings["SQLServerConnection"].ConnectionString;

        /// <summary>
        /// Assumptions:
        ///     1. You've already set up a database (SQL Server) and have the table created
        ///         - https://www.microsoft.com/en-us/download/details.aspx?id=55994
        /// </summary>
        private static void Main()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("PicHub Photo Scanner");
            Console.ForegroundColor = ConsoleColor.White;

            ReadFiles();

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("--- DONE ---");
            Console.ForegroundColor = ConsoleColor.White;
#if DEBUG
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
#endif

        }

        private static void ReadFiles()
        {
            try
            {
                using (var conn = new SqlConnection(ConnString))
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"SELECT * FROM Photos";
                        conn.Open();
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                var id = r.GetInt32(r.GetOrdinal("ID"));
                                var fullPath = r.GetString(r.GetOrdinal("FullPath"));
                                Console.WriteLine(id + " > " + fullPath);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}
