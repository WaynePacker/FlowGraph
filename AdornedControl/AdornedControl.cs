﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Diagnostics;
using Utils;

namespace AdornedControl
{
    /// <summary>
    /// A content control that allows an adorner for the content to
    /// be defined in XAML.
    /// </summary>
    public class AdornedControl : ContentControl
    {
        #region Dependency Properties / Event Definitions

        /// <summary>
        /// Dependency properties.
        /// </summary>
        public static readonly DependencyProperty IsAdornerVisibleProperty =
            DependencyProperty.Register("IsAdornerVisible", typeof(bool), typeof(AdornedControl),
                new FrameworkPropertyMetadata(IsAdornerVisible_PropertyChanged));

        public static readonly DependencyProperty AdornerContentProperty =
            DependencyProperty.Register("AdornerContent", typeof(FrameworkElement), typeof(AdornedControl),
                new FrameworkPropertyMetadata(AdornerContent_PropertyChanged));

        public static readonly DependencyProperty HorizontalAdornerPlacementProperty =
            DependencyProperty.Register("HorizontalAdornerPlacement", typeof(AdornerPlacement), typeof(AdornedControl),
                new FrameworkPropertyMetadata(AdornerPlacement.Inside));

        public static readonly DependencyProperty VerticalAdornerPlacementProperty =
            DependencyProperty.Register("VerticalAdornerPlacement", typeof(AdornerPlacement), typeof(AdornedControl),
                new FrameworkPropertyMetadata(AdornerPlacement.Inside));

        public static readonly DependencyProperty AdornerOffsetXProperty =
            DependencyProperty.Register("AdornerOffsetX", typeof(double), typeof(AdornedControl));
        public static readonly DependencyProperty AdornerOffsetYProperty =
            DependencyProperty.Register("AdornerOffsetY", typeof(double), typeof(AdornedControl));

        public static readonly DependencyProperty IsMouseOverShowEnabledProperty =
            DependencyProperty.Register("IsMouseOverShowEnabled", typeof(bool), typeof(AdornedControl),
                new FrameworkPropertyMetadata(true, IsMouseOverShowEnabled_PropertyChanged));

        public static readonly DependencyProperty FadeInTimeProperty =
            DependencyProperty.Register("FadeInTime", typeof(double), typeof(AdornedControl),
                new FrameworkPropertyMetadata(0.25));

        public static readonly DependencyProperty FadeOutTimeProperty =
            DependencyProperty.Register("FadeOutTime", typeof(double), typeof(AdornedControl),
                new FrameworkPropertyMetadata(1.0));

        public static readonly DependencyProperty CloseAdornerTimeOutProperty =
            DependencyProperty.Register("CloseAdornerTimeOut", typeof(double), typeof(AdornedControl),
                new FrameworkPropertyMetadata(2.0, CloseAdornerTimeOut_PropertyChanged));

        public static readonly DependencyProperty AdornedTemplatePartNameProperty =
            DependencyProperty.Register("AdornedTemplatePartName", typeof(string), typeof(AdornedControl),
                new FrameworkPropertyMetadata(null));

        public static readonly RoutedEvent AdornerShownEvent =
            EventManager.RegisterRoutedEvent("AdornerShown", RoutingStrategy.Bubble, typeof(AdornerEventHandler), typeof(AdornedControl));

        public static readonly RoutedEvent AdornerHiddenEvent =
            EventManager.RegisterRoutedEvent("AdornerHidden", RoutingStrategy.Bubble, typeof(AdornerEventHandler), typeof(AdornedControl));

        #endregion Dependency Properties / Event Definitions

        #region Commands

        /// <summary>
        /// Commands.
        /// </summary>
        public static readonly RoutedCommand ShowAdornerCommand = new RoutedCommand("ShowAdorner", typeof(AdornedControl));
        public static readonly RoutedCommand FadeInAdornerCommand = new RoutedCommand("FadeInAdorner", typeof(AdornedControl));
        public static readonly RoutedCommand HideAdornerCommand = new RoutedCommand("HideAdorner", typeof(AdornedControl));
        public static readonly RoutedCommand FadeOutAdornerCommand = new RoutedCommand("FadeOutAdorner", typeof(AdornedControl));

