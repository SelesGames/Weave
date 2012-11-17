using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace System.Net
{
    public class HttpWebRequestQueue
    {
        List<Tuple<HttpWebRequest, IObserver<HttpWebResponse>>> queue = new List<Tuple<HttpWebRequest, IObserver<HttpWebResponse>>>();

        int requestLimit;
        int currentNumberOfOutstandingRequests = 0;
        object syncObject = new object();
        IScheduler webRequestCallScheduler;
        IScheduler webRequestResponseScheduler;

        //public HttpWebRequestQueue(int requestLimit)
        //{
        //    this.requestLimit = requestLimit;
        //    this.scheduler = Scheduler.ThreadPool;
        //}

        public HttpWebRequestQueue(int requestLimit, IScheduler webRequestCallScheduler, IScheduler webRequestResponseScheduler)
        {
            this.requestLimit = requestLimit;
            this.webRequestCallScheduler = webRequestCallScheduler;
            this.webRequestResponseScheduler = webRequestResponseScheduler;
        }

        public enum RequestStatus
        {
            OutstandingRequests,
            NoPendingRequests
        }

        RequestStatus currentStatus = RequestStatus.NoPendingRequests;

        BehaviorSubject<RequestStatus> requestStatusChanged = new BehaviorSubject<RequestStatus>(RequestStatus.NoPendingRequests);
        public IObservable<RequestStatus> RequestStatusChanged { get { return requestStatusChanged.AsObservable(); } }

        public IObservable<HttpWebResponse> Enqueue(HttpWebRequest request)
        {
            // if we can process this request, then call it right away
            if (currentNumberOfOutstandingRequests < requestLimit)
            {
                return Observable.Create<HttpWebResponse>(observer => 
                {
                    return this.webRequestCallScheduler.Schedule(() =>
                    {
                        try
                        {
                            //DebugEx.WriteLine("what thread are we using here");
                            CallWebRequestAndNotifyObserver(request, observer);
                        }
                        catch (Exception ex) { observer.OnError(ex); }
                    });

                    //try
                    //{
                    //    DebugEx.WriteLine("what thread are we using here");
                    //    CallWebRequestAndNotifyObserver(request, observer);
                    //}
                    //catch (Exception ex) { observer.OnError(ex); }
                    //return () => { ; };
                });
            }

            // if there are too many outstanding requests, add it to the queue
            else
            {
                return Observable.Create<HttpWebResponse>(observer =>
                {
                    IDisposable disp = Disposable.Empty;
                    try
                    {
                        var tuple = Tuple.Create(request, observer);
                        lock (syncObject)
                        {
                            queue.Add(tuple);
                        }

                        disp = Disposable.Create(() =>
                        {
                            try
                            {
                                lock (syncObject)
                                {
                                    queue.Remove(tuple);
                                }
                                //tuple.Item1.Abort();
                            }
                            catch (Exception) { }
                        });
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                    return disp;
                    //return this.webRequestResponseScheduler.Schedule(() =>
                    //{
                    //    var tuple = Tuple.Create(request, observer);
                    //    lock (syncObject)
                    //    {
                    //        queue.Add(tuple);
                    //    }
                    //});
                });
            }
        }

        public void Flush()
        {
            lock (syncObject)
            {
                queue.Clear();
                //queue2.Clear();

                currentNumberOfOutstandingRequests = 0;
                if (currentStatus == RequestStatus.OutstandingRequests)
                {
                    currentStatus = RequestStatus.NoPendingRequests;
                    requestStatusChanged.OnNext(currentStatus);
                }
            }
        }

        void CallWebRequestAndNotifyObserver(HttpWebRequest request, IObserver<HttpWebResponse> observer)
        {
            try
            {
                lock (syncObject)
                {
                    currentNumberOfOutstandingRequests++;
                    if (currentStatus == RequestStatus.NoPendingRequests)
                    {
                        currentStatus = RequestStatus.OutstandingRequests;
                        requestStatusChanged.OnNext(currentStatus);
                    }
                }

                request
                    .GetHttpWebResponseAsync()
                    //.SubscribeOn(this.scheduler)  // added just now
                    .ObserveOn(this.webRequestResponseScheduler)
                    .SafelySubscribe(
                        response =>
                        {
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
            catch (Exception ex) { observer.OnError(ex); }
        }

        void CompleteRequestFinished()
        {
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
            Tuple<HttpWebRequest, IObserver<HttpWebResponse>> nextRequest;
            lock (syncObject)
            {
                nextRequest = queue.FirstOrDefault();
                //if (queue2.Count > 0)
                //    nextRequest = queue2.Dequeue();
                //else
                //    return;

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
