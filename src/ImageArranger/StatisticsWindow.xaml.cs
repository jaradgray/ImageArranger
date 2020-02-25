using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Diagnostics;

namespace ImageArranger
{
    /// <summary>
    /// Interaction logic for StatisticsWindow.xaml
    /// </summary>
    public partial class StatisticsWindow : Window
    {
        // TODO this does not belong in the code-behind
        /// <summary>
        /// The different modes for ordering displayed file and folder statistics items
        /// </summary>
        public enum SortMode
        {
            /// <summary>
            /// Order by views, descending
            /// </summary>
            Frequent,

            /// <summary>
            /// Order by timestamp last viewed, descending
            /// </summary>
            Recent
        }

        public enum TimeFrame
        {
            Day,
            ThreeDays,
            Week,
            Month,
            Year,
            AllTime, 
            // TODO delete the following, just for debugging
            ThirtySeconds,
            Minute,
            ThreeMinutes
        }

        // Instance variables

        public ObservableCollection<FileStatisticsModel> fileStatisticsCollection = new ObservableCollection<FileStatisticsModel>();
        public ObservableCollection<FolderStatisticsModel> folderStatisticsCollection = new ObservableCollection<FolderStatisticsModel>();
        private SortMode mSortMode = SortMode.Frequent;
        private TimeFrame mTimeFrame = TimeFrame.AllTime;


        // Constructor

        public StatisticsWindow()
        {
            InitializeComponent();

            // Listen for database changes from MainWindow
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow; // 'as' operator attempts to cast an object to a specific type, and returns null if it fails
            if (mainWindow != null)
            {
                mainWindow.DatabaseChanged += MainWindow_DatabaseChanged;
            }

            // TODO delete the following section
            // Populate database with dummy data to test timeframe filtering
            // Testing filtering files by time frame

            long oneSecAgo = DateTime.Now.AddSeconds(-1).Ticks;
            long today = new DateTime(2020, 2, 24).Ticks;
            long yesterday = new DateTime(2020, 2, 23).Ticks;
            long thursday = new DateTime(2020, 2, 20).Ticks;
            long feb1 = new DateTime(2020, 2, 1).Ticks;
            long jan1 = new DateTime(2020, 1, 1).Ticks;
            long jul7 = new DateTime(2019, 7, 7).Ticks;
            long longAgo = new DateTime(1920, 7, 7).Ticks;
            FileTimestampModel timestamp0 = new FileTimestampModel("A:\\Grandparent\\Parent\\jul7.jpg", oneSecAgo);
            FileTimestampModel timestamp1 = new FileTimestampModel("A:\\Grandparent\\Parent\\today.jpg", today);
            FileTimestampModel timestamp2 = new FileTimestampModel("A:\\Grandparent\\Parent\\yesterday.jpg", yesterday);
            FileTimestampModel timestamp3 = new FileTimestampModel("A:\\Grandparent\\Parent\\thursday.jpg", thursday);
            FileTimestampModel timestamp4 = new FileTimestampModel("A:\\Grandparent\\Parent\\feb1.jpg", feb1);
            FileTimestampModel timestamp5 = new FileTimestampModel("A:\\Grandparent\\Parent\\jan1.jpg", jan1);
            FileTimestampModel timestamp6 = new FileTimestampModel("A:\\Grandparent\\Parent\\jul7.jpg", jul7);
            FileTimestampModel timestamp7 = new FileTimestampModel("A:\\Grandparent\\Parent\\jul7.jpg", longAgo);

            FileTimestampModel timestamp8 = new FileTimestampModel("B:\\Grandparent\\Other Folder\\hello.jpg", oneSecAgo - 1);
            FileTimestampModel timestamp9 = new FileTimestampModel("B:\\Grandparent\\Other Folder\\hello2.jpg", today - 1);
            FileTimestampModel timestamp10 = new FileTimestampModel("B:\\Grandparent\\Other Folder\\hello3.jpg", yesterday - 1);
            FileTimestampModel timestamp11 = new FileTimestampModel("B:\\Grandparent\\Other Folder\\thursday.jpg", thursday - 1);
            SqliteDataAccess.SaveFileTimestamp(timestamp0);
            SqliteDataAccess.SaveFileTimestamp(timestamp1);
            SqliteDataAccess.SaveFileTimestamp(timestamp2);
            SqliteDataAccess.SaveFileTimestamp(timestamp3);
            SqliteDataAccess.SaveFileTimestamp(timestamp4);
            SqliteDataAccess.SaveFileTimestamp(timestamp5);
            SqliteDataAccess.SaveFileTimestamp(timestamp6);
            SqliteDataAccess.SaveFileTimestamp(timestamp7);
            SqliteDataAccess.SaveFileTimestamp(timestamp8);
            SqliteDataAccess.SaveFileTimestamp(timestamp9);
            SqliteDataAccess.SaveFileTimestamp(timestamp10);
            SqliteDataAccess.SaveFileTimestamp(timestamp11);

            LoadStatistics(mTimeFrame, mSortMode);
        }


