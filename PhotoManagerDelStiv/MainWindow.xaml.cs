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

        public string Trash = @"D:\Basura";

        public MainWindow()
        {
            InitializeComponent();
            // Put settings load here eventually
            SetButtons();
            // Check for source directory and settings here
            getFilenames(SourceRoot);

            var diditwork = LoadPicture(PicFiles[0]);

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

        public bool LoadPicture(string picturepath)
        {
            GPS_button.Content = "No GPS Coordinates Found!";
            GPS_button.IsEnabled = false;
            // Get metadata from original image
            //Get der metadata
            Bitmap imagemeta = new Bitmap(picturepath);
            System.Drawing.Imaging.PropertyItem[] propItems = imagemeta.PropertyItems;
            string Coordinates = "";
            string Altitude = "";

            //Get the image for the display without locking file.
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(picturepath);
            image.EndInit();
            FilenameLable.Content = picturepath;
            CurrentPicture.Source = image;
            CurrentPicture.UpdateLayout();
            //Metadata Extractor
            var meps = ImageMetadataReader.ReadMetadata(picturepath);
            
            IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(picturepath);


            try {
                var GPS = directories.OfType<MetadataExtractor.Formats.Exif.GpsDirectory>().FirstOrDefault();
                var tags = GPS.Tags;
                Coordinates = "GPS Coordinates: " + tags[1].Description + " " + tags[2].Description + " , " + tags[3].Description +" "+ tags[4].Description;
                Coordinates=Coordinates.Replace(@"\", "");
                Altitude = tags[6].Description +" "+ tags[5].Description;
                string myLat = tags[2].Description.Replace(@"\", "") + tags[1].Description;
                string myLong = tags[4].Description.Replace(@"\", "") + tags[3].Description;
                myGPS = myLat.Replace(" ", string.Empty) + " " + myLong.Replace(" ", string.Empty);

                Coordinate C = new Coordinate();
                Coordinate.TryParse(myGPS,out C);
                myGPS = C.Latitude.DecimalDegree + " " + C.Longitude.DecimalDegree;

                

                GPS_button.Content = Coordinates;
                GPS_button.IsEnabled = true;
                
            }
            catch
            {
                //No GPS coordinates found!
            }

            JpegBitmapDecoder jpgDecoder = new JpegBitmapDecoder(new MemoryStream(File.ReadAllBytes(picturepath)), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            BitmapMetadata myBitmapMetadata = new BitmapMetadata("jpg");
            JpegBitmapEncoder jpgEncoder = new JpegBitmapEncoder();
            BitmapSource bitmapSource;

            if (jpgDecoder != null)
            {
                bitmapSource = jpgDecoder.Frames[0];
                var Meat = bitmapSource.Metadata;  //Returns Image Metadata, not Bitmap metadata.
                myBitmapMetadata = (BitmapMetadata)Meat;


            }
            jpgEncoder = null;
            jpgDecoder = null;
            myBitmapMetadata = null;
            return true;

        }



        private void RotateClock(string file)
        {
            try {
                //Get der metadata
                Bitmap imagemeta = new Bitmap(file);
                System.Drawing.Imaging.PropertyItem[] propItems = imagemeta.PropertyItems;

                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(file);
                image.EndInit();

                Bitmap bitmap1 = BitmapImage2Bitmap(image);
                bitmap1 = (Bitmap)Bitmap.FromFile(PicFiles[0]);

                //Based on argument, select operation

                bitmap1.RotateFlip(RotateFlipType.Rotate90FlipNone);


                string testfile = @"D:\Pictures2Process\Out.Jpg";


                bitmap1.Save(testfile);
                //Add Metadata to new image
                imagemeta = new Bitmap(testfile);

                foreach (var aprop in propItems)
                {
                    imagemeta.SetPropertyItem(aprop);
                }
                imagemeta.Save(testfile);
                LoadPicture(PicFiles[0]);



            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RotateCounterClock(string file)
        {
            try
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(file);
                image.EndInit();

                Bitmap bitmap1 = BitmapImage2Bitmap(image);
                bitmap1 = (Bitmap)Bitmap.FromFile(PicFiles[0]);
                bitmap1.RotateFlip(RotateFlipType.Rotate270FlipNone);
                bitmap1.Save(file);
                LoadPicture(PicFiles[0]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }





        #region Functions

        /// <summary>
        /// Modifies a JPEG file using ImageProcessor Library.  Long1024 resizes the image to be 1024 pixels on its long edge. Resize50 resizes 50%
        /// </summary>
        /// <param name="file">The name of the file to modify</param>
        /// <param name="operation">RotateCCW, RotateCW, Resize50, Long1024</param>
        public void ModJPEG(string filename, string operation)
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
                                     imageFactory.Load(inStream).Format(format).AutoRotate().Rotate(90).Save(outStream);
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
                    
                    File.WriteAllBytes(filename, outStream.ToArray());
                    outStream.Close();
                    outStream.Dispose();

                    
                }
            }
        }

        ///UI Action Events

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            
            try {
                PicFiles.RemoveAt(0);
                LoadPicture(PicFiles[0]);
    }
            catch
            {

            }

        }

        private void RotateClockButton_Click(object sender, RoutedEventArgs e)
        {
            ModJPEG(PicFiles[0],"RotateCW");
            LoadPicture(PicFiles[0]);

        }

        private void rotateCCWbutton_Click(object sender, RoutedEventArgs e)
        {
            ModJPEG(PicFiles[0], "RotateCCW");
            LoadPicture(PicFiles[0]);
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
    }


}
