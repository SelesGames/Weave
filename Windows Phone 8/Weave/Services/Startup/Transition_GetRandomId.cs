using System;
using System.Threading.Tasks;
using Weave.ViewModels.Identity;

namespace weave.Services.Startup
{
    public class Transition_GetRandomId
    {
        IdentityInfo identityInfo;

        public enum State
        {
            Success,
            Fail
        }

        public State? CurrentState { get; private set; }

        public Transition_GetRandomId(IdentityInfo identityInfo)
        {
            this.identityInfo = identityInfo;
        }

        public Task Transition()
        {
            var newId = Guid.NewGuid();
            identityInfo.UserId = newId;
            CurrentState = State.Success;

            return Task.FromResult<object>(null);
        }
    }
}
