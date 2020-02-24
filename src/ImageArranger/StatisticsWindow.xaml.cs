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

        // Instance variables

        public ObservableCollection<FileStatisticsModel> fileStatisticsCollection = new ObservableCollection<FileStatisticsModel>();


        // Constructor

        public StatisticsWindow()
        {
            InitializeComponent();

            LoadFileStatistics(SortMode.Frequent);
        }


        // Event handlers

        private void FrequentButton_Click(object sender, RoutedEventArgs e)
        {
            LoadFileStatistics(SortMode.Frequent);
        }

        private void RecentButton_Click(object sender, RoutedEventArgs e)
        {
            LoadFileStatistics(SortMode.Recent);
        }


        // Private methods

        private void LoadFileStatistics(SortMode sortMode)
        {
            // Build a collection of FileStatisticsModels, with one element for each unique file represented in the database
            fileStatisticsCollection = new ObservableCollection<FileStatisticsModel>();
            List<string> uniqueFilePaths = SqliteDataAccess.GetUniqueFilePaths();
            foreach (string filePath in uniqueFilePaths)
            {
                List<FileTimestampModel> allTimestamps = SqliteDataAccess.GetAllTimestampsForFile(filePath);
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