        #endregion Commands

        public AdornedControl()
        {
            this.Focusable = false; // By default don't want 'AdornedControl' to be focusable.

            this.DataContextChanged += new DependencyPropertyChangedEventHandler(AdornedControl_DataContextChanged);

            closeAdornerTimer.Tick += new EventHandler(closeAdornerTimer_Tick);
            closeAdornerTimer.Interval = TimeSpan.FromSeconds(CloseAdornerTimeOut);
        }

        /// <summary>
        /// Show the adorner.
        /// </summary>
        public void ShowAdorner()
        {
            IsAdornerVisible = true;

            adornerShowState = AdornerShowState.Visible;

            if (IsMouseOverShowEnabled && !IsMouseOver)
            {
                closeAdornerTimer.Start();
            }
        }

        /// <summary>
        /// Hide the adorner.
        /// </summary>
        public void HideAdorner()
        {
            IsAdornerVisible = false;

            closeAdornerTimer.Stop();
            adornerShowState = AdornerShowState.Hidden;
        }

        /// <summary>
        /// Fade the adorner in and make it visible.
        /// </summary>
        public void FadeInAdorner()
        {
            if (adornerShowState == AdornerShowState.Visible ||
                adornerShowState == AdornerShowState.FadingIn)
            {
                // Already visible or fading in.
                return;
            }

            this.ShowAdorner();

            if (adornerShowState != AdornerShowState.FadingOut)
            {
                adorner.Opacity = 0.0;
            }

            DoubleAnimation doubleAnimation = new DoubleAnimation(1.0, new Duration(TimeSpan.FromSeconds(FadeInTime)));
            doubleAnimation.Completed += new EventHandler(fadeInAnimation_Completed);
            doubleAnimation.Freeze();
                
            adorner.BeginAnimation(FrameworkElement.OpacityProperty, doubleAnimation);

            adornerShowState = AdornerShowState.FadingIn;
        }

        /// <summary>
        /// Fade the adorner out and make it visible.
        /// </summary>
        public void FadeOutAdorner()
        {
            if (adornerShowState == AdornerShowState.FadingOut)
            {
                //
                // Already fading out.
                //
                return;
            }

            if (adornerShowState == AdornerShowState.Hidden)
            {
                //
                // Adorner has already been hidden.
                //
                return;
            }

            DoubleAnimation fadeOutAnimation = new DoubleAnimation(0.0, new Duration(TimeSpan.FromSeconds(FadeOutTime)));
            fadeOutAnimation.Completed += new EventHandler(fadeOutAnimation_Completed);
            fadeOutAnimation.Freeze();

            adorner.BeginAnimation(FrameworkElement.OpacityProperty, fadeOutAnimation);

            adornerShowState = AdornerShowState.FadingOut;
        }

        /// <summary>
        /// Shows or hides the adorner.
        /// Set to 'true' to show the adorner or 'false' to hide the adorner.
        /// </summary>
        public bool IsAdornerVisible
        {
            get
            {
                return (bool)GetValue(IsAdornerVisibleProperty);
            }
            set
            {
                SetValue(IsAdornerVisibleProperty, value);
            }
        }

        /// <summary>
        /// Used in XAML to define the UI content of the adorner.
        /// </summary>
        public FrameworkElement AdornerContent
        {
            get
            {
                return (FrameworkElement)GetValue(AdornerContentProperty);
            }
            set
            {
                SetValue(AdornerContentProperty, value);
            }
        }

        /// <summary>
        /// Specifies the horizontal placement of the adorner relative to the adorned control.
        /// </summary>
        public AdornerPlacement HorizontalAdornerPlacement
        {
            get
            {
                return (AdornerPlacement)GetValue(HorizontalAdornerPlacementProperty);
            }
            set
            {
                SetValue(HorizontalAdornerPlacementProperty, value);
            }
        }

