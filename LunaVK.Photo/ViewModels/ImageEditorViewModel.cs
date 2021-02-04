using ExifLib;
using LunaVK.Core;
using LunaVK.Core.Framework;
using LunaVK.Core.Utils;
using LunaVK.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Media.Imaging;
using Windows.Foundation;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;


namespace LunaVK.Photo.ViewModels
{
    public class ImageEditorViewModel : ViewModelBase
    {
        //private static readonly int JPEG_QUALITY = 85;
        private static readonly string NormalFilterName = "Normal";
        private Guid _sessionId;
        private SessionEffects _sessionEffectsInfo;
        private string _albumId;
        private int _seqNo;
        private ImageEffectsInfo _currentEffects;
        private Size _viewportSize;
        private bool _applyingEffects;
        private WriteableBitmap _originalImage;
        private string _orImAlbumId;
        private int _orImSeqNo;
//        private MediaLibrary _ml;
 //       private PictureAlbum _album;

        public Size ViewportSize
        {
            get
            {
                return this._viewportSize;
            }
        }

        public SolidColorBrush CropBrush
        {
            get
            {
                return this.BrushByBool(!this.CropApplied);
            }
        }

        public bool CropApplied
        {
            get
            {
                if (this._currentEffects.CropRect == null)
                    return this._currentEffects.RotateAngle != 0.0;
                return true;
            }
        }

        public SolidColorBrush TextBrush
        {
            get
            {
                return this.BrushByBool(!this.TextApplied);
            }
        }

        public bool TextApplied
        {
            get
            {
                return !string.IsNullOrEmpty(this._currentEffects.Text);
            }
        }

        public SolidColorBrush FilterBrush
        {
            get
            {
                return this.BrushByBool(!this.FilterApplied);
            }
        }

        public bool FilterApplied
        {
            get
            {
                return this._currentEffects.Filter != "Normal";
            }
        }

        public SolidColorBrush ContrastBrush
        {
            get
            {
                return this.BrushByBool(!this.ContrastApplied);
            }
        }

        public bool ContrastApplied
        {
            get
            {
                return this._currentEffects.Contrast;
            }
        }

        public bool ApplyingEffects
        {
            get
            {
                return this._applyingEffects;
            }
            set
            {
                this._applyingEffects = value;
                base.NotifyPropertyChanged<bool>((() => this.ApplyingEffects));
                base.NotifyPropertyChanged<Visibility>((() => this.ApplyingEffectsVisibility));
            }
        }

