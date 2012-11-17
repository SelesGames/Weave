using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

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