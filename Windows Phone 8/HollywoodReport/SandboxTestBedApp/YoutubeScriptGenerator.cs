

using System;
using System.Text;

public static class YouTubeScript
{
    /// <summary>
    /// YouTube player script generator, default:
    /// default width = 320
    /// default height = 240
    /// no autoplay
    /// </summary>
    /// <param name="id">id</param>
    /// <param name="auto">int</param>
    /// <returns>string</returns>
    public static string Get(string id)
    {
        // default size: 320x240, no autoplay
        return Get(id, 0, 320, 240);
    }

    #region YouTube script, autoplay option
    /// <summary>
    /// YouTube player script generator, overload:
    /// default width = 320
    /// default height = 240
    /// </summary>
    /// <param name="id">id</param>
    /// <param name="auto">int</param>
    /// <returns>string</returns>
    public static string Get(string id, int auto)
    {
        // default size: 320x240
        return Get(id, auto, 320, 240);
    }
    #endregion

    #region YouTube script: autoplay option, adjustable W/H
    /// <summary>
    /// YouTube player script generator (w/autoplay, W/H options)
    /// </summary>
    /// <param name="id">id</param>
    /// <param name="auto">int</param>
    /// <param name="W">int</param>
    /// <param name="H">int</param>
    /// <returns>string</returns>
    public static string Get(string id, int auto, int W, int H)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(@"<embed src='http://www.youtube.com/v/");
        // select the item to play
        sb.Append(id);
        sb.Append("&autoplay=");
        // set autoplay options (indicates number of plays)
        sb.Append(auto.ToString());
        sb.Append("' ");
        sb.Append("type='application/x-shockwave-flash' ");
        sb.Append("allowscriptaccess='never' enableJavascript ='false' ");
        sb.Append("allowfullscreen='true' ");
        // set width
        sb.Append("width='" + W.ToString() + "' ");
        // set height
        sb.Append("height='" + H.ToString() + "' ");
        sb.Append(@"></embed>");

        string scr = sb.ToString();
        sb = null;
        return scr;
    }
    #endregion
}
