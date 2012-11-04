using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Clarity.Demo.WrapPanel
{

    public class MyOwnVirtWrapPanel : VirtualizingPanel
    {
        public MyOwnVirtWrapPanel()
        {
            
        }
    }

    public class VirtualizingWrapPanel : VirtualizingPanel, IScrollInfo
    {
        #region Private Members

        #region Scrolling Vars
        private ScrollViewer _scrollOwner;
        private TranslateTransform _translateTransform;
        private Size _extent = new Size(0, 0);
        private Size _viewport = new Size(0, 0);
        private Point _offset = new Point(0, 0);
        private bool _vertScrollable = false;
        private Size _assumedControlSize = new Size(50, 50);
        private Size _previousMeasureSize = new Size(0, 0);
        double _scrollTarget = 100;
        ScrollDirection _scrollDirection = ScrollDirection.None;
        private Storyboard _scrollStoryboard;
        private Storyboard _smoothScrollStoryboard;
        private DoubleAnimation _scrollAnimation;
        private DoubleAnimation _smoothScrollAnimation;
        #endregion Scrolling Vars

        #region Virtualization Vars
        private IItemContainerGenerator _generator;
        private int _firstVisibleIndex = 0;
        private int _lastVisibleIndex = 0;
        private int _hiddenRowsAtTop = 0;
        private int _viewableRows = 0;
        private int _viewableColumns = 0;
        private int _firstControlIndex = 0;
        private int _lastControlIndex = 0;
        private int _startPosition = 0;
        private int _endPosition = 0;

        private List<UIElement> _realizedChildren;
        #endregion Virtualization Vars

        #endregion Private Members

        public VirtualizingWrapPanel()
            : base()
        {
            _extent = new Size(0, 0);
            _vertScrollable = false;
            _offset = new Point(0, 0);
            CreateScrollStoryboard();
            CreateSmoothScrollStoryboard();
            RenderTransform = _translateTransform = new TranslateTransform();
        }

        #region Overrides
        protected override Size MeasureOverride(Size availableSize)
        {
            Size sizeSoFar = new Size(0, 0);
            double maxWidth = 0.0;
            EnsureRealizedChildren();
            _generator = ItemContainerGenerator;
            UpdateScrollInfo(availableSize);
            CalculateControlIndices();
            RecycleContainers();

            if (_startPosition >= _endPosition && _endPosition > 0) return _previousMeasureSize;

            GeneratorPosition start = _generator.GeneratorPositionFromIndex(_startPosition);
            int childIndex = (start.Offset == 0) ? start.Index : start.Index + 1;

            using (_generator.StartAt(start, GeneratorDirection.Forward, true))
            {
                for (int i = _startPosition; i <= _endPosition; ++i)
                {
                    bool isNewlyRealized;

                    UIElement child = _generator.GenerateNext(out isNewlyRealized) as UIElement;
                    if (child == null) continue;

                    if (isNewlyRealized)
                    {
                        InsertContainer(childIndex, child, false);
                    }
                    else
                    {
                        if (childIndex >= _realizedChildren.Count || !(_realizedChildren[childIndex] == child))
                        {
                            // we have a recycled container (if it was realized container it would have been returned in the
                            // propert location). Note also that recycled containers are NOT in the _realizedChildren list.
                            InsertContainer(childIndex, child, true);
                        }
                        else
                        {
                            // previously realized child, so do nothing
                        }
                    }
                    childIndex++;

                    #region Measure Logic
                    child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                    double hiddenPanelHeight = (_hiddenRowsAtTop * _assumedControlSize.Height);

                    if (sizeSoFar.Width + child.DesiredSize.Width > availableSize.Width)
                    {
                        sizeSoFar.Width = 0.0;
                        sizeSoFar.Height += child.DesiredSize.Height;
                        SetNextPosition(child, sizeSoFar.Width, sizeSoFar.Height + hiddenPanelHeight);
                        sizeSoFar.Width += child.DesiredSize.Width;
                    }
                    else
                    {
                        SetNextPosition(child, sizeSoFar.Width, sizeSoFar.Height + hiddenPanelHeight);
                        sizeSoFar.Width += child.DesiredSize.Width;
                        maxWidth = Math.Max(maxWidth, sizeSoFar.Width);
                    }
                    #endregion Measure Logic
                }
            }

            DisconnectRecycledContainers();

            _previousMeasureSize = new Size(maxWidth, availableSize.Height);// sizeSoFar.Height);
            return _previousMeasureSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            UpdateScrollInfo(finalSize);

            for (int i = 0; i < _realizedChildren.Count; i++)
            {
                UIElement child = _realizedChildren[i];

                if (child.DesiredSize.Height == 0.0 || child.DesiredSize.Width == 0.0) continue;

                child.Arrange(new Rect(((Point)child.GetValue(NextPositionProperty)).X, ((Point)child.GetValue(NextPositionProperty)).Y, child.DesiredSize.Width, child.DesiredSize.Height));
            }

            return finalSize;
        }

        #endregion Overrides

        private static readonly DependencyProperty NextPositionProperty = DependencyProperty.RegisterAttached("NextPosition", typeof(Point), typeof(VirtualizingWrapPanel), new PropertyMetadata(new Point()));

        private void SetNextPosition(UIElement child, double x, double y)
        {
            if (child != null)
            {
                child.SetValue(NextPositionProperty, new Point(x, y));
            }
        }

        #region Virtualization
        private void InsertContainer(int childIndex, UIElement container, bool isRecycled)
        {
            // index in Children collection, whereas childIndex is the index into the _realizedChildren collection
            int visualTreeIndex = 0;
            UIElementCollection children = Children;

            if (childIndex > 0)
            {
                // find the item before where we want to insert the new item
                visualTreeIndex = ChildIndexFromRealizedIndex(childIndex - 1);
                visualTreeIndex++;
            }

            if (isRecycled && visualTreeIndex < children.Count && children[visualTreeIndex] == container)
            {
                // don't insert if a recycled container is in the proper place already
            }
            else
            {
                if (visualTreeIndex < children.Count)
                {
                    int insertIndex = visualTreeIndex;
                    if (isRecycled && VisualTreeHelper.GetParent(container) != null)
                    {
                        // If the container is recycled we have to remove it from its place in the visual tree and 
                        // insert it in the proper location.   We cant use an internal Move api, so we are removing
                        // and inserting the container
                        int containerIndex = children.IndexOf(container);
                        RemoveInternalChildRange(containerIndex, 1);
                        if (containerIndex < insertIndex)
                        {
                            insertIndex--;
                        }

                        InsertInternalChild(insertIndex, container);
                    }
                    else
                    {
                        InsertInternalChild(insertIndex, container);
                    }
                }
                else
                {
                    if (isRecycled && VisualTreeHelper.GetParent(container) != null)
                    {
                        // Recycled container is still in the tree; move it to the end
                        int originalIndex = children.IndexOf(container);
                        RemoveInternalChildRange(originalIndex, 1);
                        AddInternalChild(container);
                    }
                    else
                    {
                        AddInternalChild(container);
                    }
                }
            }

            // Keep realizedChildren in sync w/ the visual tree.
            _realizedChildren.Insert(childIndex, container);
            _generator.PrepareItemContainer(container);
        }

        /// <summary>
        ///     Takes an index from the realized list and returns the corresponding index in the Children collection
        /// </summary>
        private int ChildIndexFromRealizedIndex(int realizedChildIndex)
        {
            UIElementCollection children = Children;
            // If we're not recycling containers then we're not using a realizedChild index and no translation is necessary
            if (realizedChildIndex < _realizedChildren.Count)
            {
                UIElement child = _realizedChildren[realizedChildIndex];

                for (int i = realizedChildIndex; i < children.Count; i++)
                {
                    if (children[i] == child)
                    {
                        return i;
                    }
                }
            }

            return realizedChildIndex;
        }

        private void EnsureRealizedChildren()
        {
            if (_realizedChildren == null)
            {
                _realizedChildren = new List<UIElement>(Children.Count);

                for (int i = 0; i < Children.Count; i++)
                {
                    _realizedChildren.Add(Children[i]);
                }
            }
        }

        private void RecycleContainers()
        {
            if (Children.Count == 0) return;

            ItemsControl itemsControl = ItemsControl.GetItemsOwner(this);
            int recycleRangeStart = -1;
            int recycleRangeCount = 0;
            int childCount = _realizedChildren.Count;
            for (int i = 0; i < childCount; i++)
            {
                bool recycleContainer = false;

                int itemIndex = itemsControl.Items.IndexOf((_realizedChildren[i] as ContentPresenter).Content);

                if (itemIndex >= 0 && (itemIndex < _startPosition || itemIndex > _endPosition))
                {
                    recycleContainer = true;
                }

                if (!Children.Contains(_realizedChildren[i]))
                {
                    recycleContainer = false;
                    _realizedChildren.RemoveRange(i, 1);
                    i--;
                    childCount--;
                }

                if (recycleContainer)
                {
                    if (recycleRangeStart == -1)
                    {
                        recycleRangeStart = i;
                        recycleRangeCount = 1;
                    }
                    else
                    {
                        recycleRangeCount++;
                    }
                }
                else
                {
                    if (recycleRangeCount > 0)
                    {
                        GeneratorPosition position = new GeneratorPosition(recycleRangeStart, 0);
                        ((IRecyclingItemContainerGenerator)_generator).Recycle(position, recycleRangeCount);
                        _realizedChildren.RemoveRange(recycleRangeStart, recycleRangeCount);

                        childCount -= recycleRangeCount;
                        i -= recycleRangeCount;
                        recycleRangeCount = 0;
                        recycleRangeStart = -1;
                    }
                }
            }

            if (recycleRangeCount > 0)
            {
                GeneratorPosition position = new GeneratorPosition(recycleRangeStart, 0);
                ((IRecyclingItemContainerGenerator)_generator).Recycle(position, recycleRangeCount);
                _realizedChildren.RemoveRange(recycleRangeStart, recycleRangeCount);
            }
        }

        /// <summary>
        ///     Recycled containers still in the InternalChildren collection at the end of Measure should be disconnected
        ///     from the visual tree.  Otherwise they're still visible to things like Arrange, keyboard navigation, etc.
        /// </summary>
        private void DisconnectRecycledContainers()
        {
            int realizedIndex = 0;
            UIElement visualChild;
            UIElement realizedChild = _realizedChildren.Count > 0 ? _realizedChildren[0] : null;
            UIElementCollection children = Children;

            int removeStartRange = -1;
            int removalCount = 0;
            for (int i = 0; i < children.Count; i++)
            {
                visualChild = children[i];

                if (visualChild == realizedChild)
                {
                    if (removalCount > 0)
                    {
                        RemoveInternalChildRange(removeStartRange, removalCount);
                        i -= removalCount;
                        removalCount = 0;
                        removeStartRange = -1;
                    }

                    realizedIndex++;

                    if (realizedIndex < _realizedChildren.Count)
                    {
                        realizedChild = _realizedChildren[realizedIndex];
                    }
                    else
                    {
                        realizedChild = null;
                    }
                }
                else
                {
                    if (removeStartRange == -1)
                    {
                        removeStartRange = i;
                    }

                    removalCount++;
                }
            }

            if (removalCount > 0)
            {
                RemoveInternalChildRange(removeStartRange, removalCount);
            }
        }

        private void CalculateControlIndices()
        {
            _startPosition = _firstControlIndex;
            _endPosition = _lastControlIndex;

            int itemsCount = GetItemsCount();

            _firstVisibleIndex = GetFirstVisibleIndex();
            _lastVisibleIndex = GetLastVisibleIndex();

            _firstControlIndex = _firstVisibleIndex - _viewableColumns;
            if (_firstControlIndex < 0)
            {
                _firstControlIndex = 0;
            }

            _lastControlIndex = _lastVisibleIndex + _viewableColumns;
            if (_lastControlIndex + 1 > itemsCount)
            {
                _lastControlIndex = itemsCount - 1;
                if (_lastControlIndex < 0) _lastControlIndex = 0;
            }

            _startPosition = _firstControlIndex;
            _endPosition = _lastControlIndex;

            if (_viewableColumns == 0)
            {
                _hiddenRowsAtTop = 0;
            }
            else
            {
                _hiddenRowsAtTop = (int)Math.Floor(_startPosition / _viewableColumns);
            }
        }

        private int GetItemsCount()
        {
            ItemsControl itemsControl = ItemsControl.GetItemsOwner(this);
            return itemsControl.Items.Count;
        }

        private void CalculateViewableDimensions()
        {
            _viewableColumns = (int)Math.Floor(_viewport.Width / _assumedControlSize.Width);
            _viewableRows = (int)Math.Ceiling(_viewport.Height / _assumedControlSize.Height) + 1;
        }

        private int GetFirstVisibleIndex()
        {
            int result = 0;
            int itemCount = GetItemsCount();
            if (itemCount > 0)
            {
                CalculateViewableDimensions();
                result = (int)Math.Floor(-_translateTransform.Y / _assumedControlSize.Height) * _viewableColumns;
            }
            return result;
        }

        private int GetLastVisibleIndex()
        {
            int result = GetItemsCount();
            CalculateViewableDimensions();
            if (result > 0)
            {
                if (_viewableRows == 0 || _viewableColumns == 0)
                {
                    result = 0;
                }
                else
                {
                    result = Math.Min(_firstVisibleIndex + (_viewableRows * _viewableColumns) - 1, result);
                }
            }

            return result;
        }
        #endregion Virtualization

        private void UpdateScrollInfo(Size availableSize)
        {
            Size extent = MeasureExtent(availableSize);
            Size oldExtent = _extent;
            if (_extent != extent)
            {
                _extent = extent;
            }

            if (availableSize != _viewport)
            {
                _viewport = availableSize;
            }

            UpdateOffset(oldExtent, extent);

            if (_scrollOwner != null)
                _scrollOwner.InvalidateScrollInfo();
        }

        private void UpdateOffset(Size oldExtent, Size newExtent)
        {
            if (oldExtent == newExtent) return;

            if (newExtent.Height == 0 || oldExtent.Height == 0) return;

            double yFactor = newExtent.Height / oldExtent.Height;

            AnimateVerticalOffset((_offset.Y * yFactor) - _offset.Y);
        }

        private Size MeasureExtent(Size availableSize)
        {
            int itemCount = GetItemsCount();
            double colCount = Math.Floor(availableSize.Width / _assumedControlSize.Width);
            colCount = Math.Min(colCount, itemCount);
            colCount = colCount >= 1 ? colCount : 1;

            double rowCount = Math.Ceiling(itemCount / colCount);

            Size result = new Size(colCount * _assumedControlSize.Width, (rowCount * _assumedControlSize.Height));
            return result;
        }

        #region Scrolling Animations

        internal double OffsetMediator
        {
            get { return (double)GetValue(VerticalScrollOffsetProperty); }
            set { SetValue(VerticalScrollOffsetProperty, value); }
        }

        public void SetVerticalOffset(double value)
        {
            SetValue(VerticalScrollOffsetProperty, value);
            _scrollTarget = value;
        }

        private void AnimateVerticalOffsetSmoothly(double offset)
        {
            if (Math.Sign(offset) < 0)
            {
                if (_scrollDirection == ScrollDirection.Down) //scroll direction reversed while animating. Flip around immediately
                {
                    _scrollTarget = ScrollOwner.VerticalOffset;
                }
                _scrollDirection = ScrollDirection.Up;
            }
            else if (Math.Sign(offset) > 0)
            {
                if (_scrollDirection == ScrollDirection.Up) //scroll direction reversed while animating. Flip around immediately
                {
                    _scrollTarget = ScrollOwner.VerticalOffset;
                }
                _scrollDirection = ScrollDirection.Down;
            }

            _scrollTarget += offset;
            _scrollTarget = Math.Max(Math.Min(_scrollTarget, _extent.Height), 0);

            _smoothScrollStoryboard.Pause();
            _smoothScrollAnimation.To = _scrollTarget;
            _smoothScrollAnimation.From = ScrollOwner.VerticalOffset;

            if (_smoothScrollAnimation.From != _smoothScrollAnimation.To)
            {
                _smoothScrollStoryboard.Begin();
            }
        }

        private void AnimateVerticalOffset(double offset)
        {
            if (Math.Sign(offset) < 0)
            {
                if (_scrollDirection == ScrollDirection.Down) //scroll direction reversed while animating. Flip around immediately
                {
                    _scrollTarget = ScrollOwner.VerticalOffset;
                }
                _scrollDirection = ScrollDirection.Up;
            }
            else if (Math.Sign(offset) > 0)
            {
                if (_scrollDirection == ScrollDirection.Up) //scroll direction reversed while animating. Flip around immediately
                {
                    _scrollTarget = ScrollOwner.VerticalOffset;
                }
                _scrollDirection = ScrollDirection.Down;
            }

            _scrollTarget += offset;
            _scrollTarget = Math.Max(Math.Min(_scrollTarget, _extent.Height), 0);

            _scrollStoryboard.Pause();
            _scrollAnimation.To = _scrollTarget;
            _scrollAnimation.From = ScrollOwner.VerticalOffset;

            if (_scrollAnimation.From != _scrollAnimation.To)
            {
                _scrollStoryboard.Begin();
            }
        }

        public void SetVerticalOffsetViaMediator(double offset)
        {
            if (offset < 0 || _viewport.Height >= _extent.Height)
            {
                offset = 0;
            }
            else if (offset + _viewport.Height >= _extent.Height)
            {
                offset = _extent.Height - _viewport.Height;
            }

            _offset.Y = offset;

            if (_scrollOwner != null)
                _scrollOwner.InvalidateScrollInfo();

            _translateTransform.Y = -offset;

            InvalidateMeasure();
        }

        internal static readonly DependencyProperty VerticalScrollOffsetProperty =
           DependencyProperty.Register("VerticalScrollOffset", typeof(double), typeof(VirtualizingWrapPanel), new PropertyMetadata(0.0, OnVerticalScrollOffsetPropertyChanged));

        private static void OnVerticalScrollOffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VirtualizingWrapPanel panel = d as VirtualizingWrapPanel;
            if (panel != null && panel.ScrollOwner != null)
            {
                panel.SetVerticalOffsetViaMediator((double)e.NewValue);
            }
        }

        private void CreateScrollStoryboard()
        {
            _scrollStoryboard = new Storyboard();
            _scrollAnimation = new DoubleAnimation()
            {
                Duration = TimeSpan.FromSeconds(.3),
                EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseOut }
            };
            _scrollAnimation.SetValue(Storyboard.TargetPropertyProperty, new PropertyPath("VerticalScrollOffset"));
            Storyboard.SetTarget(_scrollAnimation, this);
            _scrollStoryboard.Children.Add(_scrollAnimation);
            _scrollStoryboard.Completed += (s, e) =>
            {
                _scrollDirection = ScrollDirection.None;
            };
        }

        private void CreateSmoothScrollStoryboard()
        {
            _smoothScrollStoryboard = new Storyboard();
            _smoothScrollAnimation = new DoubleAnimation()
            {
                Duration = TimeSpan.FromSeconds(.3),
            };
            _smoothScrollAnimation.SetValue(Storyboard.TargetPropertyProperty, new PropertyPath("VerticalScrollOffset"));
            Storyboard.SetTarget(_smoothScrollAnimation, this);
            _smoothScrollStoryboard.Children.Add(_smoothScrollAnimation);
            _smoothScrollStoryboard.Completed += (s, e) =>
            {
                _scrollDirection = ScrollDirection.None;
            };
        }
        #endregion Scrolling Animations

        #region IScrollInfo

        public ScrollViewer ScrollOwner
        {
            get
            {
                return _scrollOwner;
            }
            set
            {
                if (value != null)
                {
                    _scrollOwner = value;
                }
            }
        }

        void _scrollOwner_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            AnimateVerticalOffset(-Math.Sign(e.Delta) * _assumedControlSize.Height);
        }

        public bool CanVerticallyScroll
        {
            get
            {
                return _vertScrollable;
            }
            set
            {
                _vertScrollable = value;
            }
        }

        public double ExtentHeight
        {
            get
            {
                return _extent.Height;
            }
        }

        public double ExtentWidth
        {
            get
            {
                return _extent.Width;
            }
        }

        public double HorizontalOffset
        {
            get
            {
                return _offset.X;
            }
        }

        public double VerticalOffset
        {
            get
            {
                return _offset.Y;
            }
        }

        public double ViewportHeight
        {
            get
            {
                return _viewport.Height;
            }
        }

        public double ViewportWidth
        {
            get
            {
                return _viewport.Width;
            }
        }

        public void LineUp()
        {
            AnimateVerticalOffsetSmoothly(-20);
        }

        public void LineDown()
        {
            AnimateVerticalOffsetSmoothly(20);
        }

        public void PageUp()
        {
            AnimateVerticalOffset(-_viewport.Height);
        }

        public void PageDown()
        {
            AnimateVerticalOffset(_viewport.Height);
        }



        public Rect MakeVisible(UIElement visual, Rect rectangle)
        {
            return new Rect();
        }
        #region Not Implemented
        public void SetHorizontalOffset(double offset) { }
        public bool CanHorizontallyScroll
        {
            get
            {
                return false;
            }
            set { }
        }

        public void LineLeft() { }
        public void LineRight() { }
        public void MouseWheelUp() { }
        public void MouseWheelDown() { }
        public void MouseWheelLeft() { }
        public void MouseWheelRight() { }

        public void PageLeft() { }
        public void PageRight() { }

        #endregion Not Implemented
        #endregion IScrollInfo

    }

    public enum ScrollDirection
    {
        None = 0,
        Up = 1,
        Down = 2
    }
}