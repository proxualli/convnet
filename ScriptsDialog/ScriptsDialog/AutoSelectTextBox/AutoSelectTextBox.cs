/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ScriptsDialog.AutoSelectTextBox
{
    public class AutoSelectTextBox : TextBox
    {
        public AutoSelectTextBox()
        {
        }

        #region AutoSelectBehavior PROPERTY

        public AutoSelectBehavior AutoSelectBehavior
        {
            get
            {
                return (AutoSelectBehavior)GetValue(AutoSelectBehaviorProperty);
            }
            set
            {
                SetValue(AutoSelectBehaviorProperty, value);
            }
        }

        public static readonly DependencyProperty AutoSelectBehaviorProperty =
            DependencyProperty.Register("AutoSelectBehavior", typeof(AutoSelectBehavior), typeof(AutoSelectTextBox),
            new UIPropertyMetadata(AutoSelectBehavior.Never));

        #endregion AutoSelectBehavior PROPERTY

        #region AutoMoveFocus PROPERTY

        public bool AutoMoveFocus
        {
            get
            {
                return (bool)GetValue(AutoMoveFocusProperty);
            }
            set
            {
                SetValue(AutoMoveFocusProperty, value);
            }
        }

        public static readonly DependencyProperty AutoMoveFocusProperty =
            DependencyProperty.Register("AutoMoveFocus", typeof(bool), typeof(AutoSelectTextBox), new UIPropertyMetadata(false));

        #endregion AutoMoveFocus PROPERTY

        #region QueryMoveFocus EVENT

        public static readonly RoutedEvent QueryMoveFocusEvent = EventManager.RegisterRoutedEvent("QueryMoveFocus",
                                                                                                    RoutingStrategy.Bubble,
                                                                                                    typeof(QueryMoveFocusEventHandler),
                                                                                                    typeof(AutoSelectTextBox));
        #endregion QueryMoveFocus EVENT

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (!this.AutoMoveFocus)
            {
                base.OnPreviewKeyDown(e);
                return;
            }

            if ((e.Key == Key.Left)
            && ((Keyboard.Modifiers == ModifierKeys.None)
                || (Keyboard.Modifiers == ModifierKeys.Control)))
            {
                e.Handled = this.MoveFocusLeft();
            }

            if ((e.Key == Key.Right)
            && ((Keyboard.Modifiers == ModifierKeys.None)
                || (Keyboard.Modifiers == ModifierKeys.Control)))
            {
                e.Handled = this.MoveFocusRight();
            }

            if (((e.Key == Key.Up) || (e.Key == Key.PageUp))
            && ((Keyboard.Modifiers == ModifierKeys.None)
                || (Keyboard.Modifiers == ModifierKeys.Control)))
            {
                e.Handled = this.MoveFocusUp();
            }

            if (((e.Key == Key.Down) || (e.Key == Key.PageDown))
            && ((Keyboard.Modifiers == ModifierKeys.None)
                || (Keyboard.Modifiers == ModifierKeys.Control)))
            {
                e.Handled = this.MoveFocusDown();
            }

            base.OnPreviewKeyDown(e);
        }

        protected override void OnPreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnPreviewGotKeyboardFocus(e);

            if (this.AutoSelectBehavior == AutoSelectBehavior.OnFocus)
            {
                // If the focus was not in one of our child ( or popup ), we select all the text.
                if (!TreeHelper.IsDescendantOf(e.OldFocus as DependencyObject, this))
                {
                    this.SelectAll();
                }
            }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            if (this.AutoSelectBehavior == AutoSelectBehavior.Never)
                return;

            if (this.IsKeyboardFocusWithin == false)
            {
                this.Focus();
                e.Handled = true;  //prevent from removing the selection
            }
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            if (!this.AutoMoveFocus)
                return;

            if ((this.Text.Length != 0)
                && (this.Text.Length == this.MaxLength)
                && (this.CaretIndex == this.MaxLength))
            {
                if (this.CanMoveFocus(FocusNavigationDirection.Right, true) == true)
                {
                    FocusNavigationDirection direction = (this.FlowDirection == FlowDirection.LeftToRight)
                    ? FocusNavigationDirection.Right
                    : FocusNavigationDirection.Left;

                    this.MoveFocus(new TraversalRequest(direction));
                }
            }
        }


        private bool CanMoveFocus(FocusNavigationDirection direction, bool reachedMax)
        {
            QueryMoveFocusEventArgs e = new QueryMoveFocusEventArgs(direction, reachedMax);
            this.RaiseEvent(e);
            return e.CanMoveFocus;
        }

        private bool MoveFocusLeft()
        {
            if (this.FlowDirection == FlowDirection.LeftToRight)
            {
                //occurs only if the cursor is at the beginning of the text
                if ((this.CaretIndex == 0) && (this.SelectionLength == 0))
                {
                    if (ComponentCommands.MoveFocusBack.CanExecute(null, this))
                    {
                        ComponentCommands.MoveFocusBack.Execute(null, this);
                        return true;
                    }
                    else if (this.CanMoveFocus(FocusNavigationDirection.Left, false))
                    {
                        this.MoveFocus(new TraversalRequest(FocusNavigationDirection.Left));
                        return true;
                    }
                }
            }
            else
            {
                //occurs only if the cursor is at the end of the text
                if ((this.CaretIndex == this.Text.Length) && (this.SelectionLength == 0))
                {
                    if (ComponentCommands.MoveFocusBack.CanExecute(null, this))
                    {
                        ComponentCommands.MoveFocusBack.Execute(null, this);
                        return true;
                    }
                    else if (this.CanMoveFocus(FocusNavigationDirection.Left, false))
                    {
                        this.MoveFocus(new TraversalRequest(FocusNavigationDirection.Left));
                        return true;
                    }
                }
            }

            return false;
        }

        private bool MoveFocusRight()
        {
            if (this.FlowDirection == FlowDirection.LeftToRight)
            {
                //occurs only if the cursor is at the beginning of the text
                if ((this.CaretIndex == this.Text.Length) && (this.SelectionLength == 0))
                {
                    if (ComponentCommands.MoveFocusForward.CanExecute(null, this))
                    {
                        ComponentCommands.MoveFocusForward.Execute(null, this);
                        return true;
                    }
                    else if (this.CanMoveFocus(FocusNavigationDirection.Right, false))
                    {
                        this.MoveFocus(new TraversalRequest(FocusNavigationDirection.Right));
                        return true;
                    }
                }
            }
            else
            {
                //occurs only if the cursor is at the end of the text
                if ((this.CaretIndex == 0) && (this.SelectionLength == 0))
                {
                    if (ComponentCommands.MoveFocusForward.CanExecute(null, this))
                    {
                        ComponentCommands.MoveFocusForward.Execute(null, this);
                        return true;
                    }
                    else if (this.CanMoveFocus(FocusNavigationDirection.Right, false))
                    {
                        this.MoveFocus(new TraversalRequest(FocusNavigationDirection.Right));
                        return true;
                    }
                }
            }

            return false;
        }

        private bool MoveFocusUp()
        {
            int lineNumber = this.GetLineIndexFromCharacterIndex(this.SelectionStart);

            //occurs only if the cursor is on the first line
            if (lineNumber == 0)
            {
                if (ComponentCommands.MoveFocusUp.CanExecute(null, this))
                {
                    ComponentCommands.MoveFocusUp.Execute(null, this);
                    return true;
                }
                else if (this.CanMoveFocus(FocusNavigationDirection.Up, false))
                {
                    this.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
                    return true;
                }
            }

            return false;
        }

        private bool MoveFocusDown()
        {
            int lineNumber = this.GetLineIndexFromCharacterIndex(this.SelectionStart);

            //occurs only if the cursor is on the first line
            if (lineNumber == (this.LineCount - 1))
            {
                if (ComponentCommands.MoveFocusDown.CanExecute(null, this))
                {
                    ComponentCommands.MoveFocusDown.Execute(null, this);
                    return true;
                }
                else if (this.CanMoveFocus(FocusNavigationDirection.Down, false))
                {
                    this.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                    return true;
                }
            }

            return false;
        }
    }

    internal static class TreeHelper
    {
        /// <summary>
        /// Tries its best to return the specified element's parent. It will 
        /// try to find, in this order, the VisualParent, LogicalParent, LogicalTemplatedParent.
        /// It only works for Visual, FrameworkElement or FrameworkContentElement.
        /// </summary>
        /// <param name="element">The element to which to return the parent. It will only 
        /// work if element is a Visual, a FrameworkElement or a FrameworkContentElement.</param>
        /// <remarks>If the logical parent is not found (Parent), we check the TemplatedParent
        /// (see FrameworkElement.Parent documentation). But, we never actually witnessed
        /// this situation.</remarks>
        public static DependencyObject GetParent(DependencyObject element)
        {
            return TreeHelper.GetParent(element, true);
        }

        private static DependencyObject GetParent(DependencyObject element, bool recurseIntoPopup)
        {
            if (recurseIntoPopup)
            {
                // Case 126732 : To correctly detect parent of a popup we must do that exception case
                Popup popup = element as Popup;

                if ((popup != null) && (popup.PlacementTarget != null))
                    return popup.PlacementTarget;
            }

            Visual visual = element as Visual;
            DependencyObject parent = (visual == null) ? null : VisualTreeHelper.GetParent(visual);

            if (parent == null)
            {
                // No Visual parent. Check in the logical tree.
                FrameworkElement fe = element as FrameworkElement;

                if (fe != null)
                {
                    parent = fe.Parent;

                    if (parent == null)
                    {
                        parent = fe.TemplatedParent;
                    }
                }
                else
                {
                    FrameworkContentElement fce = element as FrameworkContentElement;

                    if (fce != null)
                    {
                        parent = fce.Parent;

                        if (parent == null)
                        {
                            parent = fce.TemplatedParent;
                        }
                    }
                }
            }

            return parent;
        }

        /// <summary>
        /// This will search for a parent of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the element to find</typeparam>
        /// <param name="startingObject">The node where the search begins. This element is not checked.</param>
        /// <returns>Returns the found element. Null if nothing is found.</returns>
        public static T FindParent<T>(DependencyObject startingObject) where T : DependencyObject
        {
            return TreeHelper.FindParent<T>(startingObject, false, null);
        }

        /// <summary>
        /// This will search for a parent of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the element to find</typeparam>
        /// <param name="startingObject">The node where the search begins.</param>
        /// <param name="checkStartingObject">Should the specified startingObject be checked first.</param>
        /// <returns>Returns the found element. Null if nothing is found.</returns>
        public static T FindParent<T>(DependencyObject startingObject, bool checkStartingObject) where T : DependencyObject
        {
            return TreeHelper.FindParent<T>(startingObject, checkStartingObject, null);
        }

        /// <summary>
        /// This will search for a parent of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the element to find</typeparam>
        /// <param name="startingObject">The node where the search begins.</param>
        /// <param name="checkStartingObject">Should the specified startingObject be checked first.</param>
        /// <param name="additionalCheck">Provide a callback to check additional properties 
        /// of the found elements. Can be left Null if no additional criteria are needed.</param>
        /// <returns>Returns the found element. Null if nothing is found.</returns>
        /// <example>Button button = TreeHelper.FindParent&lt;Button&gt;( this, foundChild => foundChild.Focusable );</example>
        public static T FindParent<T>(DependencyObject startingObject, bool checkStartingObject, Func<T, bool> additionalCheck) where T : DependencyObject
        {
            T foundElement;
            DependencyObject parent = (checkStartingObject ? startingObject : TreeHelper.GetParent(startingObject, true));

            while (parent != null)
            {
                foundElement = parent as T;

                if (foundElement != null)
                {
                    if (additionalCheck == null)
                    {
                        return foundElement;
                    }
                    else
                    {
                        if (additionalCheck(foundElement))
                            return foundElement;
                    }
                }

                parent = TreeHelper.GetParent(parent, true);
            }

            return null;
        }

        /// <summary>
        /// This will search for a child of the specified type. The search is performed 
        /// hierarchically, breadth first (as opposed to depth first).
        /// </summary>
        /// <typeparam name="T">The type of the element to find</typeparam>
        /// <param name="parent">The root of the tree to search for. This element itself is not checked.</param>
        /// <returns>Returns the found element. Null if nothing is found.</returns>
        public static T FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            return TreeHelper.FindChild<T>(parent, null);
        }

        /// <summary>
        /// This will search for a child of the specified type. The search is performed 
        /// hierarchically, breadth first (as opposed to depth first).
        /// </summary>
        /// <typeparam name="T">The type of the element to find</typeparam>
        /// <param name="parent">The root of the tree to search for. This element itself is not checked.</param>
        /// <param name="additionalCheck">Provide a callback to check additional properties 
        /// of the found elements. Can be left Null if no additional criteria are needed.</param>
        /// <returns>Returns the found element. Null if nothing is found.</returns>
        /// <example>Button button = TreeHelper.FindChild&lt;Button&gt;( this, foundChild => foundChild.Focusable );</example>
        public static T FindChild<T>(DependencyObject parent, Func<T, bool> additionalCheck) where T : DependencyObject
        {
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            T child;

            for (int index = 0; index < childrenCount; index++)
            {
                child = VisualTreeHelper.GetChild(parent, index) as T;

                if (child != null)
                {
                    if (additionalCheck == null)
                    {
                        return child;
                    }
                    else
                    {
                        if (additionalCheck(child))
                            return child;
                    }
                }
            }

            for (int index = 0; index < childrenCount; index++)
            {
                child = TreeHelper.FindChild<T>(VisualTreeHelper.GetChild(parent, index), additionalCheck);

                if (child != null)
                    return child;
            }

            return null;
        }

        /// <summary>
        /// Returns true if the specified element is a child of parent somewhere in the visual 
        /// tree. This method will work for Visual, FrameworkElement and FrameworkContentElement.
        /// </summary>
        /// <param name="element">The element that is potentially a child of the specified parent.</param>
        /// <param name="parent">The element that is potentially a parent of the specified element.</param>
        public static bool IsDescendantOf(DependencyObject element, DependencyObject parent)
        {
            return TreeHelper.IsDescendantOf(element, parent, true);
        }

        /// <summary>
        /// Returns true if the specified element is a child of parent somewhere in the visual 
        /// tree. This method will work for Visual, FrameworkElement and FrameworkContentElement.
        /// </summary>
        /// <param name="element">The element that is potentially a child of the specified parent.</param>
        /// <param name="parent">The element that is potentially a parent of the specified element.</param>
        public static bool IsDescendantOf(DependencyObject element, DependencyObject parent, bool recurseIntoPopup)
        {
            while (element != null)
            {
                if (element == parent)
                    return true;

                element = TreeHelper.GetParent(element, recurseIntoPopup);
            }

            return false;
        }
    }
}

