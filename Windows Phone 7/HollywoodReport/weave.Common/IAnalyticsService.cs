
namespace SelesGames.Logging
{
    public interface IAnalyticsService
    {
        void Track(string metaInfo, string eventName, string description = null);
    }
}
