using System;
using System.Threading.Tasks;
using Weave.SavedState;
using Weave.ViewModels;

namespace Weave.Services.Startup
{
    public class CriticalApplicationException : Exception
    {
    }

    public class StartupIdentityStateMachine
    {
        UserInfo user;
        PermanentState permState;
        IState currentState;
        bool isComplete = false;

        public enum State
        {
            UserExists,
            NoUserFound
        }

        public State? FinalState { get; private set; }

        public StartupIdentityStateMachine(UserInfo user, PermanentState permState)
        {
            this.user = user;
            this.permState = permState;
        }

        public async Task Begin()
        {
            Init();

            while (!isComplete)
            {
                await ChooseNextState();
            }
        }

        Task ChooseNextState()
        {
            //if (currentState is Transition_GetMicrosoftId)
            //{
            //    return TransitionGetMicrosoftId();
            //}

            if (currentState is Transition_GetRandomId)
            {
                return TransitionGetRandomId();
            }

            else if (currentState is Transition_GetUserById)
            {
                return TransitionGetUserById();
            }

            else throw new Exception("unidentified transition state");
        }




        #region Transition functions

        void Init()
        {
            if (user.Id != Guid.Empty)
            {
                currentState = new Transition_GetUserById(user, permState);
            }
            else
            {
                //currentState = new Transition_GetMicrosoftId(user);
                currentState = new Transition_GetRandomId(user);
            }
        }

        //async Task TransitionGetMicrosoftId()
        //{
        //    var state = (Transition_GetMicrosoftId)currentState;
        //    await state.Transition();

        //    if (state.CurrentState == Transition_GetMicrosoftId.State.Success)
        //        currentState = new Transition_GetUserById(user);

        //    else if (state.CurrentState == Transition_GetMicrosoftId.State.Fail)
        //        currentState = new Transition_GetRandomId(user);
        //}

        async Task TransitionGetRandomId()
        {
            var state = (Transition_GetRandomId)currentState;
            await state.Transition();

            if (state.CurrentState == Transition_GetRandomId.State.Success)
                currentState = new Transition_GetUserById(user, permState);

            else if (state.CurrentState == Transition_GetRandomId.State.Fail)
                throw new CriticalApplicationException();
        }

        async Task TransitionGetUserById()
        {
            var state = (Transition_GetUserById)currentState;
            await state.Transition();

            if (state.CurrentState == Transition_GetUserById.State.Success)
            {
                FinalState = State.UserExists;
            }

            else if (state.CurrentState == Transition_GetUserById.State.Fail)
            {
                FinalState = State.NoUserFound;
            }

            currentState = null;
            isComplete = true;
        }

        #endregion
    }
}