        /// <summary>
        /// Specifies the vertical placement of the adorner relative to the adorned control.
        /// </summary>
        public AdornerPlacement VerticalAdornerPlacement
        {
            get
            {
                return (AdornerPlacement)GetValue(VerticalAdornerPlacementProperty);
            }
            set
            {
                SetValue(VerticalAdornerPlacementProperty, value);
            }
        }

        /// <summary>
        /// X offset of the adorner.
        /// </summary>
        public double AdornerOffsetX
        {
            get
            {
                return (double)GetValue(AdornerOffsetXProperty);
            }
            set
            {
                SetValue(AdornerOffsetXProperty, value);
            }
        }

        /// <summary>
        /// Y offset of the adorner.
        /// </summary>
        public double AdornerOffsetY
        {
            get
            {
                return (double)GetValue(AdornerOffsetYProperty);
            }
            set
            {
                SetValue(AdornerOffsetYProperty, value);
            }
        }

        /// <summary>
        /// Set to 'true' to make the adorner automatically fade-in and become visible when the mouse is hovered
        /// over the adorned control.  Also the adorner automatically fades-out when the mouse cursor is moved
        /// aware from the adorned control (and the adorner).
        /// </summary>
        public bool IsMouseOverShowEnabled
        {
            get
            {
                return (bool)GetValue(IsMouseOverShowEnabledProperty);
            }
            set
            {
                SetValue(IsMouseOverShowEnabledProperty, value);
            }
        }

        /// <summary>
        /// Specifies the time (in seconds) it takes to fade in the adorner.
        /// </summary>
        public double FadeInTime
        {
            get
            {
                return (double)GetValue(FadeInTimeProperty);
            }
            set
            {
                SetValue(FadeInTimeProperty, value);
            }
        }

        /// <summary>
        /// Specifies the time (in seconds) it takes to fade out the adorner.
        /// </summary>
        public double FadeOutTime
        {
            get
            {
                return (double) GetValue(FadeOutTimeProperty);
            }
            set
            {
                SetValue(FadeOutTimeProperty, value);
            }
        }

        /// <summary>
        /// Specifies the time (in seconds) after the mouse cursor moves away from the 
        /// adorned control (or the adorner) when the adorner begins to fade out.
        /// </summary>
        public double CloseAdornerTimeOut
        {
            get
            {
                return (double)GetValue(CloseAdornerTimeOutProperty);
            }
            set
            {
                SetValue(CloseAdornerTimeOutProperty, value);
            }
        }

        /// <summary>
        /// By default this property is set to null.
        /// When set to non-null it specifies the part name of a UI element
        /// in the visual tree of the AdornedControl content that is to be adorned.
        /// When this property is null it is the AdornerControl content that is adorned,
        /// however when it is set the visual-tree is searched for a UI element that has the
        /// specified part name, if the part is found then that UI element is adorned, otherwise
        /// an exception "Failed to find part ..." is thrown.        /// 
        /// </summary>
        public string AdornedTemplatePartName
        {
            get
            {
                return (string)GetValue(AdornedTemplatePartNameProperty);
            }
            set
            {
                SetValue(AdornedTemplatePartNameProperty, value);
            }
        }

        /// <summary>
        /// Event raised when the adorner is shown.
        /// </summary>
        public event AdornerEventHandler AdornerShown
        {
            add { AddHandler(AdornerShownEvent, value); }
            remove { RemoveHandler(AdornerShownEvent, value); }
        }

        /// <summary>
        /// Event raised when the adorner is hidden.
        /// </summary>
        public event AdornerEventHandler AdornerHidden
        {
            add { AddHandler(AdornerHiddenEvent, value); }
            remove { RemoveHandler(AdornerHiddenEvent, value); }
        }

        #region Private Data Members

