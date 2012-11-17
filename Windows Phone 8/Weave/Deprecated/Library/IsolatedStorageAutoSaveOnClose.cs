using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;

namespace weave
{
    public class IsolatedStorageAutoSaveOnClose
    {
        public IsolatedStorageAutoSaveOnClose()
        {
            PhoneApplicationService.Current.Closing += (s, e) => Save();
            PhoneApplicationService.Current.Deactivated += (s, e) => Save();
        }

        void Save()
        {
            IsolatedStorageSettings.ApplicationSettings.Save();
        }
    }
}
