using System;

namespace SelesGames.UI.Advertising
{
    public class EventArgs<T> : EventArgs
    {
        public T Item { get; set; }
    }
}