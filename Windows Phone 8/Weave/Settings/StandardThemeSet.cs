using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace weave
{
    public class StandardThemeSet : List<Theme>, INotifyPropertyChanged
    {
        PermanentState permanentState;
        int currentThemeIndex;
        public Theme CurrentTheme { get; private set; }

        public StandardThemeSet(Application app, PermanentState permanentState)
        {
            this.permanentState = permanentState;

            var foregroundBrush = app.Resources["PhoneForegroundBrush"] as SolidColorBrush;
            var backgroundBrush = app.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
            var accentBrush = app.Resources["PhoneAccentBrush"] as SolidColorBrush;
            var subtleBrush = app.Resources["PhoneSubtleBrush"] as SolidColorBrush;

            var complementarySet = accentBrush.Color.GetComplementary();
            var complementaryLighter = new SolidColorBrush(complementarySet.ComplementaryColorLighter);
            var complementaryDarker = new SolidColorBrush(complementarySet.ComplementaryColorDarker);

            var isDarkTheme = backgroundBrush.Color == Colors.Black;

            // add metro theme
            this.Add(new Theme
            {
                Name = "metro",
                TextBrush = foregroundBrush,
                BackgroundBrush = backgroundBrush,
                SubtleBrush = subtleBrush,
                AccentBrush = accentBrush,
                ComplementaryBrush = isDarkTheme ? complementaryLighter : complementaryDarker,
            });

            // add light reading theme
            this.Add(new Theme
            {
                Name = "day",
                TextBrush = new SolidColorBrush(Color.FromArgb(255, 10, 10, 10)),
                BackgroundBrush = new SolidColorBrush(Color.FromArgb(255, 242, 242, 242)),
                SubtleBrush = new SolidColorBrush(Color.FromArgb(255, 175, 170, 170)),
                AccentBrush = accentBrush,
                ComplementaryBrush = isDarkTheme ? complementaryLighter : complementaryDarker,
            });

            // add mint reading theme
            this.Add(new Theme
            {
                Name = "paper",
                TextBrush = new SolidColorBrush(Color.FromArgb(255, 30, 33, 32)),
                BackgroundBrush = new SolidColorBrush(Color.FromArgb(255, 230, 228, 210)),
                SubtleBrush = subtleBrush,
                AccentBrush = accentBrush,
                ComplementaryBrush = isDarkTheme ? complementaryLighter : complementaryDarker,
            });

            // add light reading theme
            this.Add(new Theme
            {
                Name = "night",
                TextBrush = new SolidColorBrush(Color.FromArgb(255, 250, 250, 250)),
                BackgroundBrush = new SolidColorBrush(Color.FromArgb(255, 18, 18, 30)),
                SubtleBrush = subtleBrush,
                AccentBrush = accentBrush,
                ComplementaryBrush = isDarkTheme ? complementaryLighter : complementaryDarker,
            });

            currentThemeIndex = 0;
        }

        public void UpdateCurrentThemeFromPermanentState()
        {
            if (string.IsNullOrEmpty(permanentState.CurrentTheme))
                SetDefaultTheme();

            else
                CurrentTheme = this.FirstOrDefault(o => permanentState.CurrentTheme.Equals(o.Name, StringComparison.OrdinalIgnoreCase));

            if (CurrentTheme == null)
                SetDefaultTheme();

            currentThemeIndex = this.IndexOf(CurrentTheme);
            PropertyChanged.Raise(this, "CurrentTheme");
        }

        void SetDefaultTheme()
        {
            CurrentTheme = this.FirstOrDefault(o => o.Name.Equals("day", StringComparison.OrdinalIgnoreCase));
        }

        public void UpdatePermanentStateCurrentThemeName()
        {
            if (CurrentTheme == null)
                return;
            permanentState.CurrentTheme = CurrentTheme.Name;
        }

        public void NextTheme()
        {
            currentThemeIndex++;
            if (currentThemeIndex >= this.Count)
                currentThemeIndex = 0;
            CurrentTheme = this[currentThemeIndex];
            PropertyChanged.Raise(this, "CurrentTheme");
            UpdatePermanentStateCurrentThemeName();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
