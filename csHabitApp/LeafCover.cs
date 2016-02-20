using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HabitApp
{
    public class LeafCover
    {
        #region Variables

        public WriteableBitmap ImageOriginal { get; private set; }
        public WriteableBitmap ImageTwoColour { get; private set; }

        private Int32Rect imageRect;
        private int stride = 0;

        public static DisplayPref DisplayPreference { get; set; }
        public static int BWCutOff { get; set; }

        #endregion

        static LeafCover()
        {
            BWCutOff = 150;
        }

        public LeafCover(string imageFileName)
        {
            ImageOriginal = new WriteableBitmap(new BitmapImage(new Uri(imageFileName)));

            stride = ImageOriginal.PixelWidth * (ImageOriginal.Format.BitsPerPixel / 8);
            imageRect = new Int32Rect(0, 0, ImageOriginal.PixelWidth, ImageOriginal.PixelHeight);
            
            CreateTwoColour();
        }

        public double GetPercentageCoveredByLeaves()
        {
            double totalPixels = ImageOriginal.PixelWidth * ImageOriginal.PixelHeight;

            byte[] pixels = GetPixels(false);

            double totalBlack = 0;

            for (int i = 0; i < pixels.Length; i += 4)
            {
                if (pixels[i] == 0)
                {
                    totalBlack++;
                }
            }

            return (totalBlack / totalPixels) * 100;
        }

        private byte[] GetPixels(bool fromOriginal = true)
        {
            byte[] pixels = new byte[stride * ImageOriginal.PixelHeight];

            if (fromOriginal)
            {
                ImageOriginal.CopyPixels(imageRect, pixels, stride, 0);
            }
            else
            {
                ImageTwoColour.CopyPixels(imageRect, pixels, stride, 0);
            }

            return pixels;
        }

        public void CreateTwoColour()
        {
            byte[] pixels = GetPixels();

            byte[] pixelsTwoColour = new byte[pixels.Length];

            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i] > BWCutOff)
                {
                    if (DisplayPreference == DisplayPref.BLACK_COLOUR)
                    {
                        pixelsTwoColour[i++] = pixels[i - 1];
                        pixelsTwoColour[i++] = pixels[i - 1];
                        pixelsTwoColour[i++] = pixels[i - 1];
                        pixelsTwoColour[i] = pixels[i];
                    }
                    else if (DisplayPreference == DisplayPref.COLOUR_WHITE)
                    {
                        pixelsTwoColour[i++] = 255;
                        pixelsTwoColour[i++] = 255;
                        pixelsTwoColour[i++] = 255;
                        pixelsTwoColour[i] = 255;
                    }
                    else
                    {
                        pixelsTwoColour[i++] = 255;
                        pixelsTwoColour[i++] = 255;
                        pixelsTwoColour[i++] = 255;
                        pixelsTwoColour[i] = 255;
                    }
                }
                else
                {
                    if (DisplayPreference == DisplayPref.BLACK_COLOUR)
                    {
                        pixelsTwoColour[i++] = 0;
                        pixelsTwoColour[i++] = 0;
                        pixelsTwoColour[i++] = 0;
                        pixelsTwoColour[i] = 255;
                    }
                    else if (DisplayPreference == DisplayPref.COLOUR_WHITE)
                    {
                        pixelsTwoColour[i++] = pixels[i - 1];
                        pixelsTwoColour[i++] = pixels[i - 1];
                        pixelsTwoColour[i++] = pixels[i - 1];
                        pixelsTwoColour[i] = pixels[i];
                    }
                    else
                    {
                        pixelsTwoColour[i++] = 0;
                        pixelsTwoColour[i++] = 0;
                        pixelsTwoColour[i++] = 0;
                        pixelsTwoColour[i] = 255;
                    }
                }
            }

            ImageTwoColour = new WriteableBitmap(ImageOriginal.PixelWidth, ImageOriginal.PixelHeight, ImageOriginal.DpiX, ImageOriginal.DpiY, PixelFormats.Bgra32, null);
            ImageTwoColour.WritePixels(imageRect, pixelsTwoColour, stride, 0);
        }

        public enum DisplayPref
        {
            BLACK_WHITE, BLACK_COLOUR, COLOUR_WHITE
        }
    }
}
