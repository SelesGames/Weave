using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace System.IO.IsolatedStorage
{
    public static class IsolatedStorageSettingsExtensions
    {
        public static void UpdateValueForKeyAndSave(this IsolatedStorageSettings settings, object value, string key)
        {
            settings.UpdateValueForKey(value, key);
            settings.Save();
        }

        public static T GetValueOrDefaultForKey<T>(this IsolatedStorageSettings settings, string key, T defaultVal = default(T))
        {
            return (T)((IDictionary<string, object>)settings).GetValueOrDefaultForKey(key, defaultVal);
        }
    }
}

public static class IDictionaryExtensions
{
    public static void UpdateValueForKey<TKey, TVal>(this IDictionary<TKey, TVal> settings, TVal value, TKey key)
    {
        if (settings.ContainsKey(key))
            settings[key] = value;
        else
            settings.Add(key, value);
    }

    public static TVal GetValueOrDefaultForKey<TKey, TVal>(this IDictionary<TKey, TVal> settings, TKey key, TVal defaultVal = default(TVal))
    {
        if (settings.ContainsKey(key))
            return settings[key];
        else
            return defaultVal;
    }
}

public static class ConvenienceExtensions
{
    public static bool IsInDesignMode(this UIElement element)
    {
        return DesignerProperties.IsInDesignTool;
    }

    public static void ClipToSize(this FrameworkElement element)
    {
        element.SizeChanged += (s, e) =>
        {
            var newSize = e.NewSize;
            element.Clip = new RectangleGeometry { Rect = new Rect(new Point(0, 0), newSize) };
        };
    }
}