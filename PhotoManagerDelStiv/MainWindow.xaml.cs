using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MetadataExtractor;
using ImageProcessor.Processors;
using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using CoordinateSharp;

namespace PhotoManagerDelStiv
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public string myGPS = "";
        public string SourceRoot = @"C:\Temp";
        public string DestRoot = @"D:\Camera";
        
        public string Destination1 = @"C:\_OneDrive\OneDrive\Mobile Pictures\Scenic & Travel\Gypsies";
        public string Dest1Name = @"OneNote Gypsies";

        public string Destination2 = @"C:\Users\stiv\Desktop\2blog";
        public string Dest2Name = "To Blog";

        public string Destination3 = "";
        public string Dest3Name = "Not set";

        public string Destination4 = "";
        public string Dest4Name = "Not set";

        public string Destroot = "";

        public string Move2Root = @"D:\Camera";
        public static List<string> PicFiles = new List<string>();
        public static List<string> skipFiles = new List<string>();
        public int PicIndex = 0;

        public string Trash = @"D:\Basura";

        public MainWindow()
        {
            InitializeComponent();
            // Put settings load here eventually
            SetButtons();
            // Check for source directory and settings here
            getFilenames(SourceRoot);

            var diditwork = LoadPicture(PicFiles[PicIndex]);

        }

        private void SetButtons()
        {
            Dest1Button.Content = "CP: " + Dest1Name;
            Dest1Button.ToolTip = Destination1;
            Dest2Button.Content = "CP: " + Dest2Name;
            Dest2Button.ToolTip = Destination2;

        }

        private void SelectTargetRoot_Click(object sender, RoutedEventArgs e)
        {

        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }



        /// Stiv Functions
        /// 

        public static void getFilenames(string directory)
        {
            PicFiles = new List<string>();
            skipFiles = new List<string>();
            ProcessDirectory(directory);
        }
        // Process all files in the directory passed in, recurse on any directories
        // that are found, and process the files they contain.
        public static void ProcessDirectory(string targetDirectory)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = System.IO.Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                ProcessFile(fileName);

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = System.IO.Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory);
        }

        // Insert logic for processing found files here.
        public static void ProcessFile(string path)
        {
            string x = System.IO.Path.GetExtension(path).ToLower();
            if (x==".jpg" || x==".jpeg" )
            {
                PicFiles.Add(path);
            }
            else
            {
                skipFiles.Add(path);
            }
        }










        #region Functions

        /// <summary>
        /// Modifies a JPEG file using ImageProcessor Library.  Long1024 resizes the image to be 1024 pixels on its long edge. Resize50 resizes 50%
        /// </summary>
        /// <param name="file">The name of the file to modify</param>
        /// <param name="operation">RotateCCW, RotateCW, Resize50, Long1024</param>
        public bool ModJPEG(string filename, string operation)
        {
            byte[] photoBytes = File.ReadAllBytes(filename);   
            // Format is automatically detected though can be changed.
            ISupportedImageFormat format = new JpegFormat { Quality = 100 };
            System.Drawing.Size size = new System.Drawing.Size(150, 0);
            using (MemoryStream inStream = new MemoryStream(photoBytes))
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    // Initialize the ImageFactory using the overload to preserve EXIF metadata.
                    using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                    {
                        switch (operation)
                        {
                            case "RotateCW":
                                     imageFactory.Load(inStream).Format(format).Rotate(90).Save(outStream);
                                break;
                            case "RotateCCW":
                                imageFactory.Load(inStream).Format(format).AutoRotate().Rotate(270).Save(outStream);
                                break;
                            case "Resize50"://Not done yet.
                                imageFactory.Load(inStream)
                                    .Format(format)
                                    .AutoRotate()
                                    .Save(outStream);
                                break;
                        default:
                                break;
                        }
                        imageFactory.Dispose();
                    }
                    inStream.Close();
                    inStream.Dispose();
                    try {
                        File.WriteAllBytes(filename, outStream.ToArray());
                        OpStat.Content = "Write succeeded!";
                        OpStat.Tag = "Modified " + filename;
                        OpStat.ToolTip = "Modified " + filename;
                    }
                    catch(Exception ex)
                    {
                        OpStat.Content = "Write failed!";
                        OpStat.Tag = ex.Message;
                        OpStat.ToolTip = ex.Message;
                        return false;
                    }
                    outStream.Close();
                    outStream.Dispose();
                    return true;
                    
                }
            }
        }

        public bool LoadPicture(string picturepath)
        {
            FileStream testfile;
            try {
                testfile = File.OpenWrite(picturepath);
                SetButtonsState(true);
                testfile.Close();
            }
            catch
            {
                SetButtonsState(false);
                
            }


            GPS_button.Content = "GPS Data";
            GPS_button.IsEnabled = false;
            // Get metadata from original image
            //Get der metadata

            string Coordinates = "";
            string Altitude = "";

            //Get the image for the display without locking file.
            

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(picturepath);
            image.EndInit();
            


            FilenameLable.Content = PicIndex+1 +" of " + PicFiles.Count + " - " + picturepath;


            //Metadata Extractor
            var meps = ImageMetadataReader.ReadMetadata(picturepath);
            IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(picturepath);


            try
            {
                var GPS = directories.OfType<MetadataExtractor.Formats.Exif.GpsDirectory>().FirstOrDefault();
                var tags = GPS.Tags;
                Coordinates = "GPS Coordinates: " + tags[1].Description + " " + tags[2].Description + " , " + tags[3].Description + " " + tags[4].Description;
                Coordinates = Coordinates.Replace(@"\", "");
                Altitude = tags[6].Description + " above " + tags[5].Description;
                string myLat = tags[2].Description.Replace(@"\", "") + tags[1].Description;
                string myLong = tags[4].Description.Replace(@"\", "") + tags[3].Description;
                myGPS = myLat.Replace(" ", string.Empty) + " " + myLong.Replace(" ", string.Empty);

                Coordinate C = new Coordinate();
                Coordinate.TryParse(myGPS, out C);
                myGPS = C.Latitude.DecimalDegree + " " + C.Longitude.DecimalDegree;



                GPS_button.Content = Coordinates + "     " + Altitude;
                GPS_button.IsEnabled = true;

            }
            catch
            {
                GPS_button.Content = "No GPS Coordinates Found!";
            }


            CurrentPicture.Source = image.Clone();
            CurrentPicture.UpdateLayout();
            MainContainer.UpdateLayout();
            
            return true;
            

        }

        public void SetButtonsState(bool state)
        {
            RotateClockButton.IsEnabled = state;
            rotateCCWbutton.IsEnabled = state;
        }
        #endregion Functions

        ///UI Action Events

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            
            try {
                PicIndex++;
                LoadPicture(PicFiles[PicIndex]);
    }
            catch
            {
                PicIndex--;
                LoadPicture(PicFiles[PicIndex]);
            }

        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                PicIndex--;
                LoadPicture(PicFiles[PicIndex]);
            }
            catch
            {
                PicIndex++;
                LoadPicture(PicFiles[PicIndex]);
            }

        }

        private void RotateClockButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentPicture.Source = null;

            
            var OK = ModJPEG(PicFiles[PicIndex],"RotateCW");
            
            LoadPicture(PicFiles[PicIndex]);

        }

        private void rotateCCWbutton_Click(object sender, RoutedEventArgs e)
        {
            CurrentPicture.Source = null;
            var OK=ModJPEG(PicFiles[PicIndex], "RotateCCW");
            if (OK) LoadPicture(PicFiles[PicIndex]);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TestProcessorClick(object sender, RoutedEventArgs e)
        {
            string filename = PicFiles[0];
            //Test using ImageProcessor
            byte[] photoBytes = File.ReadAllBytes(filename);
            // Format is automatically detected though can be changed.
            ISupportedImageFormat format = new JpegFormat { Quality = 100 };
            System.Drawing.Size size = new System.Drawing.Size(150, 0);
          using (MemoryStream inStream = new MemoryStream(photoBytes))
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    // Initialize the ImageFactory using the overload to preserve EXIF metadata.
                    using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                    {
                        



                        // Load, resize, set the format and quality and save an image.
                        imageFactory.Load(inStream)
                                    .Format(format)
                                    .AutoRotate()
                                    .Save(outStream);

                    }
                    
                    File.WriteAllBytes(filename, outStream.ToArray());
                    outStream.Dispose();
                    inStream.Dispose();
                }
            }
        }

        private void GPS_button_Click(object sender, RoutedEventArgs e)
        {
            string url = @"http://maps.google.com/maps?q=" + myGPS;
            System.Diagnostics.Process.Start(url);

        }

        private void FirstButton_Click(object sender, RoutedEventArgs e)
        {
            PicIndex = 0;
            LoadPicture(PicFiles[PicIndex]);
        }

        private void lastbutton_Click(object sender, RoutedEventArgs e)
        {
            PicIndex = PicFiles.Count-1;
            LoadPicture(PicFiles[PicIndex]);
        }


    }


}
