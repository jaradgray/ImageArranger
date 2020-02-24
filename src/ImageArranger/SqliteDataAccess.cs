using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using Dapper;

namespace ImageArranger
{
    /// <summary>
    /// Provides access to the Statistics.db sqlite database.
    /// </summary>
    public class SqliteDataAccess
    {
        // API methods

        public static List<string> GetUniqueFilePaths()
        {
            // Safely open a connection to our database
            using (IDbConnection dbConnection = new SQLiteConnection(LoadConnectionString()))
            {
                // Query the database
                string query = "SELECT DISTINCT FileAbsolutePath FROM FileTimestamp";
                var output = dbConnection.Query<string>(query, new DynamicParameters());
                // Convert output to a List and return it
                return new List<string>(output);
            }
        }

        public static List<FileTimestampModel> GetAllTimestampsForFile(string fileAbsolutePath)
        {
            // Safely open a connection to our database
            using (IDbConnection dbConnection = new SQLiteConnection(LoadConnectionString()))
            {
                // Query the database
                // TODO is this syntax right ?
                string query = "SELECT * FROM FileTimestamp WHERE FileAbsolutePath = @FileAbsolutePath ORDER BY Ticks DESC";
                var parameters = new { FileAbsolutePath = fileAbsolutePath };
                var output = dbConnection.Query<FileTimestampModel>(query, parameters);
                // Convert output to a List and return it
                return new List<FileTimestampModel>(output);
            }
        }

        public static List<FileTimestampModel> GetAllTimestamps_Latest()
        {
            // Safely open a connection to our database
            using (IDbConnection dbConnection = new SQLiteConnection(LoadConnectionString()))
            {
                // Query the database
                string query = "SELECT * FROM FileTimestamp ORDER BY Ticks DESC";
                var output = dbConnection.Query<FileTimestampModel>(query, new DynamicParameters());
                // Convert output to a List and return it
                return new List<FileTimestampModel>(output);
            }
        }

        public static void SaveFileTimestamp(FileTimestampModel timestamp)
        {
            // Safely open a connection to our database
            using (IDbConnection dbConnection = new SQLiteConnection(LoadConnectionString()))
            {
                // TODO Create a record in FileTimestamp table to represent the given timestamp
            }
        }


        // Private methods

        /// <summary>
        /// Returns the ConnectionString attribute of the connectoinString
        /// in App.config whose name property matches the given id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static string LoadConnectionString(string id = "Default")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }
    }
}
