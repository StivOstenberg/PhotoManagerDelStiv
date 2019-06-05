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
using System.Windows.Input;
using System.Drawing.Imaging;

namespace PhotoManagerDelStiv
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public string myGPS = "";
        public string SourceRoot = @"C:\Users\stiv\Desktop\2blog";
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
        /// <param name="operation">RotateCCW, RotateCW, Resize50, </param>
        /// <remarks>Requires MetadataExtractor and ImageProcessor, both available with  NuGet 
        /// Written by Stiv Ostenberg  (code@stiv.com)
        /// 
        /// </remarks>
        
        public bool ModJPEG(string filename, string operation)
        {
            IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(filename);
            int Hor = 0;
            int Vert = 0;


            try // Two possible options for getting the size.   If one fails, use the other.
            {
                var myData = directories.OfType<MetadataExtractor.Formats.Exif.ExifDirectoryBase>().FirstOrDefault().Tags;
                Hor = GetNumber(myData[0].Description);
                Vert = GetNumber(myData[1].Description);
            }
            catch (Exception ex)
            {
                var myData = directories.OfType<MetadataExtractor.Formats.Jpeg.JpegDirectory>().FirstOrDefault().Tags;
                Hor = GetNumber(myData[3].Description);
                Vert = GetNumber(myData[2].Description);
            }



            byte[] photoBytes = File.ReadAllBytes(filename);   
            ISupportedImageFormat format = new JpegFormat { Quality = 100 };


            System.Drawing.Size halfsize = new System.Drawing.Size(Hor/2, Vert/2); // Compute the new image size if 1/2 size is selected.

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
                            case "Resize50":
                                 imageFactory.Load(inStream).Format(format).AutoRotate().Resize(halfsize).Save(outStream);
                                break;
                        default:
                                break;
                        }
                        imageFactory.Dispose();
                    }
                    inStream.Close();
                    inStream.Dispose();

                    //We have changed the rotation, but we need to remove the Exif rotation flag 
                    try
                    {
                        using (MemoryStream modStream = new MemoryStream())
                        {
                            var modImage = System.Drawing.Image.FromStream(outStream);
                            PropertyItem RotValue = modImage.GetPropertyItem(0x0112); // Get the Exif Rotation Property Value which is 112Hex 274 int
                                                                                      //The start point of stored data is, '01' means upper left, '03' lower right, '06' upper right, '08' lower left, '09' undefined.
                            RotValue.Value[0] = 9; 
                            modImage.SetPropertyItem(RotValue);
                            modImage.Save(modStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                            try
                            {

                                File.WriteAllBytes(filename, modStream.ToArray());
                            }
                            catch (Exception ex)
                            {
                                outStream.Close();
                                outStream.Dispose();
                                modStream.Close();
                                modStream.Dispose();
                                return false;
                            }
                            outStream.Close();
                            outStream.Dispose();
                            modStream.Close();
                            modStream.Dispose();

                        }
                    }
                    catch
                    {
                        MessageBox.Show("Oh Noes!   Something borked!");
                        return false;
                    }

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
            



            //Metadata Extractor
            IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(picturepath);
            int Hor = 0;
            int Vert = 0;


            try
            {
                var Data = directories.OfType<MetadataExtractor.Formats.Exif.ExifDirectoryBase>().FirstOrDefault().Tags;
                Hor = GetNumber(Data[0].Description);
                Vert = GetNumber(Data[1].Description);
                
            }
            catch(Exception ex)
            {
                var Data = directories.OfType<MetadataExtractor.Formats.Jpeg.JpegDirectory>().FirstOrDefault().Tags;
                Hor = GetNumber(Data[3].Description);
                Vert = GetNumber(Data[2].Description);
            }


            string Rotatey = "No Exif Rotation Tag,  Budda be praised!";
            try //Get the fucking EXIF Rotation Tag Data
            {
                
                var Rot = directories.OfType<MetadataExtractor.Formats.Exif.ExifIfd0Directory>().FirstOrDefault().Tags;
                Rotatey = Rot[4].Description;
                NukeEXIFRotationbutton.Content = "Nuke Exif Rotation\n" + Rotatey;
                NukeEXIFRotationbutton.ToolTip = Rotatey;

            }
            catch
            {
                NukeEXIFRotationbutton.Content = "Nuke Exif Rotation\n" + Rotatey;
                NukeEXIFRotationbutton.ToolTip = Rotatey;
            }





            FilenameLable.Content = PicIndex + 1 + " of " + PicFiles.Count + " - " + picturepath + "\n" + Hor + "x" + Vert;

            try
            {
                var GPS = directories.OfType<MetadataExtractor.Formats.Exif.GpsDirectory>().FirstOrDefault();
                var tags = GPS.Tags;

                var latref = "";
                var lati = "";
                var longref = "";
                var longi = "";
                var altref = "";
                var alti = "";

                foreach (Tag thistag in GPS.Tags)
                {
                    if(thistag.Name.Equals("GPS Latitude Ref"))   latref=thistag.Description ;
                    if (thistag.Name.Equals("GPS Latitude")) lati = thistag.Description;
                    if (thistag.Name.Equals("GPS Longitude Ref")) longref = thistag.Description;
                    if (thistag.Name.Equals("GPS Longitude")) longi = thistag.Description;
                    if (thistag.Name.Equals("GPS Altitude Ref")) altref = thistag.Description;
                    if (thistag.Name.Equals("GPS Altitude")) alti = thistag.Description;

                }






                Coordinates = "GPS Coordinates: " + latref + " " + lati + " , " + longref + " " + longi;
                Coordinates = Coordinates.Replace(@"\", "");
                Altitude = alti + " above " + altref;
                string myLat = latref.Replace(@"\", "") + lati;
                string myLong = longref.Replace(@"\", "") + longi;


                myGPS = myLat.Replace(" ", string.Empty) + " " + myLong.Replace(" ", string.Empty);



                Coordinate C = new Coordinate();
                Coordinate.TryParse(myGPS, out C);
                myGPS = C.Latitude.DecimalDegree + " " + C.Longitude.DecimalDegree;



                GPS_button.Content = Coordinates + "     " + Altitude;
                GPS_button.IsEnabled = true;

            }
            catch(Exception ex)
            {
                GPS_button.Content = "No GPS Coordinates Found!";
            }

            GoTo.Text = PicIndex.ToString()+1;
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


        public static int GetNumber(string Text)
        {
            int val = 0;
            for (int i = 0; i < Text.Length; i++)
            {
                char c = Text[i];
                if (c >= '0' && c <= '9')
                {
                    val *= 10;
                    //(ASCII code reference)
                    val += c - 48;
                }
            }
            return val;
        }


        public bool clearexif1()
        {
            var original = @"c:\temp\exif.jpg";
            var copy = original;
            const BitmapCreateOptions createOptions = BitmapCreateOptions.PreservePixelFormat | BitmapCreateOptions.IgnoreColorProfile;

            using (Stream originalFileStream = File.Open(copy, FileMode.Open, FileAccess.Read))
            {
                BitmapDecoder decoder = BitmapDecoder.Create(originalFileStream, createOptions, BitmapCacheOption.None);


                BitmapMetadata metadata = decoder.Frames[0].Metadata == null
                  ? new BitmapMetadata("jpg")
                  : decoder.Frames[0].Metadata.Clone() as BitmapMetadata;

                if (metadata == null)
                {
                    string isit = "";
                }
                else
                {

                }

                var keywords = metadata.Keywords == null ? new List<string>() : new List<string>(metadata.Keywords);
                metadata.Keywords = new System.Collections.ObjectModel.ReadOnlyCollection<string>(keywords);
                JpegBitmapEncoder encoder = new JpegBitmapEncoder { QualityLevel = 100 };
                encoder.Frames.Add(BitmapFrame.Create(decoder.Frames[0], decoder.Frames[0].Thumbnail, metadata,
                  decoder.Frames[0].ColorContexts));

                copy = @"c:\temp\exifout.jpg";

                using (Stream newFileStream = File.Open(copy, FileMode.Create, FileAccess.ReadWrite))
                {
                    encoder.Save(newFileStream);
                    return true;
                }
            }
        }


        #endregion Functions

        ///UI Action Events

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            
            try {
                PicIndex++;
                LoadPicture(PicFiles[PicIndex]);
    }
            catch(Exception ex)
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

        private void Resize50Button_Click(object sender, RoutedEventArgs e)
        {
            CurrentPicture.Source = null;
            var OK = ModJPEG(PicFiles[PicIndex], "Resize50");
            if (OK) LoadPicture(PicFiles[PicIndex]);
        }

        private void Move2Trash_Click(object sender, RoutedEventArgs e)
        {
            string FN =Path.GetFileName(PicFiles[PicIndex]);
            try
            {
                File.Move(PicFiles[PicIndex], @"C:\Deleted\" + FN);
                PicFiles.RemoveAt(PicIndex);
                LoadPicture(PicFiles[PicIndex]);        }
            catch
            {

            }
        }




        private void GoTo_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                int NewIndex = GetNumber(GoTo.Text);
                if ((0 <= NewIndex) && (NewIndex <= PicFiles.Count))
                {
                    PicIndex = NewIndex;
                    LoadPicture(PicFiles[PicIndex]);
                }
                else
                {
                    GoTo.Text = PicIndex.ToString();
                }

            }
        }

    }


}