        public Visibility ApplyingEffectsVisibility
        {
            get
            {
                if (!this._applyingEffects)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public bool SuppressParseEXIF { get; set; }

        public ImageEditorViewModel()
        {
            this._sessionId = Guid.NewGuid();
            this.EnsureFolder();
            this._sessionEffectsInfo = new SessionEffects();
            this._viewportSize = /*ScaleFactor.GetScaleFactor() != 150 ? new Size((double)(480 * ScaleFactor.GetScaleFactor() / 100), (double)(800 * ScaleFactor.GetScaleFactor() / 100)) :*/ new Size(720.0, 1280.0);
            this._currentEffects = new ImageEffectsInfo();
        }

        private SolidColorBrush BrushByBool(bool b)
        {
            if (!b)
                return Application.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            return new SolidColorBrush(Colors.White);
        }

        public void CleanupSession()
        {
            IAsyncAction asyncAction = ThreadPool.RunAsync((o =>
            {
                try
                {
                    this.ResetCachedMediaLibrary();
                    this.DeleteSessionDir();
                }
                catch (Exception)
                {
                }
            }));
        }

        public void ResetCachedMediaLibrary()
        {
            /*
            try
            {
                if ((this._album != null))
                {
                    this._album.Dispose();
                    this._album = null;
                }
                if (this._ml == null)
                    return;
                this._ml.Dispose();
                this._ml = null;
            }
            catch (Exception)
            {
            }
            */
        }

        public ImageEffectsInfo GetImageEffectsInfo(string albumId, int seqNo)
        {
            return this._sessionEffectsInfo.GetImageEffectsInfo(albumId, seqNo);
        }

        public void SetCurrentPhoto(string albumId, int seqNo)
        {
            this._albumId = albumId;
            this._seqNo = seqNo;
            this._currentEffects = this._sessionEffectsInfo.GetImageEffectsInfo(albumId, seqNo);
            this.CallPropertyChanged();
        }

        private void CallPropertyChanged()
        {
            base.NotifyPropertyChanged(nameof( this.ContrastBrush));
            base.NotifyPropertyChanged(nameof(this.FilterBrush));
            base.NotifyPropertyChanged(nameof(this.TextBrush));
            base.NotifyPropertyChanged(nameof(this.CropBrush));
        }

        public List<ImageEffectsInfo> GetAppliedEffects()
        {
            return this._sessionEffectsInfo.GetApplied();
        }

        public Stream GetImageStream(string albumId, int seqNo, bool preview = false)
        {
            ImageEffectsInfo imageEffectsInfo1 = this._sessionEffectsInfo.GetImageEffectsInfo(albumId, seqNo);
            if (!preview && imageEffectsInfo1.AppliedAny)
            {
                string pathForEffects = this.GetPathForEffects(albumId, seqNo);
                using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream storageFileStream = storeForApplication.OpenFile(pathForEffects, (FileMode)3, (FileAccess)1))
                    {
                        MemoryStream exifStream = new MemoryStream();
                        if (imageEffectsInfo1.Exif != null)
                            exifStream = new MemoryStream(this.ResetOrientation(imageEffectsInfo1.ParsedExif.OrientationOffset, imageEffectsInfo1.Exif, imageEffectsInfo1.ParsedExif.LittleEndian));
                        MemoryStream memoryStream = ImagePreprocessor.MergeExif((Stream)storageFileStream, exifStream);
                        exifStream.Dispose();
                        return memoryStream;
                    }
                }
            }
            else
            {
                /*
                //Stopwatch stopwatch = Stopwatch.StartNew();
                Picture galleryImage = this.GetGalleryImage(albumId, seqNo);
                if (preview)
                {
                    Stream thumbnail = null;
                    try
                    {
                        thumbnail = galleryImage.GetThumbnail();
                        galleryImage.Dispose();
                        //stopwatch.Stop();
                    }
                    catch//bugfix?
                    {

                    }


                    return thumbnail;
                }
                ImageEffectsInfo imageEffectsInfo2 = this.GetImageEffectsInfo(albumId, seqNo);
                Stream image = galleryImage.GetImage();
                if (imageEffectsInfo2.Exif == null && !this.SuppressParseEXIF)
                {
                    //Stopwatch.StartNew();
                    MemoryStream exifStream;
                    ImagePreprocessor.PatchAwayExif(image, out exifStream);
                    image.Position = 0L;
                    imageEffectsInfo2.Exif = new byte[exifStream.Length];
                    exifStream.Read(imageEffectsInfo2.Exif, 0, (int)exifStream.Length);
                    JpegInfo info = new ExifReader(image).info;
                    imageEffectsInfo2.ParsedExif = info;
                    image.Position = 0L;
                }
                //stopwatch.Stop();
                galleryImage.Dispose();


                
                return image;
                */
                return null;
            }
        }

        private byte[] ResetOrientation(long p, byte[] exifData, bool littleEndian)
        {
            return ImagePreprocessor.ResetOrientation(p, exifData, littleEndian);
        }

        public BitmapSource GetBitmapSource(string albumId, int seqNo, bool allowBackgroundCreation = true)
        {
            Stream imageStream = this.GetImageStream(albumId, seqNo, false);
            ImageEffectsInfo imageEffectsInfo = this._sessionEffectsInfo.GetImageEffectsInfo(albumId, seqNo);
            if (!imageEffectsInfo.AppliedAny && imageEffectsInfo.ParsedExif != null && (imageEffectsInfo.ParsedExif.Orientation == ExifOrientation.TopRight || imageEffectsInfo.ParsedExif.Orientation == ExifOrientation.BottomLeft || imageEffectsInfo.ParsedExif.Orientation == ExifOrientation.BottomRight))
                return (BitmapSource)this.ReadOriginalImage(albumId, seqNo);
            BitmapImage bitmapImage1 = new BitmapImage();
            if (allowBackgroundCreation)
                bitmapImage1.CreateOptions = ((BitmapCreateOptions)16);
            else
                bitmapImage1.CreateOptions = ((BitmapCreateOptions)0);
            BitmapImage bitmapImage2 = bitmapImage1;
            Size viewportSize = this.ViewportSize;
            
            int height = (int)viewportSize.Height;
            bitmapImage2.DecodePixelHeight = height;
            bitmapImage1.SetSource(imageStream.AsRandomAccessStream());
            return (BitmapSource)bitmapImage1;
        }

