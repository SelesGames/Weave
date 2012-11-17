using System;
using System.Collections.Generic;

namespace Windows.UI
{
    ///
    /// To compute complementary color as in Kuler tool
    /// http://kuler.adobe.com/#create/fromacolor
    /// 
    public static class ColorExtensions
    {
        /// <summary>
        /// oposite dark hue
        /// </summary>
        private static Dictionary<int, int> opositeDarkHue = new Dictionary<int, int>()
        {
            {0, 137}, {1, 139}, {2, 141}, {3, 143}, {4, 145}, {5, 147}, {6, 149}, {7, 151}, {8, 153}, {9, 154}, 
            {10, 156}, {11, 158}, {12, 160}, {13, 162}, {14, 164}, {15, 166}, {16, 168}, {17, 170}, {18, 172}, {19, 174},
            {20, 176}, {21, 178}, {22, 180}, {23, 182}, {24, 183}, {25, 185}, {26, 187}, {27, 189}, {28, 191}, {29, 192},
            {30, 194}, {31, 196}, {32, 198}, {33, 200}, {34, 201}, {35, 203}, {36, 206}, {37, 208}, {38, 211}, {39, 214},
            {40, 216}, {41, 219}, {42, 221}, {43, 224}, {44, 227}, {45, 229}, {46, 232}, {47, 234}, {48, 237}, {49, 240},
            {50, 242}, {51, 245}, {52, 248}, {53, 251}, {54, 253}, {55, 256}, {56, 259}, {57, 261}, {58, 264}, {59, 267},
            {60, 269}, {61, 270}, {62, 271}, {63, 272}, {64, 273}, {65, 273}, {66, 274}, {67, 275}, {68, 276}, {69, 276}, 
            {70, 277},{71, 278}, {72, 279}, {73, 280}, {74, 280}, {75, 281}, {76, 282}, {77, 283}, {78, 284},  {79, 284},
            {80, 285}, {81, 286}, {82, 287}, {83, 287}, {84, 288}, {85, 289}, {86, 290}, {87, 291}, {88, 291}, {89, 292},            
            {90, 293}, {91, 294}, {92, 294}, {93, 295}, {94, 296}, {95, 297}, {96, 298}, {97, 298}, {98, 299}, {99, 300},
            {100, 301}, {101, 303}, {102, 304}, {103, 306}, {104, 307}, {105, 308}, {106, 310}, {107, 311}, {108, 313}, {109, 314},
            {110, 316}, {111, 317}, {112, 319}, {113, 320}, {114, 321}, {115, 323}, {116, 324}, {117, 326}, {118, 327}, {119, 329},
            {120, 330}, {121, 332}, {122, 334}, {123, 335}, {124, 337}, {125, 339}, {126, 341}, {127, 342}, {128, 344}, {129, 346},
            {130, 348}, {131, 349}, {132, 351}, {133, 353}, {134, 355}, {135, 356}, {136, 358}, {137, 0}, {138, 1}, {139, 1},
            {140, 2}, {141, 2}, {142, 3}, {143, 3}, {144, 4}, {145, 4}, {146, 5}, {147, 5}, {148, 6}, {149, 6},
            {150, 7}, {151, 7}, {152, 8}, {153, 8}, {154, 9}, {155, 9}, {156, 10}, {157, 10}, {158, 11}, {159, 11},
            {160, 12}, {161, 12}, {162, 13}, {163, 13}, {164, 14}, {165, 14}, {166, 15}, {167, 15}, {168, 16}, {169, 16},
            {170, 17}, {171, 18}, {172, 18}, {173, 19}, {174, 19}, {175, 20}, {176, 20}, {177, 21}, {178, 21}, {179, 22},
            {180, 22}, {181, 23}, {182, 23}, {183, 24}, {184, 24}, {185, 25}, {186, 25}, {187, 26}, {188, 27}, {189, 27},
            {190, 28}, {191, 28}, {192, 29}, {193, 29}, {194, 30}, {195, 30}, {196, 31}, {197, 32}, {198, 32}, {199, 33},
            {200, 33}, {201, 34}, {202, 34}, {203, 35}, {204, 35}, {205, 36}, {206, 36}, {207, 36}, {208, 37}, {209, 37},
            {210, 38}, {211, 38}, {212, 38}, {213, 39}, {214, 39}, {215, 40}, {216, 40}, {217, 40}, {218, 41}, {219, 41},
            {220, 41}, {221, 42}, {222, 42}, {223, 43}, {224, 43}, {225, 43}, {226, 44}, {227, 44}, {228, 45}, {229, 45},
            {230, 45}, {231, 46}, {232, 46}, {233, 46}, {234, 47}, {235, 47}, {236, 48}, {237, 48}, {238, 48}, {239, 49},
            {240, 49}, {241, 49}, {242, 50}, {243, 50}, {244, 51}, {245, 51}, {246, 51}, {247, 52}, {248, 52}, {249, 52},
            {250, 53}, {251, 53}, {252, 54}, {253, 54}, {254, 54}, {255, 54}, {256, 55}, {257, 55}, {258, 56}, {259, 56},
            {260, 57}, {261, 57}, {262, 57}, {263, 58}, {264, 58}, {265, 58}, {266, 59}, {267, 59}, {268, 59}, {269, 60},
            {270, 61}, {271, 62}, {272, 63}, {273, 65}, {274, 66}, {275, 67}, {276, 68}, {277, 70}, {278, 71}, {279, 72},
            {280, 73}, {281, 75}, {282, 76}, {283, 77}, {284, 79}, {285, 80}, {286, 81}, {287, 82}, {288, 84}, {289, 85},
            {290, 86}, {291, 88}, {292, 89}, {293, 90}, {294, 91}, {295, 93}, {296, 94}, {297, 95}, {298, 97}, {299, 98},
            {300, 99}, {301, 100}, {302, 100}, {303, 101}, {304, 102}, {305, 103}, {306, 103}, {307, 104}, {308, 105}, {309, 105},
            {310, 106}, {311, 107}, {312, 107}, {313, 108}, {314, 109}, {315, 110}, {316, 110}, {317, 111}, {318, 112}, {319, 112},
            {320, 113}, {321, 114}, {322, 114}, {323, 115}, {324, 116}, {325, 117}, {326, 117}, {327, 118}, {328, 119}, {329, 119},
            {330, 120}, {331, 121}, {332, 121}, {333, 122}, {334, 122}, {335, 123}, {336, 123}, {337, 124}, {338, 125}, {339, 125},
            {340, 126}, {341, 126}, {342, 127}, {343, 127}, {344, 128}, {345, 128}, {346, 129}, {347, 130}, {348, 130}, {349, 131},
            {350, 131}, {351, 132}, {352, 132}, {353, 133}, {354, 134}, {355, 134}, {356, 135}, {357, 135}, {358, 136}, {359, 136},
            
        };


