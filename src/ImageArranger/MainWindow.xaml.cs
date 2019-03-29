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

        private void Canvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("Grid_PreviewMouseLeftButtonDown() called.");

            if (SelectImages())
            {
                GenerateNormalizedLists();

                // pack them
                ArrangeRects();

                // draw Rects/Images
                canvas.Children.Clear();
                //DrawRects();
                DrawImages();
            }//end if
        }//end Canvas_PreviewMouseLeftButtonDown()

        private void Canvas_PreviewDrop(object sender, DragEventArgs e)
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
                    canvas.Children.Clear();
                    //DrawRects();
                    DrawImages();
                }
            }//end if
        }//end Canvas_PreviewDrop()

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // start the resizeTimer
            resizeTimer.IsEnabled = true;
            resizeTimer.Stop();
            resizeTimer.Start();
        }

        private void resizeTimer_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("resizeTimer_Tick(): started.");
            resizeTimer.IsEnabled = false;
            if (filenames.Count <= 0) return;
            // rearrange rects/images based on new canvas size (bounds)
            GenerateNormalizedLists();
            ArrangeRects();
            // redraw images/rects
            canvas.Children.Clear();
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
            canvas.Children.Clear();
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
            canvas.Children.Clear();
        }

        private void AddImage_Click(object sender, RoutedEventArgs e)
        {
            if (SelectImages())
            {
                GenerateNormalizedLists();
                // pack them
                ArrangeRects();
                // draw Rects/Images
                canvas.Children.Clear();
                //DrawRects();
                DrawImages();
            }
        }


        // Methods

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
        /// Given a Rect and values for max width and height, return a new Rect with
        /// the same proportions as orig and whose width and height are not greater
        /// than the given max values.
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="maxWidth"></param>
        /// <param name="maxHeight"></param>
        /// <returns></returns>
        private Rect GetOptimalRect(Rect orig, double maxWidth, double maxHeight)
        {
            Console.WriteLine("GetOptimalRect(): started.");

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
            double w = ((Canvas)this.Content).ActualWidth;
            double h = ((Canvas)this.Content).ActualHeight;

            Console.WriteLine($"Canvas size: {((Canvas)this.Content).ActualWidth}, {((Canvas)this.Content).ActualHeight}");

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

        private void ArrangeRects()
        {
            double heightSum = 0.0;
            foreach (Rect r in rects)
            {
                heightSum += r.Height;
            }

            double aspect = ((Canvas)this.Content).ActualWidth / ((Canvas)this.Content).ActualHeight;

            // bounds can't be smaller than canvas
            Rect bounds = (heightSum < ((Canvas)this.Content).ActualHeight) ?
                new Rect(0.0, 0.0, ((Canvas)this.Content).ActualWidth, ((Canvas)this.Content).ActualHeight) :
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

        private bool PlaceRect(Rect rect, int col, int row)
        {
            if (grid == null)
            {
                Console.WriteLine("PlaceRect(): grid is null");
                return false;
            }

            // CHECK IF RECT CAN BE PLACED ANYWHERE IN GRID
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

        private void DrawRects()
        {
            Console.WriteLine("DrawRects(): started.");

            grid.ScaleTo(new Size(((Canvas)this.Content).ActualWidth, ((Canvas)this.Content).ActualHeight));
            rects = grid.GetRects(rects.Count);

            // DRAW GRID'S BOUNDS FOR DEBUGGING
            Rectangle boundsRect = new Rectangle();
            boundsRect.Width = grid.Bounds.Width;
            boundsRect.Height = grid.Bounds.Height;

            // create Brush for Rectangle's fill and stroke
            SolidColorBrush boundsBrush = new SolidColorBrush(Colors.DarkViolet);
            boundsRect.Fill = boundsBrush;

            // add Rectangle to Canvas
            Canvas.SetLeft(boundsRect, grid.Bounds.X);
            Canvas.SetTop(boundsRect, grid.Bounds.Y);
            canvas.Children.Add(boundsRect);
            // END DEBUGGING

            //Console.WriteLine(grid.ToString());
            // rects are now based on grid

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
                canvas.Children.Add(r);
            }
        }

        private void DrawImages()
        {
            // (heavily copied from DrawRects()

            Console.WriteLine("DrawImages(): started.");

            grid.ScaleTo(new Size(((Canvas)this.Content).ActualWidth, ((Canvas)this.Content).ActualHeight));
            rects = grid.GetRects(rects.Count);

            for (int i = 0; i < rects.Count; i++)
            {
                images[i].Width = rects[i].Width;
                images[i].Height = rects[i].Height;

                Canvas.SetLeft(images[i], rects[i].X);
                Canvas.SetTop(images[i], rects[i].Y);
                canvas.Children.Add(images[i]);
            }
        }

        
    }
}