        public void SetCrop(double rotate, CropRegion rect, WriteableBitmap imSource, Action<BitmapSource> callback)
        {
            if (this.ApplyingEffects)
            {
                return;
            }
            this.ApplyingEffects = true;
            //Deployment.Current.Dispatcher.BeginInvoke(delegate
            //{
                WriteableBitmap imSource2 = imSource;
            /*
                Picture galleryImage = this.GetGalleryImage(this._albumId, this._seqNo);
                bool flag = false;
                double num = this.GetCorrectImageSize(galleryImage, this._albumId, this._seqNo, out flag).Width / (double)imSource2.PixelWidth;
                galleryImage.Dispose();
                WriteableBitmap writeableBitmap = imSource2.Crop((int)((double)rect.X / num), (int)((double)rect.Y / num), (int)((double)rect.Width / num), (int)((double)rect.Height / num));
                Rect rect2 = RectangleUtils.ResizeToFit(new Rect(default(Point), this.ViewportSize), new Size((double)writeableBitmap.PixelWidth, (double)writeableBitmap.PixelHeight));
                WriteableBitmap writeableBitmap2 = writeableBitmap.Resize((int)rect2.Width, (int)rect2.Height, Interpolation.Bilinear);
                ImageEditorViewModel.SaveWB(writeableBitmap2, this.GetPathForCrop());
                this._currentEffects.CropRect = rect;
                this._currentEffects.RotateAngle = rotate;
                Action<WriteableBitmap> arg_167_2 = delegate (WriteableBitmap bitmap)
                {
                    callback.Invoke(bitmap);
                    this.ApplyingEffects = false;
                };

                this.ApplyEffects(writeableBitmap2, arg_167_2);
                */
                this.CallPropertyChanged();
          //  });
        }

        public void ResetCrop(Action<BitmapSource> callback)
        {
            if (this.ApplyingEffects)
                return;
            this.ApplyingEffects = true;
            this._currentEffects.RotateAngle = 0.0;
            this._currentEffects.CropRect = null;
            this.ApplyEffects(null, (Action<WriteableBitmap>)(bitmap =>
            {
                callback((BitmapSource)bitmap);
                this.ApplyingEffects = false;
            }));
            this.CallPropertyChanged();
        }

        public void SetResetFilter(string filterName, Action<BitmapSource> callback)
        {
            if (this.ApplyingEffects)
                return;
            this.ApplyingEffects = true;
            this._currentEffects.Filter = filterName;
            this.ApplyEffects(null, (Action<WriteableBitmap>)(bitmap =>
            {
                callback((BitmapSource)bitmap);
                this.ApplyingEffects = false;
            }));
            this.CallPropertyChanged();
        }

        public void SetResetContrast(bool set, Action<BitmapSource> callback)
        {
            if (this.ApplyingEffects)
                return;
            this.ApplyingEffects = true;
            this._currentEffects.Contrast = set;
            this.ApplyEffects(null, (Action<WriteableBitmap>)(bitmap =>
            {
                callback((BitmapSource)bitmap);
                this.ApplyingEffects = false;
            }));
            this.CallPropertyChanged();
        }

        public void SetResetText(string text, Action<BitmapSource> callback)
        {
            if (this.ApplyingEffects)
                return;
            this.ApplyingEffects = true;
            text = (text ?? "").Trim();
            this._currentEffects.Text = text;
            this.ApplyEffects(null, (Action<WriteableBitmap>)(bitmap =>
            {
                callback((BitmapSource)bitmap);
                this.ApplyingEffects = false;
            }));
            this.CallPropertyChanged();
        }

        private void ApplyEffects(WriteableBitmap croppedResizedWB, Action<WriteableBitmap> callback)
        {
            Stopwatch.StartNew();
            if (croppedResizedWB == null)
            {
                if (this._currentEffects.CropRect != null || this._currentEffects.RotateAngle != 0.0)
                {
                    croppedResizedWB = this.ReadCroppedImage();
                }
                else
                {
                    croppedResizedWB = this.ReadOriginalImage("", -1);
                }
            }
            croppedResizedWB = this.ApplyContrastIfNeeded(croppedResizedWB);
            this.ApplyFilterIfNeeded(croppedResizedWB, delegate (WriteableBitmap filteredWB)
            {
                Execute.ExecuteOnUIThread(delegate
                {
                    filteredWB = this.ApplyTextIfNeeded(filteredWB);
                    ImageEditorViewModel.SaveWB(filteredWB, this.GetPathForEffects(this._albumId, this._seqNo));
                    callback.Invoke(filteredWB);
                    croppedResizedWB = null;
                    filteredWB = null;
                    GC.Collect();
                });
            });
        }


