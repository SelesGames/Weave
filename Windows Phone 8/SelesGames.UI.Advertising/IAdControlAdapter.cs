using System;
using System.Windows;

namespace SelesGames.UI.Advertising
{
    internal interface IAdControlAdapter : IDisposable
    {
        UIElement Control { get; }
        event EventHandler AdClicked;
        event EventHandler AdRefreshed;
        event EventHandler<EventArgs<Exception>> AdError;
    }
}
