using SelesGames.Rest;
using System.Net;
using System.Threading.Tasks;
using Weave.SavedState;
using Weave.ViewModels;

namespace weave.Services.Startup
{
    public class Transition_GetUserById : IState
    {
        UserInfo user;
        PermanentState permState;

        public enum State
        {
            Success,
            Fail
        }

        public State? CurrentState { get; private set; }

        public Transition_GetUserById(UserInfo user, PermanentState permState)
        {
            this.user = user;
            this.permState = permState;
        }

        public async Task Transition()
        {
            if (permState.IsFirstTime)
            {
                CurrentState = State.Fail;
                return;
            }

            try
            {
                await user.Load(refreshNews: false);
                CurrentState = State.Success;
            }
            catch (ResponseException responseException)
            {
                var response = responseException.Response;
                if (response != null && response.StatusCode == HttpStatusCode.NotFound && !string.IsNullOrWhiteSpace(response.ReasonPhrase))
                    CurrentState = State.Fail;
                else
                    throw;
            }
            catch
            {
                throw;
            }
        }
    }
}
