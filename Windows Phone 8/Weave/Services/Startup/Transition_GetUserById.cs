using SelesGames.Rest;
using System.Net;
using System.Threading.Tasks;
using Weave.ViewModels;

namespace weave.Services.Startup
{
    public class Transition_GetUserById : IState
    {
        UserInfo user;

        public enum State
        {
            Success,
            Fail
        }

        public State? CurrentState { get; private set; }

        public Transition_GetUserById(UserInfo user)
        {
            this.user = user;
        }

        public async Task Transition()
        {
            try
            {
                await user.Load(true);
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