        /// <summary>
        /// Command bindings.
        /// </summary>
        private static readonly CommandBinding ShowAdornerCommandBinding = new CommandBinding(ShowAdornerCommand, ShowAdornerCommand_Executed);
        private static readonly CommandBinding FadeInAdornerCommandBinding = new CommandBinding(FadeInAdornerCommand, FadeInAdornerCommand_Executed);
        private static readonly CommandBinding HideAdornerCommandBinding = new CommandBinding(HideAdornerCommand, HideAdornerCommand_Executed);
        private static readonly CommandBinding FadeOutAdornerCommandBinding = new CommandBinding(FadeInAdornerCommand, FadeOutAdornerCommand_Executed);

        /// <summary>
        /// Specifies the current show/hide state of the adorner.
        /// </summary>
        private enum AdornerShowState
        {
            Visible,
            Hidden,
            FadingIn,
            FadingOut,
        }

        /// <summary>
        /// Specifies the current show/hide state of the adorner.
        /// </summary>
        private AdornerShowState adornerShowState = AdornerShowState.Hidden;

        /// <summary>
        /// Caches the adorner layer.
        /// </summary>
        private AdornerLayer adornerLayer = null;

        /// <summary>
        /// The actual adorner create to contain our 'adorner UI content'.
        /// </summary>
        private FrameworkElementAdorner adorner = null;

        /// <summary>
        /// This timer is used to fade out and close the adorner.
        /// </summary>
        private DispatcherTimer closeAdornerTimer = new DispatcherTimer();
        
        #endregion

        #region Private/Internal Functions

        /// <summary>
        /// Static constructor to register command bindings.
        /// </summary>
        static AdornedControl()
        {
            CommandManager.RegisterClassCommandBinding(typeof(AdornedControl), ShowAdornerCommandBinding);
            CommandManager.RegisterClassCommandBinding(typeof(AdornedControl), FadeOutAdornerCommandBinding);
            CommandManager.RegisterClassCommandBinding(typeof(AdornedControl), HideAdornerCommandBinding);
            CommandManager.RegisterClassCommandBinding(typeof(AdornedControl), FadeInAdornerCommandBinding);
        }

        /// <summary>
        /// Event raised when the DataContext of the adorned control changes.
        /// </summary>
        private void AdornedControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateAdornerDataContext();
        }

        /// <summary>
        /// Update the DataContext of the adorner from the adorned control.
        /// </summary>
        private void UpdateAdornerDataContext()
        {
            if (this.AdornerContent != null)
            {
                this.AdornerContent.DataContext = this.DataContext;
            }
        }

        /// <summary>
        /// Event raised when the Show command is executed.
        /// </summary>
        private static void ShowAdornerCommand_Executed(object target, ExecutedRoutedEventArgs e)
        {
            AdornedControl c = (AdornedControl)target;
            c.ShowAdorner();
        }

        /// <summary>
        /// Event raised when the FadeIn command is executed.
        /// </summary>
        private static void FadeInAdornerCommand_Executed(object target, ExecutedRoutedEventArgs e)
        {
            AdornedControl c = (AdornedControl)target;
            c.FadeOutAdorner();
        }

        /// <summary>
        /// Event raised when the Hide command is executed.
        /// </summary>
        private static void HideAdornerCommand_Executed(object target, ExecutedRoutedEventArgs e)
        {
            AdornedControl c = (AdornedControl)target;
            c.HideAdorner();
        }

        /// <summary>
        /// Event raised when the FadeOut command is executed.
        /// </summary>
        private static void FadeOutAdornerCommand_Executed(object target, ExecutedRoutedEventArgs e)
        {
            AdornedControl c = (AdornedControl)target;
            c.FadeOutAdorner();
        }

        /// <summary>
        /// Event raised when the value of IsAdornerVisible has changed.
        /// </summary>
        private static void IsAdornerVisible_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            AdornedControl c = (AdornedControl)o;
            if (c.AdornerContent == null)
            {
                //
                // Adorner content not loaded yet, can't do anything.
                //
                return;
            }

