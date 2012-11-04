using System;
using System.Collections.Generic;
using System.Linq;

namespace weave
{
    public class HomePageTask
    {
        public string Name { get; set; }
        public string Caption { get; set; }
        public string Icon { get; set; }

        public Action Action { get; set; }
    }

    public enum HomePagePriority
    {
        Normal,
        High,
        Guaranteed
    }

    public class HomePageTaskRule
    {
        public Func<HomePagePriority> Priority { get; set; }
        public Func<bool> Condition { get; set; }
    }

    public class HomePageTaskCollection
    {
        const int displayLimit = 4;

        List<Tuple<HomePageTask, HomePageTaskRule>> rules = new List<Tuple<HomePageTask, HomePageTaskRule>>();

        public void AddTask(HomePageTask task, HomePageTaskRule rule)
        {
            task.Name = task.Name.ToUpper();
            rules.Add(Tuple.Create(task, rule));
        }

        public void AddTask(HomePageTask task, Func<HomePagePriority> priority, Func<bool> condition)
        {
            var rule = new HomePageTaskRule { Priority = priority, Condition = condition };
            task.Name = task.Name.ToUpper();
            rules.Add(Tuple.Create(task, rule));
        }

        public List<HomePageTask> GetTasks()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var cacheRules = rules.Select(o => new { Task = o.Item1, Priority = o.Item2.Priority(), Condition = o.Item2.Condition() }).ToList();

            var stillValidRules = cacheRules.Where(o => o.Condition);

            var guaranteedTasks = stillValidRules
                .Where(o => o.Priority == HomePagePriority.Guaranteed)
                .Take(displayLimit)
                .ToList();

            var highPriority = stillValidRules
                .Where(o => o.Priority == HomePagePriority.High)
                .Select((o, i) => new { o, i })
                .RandomlySort()
                .Take(displayLimit - guaranteedTasks.Count)
                .OrderBy(o => o.i)
                .Select(o => o.o)
                .ToList();

            var normalPriority = stillValidRules
                .Where(o => o.Priority == HomePagePriority.Normal)
                .Select((o, i) => new { o, i })
                .RandomlySort()
                .Take(displayLimit - guaranteedTasks.Count - highPriority.Count)
                .OrderBy(o => o.i)
                .Select(o => o.o)
                .ToList();

            var result = guaranteedTasks
                .Union(highPriority.Union(normalPriority))
                .Select(o => o.Task)
                .ToList();

            stopwatch.Stop();
            DebugEx.WriteLine("Took {0} ms to get the tasks", stopwatch.ElapsedMilliseconds);

            return result;
        }

        public HomePageTaskCollection Merge(HomePageTaskCollection coll)
        {
            var newRules = this.rules.Union(coll.rules).ToList();
            var newColl = new HomePageTaskCollection();
            newColl.rules = newRules;
            return newColl;
        }
    }

    public class CoreHomePageTasks : HomePageTaskCollection
    {
        public CoreHomePageTasks()
        {
            var
            task = new HomePageTask
            {
                Name = "manage feeds",
                Caption = "Manage which feeds are turned on or off",
                Icon = "/weave;component/Assets/Icons/64x64_rss.png",
                //Action = () => GlobalNavigationService.ToManageSourcesPage(),
            };
            AddTask(task, () => HomePagePriority.Normal, () => AppSettings.Instance.CanManageFeeds);


            string[] blurb = 
            { 
                "Rate/review this app!  Because we'd give YOU 5 stars if we could.",
                "Rate/review this app!  Did I ever tell you you look nice today?",
                "Help us get our power level over NINE THOUSAAAAAAAND!!!",
                "Duh! #Winning!!",
            };
            task = new HomePageTask
            {
                Name = "write a review",
                Caption = blurb[new Random().Next(0, blurb.Length)],
                Icon = "/weave;component/Assets/Icons/64x64_rate.png",
                Action = () => SelesGames.Phone.TaskService.ToMarketplaceReviewTask(),
            };
            AddTask(task, () => HomePagePriority.Normal, () => true);


            task = new HomePageTask
            {
                Name = "email us",
                Caption = "Send us an email, we love to hear from you!",
                Icon = "/Assets/Icons/appbar.feature.email.rest.png",
                Action = () => SelesGames.Phone.TaskService.ToEmailComposeTask(
                    To: "info@selesgames.com",
                    Subject: string.Format("Question about {0} (version {1})", AppSettings.Instance.AppName, AppSettings.Instance.VersionNumber)),

            };
            AddTask(task, () => HomePagePriority.Normal, () => true);


            task = new HomePageTask
            {
                Name = "info / support",
                Caption = "View the info/support page for version number and contact info",
                Icon = "/weave;component/Assets/Icons/64x64_info.png",
                Action = () => GlobalNavigationService.ToInfoAndSupportPage(),
            };
            AddTask(task, () => HomePagePriority.Normal, () => true);


            task = new HomePageTask
            {
                Name = "view our apps",
                Caption = "Check out our other great apps - we have something for everyone!",
                Icon = "/weave;component/Assets/Icons/64x64_zune.png",
                Action = () => SelesGames.Phone.TaskService.ToMarketplaceAppSearchTask("Seles Games"),
            };
            AddTask(task, () => HomePagePriority.Normal, () => true);
        }
    }

    public class WeaveHomePageTasks : HomePageTaskCollection
    {
        public WeaveHomePageTasks()
        {
            HomePageTask task;
            //task = new HomePageTask
            //{
            //    Name = "add facebook",
            //    Caption = "Your Facebook feed will show up on the home page",
            //    Icon = "/Assets/Icons/facebook.png",
            //    Action = () => { ; },
            //};
            //AddTask(task, () => HomePagePriority.Guaranteed, () => weave.Services.Facebook.FacebookAccount.CurrentfacebookAccessCredentials == null);

            //task = new HomePageTask
            //{
            //    Name = "add twitter timeline",
            //    Caption = "Your Twitter timeline will show up on the home page",
            //    Icon = "/weave;component/Assets/Icons/64x64_twitter.png",
            //    Action = () => { ; },
            //};
            //AddTask(task, () => HomePagePriority.Guaranteed, () => weave.Services.Twitter.TwitterAccount.CurrentTwitterAccessCredentials == null);


            task = new HomePageTask
            {
                Name = "view changelog",
                Caption = "Check out the new features, and read about what's coming next",
                Icon = "/weave;component/Assets/Icons/64x64_info.png",
                Action = GlobalNavigationService.ToChangeLogAndComingSoonPage,
            };
            AddTask(task, () => HomePagePriority.Guaranteed, () => true);


            task = new HomePageTask
            {
                Name = "change the background",
                Caption = "Make Weave your own - pick a different background!",
                Icon = "/Assets/Icons/48x48_big_settings.png",
                Action = GlobalNavigationService.ToChangeLogAndComingSoonPage,
            };
            AddTask(task, () => HomePagePriority.Guaranteed, () => true);


            task = new HomePageTask
            {
                Name = "import google reader",
                Caption = "Add feeds from your Google Reader account (coming with Mango!)",
                Icon = "/weave;component/Assets/Icons/64x64_google.png",
                Action = () => { ; },
            };
            AddTask(task, () => HomePagePriority.High, () => true);
        }
    }
}
