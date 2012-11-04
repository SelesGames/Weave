﻿// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

// ---
// Important Workaround Note for developers using the BETA:
// There is a workaround in code that removes any CacheMode from the content of
// the control. It works around a platform bug that is slated to be fixed for
// release.
//
// If you are using the beta tools, remove the comment below:
// #define WORKAROUND_BITMAP_CACHE_BUG
// ---

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace weave
{
    /// <summary>
    /// A content control designed to wrap anything in Silverlight with a user
    /// experience concept called 'tilt', applying a transformation during 
    /// manipulation by a user.
    /// </summary>
    public class TiltContentControl : ContentControl
    {
        #region Constants
        /// <summary>
        /// Maximum angle for the tilt effect, defined in Radians.
        /// </summary>
        private const double MaxAngle = 0.3;
        
        /// <summary>
        /// The maximum depression for the tilt effect, given in pixel units.
        /// </summary>
        private const double MaxDepression = 25;

        /// <summary>
        /// The number of seconds for a tilt revert to take.
        /// </summary>
        private static readonly Duration TiltUpAnimationDuration = new Duration(TimeSpan.FromSeconds(.5));

        /// <summary>
        /// A single logarithmic ease instance.
        /// </summary>
        private static readonly IEasingFunction LogEase = new LogarithmicEase();

        #endregion

        #region Static property instances
        /// <summary>
        /// Single instance of the Rotation X property.
        /// </summary>
        private static readonly PropertyPath RotationXProperty = new PropertyPath(PlaneProjection.RotationXProperty);

        /// <summary>
        /// Single instance of the Rotation Y property.
        /// </summary>
        private static readonly PropertyPath RotationYProperty = new PropertyPath(PlaneProjection.RotationYProperty);

        /// <summary>
        /// Single instance of the Global Offset Z property.
        /// </summary>
        private static readonly PropertyPath GlobalOffsetZProperty = new PropertyPath(PlaneProjection.GlobalOffsetZProperty);
        #endregion

        /// <summary>
        /// The content element instance.
        /// </summary>
        private ContentPresenter _presenter;

        /// <summary>
        /// The original width of the control.
        /// </summary>
        private double _width;

        /// <summary>
        /// The original height of the control.
        /// </summary>
        private double _height;

        /// <summary>
        /// The storyboard used for the tilt up effect.
        /// </summary>
        private Storyboard _tiltUpStoryboard;

        /// <summary>
        /// The plane projection used to show the tilt effect.
        /// </summary>
        private PlaneProjection _planeProjection;

        /// <summary>
        /// Overrides the method called when apply template is called. We assume
        /// that the implementation root is the content presenter.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _presenter = GetImplementationRoot(this) as ContentPresenter;
        }

        /// <summary>
        /// Overrides the maniupulation started event.
        /// </summary>
        /// <param name="e">The manipulation event arguments.</param>
        protected override void OnManipulationStarted(ManipulationStartedEventArgs e)
        {
            base.OnManipulationStarted(e);

            if (_presenter != null)
            {
//////////#if WORKAROUND_BITMAP_CACHE_BUG
//////////                // WORKAROUND NOTE:
//////////                // This is a workaround for a platform bug related to cache mode
//////////                // that should be fixed before final release of the platform.
//////////                UIElement elementContent = _contentElement.Content as UIElement;
//////////                if (elementContent != null && elementContent.CacheMode != null)
//////////                {
//////////                    elementContent.CacheMode = null;
//////////                }
//////////#endif
                _planeProjection = new PlaneProjection();
                _presenter.Projection = _planeProjection;

                _tiltUpStoryboard = new Storyboard();
                _tiltUpStoryboard.Completed += TiltUpCompleted;

                DoubleAnimation tiltUpRotateXAnimation = new DoubleAnimation();
                Storyboard.SetTarget(tiltUpRotateXAnimation, _planeProjection);
                Storyboard.SetTargetProperty(tiltUpRotateXAnimation, RotationXProperty);
                tiltUpRotateXAnimation.To = 0;
                tiltUpRotateXAnimation.EasingFunction = LogEase;
                tiltUpRotateXAnimation.Duration = TiltUpAnimationDuration;

                DoubleAnimation tiltUpRotateYAnimation = new DoubleAnimation();
                Storyboard.SetTarget(tiltUpRotateYAnimation, _planeProjection);
                Storyboard.SetTargetProperty(tiltUpRotateYAnimation, RotationYProperty);
                tiltUpRotateYAnimation.To = 0;
                tiltUpRotateYAnimation.EasingFunction = LogEase;
                tiltUpRotateYAnimation.Duration = TiltUpAnimationDuration;

                DoubleAnimation tiltUpOffsetZAnimation = new DoubleAnimation();
                Storyboard.SetTarget(tiltUpOffsetZAnimation, _planeProjection);
                Storyboard.SetTargetProperty(tiltUpOffsetZAnimation, GlobalOffsetZProperty);
                tiltUpOffsetZAnimation.To = 0;
                tiltUpOffsetZAnimation.EasingFunction = LogEase;
                tiltUpOffsetZAnimation.Duration = TiltUpAnimationDuration;

                _tiltUpStoryboard.Children.Add(tiltUpRotateXAnimation);
                _tiltUpStoryboard.Children.Add(tiltUpRotateYAnimation);
                _tiltUpStoryboard.Children.Add(tiltUpOffsetZAnimation);
            }
            if (_planeProjection != null)
            {
                _width = ActualWidth;
                _height = ActualHeight;
                if (_tiltUpStoryboard != null)
                {
                    _tiltUpStoryboard.Stop();
                }
                DepressAndTilt(e.ManipulationOrigin, e.ManipulationContainer);
            }
        }

        /// <summary>
        /// Handles the manipulation delta event.
        /// </summary>
        /// <param name="e">The manipulation event arguments.</param>
        protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
        {
            base.OnManipulationDelta(e);
            // Depress and tilt regardless of whether the event was handled.
            if (_planeProjection != null)
            {
                DepressAndTilt(e.ManipulationOrigin, e.ManipulationContainer);
            }
        }

        /// <summary>
        /// Handles the manipulation completed event.
        /// </summary>
        /// <param name="e">The manipulation event arguments.</param>
        protected override void OnManipulationCompleted(ManipulationCompletedEventArgs e)
        {
            base.OnManipulationCompleted(e);
            if (_planeProjection != null)
            {
                if (_tiltUpStoryboard != null)
                {
                    _tiltUpStoryboard.Begin();
                }
                else
                {
                    _planeProjection.RotationY = 0;
                    _planeProjection.RotationX = 0;
                    _planeProjection.GlobalOffsetZ = 0;
                }
            }
        }

        /// <summary>
        /// Updates the depression and tilt based on position of the 
        /// manipulation relative to the original origin from input.
        /// </summary>
        /// <param name="manipulationOrigin">The origin of manipulation.</param>
        /// <param name="manipulationContainer">The container instance.</param>
        private void DepressAndTilt(Point manipulationOrigin, UIElement manipulationContainer)
        {
            try
            {
                GeneralTransform transform = manipulationContainer.TransformToVisual(this);
                Point transformedOrigin = transform.Transform(manipulationOrigin);
                Point normalizedPoint = new Point(
                    Math.Min(Math.Max(transformedOrigin.X / _width, 0), 1),
                    Math.Min(Math.Max(transformedOrigin.Y / _height, 0), 1));
                double xMagnitude = Math.Abs(normalizedPoint.X - 0.5);
                double yMagnitude = Math.Abs(normalizedPoint.Y - 0.5);
                double xDirection = -Math.Sign(normalizedPoint.X - 0.5);
                double yDirection = Math.Sign(normalizedPoint.Y - 0.5);
                double angleMagnitude = xMagnitude + yMagnitude;
                double xAngleContribution = xMagnitude + yMagnitude > 0 ? xMagnitude / (xMagnitude + yMagnitude) : 0;
                double angle = angleMagnitude * MaxAngle * 180 / Math.PI;
                double depression = (1 - angleMagnitude) * MaxDepression;
                // RotationX and RotationY are the angles of rotations about the x- 
                // or y-axis. To achieve a rotation in the x- or y-direction, the
                // two must be swapped. So a rotation to the left about the y-axis 
                // is a rotation to the left in the x-direction, and a rotation up 
                // about the x-axis is a rotation up in the y-direction.
                _planeProjection.RotationY = angle * xAngleContribution * xDirection;
                _planeProjection.RotationX = angle * (1 - xAngleContribution) * yDirection;
                _planeProjection.GlobalOffsetZ = -depression;
            }
            catch (Exception)
            { }
        }

        /// <summary>
        /// Handles the tilt up completed event.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event arguments.</param>
        private void TiltUpCompleted(object sender, EventArgs e)
        {
            if (_tiltUpStoryboard != null)
            {
                _tiltUpStoryboard.Stop();
            }
            _tiltUpStoryboard = null;
            _planeProjection = null;
            _presenter.Projection = null;
        }

        /// <summary>
        /// An easing function of ln(t+1)/ln(2).
        /// </summary>
        private class LogarithmicEase : EasingFunctionBase
        {
            /// <summary>
            /// Constant value of ln(2) used in the easing function.
            /// </summary>
            private const double NaturalLog2 = 0.693147181;

            /// <summary>
            /// Overrides the EaseInCore method to provide the logic portion of
            /// an ease in.
            /// </summary>
            /// <param name="normalizedTime">Normalized time (progress) of the
            /// animation, which is a value from 0 through 1.</param>
            /// <returns>A double that represents the transformed progress.</returns>
            protected override double EaseInCore(double normalizedTime)
            {
                return Math.Log(normalizedTime + 1) / NaturalLog2;
            }
        }

        /// <summary>
        /// Gets the implementation root of the Control.
        /// </summary>
        /// <param name="dependencyObject">The DependencyObject.</param>
        /// <remarks>
        /// Implements Silverlight's corresponding internal property on Control.
        /// </remarks>
        /// <returns>Returns the implementation root or null.</returns>
        public static FrameworkElement GetImplementationRoot(DependencyObject dependencyObject)
        {
            Debug.Assert(dependencyObject != null, "DependencyObject should not be null.");
            return (1 == VisualTreeHelper.GetChildrenCount(dependencyObject)) ?
                VisualTreeHelper.GetChild(dependencyObject, 0) as FrameworkElement :
                null;
        }
    }
}
