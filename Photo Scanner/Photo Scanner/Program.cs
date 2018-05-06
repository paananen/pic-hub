using MetadataExtractor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Photo_Scanner
{
    internal static class Program
    {
        private static readonly string ConnString = ConfigurationManager.ConnectionStrings["SQLServerConnection"].ConnectionString;
        //TODO: I don't like setting this in code - there's got to be a better way to do this
        private const string FtpSite = @"ftp://192.168.0.141";

        /// <summary>
        /// Assumptions:
        ///     1. You've already set up a database (SQL Server) and have the table created
        ///         - https://www.microsoft.com/en-us/download/details.aspx?id=55994
        /// </summary>
        private static void Main()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("PicHub Photo Scanner");
            Console.ForegroundColor = ConsoleColor.White;

            //--> Test read from database
            //ReadFiles();

            var photoDirs = new List<string>
            {
                @"P:\"
            };

            var dbList = GetDbFilePathList();

            foreach (var photoDir in photoDirs)
            {
                string[] fileTypes = { "*.jpg", "*.jpeg", "*.png", "*.gif" };
                var fileNames = GetFiles(photoDir, fileTypes);

                ScanPath(fileNames, dbList);
            }

            //TODO: remove db references to files that don't exist anymore...

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("--- DONE ---");
            Console.ForegroundColor = ConsoleColor.White;
#if DEBUG
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
#endif

        }

        private static void ScanPath(IEnumerable<string> fileNames, IEnumerable<string> dbList)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\t [+] - Converting to lists - This could take a little while, please be patient...");
            var fileList = new List<string>(fileNames);
            var dbFileList = new List<string>(dbList);
            Console.WriteLine("\t [+] - Converting done");

            //Create a "not in database" list
            Console.WriteLine("\t [+] - Creating a list of files that are not in the database");
            var filesNotInDb = fileList.Except(dbFileList).ToList();
            Console.WriteLine("\t [+] - Creating a list of files that are not in the file path (i.e. files that aren't there anymore)");
            var filesNotInPath = dbFileList.Except(fileList).ToList();
            Console.ForegroundColor = ConsoleColor.White;

            foreach (var file in filesNotInDb)
            {
                if (!FileInDb(file))
                {
                    var p = ExtractMetaData(file);
                    DbWrite(p);
                }
                else
                {
                    Console.WriteLine("\t\t [-] ");
                }
            }

            foreach (var file in filesNotInPath)
            {
                if (!File.Exists(file))
                {
                    DeleteDbEntry(file);
                }
            }
        }

        private static void DeleteDbEntry(string filePath)
        {
            using (var conn = new SqlConnection(ConnString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"DELETE FROM Photos WHERE FullPath = @filePath";
                    cmd.Parameters.AddWithValue("@filePath", filePath);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static PhotoData ExtractMetaData(string file)
        {
            try
            {
                IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(file);

                var p = new PhotoData
                {
                    FullPath = file,
                    FileName = file.Split('\\')[file.Split('\\').Length - 1]
                };
                p.Extension = p.FileName.Split('.')[file.Split('.').Length - 1];
                var firstSlash = file.IndexOf('\\');
                var lastSlash = file.LastIndexOf('\\');
                p.DirectoryPath = file.Substring(firstSlash, (lastSlash - firstSlash) + 1);
                // make sure spaces and slashes in the ftp path are URL safe
                p.FtpPath = FtpSite + p.DirectoryPath.Replace(@"\", @"/").Replace(" ", @"%20") + p.FileName.Replace(" ", @"%20");

                foreach (var dir in directories)
                {
                    foreach (var tag in dir.Tags)
                    {
                        //Console.WriteLine($"[{ tag.DirectoryName}] {tag.Name} = {tag.Description}");
                        if (tag.Name == "Image Height")
                        {
                            p.Height = Convert.ToInt32(tag.Description.Split(' ')[0]);
                            p.DimensionUnit = tag.Description.Split(' ')[1];
                        }
                        else if (tag.Name == "Image Width")
                        {
                            p.Width = Convert.ToInt32(tag.Description.Split(' ')[0]);
                            p.DimensionUnit = tag.Description.Split(' ')[1];
                        }
                        else if (tag.Name == "Make") { p.Make = tag.Description; }
                        else if (tag.Name == "Model") { p.Model = tag.Description; }
                        else if (tag.Name == "Orientation") { p.Orientation = tag.Description; }
                        else if (tag.Name == "X Resolution") { p.XResolution = tag.Description; }
                        else if (tag.Name == "Y Resolution") { p.YResolution = tag.Description; }
                        else if (tag.Name == "Resolution Unit") { p.ResolutionUnit = tag.Description; }
                        else if (tag.Name == "Date/Time") { p.ExifIfd0DateTime = ConvertDateTime(tag.Description); }
                        else if (tag.Name == "Artist") { p.Photographer = tag.Description; }
                        else if (tag.Name == "Exposure Time") { p.ExposureTime = tag.Description; }
                        else if (tag.Name == "Shutter Speed Value") { p.ShutterSpeed = tag.Description; }
                        else if (tag.Name == "F-Number") { p.FNumber = tag.Description; }
                        else if (tag.Name == "Exposure Program") { p.ExposureProgram = tag.Description; }
                        else if (tag.Name == "ISO Speed Ratings") { p.Iso = tag.Description; }
                        else if (tag.Name == "Date/Time Original") { p.DateTimeOriginal = ConvertDateTime(tag.Description); }
                        else if (tag.Name == "Date/Time Digitized") { p.DateTimeDigitized = ConvertDateTime(tag.Description); }
                        else if (tag.Name == "Exposure Bias Value") { p.ExposureBiasValue = tag.Description; }
                        else if (tag.Name.Contains("Max Aperture")) { p.MaxAperture = tag.Description; }
                        else if (tag.Name == "Min Aperture") { p.MinAperture = tag.Description; }
                        else if (tag.Name == "Flash") { p.Flash = tag.Description; }
                        else if (tag.Name == "Focal Length") { p.FocalLength = tag.Description; }
                        else if (tag.Name == "Exposure Mode") { p.ExposureMode = tag.Description; }
                        else if (tag.Name == "White Balance Mode") { p.WhiteBalanceMode = tag.Description; }
                        else if (tag.Name == "Body Serial Number") { p.BodySerialNumber = tag.Description; }
                        else if (tag.Name == "Lens Model") { p.LensModel = tag.Description; }
                        else if (tag.Name == "Lens Serial Number") { p.LensSerialNumber = tag.Description; }
                        else if (tag.Name == "Focus Mode") { p.FocusMode = tag.Description; }
                        else if (tag.Name == "Auto ISO") { p.AutoIso = tag.Description; }
                        else if (tag.Name == "Base ISO") { p.BaseIso = tag.Description; }
                        else if (tag.Name == "Measured EV") { p.MeasuredEv = tag.Description; }
                        else if (tag.Name == "Exposure Time") { p.ExposureTime = tag.Description; }
                        else if (tag.DirectoryName == "Exif Thumbnail" && tag.Name == "Orientation") { p.ThumbnailOrientation = tag.Description; }
                        else if (tag.DirectoryName == "Exif Thumbnail" && tag.Name == "X Resolution") { p.ThumbnailXRes = tag.Description; }
                        else if (tag.DirectoryName == "Exif Thumbnail" && tag.Name == "Y Resolution") { p.ThumbnailYRes = tag.Description; }
                        else if (tag.DirectoryName == "Exif Thumbnail" && tag.Name == "Resolution Unit") { p.ThumbnailResUnit = tag.Description; }
                        else if (tag.Name == "Thumbnail Offset") { p.ThumbnailOffset = tag.Description; }
                        else if (tag.Name == "Thumbnail Length") { p.ThumbnailLength = tag.Description; }
                        else if (tag.Name == "File Size") { p.Size = tag.Description; }
                        else if (tag.Name == "File Modified Date") { p.FileModifiedDate = ConvertDateTime(tag.Description); }
                        else if (tag.Name == "GPS Date Stamp") { p.GpsDateStamp = tag.Description; }
                        else if (tag.Name == "GPS Altitude Ref") { p.GpsAltitudeRef = tag.Description; }
                        else if (tag.Name == "GPS Longitude Ref") { p.GpsLongitudeRef = tag.Description; }
                        else if (tag.Name == "GPS Longitude") { p.GpsLongitude = tag.Description; }
                        else if (tag.Name == "GPS Latitude Ref") { p.GpsLatitudeRef = tag.Description; }
                        else if (tag.Name == "GPS Time-Stamp") { p.GpsTimeStamp = tag.Description; }
                        else if (tag.Name == "GPS Altitude") { p.GpsAltitude = tag.Description; }
                        else if (tag.Name == "GPS Latitude") { p.GpsLatitude = tag.Description; }

                        // 41°24'12.2"N 2°10'26.5"E
                        //if (!string.IsNullOrEmpty(p.GPSLatitude) && !string.IsNullOrEmpty(p.GPSLatitudeRef) && !string.IsNullOrEmpty(p.GPSLongitude) && !string.IsNullOrEmpty(p.GPSLongitudeRef))
                        //{
                        //    string lat = p.GPSLatitude + p.GPSLatitudeRef;
                        //    string lon = p.GPSLongitude + p.GPSLongitudeRef;
                        //    p.GPSCoordinates = "[" + lat + ", " + lon + "]";
                        //}
                    }
                    if (dir.HasError)
                    {
                        foreach (var error in dir.Errors)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"ERROR: {error}");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                }
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(file);
                return p;
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(file);
                Console.ForegroundColor = ConsoleColor.White;

                var p = new PhotoData
                {
                    FullPath = file,
                    FileName = file.Split('\\')[file.Split('\\').Length - 1]
                };
                p.Extension = p.FileName.Split('.')[file.Split('.').Length - 1];
                var firstSlash = file.IndexOf('\\');
                var lastSlash = file.LastIndexOf('\\');
                p.DirectoryPath = file.Substring(firstSlash, (lastSlash - firstSlash) + 1);
                p.FtpPath = FtpSite + p.DirectoryPath.Replace(@"\", @"/").Replace(" ", @"%20") + p.FileName.Replace(" ", @"%20");
                p.Size = GetProperty(file, "File Size");
                var hString = GetProperty(file, "Image Height");
                try { p.DimensionUnit = hString.Split(' ')[1]; } catch { p.DimensionUnit = string.Empty; }
                p.Width = GetWidth(file);
                p.Height = GetHeight(file);
                return p;
            }
        }
        private static string GetProperty(string file, string property)
        {
            try
            {
                var val = string.Empty;
                IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(file);

                foreach (var dir in directories)
                {
                    foreach (var tag in dir.Tags)
                    {
                        if (tag.Name == property)
                        {
                            val = tag.Description;
                        }
                    }
                }

                return val;
            }
            catch
            {
                return string.Empty;
            }
        }
        private static int? GetWidth(string file)
        {
            try
            {
                var width = 0;
                IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(file);

                foreach (var dir in directories)
                {
                    foreach (var tag in dir.Tags)
                    {
                        if (tag.Name == "Image Width")
                        {
                            width = Convert.ToInt32(tag.Description.Split(' ')[0]);
                        }
                    }
                }
                return width;
            }
            catch
            {
                return null;
            }
        }
        private static int? GetHeight(string file)
        {
            try
            {
                var height = 0;
                IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(file);

                foreach (var dir in directories)
                {
                    foreach (var tag in dir.Tags)
                    {
                        if (tag.Name == "Image Height")
                        {
                            height = Convert.ToInt32(tag.Description.Split(' ')[0]);
                        }
                    }
                }
                return height;
            }
            catch
            {
                return null;
            }
        }
        private static bool FileInDb(string filePath)
        {
            using (var conn = new SqlConnection(ConnString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT COUNT(FullPath) AS 'FileCount' FROM Photos WHERE FullPath = @FullPath";
                    cmd.Parameters.AddWithValue("@FullPath", filePath);
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        var x = 0;
                        while (r.Read())
                        {
                            x = r.GetInt32(r.GetOrdinal("FileCount"));
                        }
                        return x > 0;
                    }
                }
            }
        }

        private static IEnumerable<string> GetFiles(string path, IEnumerable<string> searchPatterns, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return searchPatterns.AsParallel().SelectMany(searchPattern => System.IO.Directory.EnumerateFiles(path, searchPattern, searchOption).Select(Path.GetFullPath));
        }

        private static List<string> GetDbFilePathList()
        {
            Console.WriteLine("\t [+] - Getting a list of files from the database");
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var x = new List<string>();
            using (var conn = new SqlConnection(ConnString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT FullPath FROM Photos";
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var fullPath = r.GetString(r.GetOrdinal("FullPath"));
                            x.Add(fullPath);
                            Console.WriteLine("\t\t" + fullPath);
                        }
                    }
                }
            }
            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            var ts = stopWatch.Elapsed;
            // Format and display the TimeSpan value.
            var elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\t [+] - Database files list created in {elapsedTime}");
            Console.ForegroundColor = ConsoleColor.White;
            return x;
        }
        /// <summary>
        /// 2018:02:10 13:49:27
        /// </summary>
        /// <param name="dateString"></param>
        /// <returns></returns>
        private static DateTime? ConvertDateTime(string dateString)
        {
            try
            {
                var date = dateString.Split(' ')[0];
                var yyyy = Convert.ToInt32(date.Split(':')[0]);
                var month = Convert.ToInt32(date.Split(':')[1]);
                var dd = Convert.ToInt32(date.Split(':')[2]);

                var time = dateString.Split(' ')[1];
                var hh = Convert.ToInt32(time.Split(':')[0]);
                var mm = Convert.ToInt32(time.Split(':')[1]);
                var ss = Convert.ToInt32(time.Split(':')[2]);

                return new DateTime(yyyy, month, dd, hh, mm, ss);
            }
            catch
            {
                return ConvertAnotherDateTimeFormat(dateString);
            }
        }
        /// <summary>
        /// Fri Jan 22 13:00:04 +10:00 2016
        /// </summary>
        /// <param name="dateString"></param>
        /// <returns></returns>
        private static DateTime? ConvertAnotherDateTimeFormat(string dateString)
        {
            try
            {
                // 0   1   2  3        4      5
                // Fri Jan 22 13:00:04 +10:00 2016
                //            0  1  2

                var d = dateString.Split(' ');
                var yyyy = Convert.ToInt32(d[5]);
                var month = 0;
                if (d[1] == "Jan") { month = 1; }
                else if (d[1] == "Feb") { month = 2; }
                else if (d[1] == "Mar") { month = 3; }
                else if (d[1] == "Apr") { month = 4; }
                else if (d[1] == "May") { month = 5; }
                else if (d[1] == "Jun") { month = 6; }
                else if (d[1] == "Jul") { month = 7; }
                else if (d[1] == "Aug") { month = 8; }
                else if (d[1] == "Sep") { month = 9; }
                else if (d[1] == "Oct") { month = 10; }
                else if (d[1] == "Nov") { month = 11; }
                else if (d[1] == "Dec") { month = 12; }
                var dd = Convert.ToInt32(d[2]);

                var t = d[3].Split(':');
                var hh = Convert.ToInt32(t[0]);
                var mm = Convert.ToInt32(t[1]);
                var ss = Convert.ToInt32(t[2]);

                return new DateTime(yyyy, month, dd, hh, mm, ss);
            }
            catch
            {
                return null;
            }
        }

        private static void DbWrite(PhotoData photo)
        {
            using (var conn = new SqlConnection(ConnString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    #region SQL
                    cmd.CommandText = @"INSERT INTO Photos
                                           (FullPath
                                           ,FTPPath
                                           ,DirectoryPath
                                           ,FileName
                                           ,Thumbnail
                                           ,SmallVersion
                                           ,MediumVersion
                                           ,LargeVersion
                                           ,Extension
                                           ,Size
                                           ,FileModifiedDate
                                           ,ExifIFD0DateTime
                                           ,DateTimeOriginal
                                           ,DateTimeDigitized
                                           ,DimensionUnit
                                           ,Width
                                           ,Height
                                           ,Orientation
                                           ,Model
                                           ,Make
                                           ,BodySerialNumber
                                           ,LensSpecification
                                           ,LensModel
                                           ,LensSerialNumber
                                           ,GPSDateStamp
                                           ,GPSTimeStamp
                                           ,GPSDateTimeStamp
                                           ,GPSAltitudeRef
                                           ,GPSAltitude
                                           ,GPSLongitudeRef
                                           ,GPSLongitude
                                           ,GPSLatitudeRef
                                           ,GPSLatitude
                                           ,GPSCoordinates
                                           ,FNumber
                                           ,FocalLength
                                           ,Flash
                                           ,Aperture
                                           ,MaxAperture
                                           ,MinAperture
                                           ,ShutterSpeed
                                           ,ISO
                                           ,AutoISO
                                           ,BaseISO
                                           ,ExposureTime
                                           ,MeasuredEV
                                           ,ExposureMode
                                           ,WhiteBalanceMode
                                           ,FocusMode
                                           ,ExposureBiasValue
                                           ,XResolution
                                           ,YResolution
                                           ,ResolutionUnit
                                           ,Photographer
                                           ,ExposureProgram
                                           ,ThumbnailOrientation
                                           ,ThumbnailXRes
                                           ,ThumbnailYRes
                                           ,ThumbnailResUnit
                                           ,ThumbnailOffset
                                           ,ThumbnailLength)
                                     VALUES
                                           (@FullPath
                                           ,@FTPPath
                                           ,@DirectoryPath
                                           ,@FileName
                                           ,@Thumbnail
                                           ,@SmallVersion
                                           ,@MediumVersion
                                           ,@LargeVersion
                                           ,@Extension
                                           ,@Size
                                           ,@FileModifiedDate
                                           ,@ExifIFD0DateTime
                                           ,@DateTimeOriginal
                                           ,@DateTimeDigitized
                                           ,@DimensionUnit
                                           ,@Width
                                           ,@Height
                                           ,@Orientation
                                           ,@Model
                                           ,@Make
                                           ,@BodySerialNumber
                                           ,@LensSpecification
                                           ,@LensModel
                                           ,@LensSerialNumber
                                           ,@GPSDateStamp
                                           ,@GPSTimeStamp
                                           ,@GPSDateTimeStamp
                                           ,@GPSAltitudeRef
                                           ,@GPSAltitude
                                           ,@GPSLongitudeRef
                                           ,@GPSLongitude
                                           ,@GPSLatitudeRef
                                           ,@GPSLatitude
                                           ,@GPSCoordinates
                                           ,@FNumber
                                           ,@FocalLength
                                           ,@Flash
                                           ,@Aperture
                                           ,@MaxAperture
                                           ,@MinAperture
                                           ,@ShutterSpeed
                                           ,@ISO
                                           ,@AutoISO
                                           ,@BaseISO
                                           ,@ExposureTime
                                           ,@MeasuredEV
                                           ,@ExposureMode
                                           ,@WhiteBalanceMode
                                           ,@FocusMode
                                           ,@ExposureBiasValue
                                           ,@XResolution
                                           ,@YResolution
                                           ,@ResolutionUnit
                                           ,@Photographer
                                           ,@ExposureProgram
                                           ,@ThumbnailOrientation
                                           ,@ThumbnailXRes
                                           ,@ThumbnailYRes
                                           ,@ThumbnailResUnit
                                           ,@ThumbnailOffset
                                           ,@ThumbnailLength)";
                    #endregion

                    #region cmd.Parameters
                    if (!string.IsNullOrEmpty(photo.FullPath)) { cmd.Parameters.AddWithValue("@FullPath", photo.FullPath); } else { cmd.Parameters.AddWithValue("@FullPath", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.FtpPath)) { cmd.Parameters.AddWithValue("@FTPPath", photo.FtpPath); } else { cmd.Parameters.AddWithValue("@FTPPath", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.DirectoryPath)) { cmd.Parameters.AddWithValue("@DirectoryPath", photo.DirectoryPath); } else { cmd.Parameters.AddWithValue("@DirectoryPath", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.FileName)) { cmd.Parameters.AddWithValue("@FileName", photo.FileName); } else { cmd.Parameters.AddWithValue("@FileName", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.Thumbnail)) { cmd.Parameters.AddWithValue("@Thumbnail", photo.Thumbnail); } else { cmd.Parameters.AddWithValue("@Thumbnail", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.SmallVersion)) { cmd.Parameters.AddWithValue("@SmallVersion", photo.SmallVersion); } else { cmd.Parameters.AddWithValue("@SmallVersion", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.MediumVersion)) { cmd.Parameters.AddWithValue("@MediumVersion", photo.MediumVersion); } else { cmd.Parameters.AddWithValue("@MediumVersion", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.LargeVersion)) { cmd.Parameters.AddWithValue("@LargeVersion", photo.LargeVersion); } else { cmd.Parameters.AddWithValue("@LargeVersion", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.Extension)) { cmd.Parameters.AddWithValue("@Extension", photo.Extension); } else { cmd.Parameters.AddWithValue("@Extension", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.Size)) { cmd.Parameters.AddWithValue("@Size", photo.Size); } else { cmd.Parameters.AddWithValue("@Size", DBNull.Value); }
                    if (photo.FileModifiedDate.HasValue) { cmd.Parameters.AddWithValue("@FileModifiedDate", photo.FileModifiedDate.Value.ToString("yyyy-MM-dd hh:mm:ss")); } else { cmd.Parameters.AddWithValue("@FileModifiedDate", DBNull.Value); }
                    if (photo.ExifIfd0DateTime.HasValue) { cmd.Parameters.AddWithValue("@ExifIFD0DateTime", photo.ExifIfd0DateTime.Value.ToString("yyyy-MM-dd hh:mm:ss")); } else { cmd.Parameters.AddWithValue("@ExifIFD0DateTime", DBNull.Value); }
                    if (photo.DateTimeOriginal.HasValue) { cmd.Parameters.AddWithValue("@DateTimeOriginal", photo.DateTimeOriginal.Value.ToString("yyyy-MM-dd hh:mm:ss")); } else { cmd.Parameters.AddWithValue("@DateTimeOriginal", DBNull.Value); }
                    if (photo.DateTimeDigitized.HasValue) { cmd.Parameters.AddWithValue("@DateTimeDigitized", photo.DateTimeDigitized.Value.ToString("yyyy-MM-dd hh:mm:ss")); } else { cmd.Parameters.AddWithValue("@DateTimeDigitized", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.DimensionUnit)) { cmd.Parameters.AddWithValue("@DimensionUnit", photo.DimensionUnit); } else { cmd.Parameters.AddWithValue("@DimensionUnit", DBNull.Value); }
                    if (photo.Width.HasValue) { cmd.Parameters.AddWithValue("@Width", photo.Width); } else { cmd.Parameters.AddWithValue("@Width", DBNull.Value); }
                    if (photo.Height.HasValue) { cmd.Parameters.AddWithValue("@Height", photo.Height); } else { cmd.Parameters.AddWithValue("@Height", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.Orientation)) { cmd.Parameters.AddWithValue("@Orientation", photo.Orientation); } else { cmd.Parameters.AddWithValue("@Orientation", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.Model)) { cmd.Parameters.AddWithValue("@Model", photo.Model); } else { cmd.Parameters.AddWithValue("@Model", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.Make)) { cmd.Parameters.AddWithValue("@Make", photo.Make); } else { cmd.Parameters.AddWithValue("@Make", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.BodySerialNumber)) { cmd.Parameters.AddWithValue("@BodySerialNumber", photo.BodySerialNumber); } else { cmd.Parameters.AddWithValue("@BodySerialNumber", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.LensSpecification)) { cmd.Parameters.AddWithValue("@LensSpecification", photo.LensSpecification); } else { cmd.Parameters.AddWithValue("@LensSpecification", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.LensModel)) { cmd.Parameters.AddWithValue("@LensModel", photo.LensModel); } else { cmd.Parameters.AddWithValue("@LensModel", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.LensSerialNumber)) { cmd.Parameters.AddWithValue("@LensSerialNumber", photo.LensSerialNumber); } else { cmd.Parameters.AddWithValue("@LensSerialNumber", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.GpsDateStamp)) { cmd.Parameters.AddWithValue("@GPSDateStamp", photo.GpsDateStamp); } else { cmd.Parameters.AddWithValue("@GPSDateStamp", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.GpsTimeStamp)) { cmd.Parameters.AddWithValue("@GPSTimeStamp", photo.GpsTimeStamp); } else { cmd.Parameters.AddWithValue("@GPSTimeStamp", DBNull.Value); }
                    if (photo.GpsDateTimeStamp.HasValue) { cmd.Parameters.AddWithValue("@GPSDateTimeStamp", photo.GpsDateTimeStamp.Value.ToString("yyyy-MM-dd hh:mm:ss")); } else { cmd.Parameters.AddWithValue("@GPSDateTimeStamp", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.GpsAltitudeRef)) { cmd.Parameters.AddWithValue("@GPSAltitudeRef", photo.GpsAltitudeRef); } else { cmd.Parameters.AddWithValue("@GPSAltitudeRef", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.GpsAltitude)) { cmd.Parameters.AddWithValue("@GPSAltitude", photo.GpsAltitude); } else { cmd.Parameters.AddWithValue("@GPSAltitude", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.GpsLongitudeRef)) { cmd.Parameters.AddWithValue("@GPSLongitudeRef", photo.GpsLongitudeRef); } else { cmd.Parameters.AddWithValue("@GPSLongitudeRef", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.GpsLongitude)) { cmd.Parameters.AddWithValue("@GPSLongitude", photo.GpsLongitude); } else { cmd.Parameters.AddWithValue("@GPSLongitude", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.GpsLatitudeRef)) { cmd.Parameters.AddWithValue("@GPSLatitudeRef", photo.GpsLatitudeRef); } else { cmd.Parameters.AddWithValue("@GPSLatitudeRef", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.GpsLatitude)) { cmd.Parameters.AddWithValue("@GPSLatitude", photo.GpsLatitude); } else { cmd.Parameters.AddWithValue("@GPSLatitude", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.GpsCoordinates)) { cmd.Parameters.AddWithValue("@GPSCoordinates", photo.GpsCoordinates); } else { cmd.Parameters.AddWithValue("@GPSCoordinates", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.FNumber)) { cmd.Parameters.AddWithValue("@FNumber", photo.FNumber); } else { cmd.Parameters.AddWithValue("@FNumber", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.FocalLength)) { cmd.Parameters.AddWithValue("@FocalLength", photo.FocalLength); } else { cmd.Parameters.AddWithValue("@FocalLength", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.Flash)) { cmd.Parameters.AddWithValue("@Flash", photo.Flash); } else { cmd.Parameters.AddWithValue("@Flash", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.Aperture)) { cmd.Parameters.AddWithValue("@Aperture", photo.Aperture); } else { cmd.Parameters.AddWithValue("@Aperture", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.MaxAperture)) { cmd.Parameters.AddWithValue("@MaxAperture", photo.MaxAperture); } else { cmd.Parameters.AddWithValue("@MaxAperture", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.MinAperture)) { cmd.Parameters.AddWithValue("@MinAperture", photo.MinAperture); } else { cmd.Parameters.AddWithValue("@MinAperture", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.ShutterSpeed)) { cmd.Parameters.AddWithValue("@ShutterSpeed", photo.ShutterSpeed); } else { cmd.Parameters.AddWithValue("@ShutterSpeed", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.Iso)) { cmd.Parameters.AddWithValue("@ISO", photo.Iso); } else { cmd.Parameters.AddWithValue("@ISO", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.AutoIso)) { cmd.Parameters.AddWithValue("@AutoISO", photo.AutoIso); } else { cmd.Parameters.AddWithValue("@AutoISO", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.BaseIso)) { cmd.Parameters.AddWithValue("@BaseISO", photo.BaseIso); } else { cmd.Parameters.AddWithValue("@BaseISO", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.ExposureTime)) { cmd.Parameters.AddWithValue("@ExposureTime", photo.ExposureTime); } else { cmd.Parameters.AddWithValue("@ExposureTime", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.MeasuredEv)) { cmd.Parameters.AddWithValue("@MeasuredEV", photo.MeasuredEv); } else { cmd.Parameters.AddWithValue("@MeasuredEV", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.ExposureMode)) { cmd.Parameters.AddWithValue("@ExposureMode", photo.ExposureMode); } else { cmd.Parameters.AddWithValue("@ExposureMode", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.WhiteBalanceMode)) { cmd.Parameters.AddWithValue("@WhiteBalanceMode", photo.WhiteBalanceMode); } else { cmd.Parameters.AddWithValue("@WhiteBalanceMode", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.FocusMode)) { cmd.Parameters.AddWithValue("@FocusMode", photo.FocusMode); } else { cmd.Parameters.AddWithValue("@FocusMode", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.ExposureBiasValue)) { cmd.Parameters.AddWithValue("@ExposureBiasValue", photo.ExposureBiasValue); } else { cmd.Parameters.AddWithValue("@ExposureBiasValue", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.XResolution)) { cmd.Parameters.AddWithValue("@XResolution", photo.XResolution); } else { cmd.Parameters.AddWithValue("@XResolution", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.YResolution)) { cmd.Parameters.AddWithValue("@YResolution", photo.YResolution); } else { cmd.Parameters.AddWithValue("@YResolution", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.ResolutionUnit)) { cmd.Parameters.AddWithValue("@ResolutionUnit", photo.ResolutionUnit); } else { cmd.Parameters.AddWithValue("@ResolutionUnit", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.Photographer)) { cmd.Parameters.AddWithValue("@Photographer", photo.Photographer); } else { cmd.Parameters.AddWithValue("@Photographer", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.ExposureProgram)) { cmd.Parameters.AddWithValue("@ExposureProgram", photo.ExposureProgram); } else { cmd.Parameters.AddWithValue("@ExposureProgram", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.ThumbnailOrientation)) { cmd.Parameters.AddWithValue("@ThumbnailOrientation", photo.ThumbnailOrientation); } else { cmd.Parameters.AddWithValue("@ThumbnailOrientation", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.ThumbnailXRes)) { cmd.Parameters.AddWithValue("@ThumbnailXRes", photo.ThumbnailXRes); } else { cmd.Parameters.AddWithValue("@ThumbnailXRes", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.ThumbnailYRes)) { cmd.Parameters.AddWithValue("@ThumbnailYRes", photo.ThumbnailYRes); } else { cmd.Parameters.AddWithValue("@ThumbnailYRes", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.ThumbnailResUnit)) { cmd.Parameters.AddWithValue("@ThumbnailResUnit", photo.ThumbnailResUnit); } else { cmd.Parameters.AddWithValue("@ThumbnailResUnit", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.ThumbnailOffset)) { cmd.Parameters.AddWithValue("@ThumbnailOffset", photo.ThumbnailOffset); } else { cmd.Parameters.AddWithValue("@ThumbnailOffset", DBNull.Value); }
                    if (!string.IsNullOrEmpty(photo.ThumbnailLength)) { cmd.Parameters.AddWithValue("@ThumbnailLength", photo.ThumbnailLength); } else { cmd.Parameters.AddWithValue("@ThumbnailLength", DBNull.Value); }
                    #endregion

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    Console.WriteLine(JsonConvert.SerializeObject(photo, Formatting.Indented));
                }
            }
        }

    }
}
