using Microsoft.Phone.Controls;
using System;
using System.Windows.Navigation;

namespace SelesGames.Phone
{
    public static class NavigationServiceExtensions
    {
        public static Exception TryGoBack(this NavigationService navService)
        {
            Exception result = null;

            try
            {
                if (navService.CanGoBack)
                    navService.GoBack();
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex); 
#endif
                result = ex;
            }

            return result;
        }

        public static Exception TryNavigate(this NavigationService navService, string uri)
        {
            if (string.IsNullOrEmpty(uri)) throw new ArgumentException("uri in NavigationServiceExtensions.TryNavigate");

            return TryNavigate(navService, new Uri(uri, UriKind.Relative));
        }

        public static Exception TryNavigate(this NavigationService navService, Uri uri)
        {
            Exception result = null;

            try
            {
                if (uri == null) throw new ArgumentNullException("uri in NavigationServiceExtensions.TryNavigate");
                if (navService == null) throw new ArgumentNullException("navService in NavigationServiceExtensions.TryNavigate");

                navService.Navigate(uri);
            }
            catch (Exception ex) 
            {
#if DEBUG
                Debug.WriteLine(ex); 
#endif
                result = ex;
            }

            return result;
        }

        public static Exception TryNavigate(this PhoneApplicationFrame frame, string uri)
        {
            if (string.IsNullOrEmpty(uri)) throw new ArgumentException("uri in NavigationServiceExtensions.TryNavigate");

            return TryNavigate(frame, new Uri(uri, UriKind.Relative));
        }


        public static Exception TryNavigate(this PhoneApplicationFrame frame, Uri uri)
        {
            Exception result = null;

            try
            {
                if (uri == null) throw new ArgumentNullException("uri in NavigationServiceExtensions.TryNavigate");
                if (frame == null) throw new ArgumentNullException("navService in NavigationServiceExtensions.TryNavigate");

                frame.Navigate(uri);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex); 
#endif
                result = ex;
            }

            return result;
        }
    }
}
