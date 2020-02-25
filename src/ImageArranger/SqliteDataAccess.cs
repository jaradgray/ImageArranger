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
        /// Returns a List containing each unique FileAbsolutePath value for records in the database whose
        /// ParentDirAbsolutePath value matches @directory's AbsolutePath property.
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static List<string> GetUniqueFilePathsInDirectory(FolderStatisticsModel directory)
        {
            // Safely open a connection to our database
            using (IDbConnection dbConnection = new SQLiteConnection(LoadConnectionString()))
            {
                // Query the database
                string query = "SELECT DISTINCT FileAbsolutePath FROM FileTimestamp WHERE ParentDirAbsolutePath = '" + directory.AbsolutePath + "'";
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
        /// Returns a List of all FileTimestampModel records in FileTimestamp table whose FileAbsolutePath value
        /// is @fileAbsolutePath.
        /// </summary>
        /// <param name="fileAbsolutePath"></param>
        /// <returns></returns>
        public static List<FileTimestampModel> GetAllTimestampsForFile(string fileAbsolutePath)
        {
            return GetTimestampsForFileInTimeFrame(fileAbsolutePath, 0, long.MaxValue);
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

        /// <summary>
        /// Sets values for each FileTimestamp record represented in @timestamps to match the
        /// corresponding object's properties.
        /// </summary>
        /// <param name="timestamps"></param>
        public static void UpdateFileTimestamps(List<FileTimestampModel> timestamps)
        {
            foreach (FileTimestampModel timestamp in timestamps)
                UpdateFileTimestamp(timestamp);
        }

        /// <summary>
        /// Sets values for the record in FileTimestamp table whose Id matches @timestamp.Id
        /// to match @timestamp's properties.
        /// </summary>
        /// <param name="timestamp"></param>
        public static void UpdateFileTimestamp(FileTimestampModel timestamp)
        {
            // Safely open a connection to our database
            using (IDbConnection dbConnection = new SQLiteConnection(LoadConnectionString()))
            {
                // Update the record in FileTimestamp table whose Id matches @timestamp.Id with values of @timestamp's properties
                string sql 
                    = "UPDATE FileTimestamp "
                    + "SET FileAbsolutePath = @FileAbsolutePath, ParentDirAbsolutePath = @ParentDirAbsolutePath, Ticks = @Ticks "
                    + "WHERE Id = @Id";
                dbConnection.Execute(sql, timestamp);
            }
        }

        /// <summary>
        /// Deletes the FileTimestamp record whose Id value matches @timestamp's Id property.
        /// </summary>
        /// <param name="timestamp"></param>
        public static void DeleteFileTimestamp(FileTimestampModel timestamp)
        {
            // Safely open a connection to our database
            using (IDbConnection dbConnection = new SQLiteConnection(LoadConnectionString()))
            {
                // Delete the FileTimestamp record whose Id value matches @timestamp's Id property
                string sql = "DELETE FROM FileTimestamp WHERE Id = @Id";
                dbConnection.Execute(sql, timestamp);
            }
        }

        /// <summary>
        /// Deletes all records from the FileTimestep table whose FileAbsolutePath value matches @fileAbsolutePath.
        /// </summary>
        /// <param name="fileAbsolutePath"></param>
        public static void DeleteDataForFile(string fileAbsolutePath)
        {
            // Safely open a connection to our database
            using (IDbConnection dbConnection = new SQLiteConnection(LoadConnectionString()))
            {
                // Delete all records from FileTimestamp table whose FileAbsolutePath value matches the parameter
                string sql = "DELETE FROM FileTimestamp WHERE FileAbsolutePath = '" + fileAbsolutePath + "'";
                dbConnection.Execute(sql);
            }
        }

        /// <summary>
        /// Deletes all records from the FileTimestep table whose ParentDirAbsolutePath value is contained in @parentDirAbsolutePaths.
        /// </summary>
        /// <param name="parentDirAbsolutePaths"></param>
        public static void DeleteDataForFolders(List<FolderStatisticsModel> folders)
        {
            // Safely open a connection to our database
            using (IDbConnection dbConnection = new SQLiteConnection(LoadConnectionString()))
            {
                // Build the string of folder paths we can use in the SQL statement
                string sqlList = "(";
                int index = 0;
                while (index < folders.Count)
                {
                    sqlList += "'" + folders[index].AbsolutePath + "'";
                    sqlList += (++index < folders.Count) ? ", " : ")";
                }

                // Delete all records from FileTimestamp table whose ParentAbsolutePath value is in sqlList
                string sql = "DELETE FROM FileTimestamp WHERE ParentDirAbsolutePath IN " + sqlList;
                dbConnection.Execute(sql);
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
