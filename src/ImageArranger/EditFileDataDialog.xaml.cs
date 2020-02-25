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
    /// Interaction logic for EditFileDataDialog.xaml
    /// </summary>
    public partial class EditFileDataDialog : Window
    {
        // Instance variables

        public ObservableCollection<FileTimestampModel> timestamps = new ObservableCollection<FileTimestampModel>();

        /// <summary>
        /// The event that notifies subscribers that the database has changed.
        /// </summary>
        public event EventHandler DatabaseChanged;
        public void OnDatabaseChanged()
        {
            if (DatabaseChanged != null) DatabaseChanged(this, EventArgs.Empty);
        }


        // Properties

        private string mFilePath;
        /// <summary>
        /// The absolute path to the file whose data we're viewing.
        /// Setting this property causes all UI to update appropriately.
        /// </summary>
        public string FilePath
        {
            get { return mFilePath; }
            set
            {
                // Set mFilePath
                mFilePath = value;

                // Update tbFilename's text
                tbFilename.Text = System.IO.Path.GetFileName(value);

                // Populate timestamps
                timestamps = new ObservableCollection<FileTimestampModel>(SqliteDataAccess.GetAllTimestampsForFile(mFilePath));
                lbTimestamps.ItemsSource = timestamps;
            }
        }


        // Constructor

        public EditFileDataDialog()
        {
            InitializeComponent();

            this.DataContext = this;
        }


        // Event handlers

        private void tboxPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = tboxPath.Text;
            btnUpdatePath.Visibility = (text.Equals(mFilePath)) ? Visibility.Collapsed : Visibility.Visible;

            // Indicate whether tboxPath's text is a real file path
            tbFileNotFound.Visibility = (System.IO.File.Exists(text)) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void btnUpdatePath_Click(object sender, RoutedEventArgs e)
        {
            // Hide button
            btnUpdatePath.Visibility = Visibility.Hidden;

            string newFilePath = tboxPath.Text;
            string newParentDirPath = System.IO.Path.GetDirectoryName(newFilePath);

            // Set properties of our cached list of timestamps to reflect the values we want to update in the database
            foreach (FileTimestampModel timestamp in timestamps)
            {
                timestamp.FileAbsolutePath = newFilePath;
                timestamp.ParentDirAbsolutePath = newParentDirPath;
            }

            // Update database
            SqliteDataAccess.UpdateFileTimestamps(timestamps.ToList());

            // Update FilePath property
            this.FilePath = newFilePath;

            // Notify listeners that the database has changed
            OnDatabaseChanged();
        }

        private void lbTimestamps_CmDeleteClick(object sender, RoutedEventArgs e)
        {
            if (lbTimestamps.SelectedItems == null || lbTimestamps.SelectedItems.Count != 1) return;

            FileTimestampModel selectedTimestamp = (FileTimestampModel) lbTimestamps.SelectedItems[0];

            // Delete the selected timestamp from the database
            SqliteDataAccess.DeleteFileTimestamp(selectedTimestamp);

            // Refresh list of timestamps
            timestamps = new ObservableCollection<FileTimestampModel>(SqliteDataAccess.GetAllTimestampsForFile(mFilePath));
            lbTimestamps.ItemsSource = timestamps;

            // Notify listeners that the database has changed
            OnDatabaseChanged();

            // Close this window if we deleted the last timestamp
            if (timestamps.Count == 0)
                this.DialogResult = false;
        }

        public void btnClose_Click(object sender, RoutedEventArgs e)
        {
            // TODO can change to true if needed
            this.DialogResult = false;
        }

        
    }
}