        private WriteableBitmap ApplyTextIfNeeded(WriteableBitmap wb)
        {
            if (string.IsNullOrWhiteSpace(this._currentEffects.Text))
                return wb;
            WriteableBitmap writeableBitmap = wb;//new WriteableBitmap(wb);


            

            double fontSize;
            double yTranlsation;
            this.GetFontSizeAndYTranslation(((BitmapSource)wb).PixelWidth, ((BitmapSource)wb).PixelHeight, out fontSize, out yTranlsation);
            TextBlock textBlock1 = ImageEditorViewModel.CreateTextBlock(this._currentEffects.Text, fontSize);
            textBlock1.Foreground = ((Brush)new SolidColorBrush(Colors.Black));
            ((UIElement)textBlock1).Opacity = 0.6;
            ((FrameworkElement)textBlock1).Width = ((double)((BitmapSource)wb).PixelWidth - yTranlsation * 2.0);
            TextBlock textBlock2 = ImageEditorViewModel.CreateTextBlock(this._currentEffects.Text, fontSize);
            ((FrameworkElement)textBlock2).Width = ((double)((BitmapSource)wb).PixelWidth - yTranlsation * 2.0);
            Size size;
            
            size = new Size((double)((BitmapSource)wb).PixelWidth, (double)((BitmapSource)wb).PixelHeight);
            var rectangle1 = new Windows.UI.Xaml.Shapes.Rectangle();
            var rectangle2 = rectangle1;
            GradientStopCollection gradientStopCollection = new GradientStopCollection();
            GradientStop gradientStop1 = new GradientStop();
            Color color1 = new Color();
            
            color1.A = 0;
            Color color2 = color1;
            gradientStop1.Color = color2;
            gradientStopCollection.Add(gradientStop1);
            GradientStop gradientStop2 = new GradientStop();
            color1 = new Color();
            
            color1.A = byte.MaxValue;
            gradientStop2.Color = color1;
            gradientStop2.Offset = 1.0;
            gradientStopCollection.Add(gradientStop2);
            LinearGradientBrush linearGradientBrush = new LinearGradientBrush(gradientStopCollection, 90.0);
            rectangle2.Fill = ((Brush)linearGradientBrush);
            rectangle1.Opacity = 0.4;
            rectangle1.Height = (((FrameworkElement)textBlock2).ActualHeight + yTranlsation + yTranlsation);
            rectangle1.Width = ((double)((BitmapSource)wb).PixelWidth);
            Windows.UI.Xaml.Shapes.Rectangle rectangle3 = rectangle1;
            TranslateTransform translateTransform1 = new TranslateTransform();
            
            double num3 = ((Size)@size).Height - ((FrameworkElement)rectangle1).Height;
            translateTransform1.Y = num3;
//            writeableBitmap.Render((UIElement)rectangle3, (Transform)translateTransform1);
            TextBlock textBlock3 = textBlock1;
            TranslateTransform translateTransform2 = new TranslateTransform();
            double num4 = yTranlsation + 1.0;
            translateTransform2.X = num4;
            
            double num5 = ((Size)@size).Height - ((FrameworkElement)textBlock2).ActualHeight - yTranlsation;
            translateTransform2.Y = num5;
 //           writeableBitmap.Render((UIElement)textBlock3, (Transform)translateTransform2);
            TextBlock textBlock4 = textBlock2;
            TranslateTransform translateTransform3 = new TranslateTransform();
            double num6 = yTranlsation;
            translateTransform3.X = num6;
            
            double num7 = ((Size)@size).Height - ((FrameworkElement)textBlock2).ActualHeight - yTranlsation - 1.0;
            translateTransform3.Y = num7;
//            writeableBitmap.Render((UIElement)textBlock4, (Transform)translateTransform3);
            writeableBitmap.Invalidate();
            return writeableBitmap;
        }

        private void GetFontSizeAndYTranslation(int wbWidth, int wbHeight, out double fontSize, out double yTranlsation)
        {
            int num = Math.Min(wbHeight, wbWidth);
            fontSize = (double)num / 13.0;
            fontSize = Math.Max(14.0, fontSize);
            yTranlsation = fontSize / 2.0;
        }

