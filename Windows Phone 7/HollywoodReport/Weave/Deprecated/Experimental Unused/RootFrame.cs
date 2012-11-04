using System.Windows;
using Microsoft.Phone.Controls;

namespace weave
{
    [TemplatePart(Name = "DetailsOverlay", Type = typeof(UIElement))]
    public class PhoneApplicationFrameWithOverlays : PhoneApplicationFrame
    {
        ArticlePopupWindow articlePopup;

        public PhoneApplicationFrameWithOverlays()
        {
            DefaultStyleKey = typeof(PhoneApplicationFrameWithOverlays);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            articlePopup = base.GetTemplateChild("DetailsOverlay") as ArticlePopupWindow;
            articlePopup.Visibility = Visibility.Collapsed;
        }

        public void ShowDetails(NewsItem newsItem)
        {
            currentState = State.Details;
            articlePopup.DataContext = newsItem;
            articlePopup.Visibility = Visibility.Visible;
            articlePopup.PopupSB.Begin();
        }

        public void HideDetails()
        {
            currentState = State.Normal;
            articlePopup.Visibility = Visibility.Collapsed;
        }

        internal State currentState = State.Normal;

        public enum State
        {
            Normal,
            Details
        }
    }
}