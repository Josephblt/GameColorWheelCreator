using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Text.RegularExpressions;

namespace GameColorWheelCreator
{
    public static class Utils
    {
        #region Extension Methods

        public static string ToHexValue(this Color color)
        {
            return "#" + color.R.ToString("X2") +
                         color.G.ToString("X2") +
                         color.B.ToString("X2");
        }

        public static string ToRGBValue(this Color color)
        {
            return color.R.ToString("000") + "; " +
                   color.G.ToString("000") + "; " +
                   color.B.ToString("000");
        }

        public static bool InvalidHexValue(this string hexValue)
        {
            return Regex.IsMatch(hexValue, @"[^#\da-fA-F]");
        }

        #endregion

        #region Helper Methods

        public static string GetOpenImageFilter()
        {
            StringBuilder allImageExtensions = new StringBuilder();
            var separator = "";
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            Dictionary<string, string> images = new Dictionary<string, string>();
            foreach (ImageCodecInfo codec in codecs)
            {
                var extensionSeparator = new string[] { ";" };
                var extensions = codec.FilenameExtension.Split(extensionSeparator, StringSplitOptions.None);

                allImageExtensions.Append(separator);
                allImageExtensions.Append(extensions[0]);
                separator = ";";

                images.Add(string.Format("{0} Files: ({1})", codec.FormatDescription, extensions[0]), extensions[0]);
            }

            StringBuilder sb = new StringBuilder();
            if (allImageExtensions.Length > 0)
                sb.AppendFormat("{0}|{1}", "All Images", allImageExtensions.ToString());

            images.Add("All Files", "*.*");
            foreach (KeyValuePair<string, string> image in images)
                sb.AppendFormat("|{0}|{1}", image.Key, image.Value);

            return sb.ToString();
        }

        public static string GetSaveImageFilter()
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            Dictionary<string, string> images = new Dictionary<string, string>();
            foreach (ImageCodecInfo codec in codecs)
            {
                var extensionSeparator = new string[] { ";" };
                var extensions = codec.FilenameExtension.Split(extensionSeparator, StringSplitOptions.None);

                images.Add(string.Format("{0} Files: ({1})", codec.FormatDescription, extensions[0]), extensions[0]);
            }

            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> image in images)
            {
                if (sb.Length > 0)
                    sb.Append("|");
                sb.AppendFormat("{0}|{1}", image.Key, image.Value);
            }

            return sb.ToString();
        }

        #endregion
    }
}