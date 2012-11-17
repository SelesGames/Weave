using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SelesGames.Phone.Controls
{
    public class ImageWithLabelButton : Button
    {
        Image image;

        public ImageWithLabelButton()
        {
            DefaultStyleKey = typeof(Button);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            image = GetTemplateChild("Image") as Image;

            if (image != null)
                image.Source = Source;
        }




        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(ImageWithLabelButton), new PropertyMetadata(OnSourceChanged));

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        static void OnSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var button = (ImageWithLabelButton)obj;

            if (e.NewValue is ImageSource && button.image != null)
            {
                var image = (ImageSource)e.NewValue;
                button.image.Source = image;
            }
        }
    }
}
