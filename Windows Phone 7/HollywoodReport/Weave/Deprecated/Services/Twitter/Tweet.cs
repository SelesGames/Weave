using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace weave.Services.Twitter
{
    public class Tweet
    {
        public string CreatedAt { get; set; }
        public string UserName { get; set; }
        public string DisplayUserName { get; set; }
        public string Text { get; set; }
        public string ImageUrl { get; set; }
        public long Id { get; set; }
        public bool NewTweet { get; set; }
        public string Source { get; set; }
    }
}

/*
<status>
  <created_at>Thu Apr 21 05:14:05 +0000 2011</created_at>
  <id>60934306348007424</id>
  <text>Cool site - ZeroCater for scheduled catering from local restaurants: http://www.zerocater.com/</text>
  <source>web</source>
  <truncated>false</truncated>
  <favorited>false</favorited>
  <in_reply_to_status_id></in_reply_to_status_id>
  <in_reply_to_user_id></in_reply_to_user_id>
  <in_reply_to_screen_name></in_reply_to_screen_name>
  <retweet_count>0</retweet_count>
  <retweeted>false</retweeted>
  <user>
    <id>11740902</id>
    <name>Tim Ferriss</name>
    <screen_name>tferriss</screen_name>
    <location>Global</location>
    <description>Author of #1 NY Times bestseller, The 4-Hour Workweek, Japanophile, tea drinker, tango world record holder, language learning fanatic </description>
    <profile_image_url>http://a2.twimg.com/profile_images/49918572/half-face-ice_normal.jpg</profile_image_url>
    <url>http://www.fourhourworkweek.com/blog</url>
    <protected>false</protected>
    <followers_count>247969</followers_count>
    <profile_background_color>9ae4e8</profile_background_color>
    <profile_text_color>000000</profile_text_color>
    <profile_link_color>0000ff</profile_link_color>
    <profile_sidebar_fill_color>e0ff92</profile_sidebar_fill_color>
    <profile_sidebar_border_color>87bc44</profile_sidebar_border_color>
    <friends_count>305</friends_count>
    <created_at>Wed Jan 02 04:36:53 +0000 2008</created_at>
    <favourites_count>21</favourites_count>
    <utc_offset>12600</utc_offset>
    <time_zone>Tehran</time_zone>
    <profile_background_image_url>http://a3.twimg.com/profile_background_images/2770739/greek_dessert.jpg</profile_background_image_url>
    <profile_background_tile>true</profile_background_tile>
    <profile_use_background_image>true</profile_use_background_image>
    <notifications>false</notifications>
    <geo_enabled>false</geo_enabled>
    <verified>false</verified>
    <following>true</following>
    <statuses_count>2398</statuses_count>
    <lang>en</lang>
    <contributors_enabled>false</contributors_enabled>
    <follow_request_sent>false</follow_request_sent>
    <listed_count>8789</listed_count>
    <show_all_inline_media>false</show_all_inline_media>
    <default_profile>false</default_profile>
    <default_profile_image>false</default_profile_image>
    <is_translator>false</is_translator>
  </user>
  <geo />
  <coordinates />
  <place />
  <contributors />
</status>

*/
