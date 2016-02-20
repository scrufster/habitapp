using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Microsoft.Win32;

namespace HabitApp
{
    public partial class MainWindow : Window
    {
        #region Variables

        private LeafCover myCoverInfo = null;
        private string lastUsed_SaveFileDirectory = "";

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            bwCutOff_Slider.Value = LeafCover.BWCutOff;

            displayPref_ComboBox.SelectedIndex = (int)LeafCover.DisplayPreference;

            UpdateBWCutOffLabel();
        }

        #region Window events

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && Keyboard.IsKeyDown(Key.C))
            {
                if (myCoverInfo != null)
                {
                    Clipboard.SetImage(imageCanvas.GetRenderDataBitmap());
                }
            }
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DrawImage();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ApplyAspectRatio();

            DrawImage();
        }

        #endregion

        private void ApplyAspectRatio()
        {
            if (myCoverInfo != null)
            {
                double maxWidth = photo_GroupBox.ActualWidth - 25;
                double maxHeight = photo_GroupBox.ActualHeight - 100;

                double ratioX = maxWidth / (double)myCoverInfo.ImageOriginal.PixelWidth;
                double ratioY = maxHeight / (double)myCoverInfo.ImageOriginal.PixelHeight;

                //use whichever multiplier is smaller:
                double ratio = ratioX < ratioY ? ratioX : ratioY;

                // now we can get the new height and width:
                int newHeight = (int)(myCoverInfo.ImageOriginal.PixelHeight * ratio);
                int newWidth = (int)(myCoverInfo.ImageOriginal.PixelWidth * ratio);

                imageCanvas.Width = newWidth;
                imageCanvas.Height = newHeight;
            }
        }

        private void DrawImage()
        {
            if (IsLoaded)
            {
                imageCanvas.RemoveAllVisuals();

                DrawingVisual dV = new DrawingVisual();

                using (DrawingContext dC = dV.RenderOpen())
                {
                    if (myCoverInfo != null)
                    {
                        dC.DrawImage(showTwoColourImage_RadioButton.IsChecked.Value ? myCoverInfo.ImageTwoColour : myCoverInfo.ImageOriginal,
                            new Rect(new Point(0, 0), new Point(imageCanvas.Width, imageCanvas.Height)));
                    }
                    else
                    {
                        dC.DrawRectangle(new SolidColorBrush(Color.FromRgb(241, 241, 241)), null, new Rect(new Point(0, 0), new Point(imageCanvas.ActualWidth, imageCanvas.ActualHeight)));
                        
                        FormattedText fText = new FormattedText("Drag and drop a photo here", new CultureInfo("en-us"), FlowDirection.LeftToRight, 
                            new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 15, new SolidColorBrush(Colors.DarkGray));

                        double textStartX = (imageCanvas.ActualWidth / 2) - (fText.Width / 2);
                        double textStartY = (imageCanvas.ActualHeight / 2) - (fText.Height / 2);

                        dC.DrawText(fText, new Point(textStartX, textStartY));
                    }
                }

                imageCanvas.AddVisual(dV);
            }
        }

        private void LoadImage(string fileName)
        {
            myCoverInfo = new LeafCover(fileName);

            //imageCanvas.Measure(photo_GroupBox.RenderSize);
            //imageCanvas.Arrange(new Rect(new Size(photo_GroupBox.ActualWidth - 25, photo_GroupBox.ActualHeight - 100)));

            ApplyAspectRatio();

            UpdateLeafCoverResults();

            if (!showOriginalImage_RadioButton.IsChecked.Value)
            {
                //this will trigger a re-draw itself:
                showOriginalImage_RadioButton.IsChecked = true;
            }
            else
            {
                DrawImage();
            }
        }

        private static void OpenDirectories(Window owner, List<string> fileNames)
        {
            if (fileNames.Count > 0)
            {
                //get the unique directories:
                List<string> directories = new List<string>(fileNames.Count);

                for (int i = 0; i < fileNames.Count; i++)
                {
                    string directory = Directory.GetParent(fileNames[i]).FullName;

                    if (!directories.Contains(directory) && Directory.Exists(directory))
                    {
                        directories.Add(directory);
                    }
                }
                
                if (MessageBox.Show(owner, string.Format("Do you want to open the output {0}?", directories.Count == 1 ? "directory" : "directories"),
                    "HabitApp Desktop", MessageBoxButton.YesNo, MessageBoxImage.Question)
                    == MessageBoxResult.Yes)
                {
                    for (int i = 0; i < directories.Count; i++)
                    {
                        try
                        {
                            if (Directory.Exists(directories[i]))
                            {
                                System.Diagnostics.Process.Start(directories[i]);
                            }
                        }
                        catch { }
                    }
                }
            }
        }

        private void ReDrawTwoColour(bool updateResults)
        {
            if (myCoverInfo != null)
            {
                myCoverInfo.CreateTwoColour();

                if (updateResults)
                {
                    UpdateLeafCoverResults();
                }

                if (showTwoColourImage_RadioButton.IsChecked.Value)
                {
                    DrawImage();
                }
            }
        }

        private void UpdateBWCutOffLabel()
        {
            bwCutOff_TextBlock.Text = string.Format("Black/white cut-off: ({0})", (int)bwCutOff_Slider.Value);
        }

        private void UpdateLeafCoverResults()
        {
            if (myCoverInfo != null)
            {
                results_TextBlock.Text = string.Format("{0}%", Math.Round(myCoverInfo.GetPercentageCoveredByLeaves(), 0));
            }
        }

        #region Events
        
        private void BwCutOff_Slider_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left || e.Key == Key.Right)
            {
                ReDrawTwoColour(true);
            }
        }

        private void BwCutOff_Slider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            ReDrawTwoColour(true);
        }

        private void BWCutOff_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                LeafCover.BWCutOff = (int)bwCutOff_Slider.Value;

                UpdateBWCutOffLabel();
            }
        }

        private void DisplyPref_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && displayPref_ComboBox.SelectedIndex != -1)
            {
                LeafCover.DisplayPreference = (LeafCover.DisplayPref)displayPref_ComboBox.SelectedIndex;

                ReDrawTwoColour(false);
            }
        }
        
        private void ImageCanvas_Drop(object sender, DragEventArgs e)
        {
            DataObject droppedObject = (DataObject)e.Data;

            //if what was dragged contains files, load them:
            if (droppedObject.ContainsFileDropList())
            {
                //get the names of the dropped files:
                System.Collections.Specialized.StringCollection droppedStringCollection = droppedObject.GetFileDropList();

                LoadImage(droppedStringCollection[0]);
            }
        }

        private void ImageDisplay_RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            DrawImage();
        }

        private void OpenPhoto_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openPhotoDialog = new OpenFileDialog();
            openPhotoDialog.Filter = "All image files|*.bmp; *.jpg; *.jpeg; *.png; *.tif; *.tiff";

            if (openPhotoDialog.ShowDialog().Value)
            {
                LoadImage(openPhotoDialog.FileName);
            }
        }
 
        private void SaveCoverageImage_Button_Click(object sender, RoutedEventArgs e)
        {
            if (myCoverInfo != null)
            {
                SaveFileDialog saveImageDialog = new SaveFileDialog();
                saveImageDialog.Filter = "Bitmap|*.bmp|JPG file|*.jpg|PNG file|*.png|TIF file|*.tif";
                saveImageDialog.FilterIndex = 2;

                if (!lastUsed_SaveFileDirectory.Equals(""))
                {
                    saveImageDialog.InitialDirectory = lastUsed_SaveFileDirectory;
                }

                if (saveImageDialog.ShowDialog().Value)
                {
                    //the image encoder:
                    BitmapEncoder imageEncoder = null;

                    switch (saveImageDialog.FilterIndex)
                    {
                        case 1: imageEncoder = new BmpBitmapEncoder(); break;
                        case 3: imageEncoder = new PngBitmapEncoder(); break;
                        case 4: imageEncoder = new TiffBitmapEncoder(); break;
                        default: imageEncoder = new JpegBitmapEncoder(); break;
                    }

                    imageEncoder.Frames.Add(BitmapFrame.Create(myCoverInfo.ImageTwoColour));

                    //save the image:
                    using (Stream s = File.Create(saveImageDialog.FileName))
                    {
                        imageEncoder.Save(s);
                    }

                    lastUsed_SaveFileDirectory = Path.GetDirectoryName(saveImageDialog.FileName);

                    OpenDirectories(this, new List<string> { saveImageDialog.FileName });
                }
            }
        }

        #endregion
    }
}