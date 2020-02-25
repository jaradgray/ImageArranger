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

        /// <summary>
        /// Returns a List containing each unique FileAbsolutePath value stored in the database
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Returns a List containing each unique ParentDirAbsolutePath value stored in the database
        /// </summary>
        /// <returns></returns>
        public static List<string> GetUniqueDirectoryPaths()
        {
            // Safely open a connection to our database
            using (IDbConnection dbConnection = new SQLiteConnection(LoadConnectionString()))
            {
                // Query the database
                string query = "SELECT DISTINCT ParentDirAbsolutePath FROM FileTimestamp";
                var output = dbConnection.Query<string>(query, new DynamicParameters());
                // Convert output to a List and return it
                return new List<string>(output);
            }
        }

        /// <summary>
        /// Returns a List of FileTimestampModel records in the database whose FileAbsolutePath property
        /// matches the given string and whose Ticks property is between @minTicks and @maxTicks (both inclusive),
        /// ordered by Ticks descending.
        /// </summary>
        /// <param name="fileAbsolutePath"></param>
        /// <param name="minTicks"></param>
        /// <param name="maxTicks"></param>
        /// <returns></returns>
        public static List<FileTimestampModel> GetTimestampsForFileInTimeFrame(string fileAbsolutePath, long minTicks, long maxTicks)
        {
            // Safely open a connection to our database
            using (IDbConnection dbConnection = new SQLiteConnection(LoadConnectionString()))
            {
                // Query the database
                string query = "SELECT * FROM FileTimestamp WHERE FileAbsolutePath = @FileAbsolutePath AND Ticks >= " + minTicks + " AND Ticks <= " + maxTicks + " ORDER BY Ticks DESC";
                var parameters = new { FileAbsolutePath = fileAbsolutePath};
                var output = dbConnection.Query<FileTimestampModel>(query, parameters);
                // Convert output to a List and return it
                return new List<FileTimestampModel>(output);
            }
        }

        /// <summary>
        /// Returns a List of FileTimestampModel records in the database whose ParentDirAbsolutePath property
        /// matches the given string and whose Ticks property is between @minTicks and @maxTicks (both inclusive),
        /// ordered by Ticks descending.
        /// </summary>
        /// <param name="parentDirAbsolutePath"></param>
        /// <param name="minTicks"></param>
        /// <param name="maxTicks"></param>
        /// <returns></returns>
        public static List<FileTimestampModel> GetTimestampsForDirectoryInTimeFrame(string parentDirAbsolutePath, long minTicks, long maxTicks)
        {
            // Safely open a connection to our database
            using (IDbConnection dbConnection = new SQLiteConnection(LoadConnectionString()))
            {
                // Query the database
                string query = "SELECT * FROM FileTimestamp WHERE ParentDirAbsolutePath = @ParentDirAbsolutePath AND Ticks >= " + minTicks + " AND Ticks <= " + maxTicks + " ORDER BY Ticks DESC";
                var parameters = new { ParentDirAbsolutePath = parentDirAbsolutePath };
                var output = dbConnection.Query<FileTimestampModel>(query, parameters);
                // Convert output to a List and return it
                return new List<FileTimestampModel>(output);
            }
        }

        public static void SaveFileTimestamp(FileTimestampModel timestamp)
        {
            // Safely open a connection to our database
            using (IDbConnection dbConnection = new SQLiteConnection(LoadConnectionString()))
            {
                // Create a record in FileTimestamp table to represent the given timestamp
                string sql = "INSERT INTO FileTimestamp (FileAbsolutePath, ParentDirAbsolutePath, Ticks) VALUES (@FileAbsolutePath, @ParentDirAbsolutePath, @Ticks)";
                dbConnection.Execute(sql, timestamp);
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