            c.ShowOrHideAdornerInternal();
        }

        /// <summary>
        /// Event raised when the IsMouseOverShowEnabled property has changed.
        /// </summary>
        private static void IsMouseOverShowEnabled_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            AdornedControl c = (AdornedControl)o;
            c.closeAdornerTimer.Stop();
            c.HideAdorner();
        }

        /// <summary>
        /// Event raised when the CloseAdornerTimeOut property has change.
        /// </summary>
        private static void CloseAdornerTimeOut_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            AdornedControl c = (AdornedControl)o;
            c.closeAdornerTimer.Interval = TimeSpan.FromSeconds(c.CloseAdornerTimeOut);
        }

        /// <summary>
        /// Event raised when the value of AdornerContent has changed.
        /// </summary>
        private static void AdornerContent_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            AdornedControl c = (AdornedControl)o;
            c.ShowOrHideAdornerInternal();

            FrameworkElement oldAdornerContent = (FrameworkElement)e.OldValue;
            if (oldAdornerContent != null)
            {
                oldAdornerContent.MouseEnter -= new MouseEventHandler(c.adornerContent_MouseEnter);
                oldAdornerContent.MouseLeave -= new MouseEventHandler(c.adornerContent_MouseLeave);
            }

            FrameworkElement newAdornerContent = (FrameworkElement)e.NewValue;
            if (newAdornerContent != null)
            {
                newAdornerContent.MouseEnter += new MouseEventHandler(c.adornerContent_MouseEnter);
                newAdornerContent.MouseLeave += new MouseEventHandler(c.adornerContent_MouseLeave);
            }
        }

        /// <summary>
        /// Event raised when the mouse cursor enters the area of the adorner.
        /// </summary>
        private void adornerContent_MouseEnter(object sender, MouseEventArgs e)
        {
            MouseEnterLogic();
        }

        /// <summary>
        /// Event raised when the mouse cursor leaves the area of the adorner.
        /// </summary>
        private void adornerContent_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseLeaveLogic();
        }

        /// <summary>
        /// Internal method to show or hide the adorner based on the value of IsAdornerVisible.
        /// </summary>
        private void ShowOrHideAdornerInternal()
        {
            if (IsAdornerVisible)
            {
                ShowAdornerInternal();
            }
            else
            {
                HideAdornerInternal();
            }
        }

        /// <summary>
        /// Finds a child element in the visual tree that has the specified name.
        /// Returns null if no child with that name exists.
        /// </summary>
        public static FrameworkElement FindNamedChild(FrameworkElement rootElement, string childName)
        {
            int numChildren = VisualTreeHelper.GetChildrenCount(rootElement);
            for (int i = 0; i < numChildren; ++i)
            {
                DependencyObject child = VisualTreeHelper.GetChild(rootElement, i);
                FrameworkElement childElement = child as FrameworkElement;
                if (childElement != null && childElement.Name == childName)
                {
                    return childElement;
                }

                FrameworkElement foundElement = FindNamedChild(childElement, childName);
                if (foundElement != null)
                {
                    return foundElement;
                }
            }

            return null;
        }


        /// <summary>
        /// Internal method to show the adorner.
        /// </summary>
        private void ShowAdornerInternal()
        {
            if (this.adorner != null)
            {
                // Already adorned.
                return;
            }

            AddAdorner();

            RaiseEvent(new AdornerEventArgs(AdornerShownEvent, this, this.adorner.Child));
        }

        /// <summary>
        /// Internal method to hide the adorner.
        /// </summary>
        private void HideAdornerInternal()
        {
            if (this.adornerLayer == null || this.adorner == null)
            {
                // Not already adorned.
                return;
            }

            RaiseEvent(new AdornerEventArgs(AdornerHiddenEvent, this, this.adorner.Child));

            RemoveAdorner();
        }

        /// <summary>
        /// Called to build the visual tree.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ShowOrHideAdornerInternal();
        }

        /// <summary>
        /// Called when the mouse cursor enters the area of the adorned control.
        /// </summary>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            MouseEnterLogic();
        }

        /// <summary>
        /// Called when the mouse cursor leaves the area of the adorned control.
        /// </summary>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            MouseLeaveLogic();
        }

        /// <summary>
        /// Shared mouse enter code.
        /// </summary>
        private void MouseEnterLogic()
        {
            if (!IsMouseOverShowEnabled)
            {
                return;
            }

            closeAdornerTimer.Stop();

            FadeInAdorner();
        }

        /// <summary>
        /// Shared mouse leave code.
        /// </summary>
        private void MouseLeaveLogic()
        {
            if (!IsMouseOverShowEnabled)
            {
                return;
            }

            closeAdornerTimer.Start();
        }

        /// <summary>
        /// Called when the close adorner time-out has ellapsed, the mouse has moved
        /// away from the adorned control and the adorner and it is time to close the adorner.
        /// </summary>
        private void closeAdornerTimer_Tick(object sender, EventArgs e)
        {
            closeAdornerTimer.Stop();

            FadeOutAdorner();
        }

        /// <summary>
        /// Event raised when the fade in animation has completed.
        /// </summary>
        private void fadeInAnimation_Completed(object sender, EventArgs e)
        {
            if (adornerShowState == AdornerShowState.FadingIn)
            {
                // Still fading in, eg it wasn't aborted.
                adornerShowState = AdornerShowState.Visible;
            }
        }

        /// <summary>
        /// Event raised when the fade-out animation has completed.
        /// </summary>
        private void fadeOutAnimation_Completed(object sender, EventArgs e)
        {
            if (adornerShowState == AdornerShowState.FadingOut)
            {
                // Still fading out, eg it wasn't aborted.
                this.HideAdorner();
            }
        }

        /// <summary>
        /// Instance the adorner and add it to the adorner layer.
        /// </summary>
        private void AddAdorner()
        {
            if (this.AdornerContent != null)
            {
                if (this.adornerLayer == null)
                {
                    this.adornerLayer = AdornerLayer.GetAdornerLayer(this);
                }

                if (this.adornerLayer != null)
                {
                    FrameworkElement adornedControl = this; // The control to be adorned defaults to 'this'.

                    if (!string.IsNullOrEmpty(this.AdornedTemplatePartName))
                    {
                        //
                        // If 'AdornedTemplatePartName' is set to a valid string then search the visual-tree
                        // for a UI element that has the specified part name.  If we find it then use it as the
                        // adorned control, otherwise throw an exception.
                        //
                        adornedControl = FindNamedChild(this, this.AdornedTemplatePartName);
                        if (adornedControl == null)
                        {
                            throw new ApplicationException("Failed to find a FrameworkElement in the visual-tree with the part name '" + this.AdornedTemplatePartName + "'.");
                        }
                    }

                    this.adorner = new FrameworkElementAdorner(this.AdornerContent, adornedControl, 
                                                               this.HorizontalAdornerPlacement, this.VerticalAdornerPlacement,
                                                               this.AdornerOffsetX, this.AdornerOffsetY);
                    this.adornerLayer.Add(this.adorner);

					//
					// Update the layout of the adorner layout so that clients that depend
					// on the 'AdornerShown' event can use the visual tree of the adorner.
					//
					this.adornerLayer.UpdateLayout();

                    UpdateAdornerDataContext();
                }
            }
        }

        /// <summary>
        /// Remove the adorner from the adorner layer and let it be garbage collected.
        /// </summary>
        private void RemoveAdorner()
        {
            //
            // Stop the timer that might be about to fade out the adorner.
            //
            closeAdornerTimer.Stop();

            if (this.adornerLayer != null && this.adorner != null)
            {
                this.adornerLayer.Remove(this.adorner);
                this.adorner.DisconnectChild();
            }

            this.adorner = null;
            this.adornerLayer = null;

            //
            // Ensure that the state of the adorned control reflects that
            // the the adorner is no longer.
            //
            this.IsAdornerVisible = false;
            this.adornerShowState = AdornerShowState.Hidden;
		}

        #endregion
    }
}
