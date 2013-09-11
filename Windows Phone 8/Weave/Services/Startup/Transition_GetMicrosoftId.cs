using Microsoft.Phone.Info;
using SelesGames.Common.Hashing;
using System.Threading.Tasks;
using Weave.ViewModels.Identity;

namespace weave.Services.Startup
{
    public class Transition_GetMicrosoftId
    {
        IdentityInfo identityInfo;

        public enum State
        {
            Success,
            Fail
        }

        public State? CurrentState { get; private set; }

        public Transition_GetMicrosoftId(IdentityInfo identityInfo)
        {
            this.identityInfo = identityInfo;
        }

        public Task Transition()
        {
            var anid2 = (string)UserExtendedProperties.GetValue("ANID2");
            if (!string.IsNullOrEmpty(anid2))
            {
                var newId = CryptoHelper.ComputeHash(anid2);
                identityInfo.UserId = newId;
                CurrentState = State.Success;
            }
            else
            {
                CurrentState = State.Fail;
            }

            return Task.FromResult<object>(null);
        }
    }
}
