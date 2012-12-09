using System.ComponentModel;
using System.Windows;

public static class ConvenienceExtensions
{
    public static bool IsInDesignMode(this UIElement element)
    {
        return DesignerProperties.IsInDesignTool;
    }
}