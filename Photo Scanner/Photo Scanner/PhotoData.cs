using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Photo_Scanner
{
    internal class PhotoData
    {
        /// <summary>
        /// P:\2018\IMG_3319.JPG
        /// </summary>
        public string FullPath { get; set; }
        /// <summary>
        /// Example: ftp://192.168.0.10/2016/2016-11-07/IMG_4943.JPG
        /// </summary>
        public string FtpPath { get; set; }
        /// <summary>
        /// \2018\SomeEvent\
        /// </summary>
        public string DirectoryPath { get; set; }
        /// <summary>
        /// IMG_3319.JPG
        /// </summary>
        public string FileName { get; set; }
        public string Thumbnail { get; set; }
        public string SmallVersion { get; set; }
        public string MediumVersion { get; set; }
        public string LargeVersion { get; set; }
        public string Extension { get; set; }
        public string Size { get; set; }
        /// <summary>
        /// Example of original Fri Jan 22 13:00:04 +10:00 2016
        /// </summary>
        public DateTime? FileModifiedDate { get; set; }
        /// <summary>
        /// Example of original 2018:02:10 13:49:27
        /// </summary>
        public DateTime? ExifIfd0DateTime { get; set; }
        /// <summary>
        /// Example of original 2018:02:10 13:49:27
        /// </summary>
        public DateTime? DateTimeOriginal { get; set; }
        /// <summary>
        /// Example of original 2018:02:10 13:49:27
        /// </summary>
        public DateTime? DateTimeDigitized { get; set; }
        /// <summary>
        /// Example of original 12345 pixels
        /// Height = Convert.ToInt32(tag.Description.Split(' ')[0]);
        /// DimensionUnit = tag.Description.Split(' ')[1];
        /// </summary>
        public string DimensionUnit { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        /// <summary>
        /// Example of original Right side, top (Rotate 90 CW)
        /// [Exif IFD0] Orientation = Top, left side (Horizontal / normal)
        /// </summary>
        public string Orientation { get; set; }
        public string Model { get; set; }
        public string Make { get; set; }
        public string BodySerialNumber { get; set; }
        public string LensSpecification { get; set; }
        public string LensModel { get; set; }
        public string LensSerialNumber { get; set; }
        /// <summary>
        /// Example of original 2016:01:22
        /// </summary>
        public string GpsDateStamp { get; set; }
        /// <summary>
        /// Example of original 02:59:57.000 UTC
        /// </summary>
        public string GpsTimeStamp { get; set; }
        public DateTime? GpsDateTimeStamp { get; set; }
        public string GpsAltitudeRef { get; set; }
        public string GpsAltitude { get; set; }
        public string GpsLongitudeRef { get; set; }
        public string GpsLongitude { get; set; }
        public string GpsLatitudeRef { get; set; }
        public string GpsLatitude { get; set; }
        public string GpsCoordinates { get; set; }
        /// <summary>
        /// Example of original f/1.8
        /// </summary>
        public string FNumber { get; set; }
        public string FocalLength { get; set; }
        public string Flash { get; set; }
        public string Aperture { get; set; }
        public string MaxAperture { get; set; }
        public string MinAperture { get; set; }
        public string ShutterSpeed { get; set; }
        public string Iso { get; set; }
        public string AutoIso { get; set; }
        public string BaseIso { get; set; }
        public string ExposureTime { get; set; }
        public string MeasuredEv { get; set; }
        public string ExposureMode { get; set; }
        public string WhiteBalanceMode { get; set; }
        public string FocusMode { get; set; }
        /// <summary>
        /// [Exif SubIFD] Exposure Bias Value = 0 EV
        /// </summary>
        public string ExposureBiasValue { get; set; }
        /// <summary>
        /// [Exif IFD0] X Resolution = 72 dots per inch
        /// [Exif IFD0] X Resolution = 350 dots per inch
        /// </summary>
        public string XResolution { get; set; }
        /// <summary>
        /// [Exif IFD0] y Resolution = 72 dots per inch
        /// [Exif IFD0] Y Resolution = 350 dots per inch
        /// </summary>
        public string YResolution { get; set; }
        public string ResolutionUnit { get; set; }
        /// <summary>
        /// [Exif IFD0] Artist = Tim Paananen
        /// </summary>
        public string Photographer { get; set; }
        /// <summary>
        /// [Exif SubIFD] Exposure Program = Aperture priority
        /// </summary>
        public string ExposureProgram { get; set; }
        public string ThumbnailOrientation { get; set; }
        public string ThumbnailXRes { get; set; }
        public string ThumbnailYRes { get; set; }
        public string ThumbnailResUnit { get; set; }
        public string ThumbnailOffset { get; set; }
        public string ThumbnailLength { get; set; }
    }
}
