using System.Diagnostics;

namespace SelesGames.Instapaper
{
    class Program
    {

        static void Main()
        {
            //// doc http://blog.instapaper.com/post/73123968/read-later-api
            //var api = "https://www.instapaper.com/api/add?username={0}&url={1}&title={2}";
            //var userName = HttpUtility.UrlEncode("aemami99@gmail.com");
            //var url = HttpUtility.UrlEncode("http://gizmo.do/e8ZP5B");
            //var properuri = string.Format(api, userName, url);

            var properUri = new InstapaperAccount
            {
                UserName = "aemami99@gmail.com",
                Password = "piccolo"
            }
            .CreateAddToInstapaperUrlString(
                "http://gizmo.do/e8ZP5B",
                "App Deals of the Day, now with WP7",
                "Lord Adair Turner, the chairman of Britain’s top financial watchdog, the Financial Services Authority, has described much of what happens on Wall Street and in other financial centers as “socially useless activity”—a comment that suggests it could be eliminated without doing any damage to the economy. In a recent article titled “What Do Banks Do?,” which appeared in a collection of essays devoted to the future of finance, Turner pointed out that although certain financial activities were genuinely valuable, others generated revenues and profits without delivering anything of real worth—payments that economists refer to as rents. “It is possible for financial activity to extract rents from the real economy rather than to deliver economic value,” Turner wrote. “Financial innovation … may in some ways and under some circumstances foster economic value creation, but that needs to be illustrated at the level of specific effects: it cannot be asserted a priori.  It takes a while to notice Ruben’s scars. Though they’re hardly subtle, they don’t catch your eye as readily as his strong, smooth features or the big-ass smile that’s totally disarming despite his size: six foot three, 225 pounds. Neck like a waist. Friendly as you please. When I pointed to each of the healed-up gashes on his fists and asked what they were from, he replied, “Teeth. Teeth. These are all from teeth.” He charges $1,000 for every one that he knocks out of a person’s head. It’s the same price for each bone he breaks in a face, a practice that’s cost him a couple of knuckles.");

            Debug.WriteLine(properUri);
        }
    }
}
