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

namespace Weave.UI.Controls
{
    [TemplatePart(Name = "LoginStateText", Type = typeof(TextBlock))]
    [TemplateVisualState(GroupName = "LoginStates", Name = "LoggedIn")]
    [TemplateVisualState(GroupName = "LoginStates", Name = "LoggedOut")]
    public class LoginButton : Button
    {
        Image image;
        TextBlock loginStateText;

        public LoginButton()
        {
            DefaultStyleKey = typeof(LoginButton);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            loginStateText = GetTemplateChild("LoginStateText") as TextBlock;

            SetLoginVisualState();
        }

        void SetLoginVisualState()
        {
            if (loginStateText == null)
                return;

            loginStateText.Text = IsLoggedIn ? LoggedInText : LoggedOutText;

            if (IsLoggedIn)
            {
                loginStateText.Text = LoggedInText;
                VisualStateManager.GoToState(this, "LoggedIn", true);
            }
            else
            {
                loginStateText.Text = LoggedOutText;
                VisualStateManager.GoToState(this, "LoggedOut", true);
            }
        }




        #region Dependency Properties

        public static readonly DependencyProperty LoggedInTextProperty = DependencyProperty.Register(
            "LoggedInText", 
            typeof(string), 
            typeof(LoginButton), 
            new PropertyMetadata("Logged in"));

        public string LoggedInText
        {
            get { return (string)GetValue(LoggedInTextProperty); }
            set { SetValue(LoggedInTextProperty, value); }
        }

        public static readonly DependencyProperty LoggedOutTextProperty = DependencyProperty.Register(
            "LoggedOutText",
            typeof(string),
            typeof(LoginButton),
            new PropertyMetadata("Logged out"));

        public string LoggedOutText
        {
            get { return (string)GetValue(LoggedOutTextProperty); }
            set { SetValue(LoggedOutTextProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", 
            typeof(ImageSource), 
            typeof(LoginButton), 
            new PropertyMetadata(OnSourceChanged));

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        static void OnSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var button = (LoginButton)obj;

            if (e.NewValue is ImageSource && button.image != null)
            {
                var image = (ImageSource)e.NewValue;
                button.image.Source = image;
            }
        }

        public static readonly DependencyProperty IsLoggedInProperty = DependencyProperty.Register(
            "IsLoggedIn",
            typeof(bool),
            typeof(LoginButton),
            new PropertyMetadata(OnIsLoggedInChanged));

        public bool IsLoggedIn
        {
            get { return (bool)GetValue(IsLoggedInProperty); }
            set { SetValue(IsLoggedInProperty, value); }
        }

        static void OnIsLoggedInChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var button = (LoginButton)obj;
            button.SetLoginVisualState();
        }

        #endregion
    }
}
