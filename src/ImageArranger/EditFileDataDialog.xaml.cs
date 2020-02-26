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
using Microsoft.Win32;

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

                // Indicate whether mFilePath is a real file path
                bool fileExists = System.IO.File.Exists(mFilePath);
                btnLocateFile.Visibility = (fileExists) ? Visibility.Collapsed : Visibility.Visible;
                tbPath.Foreground = (fileExists) ? Brushes.Black : Brushes.Red;
            }
        }


        // Constructor

        public EditFileDataDialog()
        {
            InitializeComponent();

            this.DataContext = this;
        }


        // Event handlers

        /// <summary>
        /// Shows an OpenFileDialog and updates database records and this EditFileDataDialog's FilePath property
        /// to be based on the browsed-to file's path.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLocateFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.InitialDirectory = System.IO.Path.GetDirectoryName(tbPath.Text);
            ofd.Filter = "Image Arranger Files|*.bmp;*.gif;*.jpeg;*.jpg;*.png;*.iaa"
                        + "|Image files (*.bmp, *.gif, *.jpeg, *.jpg, *.png)|*.bmp;*.gif;*.jpeg;*.jpg;*.png"
                        + "|Image Arranger Arrangements (*.iaa)|*.iaa";
            Nullable<bool> result = ofd.ShowDialog();
            if (result.HasValue && result.Value)
            {
                // Get the string paths for the selected file and its parent directory
                string newFilePath = ofd.FileName;
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
