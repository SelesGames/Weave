using Microsoft.Phone.Info;
using SelesGames.Common.Hashing;
using System.Threading.Tasks;
using Weave.ViewModels;

namespace weave.Services.Startup
{
    public class Transition_GetMicrosoftId : IState
    {
        UserInfo user;

        public enum State
        {
            Success,
            Fail
        }

        public State? CurrentState { get; private set; }

        public Transition_GetMicrosoftId(UserInfo user)
        {
            this.user = user;
        }

        public Task Transition()
        {
            var anid2 = TryGetAnid2();

            if (!string.IsNullOrEmpty(anid2))
            {
                var newId = CryptoHelper.ComputeHash(anid2);
                user.Id = newId;
                CurrentState = State.Success;
            }
            else
            {
                CurrentState = State.Fail;
            }

            return Task.FromResult<object>(null);
        }

        string TryGetAnid2()
        {
            string anid2 = null;

            try
            {
                anid2 = (string)UserExtendedProperties.GetValue("ANID2");
            }
            catch { }

            return anid2;
        }
    }
}