        /// <summary>
        /// Extension methid that will compute complementary colors
        /// </summary>
        /// <param name="baseColor">base color</param>
        /// <returns>complementary colors</returns>
        public static ComplementaryColors GetComplementary(this Color baseColor)
        {
            ComplementaryColors result = new ComplementaryColors();

            double hue;
            double sat;
            double val;
            ColorToHSV(baseColor, out hue, out sat, out val);
            

            //complementary
            double hueComp = getOpositeDarkHue((int)Math.Round(hue));
            hueComp = NormalizeHue(hueComp);

            double satComp1 = Math.Min(sat + .2, 1);
            double valComp1 = val <= .5 ? (val + .30) : (.20 + (val - .50));
            double satComp2 = sat;
            double valComp2 = val;

            result.ComplementaryColorLighter = ColorFromHSV(hueComp, satComp2, valComp2);
            result.ComplementaryColorDarker = ColorFromHSV(hueComp, satComp1, valComp1);


            //original
            double valOrg1 = val < .5 ? (val + .30) : (.20 + (val - .50));
            double satOrg1 = sat < .8 ? sat + .1 : .9 + (sat - .8) / 2;
            double valOrg2 = Math.Min(val + .30, 1);
            double satOrg2 = Math.Max(0, sat - .10);

            result.OriginalColorDarker = ColorFromHSV(hue, satOrg1, valOrg1);
            result.OriginalColorLighter = ColorFromHSV(hue, satOrg2, valOrg2);

            return result;
        }


        private static double NormalizeHue(double hueCompLight)
        {
            hueCompLight = hueCompLight > 360 ? hueCompLight - 360 : hueCompLight;
            hueCompLight = hueCompLight < 0 ? hueCompLight + 360 : hueCompLight;
            return hueCompLight;
        }


        private static int getOpositeDarkHue(int hue)
        {
            return opositeDarkHue[hue];
        }


        /// <summary>
        /// Color to HSV
        /// </summary>
        /// <param name="color"></param>
        /// <param name="hue"></param>
        /// <param name="saturation"></param>
        /// <param name="value"></param>
        public static void ColorToHSV(Color color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));
            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }


        /// <summary>
        /// Color from HSV
        /// </summary>
        /// <param name="hue"></param>
        /// <param name="saturation"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);
            value = value * 255;
            byte v = Convert.ToByte(value);
            byte p = Convert.ToByte(value * (1 - saturation));
            byte q = Convert.ToByte(value * (1 - f * saturation));
            byte t = Convert.ToByte(value * (1 - (1 - f) * saturation));
            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }


        /// <summary>Gets the hue-saturation-brightness (HSB) hue value, in degrees, for
        /// this <see cref="T:System.Drawing.Color"/> structure.</summary>
        /// <returns>The hue, in degrees, of this <see cref="T:System.Drawing.Color"/>. The
        /// hue is measured in degrees, ranging from 0.0 through 360.0, in HSB color space.
        /// </returns>
        public static float GetHue(this Color colorBase)
        {
            if (colorBase.R == colorBase.G && colorBase.G == colorBase.B)
            {
                return 0;
            }
            float r = (float)colorBase.R / 255;
            float g = (float)colorBase.G / 255;
            float b = (float)colorBase.B / 255;
            float single1 = 0;
            float single2 = r;
            float single3 = r;
            if (g > single2)
            {
                single2 = g;
            }
            if (b > single2)
            {
                single2 = b;
            }
            if (g < single3)
            {
                single3 = g;
            }
            if (b < single3)
            {
                single3 = b;
            }
            float single4 = single2 - single3;
            if (r == single2)
            {
                single1 = (g - b) / single4;
            }
            else
            {
                if (g == single2)
                {
                    single1 = 2 + ((b - r) / single4);
                }
                else
                {
                    if (b == single2)
                    {
                        single1 = 4 + ((r - g) / single4);
                    }
                }
            }
            single1 = single1 * 60;
            if (single1 < 0)
            {
                single1 = single1 + 360;
            }
            return single1;
        }
    }
}
