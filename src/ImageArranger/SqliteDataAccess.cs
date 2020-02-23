using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using Dapper;

namespace ImageArranger
{
    public class SqliteDataAccess
    {
        // API methods

        public static ObservableCollection<FileTimestampModel> GetAllTimestamps()
        {
            // Safely open a connection to our database
            using (IDbConnection dbConnection = new SQLiteConnection(LoadConnectionString()))
            {
                // Query the database
                string query = "SELECT * FROM FileTimestamp";
                var output = dbConnection.Query<FileTimestampModel>(query, new DynamicParameters());
                // Convert output to an ObservableCollection and return it
                return new ObservableCollection<FileTimestampModel>(output);
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
