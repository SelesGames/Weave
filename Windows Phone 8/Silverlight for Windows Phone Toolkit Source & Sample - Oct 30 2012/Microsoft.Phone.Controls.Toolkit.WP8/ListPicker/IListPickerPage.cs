using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Displays the list of items and allows single or multiple selection.
    /// </summary>
    public interface IListPickerPage
    {
        /// <summary>
        /// Gets or sets the item template
        /// </summary>
        DataTemplate FullModeItemTemplate { get; set; }

        /// <summary>
        /// Gets or sets the string of text to display as the header of the page.
        /// </summary>
        string HeaderText { get; set; }

        /// <summary>
        /// Gets or sets the list of items to display.
        /// </summary>
        IList Items { get; }

        /// <summary>
        /// Gets or sets the selection mode.
        /// </summary>
        SelectionMode SelectionMode { get; set; }

        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        object SelectedItem { get; set; }

        /// <summary>
        /// Gets or sets the list of items to select.
        /// </summary>
        IList SelectedItems { get; }
    }
}
