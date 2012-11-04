using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Microsoft.Phone.Reactive
{
    public class ProcessQueue<T1, T2>
    {
        List<Tuple<T1, IObserver<T2>>> queue = new List<Tuple<T1,IObserver<T2>>>();

        int requestLimit;
        int currentNumberOfOutstandingRequests = 0;
        object syncObject = new object();

        Func<T1, IObservable<T2>> processFunction;

        public ProcessQueue(int requestLimit, Func<T1, IObservable<T2>> processFunction)
        {
            this.requestLimit = requestLimit;
            this.processFunction = processFunction;
        }

        public enum RequestStatus
        {
            OutstandingRequests,
            NoPendingRequests
        }

        RequestStatus currentStatus = RequestStatus.NoPendingRequests;

        BehaviorSubject<RequestStatus> requestStatusChanged = new BehaviorSubject<RequestStatus>(RequestStatus.NoPendingRequests);
        public IObservable<RequestStatus> RequestStatusChanged { get { return requestStatusChanged.AsObservable(); } }

        public IObservable<T2> Enqueue(T1 request)
        {
            // if we can process this request, then call it right away
            if (currentNumberOfOutstandingRequests < requestLimit)
            {
                return Observable.Create<T2>(observer => 
                {
                    DebugEx.WriteLine("Immediate observable created");
                    CallWebRequestAndNotifyObserver(request, observer);
                    return () => { ; };
                });
            }

            // if there are too many outstanding requests, add it to the queue
            else
            {
                return Observable.Create<T2>(observer =>
                {
                    DebugEx.WriteLine("Delayed observable created");

                    var tuple = Tuple.Create(request, observer);
                    lock(syncObject)
                    {
                        queue.Add(tuple);
                    }
                    return () => { ; };
                });
            }
        }

        void CallWebRequestAndNotifyObserver(T1 request, IObserver<T2> observer)
        {
            DebugEx.WriteLine("CallWebRequestAndNotifyObserver {0}", request.ToString());
            lock (syncObject)
            {
                currentNumberOfOutstandingRequests++;
                if (currentStatus == RequestStatus.NoPendingRequests)
                {
                    currentStatus = RequestStatus.OutstandingRequests;
                    requestStatusChanged.OnNext(currentStatus);
                }
            }

            processFunction(request)
                .Subscribe(
                    response =>
                    {
                        DebugEx.WriteLine("SUBSCRIPTION");
                        observer.OnNext(response);
                        observer.OnCompleted();
                        CompleteRequestFinished();
                    },
                    ex =>
                    {
                        observer.OnError(ex);
                        CompleteRequestFinished();
                    });
        }

        void CompleteRequestFinished()
        {
            DebugEx.WriteLine("CompleteRequestFinished");
            lock (syncObject)
            {
                currentNumberOfOutstandingRequests--;
                if (currentNumberOfOutstandingRequests == 0 && currentStatus == RequestStatus.OutstandingRequests)
                {
                    currentStatus = RequestStatus.NoPendingRequests;
                    requestStatusChanged.OnNext(currentStatus);
                }
            }
            CheckForOutstandingWebRequests();
        }

        void CheckForOutstandingWebRequests()
        {
            DebugEx.WriteLine("CheckForOutstandingWebRequests");
            Tuple<T1, IObserver<T2>> nextRequest;
            lock (syncObject)
            {
                nextRequest = queue.FirstOrDefault();
                if (nextRequest == null)
                    return;

                queue.Remove(nextRequest);
            }

            var request = nextRequest.Item1;
            var observer = nextRequest.Item2;

            CallWebRequestAndNotifyObserver(request, observer);
        }
    }
}