        // Event handlers

        private void CmbSortMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Set mSortMode based on currently selected index
            switch (cmbSortMode.SelectedIndex)
            {
                // most views
                case 0:
                    mSortMode = SortMode.Frequent;
                    break;

                // most recently viewed
                case 1:
                    mSortMode = SortMode.Recent;
                    break;
            }

            // Update displayed items
            LoadStatistics(mTimeFrame, mSortMode);
        }

        private void FilesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                // Cast the selected item to a FileStatisticsModel object
                FileStatisticsModel selected = (FileStatisticsModel) e.AddedItems[0];
                // Get the selected item's AbsolutePath attribute
                string path = selected.AbsolutePath;

                // TODO handle preview for .iaa files

                // If the selected item's path leads to an image...
                string[] validImageExtensions = { ".BMP", ".GIF", ".JPEG", ".JPG", ".PNG" };
                if (validImageExtensions.Contains(System.IO.Path.GetExtension(path).ToUpperInvariant()))
                {
                    // Set preview Image's source to the selected item's AbsolutePath
                    try
                    {
                        previewImage.Source = new BitmapImage(new Uri(path));
                    }
                    catch
                    {
                        previewImage.Source = null;
                    }
                }
            }
            else
            {
                previewImage.Source = null;
            }
        }

        private void FoldersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO filter displayed files based on selected folder item/s
            int numSelected = foldersListView.SelectedItems.Count;
            if (numSelected > 0)
            {
                List<FolderStatisticsModel> folders = foldersListView.SelectedItems.Cast<FolderStatisticsModel>().ToList();
                LoadFileStatistics(mTimeFrame, mSortMode, folders);
            }
        }

        private void CmbTimeFrame_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO filter displayed folder and file statistics items based on selected time frame
            switch (cmbTimeFrame.SelectedIndex)
            {
                // day
                case 0:
                    mTimeFrame = TimeFrame.Day;
                    break;

                // 3 days
                case 1:
                    mTimeFrame = TimeFrame.ThreeDays;
                    break;

                // week
                case 2:
                    mTimeFrame = TimeFrame.Week;
                    break;

                // month
                case 3:
                    mTimeFrame = TimeFrame.Month;
                    break;

                // year
                case 4:
                    mTimeFrame = TimeFrame.Year;
                    break;

                // all time
                case 5:
                    mTimeFrame = TimeFrame.AllTime;
                    break;

                // TODO delete the following
                case 6:
                    mTimeFrame = TimeFrame.ThirtySeconds;
                    break;
                case 7:
                    mTimeFrame = TimeFrame.Minute;
                    break;
                case 8:
                    mTimeFrame = TimeFrame.ThreeMinutes;
                    break;
            }

            LoadStatistics(mTimeFrame, mSortMode);
        }

        private void filesListViewCM_EditData_Click(object sender, RoutedEventArgs e)
        {
            FileStatisticsModel selectedItem = (FileStatisticsModel)filesListView.SelectedItem;
            if (selectedItem == null) return;

            // Show an EditFileDataDialog dialog to edit the selected item's data
            EditFileDataDialog dialog = new EditFileDataDialog();
            dialog.FilePath = selectedItem.AbsolutePath;
            dialog.DatabaseChanged += EditFileDataDialog_DatabaseChanged;
            dialog.Owner = this;
            Nullable<bool> result = dialog.ShowDialog();
        }

        /// <summary>
        /// Reloads data for UI from database.
        /// Called when an EditFileDataDialog instance updates the database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditFileDataDialog_DatabaseChanged(object sender, EventArgs e)
        {
            LoadStatistics(mTimeFrame, mSortMode);
        }

        /// <summary>
        /// Reloads data for UI from database.
        /// Called when MainWindow updates the database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_DatabaseChanged(object sender, EventArgs e)
        {
            LoadStatistics(mTimeFrame, mSortMode);
        }

        /// <summary>
        /// On user's confirmation, deletes records for the selected file from the database, and refreshes
        /// the lists of displayed items.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void filesListViewCM_DeleteData_Click(object sender, RoutedEventArgs e)
        {
            FileStatisticsModel selectedItem = (FileStatisticsModel)filesListView.SelectedItem;
            if (selectedItem == null) return;

            // Prompt user to confirm deleting selected file's data
            DialogBox dialog = new DialogBox();
            dialog.Prompt = "Delete data for " + selectedItem.Name + "?";
            dialog.Owner = this;
            Nullable<bool> result = dialog.ShowDialog();
            if (result != null && result == true)
            {
                // Delete selected file's data and refresh lists
                SqliteDataAccess.DeleteDataForFile(selectedItem.AbsolutePath);
                LoadStatistics(mTimeFrame, mSortMode);
            }
        }

        /// <summary>
        /// Starts a File Explorer process and navigates to filesListView's selected item's file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void filesListViewCM_RevealInExplorer_Click(object sender, RoutedEventArgs e)
        {
            FileStatisticsModel selectedItem = (FileStatisticsModel)filesListView.SelectedItem;
            if (selectedItem == null) return;
            if (!File.Exists(selectedItem.AbsolutePath)) return;

            // Launch File Explorer and select the file corresponding to selectedItem
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                Arguments = "/select, " + selectedItem.AbsolutePath /* command-line arguments */,
                FileName = "explorer.exe"
            };
            Process.Start(startInfo);
        }

        /// <summary>
        /// On user's confirmation, deletes records for the selected folder(s) from the database, and refreshes
        /// the lists of displayed items.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void foldersListViewCM_DeleteData_Click(object sender, RoutedEventArgs e)
        {
            List<FolderStatisticsModel> selectedFolders = foldersListView.SelectedItems.Cast<FolderStatisticsModel>().ToList();
            if (selectedFolders == null || selectedFolders.Count <= 0) return;

            // Prompt user to confirm deleting selected folder(s)'s data
            DialogBox dialog = new DialogBox();
            dialog.Prompt = (selectedFolders.Count == 1)
                ? "Delete data for " + selectedFolders[0].Name + "?"
                : "Delete data for " + selectedFolders.Count + " folders?";
            dialog.Owner = this;
            Nullable<bool> result = dialog.ShowDialog();
            if (result != null && result == true)
            {
                // Delete data of selected folder(s) and refresh lists
                SqliteDataAccess.DeleteDataForFolders(selectedFolders);
                LoadStatistics(mTimeFrame, mSortMode);
            }
        }

        /// <summary>
        /// Starts a File Explorer process and navigates to foldersListView's selected item's folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void foldersListViewCM_RevealInExplorer_Click(object sender, RoutedEventArgs e)
        {
            // Can't browse to less than or more than one folder
            if (foldersListView.SelectedItems == null || foldersListView.SelectedItems.Count > 1) return;

            FolderStatisticsModel selectedItem = (FolderStatisticsModel)foldersListView.SelectedItem;
            if (selectedItem == null) return;
            if (!Directory.Exists(selectedItem.AbsolutePath)) return;

            // Launch File Explorer and select the file corresponding to selectedItem
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                Arguments = selectedItem.AbsolutePath /* command-line arguments */,
                FileName = "explorer.exe"
            };
            Process.Start(startInfo);
        }


        // Private methods

        private void LoadStatistics(TimeFrame timeFrame, SortMode sortMode)
        {
            LoadFileStatistics(timeFrame, sortMode);
            LoadFolderStatistics(timeFrame, sortMode);
        }

        /// <summary>
        /// Populate fileStatisticsCollection with elements representing each unique file in the database
        /// within @timeFrame, order the collection according to @sortMode, and display the list in filesListView.
        /// </summary>
        /// <param name="timeFrame"></param>
        /// <param name="sortMode"></param>
        private void LoadFileStatistics(TimeFrame timeFrame, SortMode sortMode)
        {
            if (filesListView == null) return;

            // Get min and max Ticks for the given TimeFrame
            long now = DateTime.Now.Ticks;
            long minTicks = GetMinTicksForTimeFrame(timeFrame);

            // Build a collection of FileStatisticsModels, with one element for each unique file represented in the database
            fileStatisticsCollection = new ObservableCollection<FileStatisticsModel>();
            List<string> uniqueFilePaths = SqliteDataAccess.GetUniqueFilePaths();
            foreach (string filePath in uniqueFilePaths)
            {
                List<FileTimestampModel> allTimestamps = SqliteDataAccess.GetTimestampsForFileInTimeFrame(filePath, minTicks, now);
                if (allTimestamps == null || allTimestamps.Count <= 0) continue;
                fileStatisticsCollection.Add(new FileStatisticsModel(filePath, allTimestamps));
            }

            // Re-order the collection based on sortMode parameter
            switch (sortMode)
            {
                case SortMode.Frequent:
                    // Order by NumViews property
                    fileStatisticsCollection = new ObservableCollection<FileStatisticsModel>(fileStatisticsCollection.OrderByDescending(fsm => fsm.NumViews));
                    break;

                case SortMode.Recent:
                    // Order by last opened Ticks property
                    fileStatisticsCollection = new ObservableCollection<FileStatisticsModel>(fileStatisticsCollection.OrderByDescending(fsm => fsm.TimestampLastOpened.Ticks));
                    break;
            }

            // Set filesListView's ItemsSource property
            filesListView.ItemsSource = fileStatisticsCollection;
        }

        /// <summary>
        /// Populates fileStatisticsCollection with elements representing each unique file any folder in @folders
        /// within @timeFrame, order the collection according to @sortMode, and display the list in filesListView.
        /// </summary>
        /// <param name="timeFrame"></param>
        /// <param name="sortMode"></param>
        /// <param name="folders"></param>
        private void LoadFileStatistics(TimeFrame timeFrame, SortMode sortMode, List<FolderStatisticsModel> folders)
        {
            if (filesListView == null) return;

            // Get min Ticks for the given TimeFrame
            long minTicks = GetMinTicksForTimeFrame(timeFrame);
            
            // Build the list of unique file paths in the given folders
            List<string> uniqueFilePaths = new List<string>();
            foreach (FolderStatisticsModel folder in folders)
                uniqueFilePaths.AddRange(SqliteDataAccess.GetUniqueFilePathsInDirectory(folder));

            // Populate collection with elements representing each unique file path
            fileStatisticsCollection = new ObservableCollection<FileStatisticsModel>();
            foreach (string filePath in uniqueFilePaths)
            {
                List<FileTimestampModel> allTimestamps = SqliteDataAccess.GetTimestampsForFileInTimeFrame(filePath, minTicks, DateTime.Now.Ticks);
                if (allTimestamps == null || allTimestamps.Count <= 0) continue;
                fileStatisticsCollection.Add(new FileStatisticsModel(filePath, allTimestamps));
            }

            // Re-order the collection based on sortMode parameter
            switch (sortMode)
            {
                case SortMode.Frequent:
                    // Order by NumViews property
                    fileStatisticsCollection = new ObservableCollection<FileStatisticsModel>(fileStatisticsCollection.OrderByDescending(fsm => fsm.NumViews));
                    break;

                case SortMode.Recent:
                    // Order by last opened Ticks property
                    fileStatisticsCollection = new ObservableCollection<FileStatisticsModel>(fileStatisticsCollection.OrderByDescending(fsm => fsm.TimestampLastOpened.Ticks));
                    break;
            }

            // Set filesListView's ItemsSource property
            filesListView.ItemsSource = fileStatisticsCollection;
        }

        /// <summary>
        /// Populate folderStatisticsCollection with elements representing each unique parent directory in the database
        /// within @timeFrame, order the collection according to @sortMode, and display the list in foldersListView.
        /// </summary>
        /// <param name="timeFrame"></param>
        /// <param name="sortMode"></param>
        private void LoadFolderStatistics(TimeFrame timeFrame, SortMode sortMode)
        {
            if (foldersListView == null) return;

            // Get min Ticks for the given TimeFrame
            long minTicks = GetMinTicksForTimeFrame(timeFrame);

            // Build a collection of FolderStatisticsModels, with one element for each unique parent directory path in the database
            folderStatisticsCollection = new ObservableCollection<FolderStatisticsModel>();
            List<string> uniqueDirPaths = SqliteDataAccess.GetUniqueDirectoryPaths();
            // For each unique directory path...
            foreach (string dirPath in uniqueDirPaths)
            {
                // Add a new FolderStatisticsModel object to the collection
                List<FileTimestampModel> allTimestamps = SqliteDataAccess.GetTimestampsForDirectoryInTimeFrame(dirPath, minTicks, DateTime.Now.Ticks);
                if (allTimestamps == null || allTimestamps.Count <= 0) continue;
                folderStatisticsCollection.Add(new FolderStatisticsModel(dirPath, allTimestamps));
            }

            // Re-order the collection based on sortMode parameter
            switch (sortMode)
            {
                case SortMode.Frequent:
                    // Order by NumViews property
                    folderStatisticsCollection = new ObservableCollection<FolderStatisticsModel>(folderStatisticsCollection.OrderByDescending(fsm => fsm.NumViews));
                    break;

                case SortMode.Recent:
                    // Order by last opened Ticks property
                    folderStatisticsCollection = new ObservableCollection<FolderStatisticsModel>(folderStatisticsCollection.OrderByDescending(fsm => fsm.MostRecentTimestamp.Ticks));
                    break;
            }

            // Set foldersListView's ItemsSource property
            foldersListView.ItemsSource = folderStatisticsCollection;
        }

        /// <summary>
        /// Returns the minimum value of DateTime.Ticks corresponding to @timeFrame.
        /// </summary>
        /// <param name="timeFrame"></param>
        /// <returns></returns>
        private long GetMinTicksForTimeFrame(TimeFrame timeFrame)
        {
            long result = timeFrame.Equals(TimeFrame.Year) ? DateTime.Now.AddYears(-1).Ticks
                : timeFrame.Equals(TimeFrame.Month) ? DateTime.Now.AddMonths(-1).Ticks
                : timeFrame.Equals(TimeFrame.Week) ? DateTime.Now.AddDays(-7).Ticks
                : timeFrame.Equals(TimeFrame.ThreeDays) ? DateTime.Now.AddDays(-3).Ticks
                : timeFrame.Equals(TimeFrame.Day) ? DateTime.Now.AddHours(-24).Ticks
                : 0;
            return result;
        }

        
    }
}
