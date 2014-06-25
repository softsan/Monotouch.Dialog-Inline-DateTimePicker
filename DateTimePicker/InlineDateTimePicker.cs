namespace DateTimePicker.Controls
{
    #region Namespace
    using System;
    using System.Collections.Generic;
    using System.Drawing;

    using MonoTouch.Dialog;
    using MonoTouch.Foundation;
    using MonoTouch.UIKit;
    #endregion

    /// <summary>
    /// Inline null-able date and time element.
    /// </summary>
    public class InlineDateTimePicker : StringElement
    {
        /// <summary>
        /// The cell key.
        /// </summary>
        private static NSString skey = new NSString("InlineDateTimePicker");

        /// <summary>
        /// The formatter 
        /// </summary>
        private NSDateFormatter formatter = new NSDateFormatter()
        {
            DateStyle = NSDateFormatterStyle.Long
        };

        /// <summary>
        /// The date value.
        /// </summary>
        private DateTime? dateValue;

        /// <summary>
        /// The picker present.
        /// </summary>
        private bool pickerPresent = false;

        /// <summary>
        /// The inline date element.
        /// </summary>
        private InlineDateElement inlineDateElement = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="InlineDateTimePicker"/> class.
        /// </summary>
        /// <param name="caption"> The caption. </param>
        /// <param name="date"> The date and time. </param>
        /// <param name="pickerMode"> The picker mode (Date, date time or time). </param>
        public InlineDateTimePicker(string caption, DateTime? date, UIDatePickerMode pickerMode)
            : base(caption)
        {
            this.PickerMode = pickerMode;
            this.dateValue = date;
            this.Value = this.FormatDate(date);
        }

        /// <summary>
        /// The date selected.
        /// </summary>
        public event Action DateSelected;

        /// <summary>
        /// The picker closed.
        /// </summary>
        public event Action PickerClosed;

        /// <summary>
        /// The picker opened.
        /// </summary>
        public event Action PickerOpened;

        /// <summary>
        /// Gets or sets the picker mode.
        /// </summary>
        public UIDatePickerMode PickerMode { get; set; }

        /// <summary>
        /// Returns true if the picker is currently open
        /// </summary>
        /// <returns>true if picker is open, false other wise</returns>
        public bool IsPickerOpen()
        {
            return this.pickerPresent;
        }

        /// <summary>
        /// The close picker if open.
        /// </summary>
        /// <param name="dialogViewController">  The dialog view controller </param>
        public void ClosePickerIfOpen(DialogViewController dialogViewController)
        {
            if (this.pickerPresent)
            {
                var indexPath = this.IndexPath;
                var tableView = this.GetContainerTableView();

                this.Selected(dialogViewController, tableView, indexPath);
            }
        }

        /// <summary>
        /// The set date.
        /// </summary>
        /// <param name="date">The date. </param>
        public void SetDate(DateTime? date)
        {
            this.dateValue = date;
            this.Value = this.FormatDate(date);
            var immediateRoot = this.GetImmediateRootElement();
            immediateRoot.Reload(this, UITableViewRowAnimation.None);
        }

        /// <summary>
        /// The selected.
        /// </summary>
        /// <param name="dialogViewController"> The dialog view controller.</param>
        /// <param name="tableView"> The table view. </param>
        /// <param name="path"> Index path. </param>
        public override void Selected(DialogViewController dialogViewController, UITableView tableView, NSIndexPath path)
        {
            this.TogglePicker(dialogViewController, tableView, path);

            // Deselect the row so the row highlight tint fades away.
            tableView.DeselectRow(path, true);
        }

        /// <summary>
        /// The get cell.
        /// </summary>
        /// <param name="tableView"> The table view. </param>
        /// <returns> The  table cell </returns>
        public override UITableViewCell GetCell(UITableView tableView)
        {
            var cell = base.GetCell(tableView);
            cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
            cell.DetailTextLabel.Font = UIFont.SystemFontOfSize(14);
            return cell;
        }

        /// <summary>
        /// The get date with kind.
        /// </summary>
        /// <param name="dateTime"> The date time </param>
        /// <returns>  The  date time </returns>
        protected DateTime? GetDateWithKind(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
            {
                return dateTime;
            }

            if (dateTime.Value.Kind == DateTimeKind.Unspecified)
            {
                return DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Local);
            }

            return dateTime;
        }

        /// <summary>
        /// The format date.
        /// </summary>
        /// <param name="dt">The date time </param>
        /// <returns> The formatted date </returns>
        protected virtual string FormatDate(DateTime? dt)
        {
            if (!dt.HasValue)
            {
                return " ";
            }

            dt = this.GetDateWithKind(dt);
            if (this.PickerMode == UIDatePickerMode.Time)
            {
                this.formatter.DateFormat = "HH:mm";
            }
            else if (this.PickerMode == UIDatePickerMode.DateAndTime)
            {
                this.formatter.DateFormat = "MMMM d, y HH:mm";
            }

            return this.formatter.ToString(dt);
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (this.formatter != null)
                {
                    this.formatter.Dispose();
                    this.formatter = null;
                }
            }
        }

        /// <summary>
        /// Shows or hides the null picker
        /// </summary>
        /// <param name="dialogViewController">dialog view controller</param>
        /// <param name="tableView">table view</param>
        /// <param name="path">index path</param>
        private void TogglePicker(DialogViewController dialogViewController, UITableView tableView, NSIndexPath path)
        {
            var sectionAndIndex = this.GetSectionAndIndex(dialogViewController);
            if (sectionAndIndex.Key != null)
            {
                Section section = sectionAndIndex.Key;
                int index = sectionAndIndex.Value;

                var cell = tableView.CellAt(path);

                if (this.pickerPresent)
                {
                    // Remove the picker.
                    cell.DetailTextLabel.TextColor = UIColor.Gray;
                    section.Remove(this.inlineDateElement);
                    this.pickerPresent = false;
                    if (this.PickerClosed != null)
                    {
                        this.PickerClosed();
                    }
                }
                else
                {
                    // Show the picker.
                    cell.DetailTextLabel.TextColor = UIColor.Red;
                    this.inlineDateElement = new InlineDateElement(this.dateValue, this.PickerMode);

                    this.inlineDateElement.DateSelected += (DateTime? date) =>
                    {
                        this.dateValue = date;
                        cell.DetailTextLabel.Text = FormatDate(date);
                        Value = cell.DetailTextLabel.Text;
                        if (this.DateSelected != null) // Fire changed event.
                        {
                            this.DateSelected();
                        }
                    };

                    this.inlineDateElement.ClearPressed += () =>
                    {
                        DateTime? nullDate = null;
                        dateValue = nullDate;
                        cell.DetailTextLabel.Text = " ";
                        Value = cell.DetailTextLabel.Text;
                        cell.DetailTextLabel.TextColor = UIColor.Gray;
                        section.Remove(this.inlineDateElement);
                        this.pickerPresent = false;
                        if (this.PickerClosed != null)
                        {
                            this.PickerClosed();
                        }
                    };

                    section.Insert(index + 1, UITableViewRowAnimation.Bottom, this.inlineDateElement);
                    this.pickerPresent = true;
                    tableView.ScrollToRow(this.inlineDateElement.IndexPath, UITableViewScrollPosition.None, true);

                    if (this.PickerOpened != null)
                    {
                        this.PickerOpened();
                    }
                }
            }
        }

        /// <summary>
        /// Locates this instance of this Element within a given DialogViewController.
        /// </summary>
        /// <returns>The Section instance and the index within that Section of this instance.</returns>
        /// <param name="dialogViewController">Dialog view controller.</param>
        private KeyValuePair<Section, int> GetSectionAndIndex(DialogViewController dialogViewController)
        {
            foreach (var section in dialogViewController.Root)
            {
                for (int i = 0; i < section.Count; i++)
                {
                    if (section[i] == this)
                    {
                        return new KeyValuePair<Section, int>(section, i);
                    }
                }
            }

            return new KeyValuePair<Section, int>();
        }

        /// <summary>
        /// Class that has the UIDatePicker and a button for clearing/cancelling
        /// </summary>
        private class InlineDateElement : Element, IElementSizing
        {
            /// <summary>
            /// The cell key.
            /// </summary>
            private static NSString skey = new NSString("InlineDateElement");

            /// <summary>
            /// The date picker.
            /// </summary>
            private UIDatePicker datePicker;

            /// <summary>
            /// The clear cancel button.
            /// </summary>
            private UIButton clearCancelButton;

            /// <summary>
            /// The current date.
            /// </summary>
            private DateTime? currentDate;

            /// <summary>
            /// The picker size.
            /// </summary>
            private SizeF pickerSize;

            /// <summary>
            /// The cell size.
            /// </summary>
            private SizeF cellSize;

            /// <summary>
            /// Initializes a new instance of the <see cref="InlineDateElement"/> class.
            /// </summary>
            /// <param name="currentDate">  The current date. </param>
            /// <param name="pickerMode"> The picker mode. </param>
            public InlineDateElement(DateTime? currentDate, UIDatePickerMode pickerMode)
                : base(string.Empty)
            {
                this.currentDate = currentDate;
                this.datePicker = new UIDatePicker { Mode = pickerMode };
                this.pickerSize = this.datePicker.SizeThatFits(SizeF.Empty);
                this.cellSize = this.pickerSize;
                this.cellSize.Height += 30f; // Add a little bit for the clear button
            }

            /// <summary>
            /// The date selected.
            /// </summary>
            public event Action<DateTime?> DateSelected;

            /// <summary>
            /// The clear pressed.
            /// </summary>
            public event Action ClearPressed;

            /// <summary>
            /// Returns the cell, with some additions
            /// </summary>
            /// <param name="tableView">table view</param>
            /// <returns>table cell </returns>
            public override UITableViewCell GetCell(UITableView tableView)
            {
                var cell = base.GetCell(tableView);

                if (!this.currentDate.HasValue && this.DateSelected != null)
                {
                    this.DateSelected(DateTime.Now);
                }
                else if (this.currentDate.HasValue)
                {
                    this.datePicker.Date = this.currentDate;
                }

                this.datePicker.ValueChanged += (object sender, EventArgs e) =>
                {
                    if (DateSelected != null)
                    {
                        DateSelected(this.datePicker.Date);
                    }
                };

                if (this.clearCancelButton == null)
                {
                    this.clearCancelButton = UIButton.FromType(UIButtonType.RoundedRect);
                    this.clearCancelButton.SetTitle("Clear", UIControlState.Normal);
                }

                this.clearCancelButton.Frame = new
                    RectangleF(
                    (tableView.Frame.Width / 2) - 20f,
                    this.cellSize.Height - 40f,
                    40f,
                    40f);
                this.datePicker.Frame = new
                    RectangleF(
                    (tableView.Frame.Width / 2) - (this.pickerSize.Width / 2),
                    (this.cellSize.Height / 2) - (this.pickerSize.Height / 2),
                    this.pickerSize.Width,
                    this.pickerSize.Height);
                this.clearCancelButton.TouchUpInside += (object sender, EventArgs e) =>
                {
                    // Clear button pressed. 
                    if (this.ClearPressed != null)
                    {
                        this.ClearPressed();
                    }
                };

                cell.AddSubview(this.datePicker);

                cell.AddSubview(this.clearCancelButton);

                return cell;
            }

            /// <summary>
            /// Returns the height of the cell
            /// </summary>
            /// <param name="tableView">table view</param>
            /// <param name="indexPath">index path</param>
            /// <returns>cell height </returns>
            public float GetHeight(UITableView tableView, NSIndexPath indexPath)
            {
                return this.cellSize.Height;
            }
        }
    }
}