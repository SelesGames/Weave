using System;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Reactive.Linq;
using System.IO;

namespace weave
{
    public class SlowLoadingImageControl : Control
    {
        UIElement LayoutRoot;
        ImageBrush ImageBrush;
        //SerialDisposable disposable = new SerialDisposable();

        //static ImageCache cache = new ImageCache(4000);

        public SlowLoadingImageControl()
        {
            DefaultStyleKey = typeof(SlowLoadingImageControl);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            LayoutRoot = base.GetTemplateChild("LayoutRoot") as UIElement;
            ImageBrush = base.GetTemplateChild("ImageBrush") as ImageBrush;

            if (ImageBrush != null && ImageBrush.ImageSource != null && ImageUrl != null)
                ((BitmapImage)ImageBrush.ImageSource).UriSource = ImageUrl;
            //if (Source != null && ImageBrush != null)
            //    ImageBrush.ImageSource = Source;
            //    UpdateImageSource(Source);
            //else if (ImageUrlString != null)
            //    UpdateImageUrlString(ImageUrlString);
        }

        //void BeginImageUpdate()
        //{
        //    //disposable.Disposable = null;
        //    if (ImageBrush.ImageSource is BitmapImage)
        //    {
        //        var bitmap = (BitmapImage)ImageBrush.ImageSource;
        //        bitmap.UriSource = null;
        //        bitmap = null;
        //    }
        //    ImageBrush.ImageSource = null;
        //    LayoutRoot.Visibility = Visibility.Visible;
        //}

        //void UpdateImageSource(ImageSource image)
        //{
        //    BeginImageUpdate();

        //    disposable.Disposable = ImageBrush.ImageOpenOrFailed().Subscribe(o =>
        //    {
        //        try
        //        {
        //            if (o.IsFaulted)
        //                LayoutRoot.Visibility = Visibility.Collapsed;
        //        }
        //        catch { }
        //    });

        //    ImageBrush.ImageSource = image;
        //}

        //void UpdateImageUrlString(string imageUrlString)
        //{
        //    BeginImageUpdate();

        //    cache.GetImageAsync(imageUrlString)
        //            .SafelySubscribe(result =>
        //            {
        //                if (!result.WasCached)
        //                {
        //                    disposable.Disposable = ImageBrush.ImageOpenOrFailed().Subscribe(o =>
        //                    {
        //                        try
        //                        {
        //                            if (o.IsFaulted)
        //                                LayoutRoot.Visibility = Visibility.Collapsed;
        //                        }
        //                        catch { }
        //                    });                        
        //                }
                            
        //                ImageBrush.ImageSource = result.Image;

        //                //if (result.WasCached)
        //                //    LayoutRoot.InvalidateArrange();
        //            }, ex => LayoutRoot.Visibility = Visibility.Collapsed);
        //}




        #region Source property, for taking in an ImageSource regularly

        /// <summary>
        /// Identifies the Source DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(SlowLoadingImageControl), null);//, new PropertyMetadata(OnSourceChanged));

        /// <summary>
        /// Gets or sets the Image source.
        /// </summary>
        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        //static void OnSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        //{
        //    var control = (SlowLoadingImageControl)obj;

        //    if (e.NewValue is ImageSource && control.LayoutRoot != null && control.ImageBrush != null)
        //    {
        //        var image = (ImageSource)e.NewValue;
        //        control.UpdateImageSource(image);
        //    }
        //}

        #endregion




        //#region ImageUrlString - high performance and uses an image resizer

        public static readonly DependencyProperty ImageUrlProperty =
            DependencyProperty.Register("ImageUrl", typeof(Uri), typeof(SlowLoadingImageControl), null);//new PropertyMetadata(OnImageUrlStringChanged));


        public Uri ImageUrl
        {
            get { return (Uri)GetValue(ImageUrlProperty); }
            set { SetValue(ImageUrlProperty, value); }
        }

        //static void OnImageUrlStringChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        //{
        //    var control = (SlowLoadingImageControl)obj;

        //    if (e.NewValue is string && control.LayoutRoot != null && control.ImageBrush != null)
        //    {
        //        var imageUrlString = (string)e.NewValue;
        //        control.UpdateImageUrlString(imageUrlString);
        //    }
        //}

        //#endregion
    }
}
