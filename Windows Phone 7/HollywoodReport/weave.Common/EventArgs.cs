using System;

namespace SelesGames
{
    public class EventArgs<T> : EventArgs
    {
        public T Item { get; set; }
    }
}
