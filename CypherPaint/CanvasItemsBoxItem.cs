﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace CypherPaint
{
    public class CanvasItemsBoxItem : ContentControl
    {
        private Point? dragStartPoint = null;

        static CanvasItemsBoxItem()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolboxItem), new FrameworkPropertyMetadata(typeof(ToolboxItem)));
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            this.dragStartPoint = new Point?(e.GetPosition(this));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                this.dragStartPoint = null;
            }

            if (this.dragStartPoint.HasValue)
            {
                Point position = e.GetPosition(this);
                if ((SystemParameters.MinimumHorizontalDragDistance <=
                    Math.Abs(position.X - this.dragStartPoint.Value.X)) ||
                    (SystemParameters.MinimumVerticalDragDistance <=
                    Math.Abs(position.Y - this.dragStartPoint.Value.Y)))
                {
                    string xamlString = XamlWriter.Save(this.Content);
                    DataObject dataObject = new DataObject("DESIGNER_ITEM", xamlString);

                    if (dataObject != null)
                    {
                        DragDrop.DoDragDrop(this, dataObject, DragDropEffects.Copy);
                    }
                }

                e.Handled = true;
            }
        }
    }
}