        private void ApplyFilterIfNeeded(WriteableBitmap wb, Action<WriteableBitmap> callback)
        {
            if (this._currentEffects.Filter == ImageEditorViewModel.NormalFilterName || string.IsNullOrEmpty(this._currentEffects.Filter))
                callback(wb);
            /*
            else if (FilterStage.IsRendering)
            {
                callback(wb);
            }
            else
            {
                string key = FilterStage.CreateKey(this._currentEffects.GetUniqueKeyForFiltering(), ((BitmapSource)wb).PixelWidth, ((BitmapSource)wb).PixelHeight, (int[])null, false);
                Filter filter = (Filter)Enum.Parse(typeof(Filter), this._currentEffects.Filter);
                FilterStage.ApplyFilter(wb, key, filter, callback, (res => callback(wb)));
            }*/
        }

        private WriteableBitmap ApplyContrastIfNeeded(WriteableBitmap wb)
        {
 //           if (this._currentEffects.Contrast)
 //               return wb.Convolute(WriteableBitmapExtensions.KernelSharpen3x3);
            return wb;
        }

        public WriteableBitmap ReadOriginalImage(string albumId = "", int seqNo = -1)
        {
            string str = this._albumId;
            int num1 = this._seqNo;
            if (albumId != "" && seqNo != -1)
            {
                str = albumId;
                num1 = seqNo;
            }
            /*
            if (this._originalImage != null && this._orImAlbumId == str && this._orImSeqNo == num1)
                return this._originalImage.Clone();
            Stopwatch stopwatch = Stopwatch.StartNew();

            Picture galleryImage = this.GetGalleryImage(str, num1);
            int num2 = galleryImage.Width;
            int num3 = galleryImage.Height;
            //if (!MemoryInfo.IsLowMemDevice)
           // {
                int num4 = galleryImage.Width * galleryImage.Height;
            if (num4 > VKConstants.ResizedImageSize)
            {
                double num5 = Math.Sqrt((double)num4 / (double)VKConstants.ResizedImageSize);
                num2 = (int)Math.Round((double)galleryImage.Width / num5);
                num3 = (int)Math.Round((double)galleryImage.Height / num5);
            }
            */
            //// }
            //else
            //  {
            /*
            Size viewportSize = this.ViewportSize;

            double width = ((Size)@viewportSize).Width;
            viewportSize = this.ViewportSize;

            double height = ((Size)@viewportSize).Height;
            Rect fit = RectangleUtils.ResizeToFit(new Rect(0.0, 0.0, width, height), new Size((double)galleryImage.Width, (double)galleryImage.Height));
            double num6 = 300.0;


            double num7 = Math.Min(((Rect)@fit).Width, ((Rect)@fit).Height);
            double num8 = 1.0;
            if (num7 < num6)
                num8 = num6 / num7;

            num2 = (int)(((Rect)@fit).Width * num8);

            num3 = (int)(((Rect)@fit).Height * num8);
            */
            //  }
            /*
            WriteableBitmap wb = new WriteableBitmap(0, 0);// PictureDecoder.DecodeJpeg(galleryImage.GetImage(), num2, num3);
        WriteableBitmap bmp = this.RotateIfNeeded(str, num1, wb);
        if (albumId == "")
        {
            this._originalImage = bmp.Clone();
            this._orImAlbumId = str;
            this._orImSeqNo = num1;
        }
        stopwatch.Stop();
        galleryImage.Dispose();
        return bmp;
        */
            return null;
        }

        public WriteableBitmap RotateIfNeeded(string aId, int sNo, WriteableBitmap wb)
        {
            ImageEffectsInfo imageEffectsInfo = this._sessionEffectsInfo.GetImageEffectsInfo(aId, sNo);
            if (imageEffectsInfo.ParsedExif != null)
            {
                /*
                switch (imageEffectsInfo.ParsedExif.Orientation)
                {
                    case ExifOrientation.BottomRight:
                        wb = wb.Rotate(180);
                        break;
                    case ExifOrientation.TopRight:
                        wb = wb.Rotate(90);
                        break;
                    case ExifOrientation.BottomLeft:
                        wb = wb.Rotate(270);
                        break;
                }
                */
            }
            return wb;
        }

