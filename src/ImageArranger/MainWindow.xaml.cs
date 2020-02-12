using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;
using System.Diagnostics;

namespace ImageArranger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Instance Variables
        private List<string> filenames;
        private List<Image> images;
        private List<Rect> rects;
        private DynamicGrid grid;
        private DispatcherTimer resizeTimer; // resize timer idea from https://stackoverflow.com/questions/4474670/how-to-catch-the-ending-resize-window


        // Constructor
        public MainWindow()
        {
            InitializeComponent();

            filenames = new List<string>();
            images = new List<Image>();
            rects = new List<Rect>();
            resizeTimer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 0, 0, 100),
                IsEnabled = false
            };
            resizeTimer.Tick += resizeTimer_Tick;
        }


        // Event Handlers

        private void NewCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Launch a new ImageArranger process
            string processName = Process.GetCurrentProcess().ProcessName;
            Process.Start(processName);
        }

        private void QuitCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void QuitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void FullScreenCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void FullScreenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO go full-screen
        }



        private void MainCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectImages())
            {
                GenerateNormalizedLists();
                ArrangeRects();
                MainCanvas.Children.Clear();
                //DrawRects();
                DrawImages();
            }
        }//end MainCanvas_PreviewMouseLeftButtonDown()

        private void MainCanvas_PreviewDrop(object sender, DragEventArgs e)
        {
            string[] validExtensions = { ".BMP", ".GIF", ".JPEG", ".JPG", ".PNG" };
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                bool fileAdded = false;
                string[] droppedFilenames = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string s in droppedFilenames)
                {
                    if (validExtensions.Contains(System.IO.Path.GetExtension(s).ToUpperInvariant()))
                    {
                        // add s to filenames if it's not already there
                        if (!filenames.Contains(s))
                        {
                            filenames.Add(s);
                            fileAdded = true;
                        }
                    }
                }//end foreach
                if (fileAdded)
                {
                    GenerateNormalizedLists();
                    ArrangeRects();
                    MainCanvas.Children.Clear();
                    //DrawRects();
                    DrawImages();
                }
            }//end if
        }//end MainCanvas_PreviewDrop()

        private void MainCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // start the resizeTimer
            resizeTimer.IsEnabled = true;
            resizeTimer.Stop();
            resizeTimer.Start();
        }

        private void resizeTimer_Tick(object sender, EventArgs e)
        {
            resizeTimer.IsEnabled = false;
            if (filenames.Count <= 0) return;
            // rearrange rects/images based on new canvas size (bounds)
            GenerateNormalizedLists();
            ArrangeRects();
            // redraw images/rects
            MainCanvas.Children.Clear();
            //DrawRects();
            DrawImages();
        }

        private void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            if (mi == null) return;
            ContextMenu cm = (ContextMenu)mi.Parent;
            if (cm == null) return;
            Image target = (Image)cm.PlacementTarget;
            if (target == null) return;

            // remove the Image's filename from filenames and redo arrangement
            filenames.Remove(((BitmapImage)target.Source).UriSource.OriginalString);
            GenerateNormalizedLists();
            MainCanvas.Children.Clear();
            if (filenames.Count == 0) return;
            ArrangeRects();
            //DrawRects();
            DrawImages();
        }

        private void RemoveAllImages_Click(object sender, RoutedEventArgs e)
        {
            filenames.Clear();
            images.Clear();
            rects.Clear();
            MainCanvas.Children.Clear();
        }

        private void AddImage_Click(object sender, RoutedEventArgs e)
        {
            if (SelectImages())
            {
                GenerateNormalizedLists();
                ArrangeRects();
                MainCanvas.Children.Clear();
                //DrawRects();
                DrawImages();
            }
        }


        // Methods

        /// <summary>
        /// Shows an OpenFileDialog for user to select image files and adds to the filenames List any
        /// filename it doesn't already contain.
        /// </summary>
        /// <returns>True if one or more files were added to filenames List, false otherwise.</returns>
        private bool SelectImages()
        {
            bool fileAdded = false;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "Image files (*.bmp, *.gif, *.jpeg, *.jpg, *.png)|*.bmp;*.gif;*.jpeg;*.jpg;*.png";
            Nullable<bool> result = ofd.ShowDialog();
            if (result.HasValue && result.Value)
            {
                string[] files = ofd.FileNames;
                foreach (string name in files)
                {
                    if (!filenames.Contains(name))
                    {
                        filenames.Add(name);
                        fileAdded = true;
                    }
                }
            }
            return fileAdded;
        }

        /// <summary>
        /// Given a Rect and values for max width and height, returns a new Rect with
        /// the same proportions as orig and is as large as possible without its width 
        /// or height exceeding the given max values.
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="maxWidth"></param>
        /// <param name="maxHeight"></param>
        /// <returns></returns>
        private Rect GetOptimalRect(Rect orig, double maxWidth, double maxHeight)
        {
            double aspect = maxWidth / maxHeight;

            Rect result = new Rect(0d, 0d, 0d, 0d);
            // SET result'S SIZE TO OPTIMALLY FIT WINDOW
            // if orig's aspect is smaller than window's, height is the limiting dimension;
            // if it's larger, width is the limiting dimension
            double sizeFactor = (orig.Width / orig.Height <= aspect) ?
                maxHeight / orig.Height : maxWidth / orig.Width;
            result.Width = orig.Width * sizeFactor;
            result.Height = orig.Height * sizeFactor;
            return result;
        }

        /// <summary>
        /// - Recreates rects and images Lists based on files in filenames List;
        /// - Normalizes each item in rects and images (resizes each to its largest size
        ///     which fits within the canvas and maintains its original aspect ratio);
        /// - Sorts images and rects by nonincreasing area
        /// 
        /// Post: images and rects are normalized and sorted by nonincreasing area
        /// </summary>
        private void GenerateNormalizedLists()
        {
            double w = MainCanvas.ActualWidth;
            double h = MainCanvas.ActualHeight;

            //Console.WriteLine($"Canvas size: {MainCanvas.ActualWidth}, {MainCanvas.ActualHeight}");

            rects.Clear();
            images.Clear();
            foreach (string name in filenames)
            {
                // create Image with filename as source
                Image img = new Image();
                img.ContextMenu = (ContextMenu)this.Resources["ImageContextMenu"];
                img.Source = new BitmapImage(new Uri(name));
                // create Rect and add to rects List
                Rect rect = GetOptimalRect(new Rect(0d, 0d, img.Source.Width, img.Source.Height), w, h);
                rects.Add(rect);
                img.Width = rect.Width;
                img.Height = rect.Height;
                images.Add(img);
            }

            // sort images by nonincreasing area. Copied from:
            // https://stackoverflow.com/questions/3309188/how-to-sort-a-listt-by-a-property-in-the-object
            rects.Sort((x, y) => (y.Width * y.Height).CompareTo(x.Width * x.Height));
            images.Sort((x, y) => (y.Width * y.Height).CompareTo(x.Width * x.Height));
        }

        /// <summary>
        /// Finds the optimal arrangement of items in rects List within a bounding rectangle whose
        /// aspect ratio is the same as that of the application window.
        /// 
        /// Starts with a bounding rectangle whose height is the sum of the heights of the items in rects,
        /// places them with a topmost-row-of-leftmost-column algorithm, and repeatedly shrinks the size
        /// of the bounding rectangle and re-places the Rects until an attempt to place the Rects fails.
        /// 
        /// The placement algorithm is a modified version of the one outlined here:
        /// https://www.codeproject.com/Articles/210979/Fast-optimizing-rectangle-packing-algorithm-for-bu
        /// 
        /// Post: grid's state represents the optimal arrangements of the items in rects and
        /// images within the bounds of the application window.
        /// </summary>
        private void ArrangeRects()
        {
            double heightSum = 0.0;
            foreach (Rect r in rects)
            {
                heightSum += r.Height;
            }

            double aspect = MainCanvas.ActualWidth / MainCanvas.ActualHeight;

            // bounds can't be smaller than canvas
            Rect bounds = (heightSum < MainCanvas.ActualHeight) ?
                new Rect(0.0, 0.0, MainCanvas.ActualWidth, MainCanvas.ActualHeight) :
                new Rect(0.0, 0.0, heightSum * aspect, heightSum);

            // PLACE RECTS VIA THE TOPMOST ROW OF LEFTMOST COL ALGORITHM
            // guaranteed to work the first time
            grid = new DynamicGrid(bounds);
            DynamicGrid optimalArrangement = null;
            while (PlaceRects())
            {
                optimalArrangement = grid.GetCopy();
                if (!grid.ShrinkBounds()) break;
                grid = new DynamicGrid(grid.Bounds);
            }

            if (optimalArrangement == null)
            {
                Console.WriteLine("ArrangeRects(): Error - optimalArrangement is null");
                return;
            }

            grid = optimalArrangement.GetCopy();
        }//end ArrangeRects()

        /// <summary>
        /// Places as many items from rects List into grid as will fit
        /// </summary>
        /// <returns></returns>
        private bool PlaceRects()
        {
            // the purpose of the following code is to find a col and row to call
            // grid.InsertRect(rect, col, row)

            foreach (Rect rect in rects)
            {
                bool rectPlaced = false;
                // consider each row in the col before moving to next col
                for (int col = 0; col < grid.NumCols; col++)
                {
                    for (int row = 0; row < grid.NumRows; row++)
                    {
                        if (grid.IsCellFree(col, row))
                        {
                            if (PlaceRect(rect, col, row))
                            {
                                rectPlaced = true;
                            }
                        }
                        if (rectPlaced) break;
                    }//end for row
                    if (rectPlaced) break;
                }//end for col
                if (!rectPlaced)
                {
                    Console.WriteLine("PlaceRects(): couldn't place all Rects");
                    return false;
                }
            }//end foreach rect

            return true; // all Rects successfully placed within bounds
        }//end PlaceRects()

        /// <summary>
        /// Given a Rect and column and row indexes, checks if grid can accommodate rect at the given
        /// column and row, and inserts the rect into grid if so.
        /// 
        /// Post: grid's state represents that of rect having been inserted at [col, row] if successful,
        /// grid's state is unchanged if unsuccessful.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="col"></param>
        /// <param name="row"></param>
        /// <returns>True if rect was successfully placed in grid at [col, row], false otherwise</returns>
        private bool PlaceRect(Rect rect, int col, int row)
        {
            if (grid == null)
            {
                Console.WriteLine("PlaceRect(): Error - grid is null");
                return false;
            }

            // CHECK IF RECT CAN BE PLACED IN GRID AT CELL [col, row]
            double availableHeight = grid.GetRowHeight(row);
            double availableWidth = grid.GetColWidth(col);
            int nextRowIndex = row + 1;
            int nextColIndex = col + 1;
            while (rect.Height > availableHeight)
            {
                if (nextRowIndex >= grid.NumRows) return false;
                if (!grid.IsCellFree(col, nextRowIndex)) return false;
                availableHeight += grid.GetRowHeight(nextRowIndex);
                nextRowIndex++;
            }
            while (rect.Width > availableWidth)
            {
                if (nextColIndex >= grid.NumCols) return false;
                if (!grid.IsCellFree(nextColIndex, row)) return false;
                availableWidth += grid.GetColWidth(nextColIndex);
                nextColIndex++;
            }

            // PLACE RECT
            grid.InsertRect(rect, col, row);

            return true;
        }

        /// <summary>
        /// Scales grid to fit application window, updates sizes of items in rects based on state of grid,
        /// creates Rectangles based on items in rects List and adds them to canvas.Children.
        /// </summary>
        private void DrawRects()
        {
            grid.ScaleTo(new Size(MainCanvas.ActualWidth, MainCanvas.ActualHeight));
            rects = grid.GetRects(rects.Count);

            // DRAW GRID'S BOUNDS
            Rectangle boundsRect = new Rectangle();
            boundsRect.Width = grid.Bounds.Width;
            boundsRect.Height = grid.Bounds.Height;

            // create Brush for Rectangle's fill and stroke
            SolidColorBrush boundsBrush = new SolidColorBrush(Colors.DarkViolet);
            boundsRect.Fill = boundsBrush;

            // add Rectangle to Canvas
            Canvas.SetLeft(boundsRect, grid.Bounds.X);
            Canvas.SetTop(boundsRect, grid.Bounds.Y);
            MainCanvas.Children.Add(boundsRect);

            // DRAW EACH RECT
            foreach (Rect rect in rects)
            {
                Rectangle r = new Rectangle();
                r.Width = rect.Width;
                r.Height = rect.Height;

                // create Brush for Rectangle's fill and stroke
                SolidColorBrush b = new SolidColorBrush(Colors.AliceBlue);
                r.Fill = b;
                b = new SolidColorBrush(Colors.Red);
                r.StrokeThickness = 4d;
                r.Stroke = b;

                // add Rectangle to Canvas
                Canvas.SetLeft(r, rect.X);
                Canvas.SetTop(r, rect.Y);
                MainCanvas.Children.Add(r);
            }
        }

        /// <summary>
        /// Scales grid to fit application window, updates sizes of items in rects and images based on
        /// state of grid, adds items in images List to canvas.Children.
        /// </summary>
        private void DrawImages()
        {
            grid.ScaleTo(new Size(MainCanvas.ActualWidth, MainCanvas.ActualHeight));
            rects = grid.GetRects(rects.Count);

            for (int i = 0; i < rects.Count; i++)
            {
                images[i].Width = rects[i].Width;
                images[i].Height = rects[i].Height;

                Canvas.SetLeft(images[i], rects[i].X);
                Canvas.SetTop(images[i], rects[i].Y);
                MainCanvas.Children.Add(images[i]);
            }
        }

        
    }
}
