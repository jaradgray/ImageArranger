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
        private SortMode mSortMode = SortMode.Frequent;
        private TimeFrame mTimeFrame = TimeFrame.AllTime;


        // Constructor

        public StatisticsWindow()
        {
            InitializeComponent();

            // TODO delete the following section
            // Populate database with dummy data to test timeframe filtering
            // Testing filtering files by time frame
            //long today = new DateTime(2020, 2, 24).Ticks;
            //long yesterday = new DateTime(2020, 2, 23).Ticks;
            //long thrusday = new DateTime(2020, 2, 20).Ticks;
            //long feb1 = new DateTime(2020, 2, 1).Ticks;
            //long jan1 = new DateTime(2020, 1, 1).Ticks;
            //long jul7 = new DateTime(2019, 7, 7).Ticks;
            //long longAgo = new DateTime(1920, 7, 7).Ticks;
            //FileTimestampModel timestamp1 = new FileTimestampModel("A:\\Grandparent\\Parent\\today.jpg", today);
            //FileTimestampModel timestamp2 = new FileTimestampModel("A:\\Grandparent\\Parent\\yesterday.jpg", yesterday);
            //FileTimestampModel timestamp3 = new FileTimestampModel("A:\\Grandparent\\Parent\\thursday.jpg", thrusday);
            //FileTimestampModel timestamp4 = new FileTimestampModel("A:\\Grandparent\\Parent\\feb1.jpg", feb1);
            //FileTimestampModel timestamp5 = new FileTimestampModel("A:\\Grandparent\\Parent\\jan1.jpg", jan1);
            //FileTimestampModel timestamp6 = new FileTimestampModel("A:\\Grandparent\\Parent\\jul7.jpg", jul7);
            //FileTimestampModel timestamp7 = new FileTimestampModel("A:\\Grandparent\\Parent\\jul7.jpg", longAgo);
            //SqliteDataAccess.SaveFileTimestamp(timestamp1);
            //SqliteDataAccess.SaveFileTimestamp(timestamp2);
            //SqliteDataAccess.SaveFileTimestamp(timestamp3);
            //SqliteDataAccess.SaveFileTimestamp(timestamp4);
            //SqliteDataAccess.SaveFileTimestamp(timestamp5);
            //SqliteDataAccess.SaveFileTimestamp(timestamp6);
            //SqliteDataAccess.SaveFileTimestamp(timestamp7);

            LoadFileStatistics(mTimeFrame, mSortMode);
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
            LoadFileStatistics(mTimeFrame, mSortMode);
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

            LoadFileStatistics(mTimeFrame, mSortMode);
        }


        // Private methods

        private void LoadFileStatistics(TimeFrame timeFrame, SortMode sortMode)
        {
            if (filesListView == null) return;

            // Get min and max Ticks for the given TimeFrame
            long now = DateTime.Now.Ticks;
            long minTicks;
            switch (timeFrame)
            {
                case TimeFrame.Day:
                    minTicks = DateTime.Now.AddHours(-24).Ticks;
                    break;

                case TimeFrame.ThreeDays:
                    minTicks = DateTime.Now.AddDays(-3).Ticks;
                    break;

                case TimeFrame.Week:
                    minTicks = DateTime.Now.AddDays(-7).Ticks;
                    break;

                case TimeFrame.Month:
                    minTicks = DateTime.Now.AddMonths(-1).Ticks;
                    break;

                case TimeFrame.Year:
                    minTicks = DateTime.Now.AddYears(-1).Ticks;
                    break;

                // TODO delete the following 3 cases
                case TimeFrame.ThirtySeconds:
                    minTicks = DateTime.Now.AddSeconds(-30).Ticks;
                    break;
                case TimeFrame.Minute:
                    minTicks = DateTime.Now.AddMinutes(-1).Ticks;
                    break;
                case TimeFrame.ThreeMinutes:
                    minTicks = DateTime.Now.AddMinutes(-3).Ticks;
                    break;

                default:
                    minTicks = 0;
                    break;
            }

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

        
    }
}
