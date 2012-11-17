using System;
using System.Windows.Media;

namespace weave
{
    public static class FontThicknessExtensions
    {
        static readonly FontFamily SegoeWPLight;
        static readonly FontFamily SegoeWPSemiLight;
        static readonly FontFamily SegoeWPNormal;
        static readonly FontFamily SegoeWPSemibold;

        static FontThicknessExtensions()
        {
            var resources = App.Current.Resources;
            SegoeWPLight = resources["PhoneFontFamilyLight"] as FontFamily;
            SegoeWPSemiLight = resources["PhoneFontFamilySemiLight"] as FontFamily;
            SegoeWPNormal = resources["PhoneFontFamilyNormal"] as FontFamily;
            SegoeWPSemibold = resources["PhoneFontFamilySemiBold"] as FontFamily;
        }

        internal static FontFamily ToFontFamily(this FontThickness fontSize)
        {
            switch (fontSize)
            {
                case FontThickness.VerySkinny:
                    return SegoeWPLight;

                case FontThickness.Skinny:
                    return SegoeWPSemiLight;

                case FontThickness.Regular:
                    return SegoeWPNormal;

                case FontThickness.Fat:
                    return SegoeWPSemibold;

                default:
                    throw new Exception("unexexpected FontThickness value");
            }
        }
    }
}