        private WriteableBitmap ReadCroppedImage()
        {
            string pathForCrop = this.GetPathForCrop();
            using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream storageFileStream1 = storeForApplication.OpenFile(pathForCrop, (FileMode)3, (FileAccess)1))
                {
                    int num1 = 0;
                    int num2 = 0;
                    if (this._currentEffects.CropRect != null)
                    {
                        num1 = this._currentEffects.CropRect.Width;
                        num2 = this._currentEffects.CropRect.Height;
                    }
                    BitmapImage bitmapImage = new BitmapImage();
                    Rect fit = RectangleUtils.ResizeToFit(new Rect(new Point(), this.ViewportSize), new Size((double)num1, (double)num2));
                    
                    int height = (int)((Rect)@fit).Height;
                    bitmapImage.DecodePixelHeight = height;
                    IsolatedStorageFileStream storageFileStream2 = storageFileStream1;
                    ((BitmapSource)bitmapImage).SetSource(storageFileStream2.AsRandomAccessStream());
                    return null;//new WriteableBitmap((BitmapSource)bitmapImage);
                }
            }
        }
        /*
        public Picture GetGalleryImage(string albumId, int seqNo)
        {
            if (this._ml == null)
            {
                this._ml = new MediaLibrary();
            }
            if (this._album == null || this._album.Name != albumId)
            {
                if (this._album != null)
                {
                    this._album.Dispose();
                }
                this._album = Enumerable.FirstOrDefault<PictureAlbum>(this._ml.RootPictureAlbum.Albums, (PictureAlbum a) => a.Name == albumId);
            }
            if (this._album != null && this._album.Pictures.Count > seqNo)
            {
                Picture picture = this._album.Pictures[seqNo];
                if (picture != null)
                {
                    return picture;
                }
            }
            return null;
        }


        public Size GetCorrectImageSize(Picture p, string albumId, int seqNo, out bool rotated90)
        {
            ImageEffectsInfo imageEffectsInfo = this.GetImageEffectsInfo(albumId, seqNo);
            rotated90 = false;
            if (imageEffectsInfo.ParsedExif == null || imageEffectsInfo.ParsedExif.Orientation != ExifOrientation.TopRight && imageEffectsInfo.ParsedExif.Orientation != ExifOrientation.BottomLeft)
                return new Size((double)p.Width, (double)p.Height);
            rotated90 = true;
            return new Size((double)p.Height, (double)p.Width);
        }
        */
        private string GetPathForCrop()
        {
            return this._sessionId.ToString() + "/" + this._albumId.Replace(" ", "") + this._seqNo + "_crop";
        }

        private string GetPathForEffects(string albumId, int seqNo)
        {
            return this._sessionId.ToString() + "/" + albumId.Replace(" ", "") + seqNo + "_effects";
        }

        private static void SaveWB(WriteableBitmap wb, string path)
        {
            try
            {
                using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                {
//                    using (IsolatedStorageFileStream storageFileStream = storeForApplication.OpenFile(path, (FileMode)2, (FileAccess)2))
//                        System.Windows.Media.Imaging.Extensions.SaveJpeg(wb, (Stream)storageFileStream, ((BitmapSource)wb).PixelWidth, ((BitmapSource)wb).PixelHeight, 0, VKConstants.JPEGQUALITY);
                }
            }
            catch (Exception)
            {
            }
        }

        private static TextBlock CreateTextBlock(string textStr, double fontSize)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = textStr;
            textBlock.TextWrapping = ((TextWrapping)2);
            textBlock.TextAlignment = ((TextAlignment)0);
            textBlock.FontSize = fontSize;
            textBlock.FontFamily = new FontFamily("Lobster.ttf#Lobster 1.4");
            SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.White);
            textBlock.Foreground = ((Brush)solidColorBrush);
            ((FrameworkElement)textBlock).HorizontalAlignment = ((HorizontalAlignment)1);
            ((FrameworkElement)textBlock).VerticalAlignment = ((VerticalAlignment)2);
            return textBlock;
        }

        private void EnsureFolder()
        {
            using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                storeForApplication.CreateDirectory(this._sessionId.ToString());
        }

        private void DeleteSessionDir()
        {
            try
            {
                using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    foreach (string fileName in storeForApplication.GetFileNames(this._sessionId.ToString() + "\\*"))
                        storeForApplication.DeleteFile(this._sessionId.ToString() + "\\" + fileName);
                    storeForApplication.DeleteDirectory(this._sessionId.ToString());
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
