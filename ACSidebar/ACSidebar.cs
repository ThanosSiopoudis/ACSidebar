using System;
using AppKit;
using Foundation;
using CoreGraphics;
using ObjCRuntime;
using System.Runtime.InteropServices;

namespace ACSidebar
{
    [Register("ACSidebar")]
    public class ACSidebar : NSView, IDisposable
    {
        private static NSColor kDefautBackgroundColour = NSColor.FromDeviceWhite(0.16f, 1.0f);
        private static NSScrollerKnobStyle kDefaultScrollerKnobStyle = NSScrollerKnobStyle.Light;
        private static bool kDefaultAllowsEmptySelection = true;
        private NSMatrix matrix;
        private NSObject _target;
        private Selector _action;

        public ACSidebar () : base ()
        {}

        public ACSidebar (IntPtr handle) : base (handle)
        {}

        public ACSidebar (NSCoder coder) : base (coder)
        {
            this.Initialise ();
        }

        public ACSidebar (CGRect frameRect) : base (frameRect)
        {
            this.Initialise ();
        }
            
        protected override void Dispose (bool disposing)
        {
            NSNotificationCenter.DefaultCenter.RemoveObserver (this);
            base.Dispose (disposing);
        }

        private void Initialise() {
            this.AddMatrix ();
        }

        public override void AwakeFromNib ()
        {
            this.ResizeMatrix (null);
            this.InitialiseScrollView ();
        }

        private void InitialiseScrollView() {
            this.EnclosingScrollView.DrawsBackground = true;

            // Style scroll view
            this.EnclosingScrollView.BorderType = NSBorderType.NoBorder;
            this.BackgroundColour = kDefautBackgroundColour;
            this.ScrollerKnobStyle = kDefaultScrollerKnobStyle;

            this.EnclosingScrollView.DrawsBackground = true;

            NSClipView clipView = this.EnclosingScrollView.ContentView;
            clipView.PostsBoundsChangedNotifications = true;
            NSNotificationCenter.DefaultCenter.AddObserver (NSView.BoundsChangedNotification, ResizeMatrix, clipView);
        }

        private void AddMatrix() {
            this.matrix = new NSMatrix (this.Frame, NSMatrixMode.Radio, ACSidebar.CellClass, 0, 1);
            this.matrix.AllowsEmptySelection = kDefaultAllowsEmptySelection;
            this.matrix.CellSize = new CGSize (62, 62);

            this.ResizeMatrix (null);
            this.AddSubview (this.matrix);
        }

        #region Scroll View
        public NSColor BackgroundColour {
            get {
                return this.EnclosingScrollView.BackgroundColor;
            }
            set {
                this.EnclosingScrollView.BackgroundColor = value;
            }
        }

        public NSScrollerKnobStyle ScrollerKnobStyle {
            get {
                return this.EnclosingScrollView.ScrollerKnobStyle;
            }
            set {
                this.EnclosingScrollView.ScrollerKnobStyle = value;
            }
        }
        #endregion

        #region Key Handling
        public override bool AcceptsFirstResponder ()
        {
            return true;
        }

        public override void KeyDown (NSEvent theEvent)
        {
            switch (theEvent.KeyCode) {
            case 126:
                this.SelectPreviousItem ();
                break;
            case 125:
                this.SelectNextItem ();
                break;
            case 53:
                this.DeselectAllItems ();
                break;
            default:
                base.KeyDown (theEvent);
                break;
            }
        }

        public void DeselectAllItems() {
            this.matrix.DeselectSelectedCell ();
        }

        public void SelectNextItem() {
            this.SelectNeighbourItemWithValue (1);
        }

        public void SelectPreviousItem() {
            this.SelectNeighbourItemWithValue (-1);
        }

        public void SelectNeighbourItemWithValue(int value) {
            this.SelectedIndex = this.SelectedIndex + value;
        }
        #endregion

        #region Cells
        public static Class CellClass {
            get {
                return new Class("ACSidebarItemCell");
            }
        }

        private ACSidebarItemCell SelectedItem {
            get {
                return (ACSidebarItemCell)this.matrix.SelectedCell;
            }
            set {
                this.SelectedIndex = Array.IndexOf (this.matrix.Cells, value);
            }
        }

        private int SelectedIndex {
            get {
                ACSidebarItemCell cell = this.SelectedItem;
                return Array.IndexOf (this.matrix.Cells, cell);
            }
            set {
                if (value < this.matrix.Cells.Length) {
                    this.matrix.SelectCell (this.matrix.Cells [value]);

                    // Again, no action
                    this.MatrixCallback(this);
                }
            }
        }

        private CGSize CellSize {
            get {
                return this.matrix.CellSize;
            }
            set {
                this.matrix.CellSize = value;
            }
        }

        private bool AllowsEmptySelection {
            get {
                return this.matrix.AllowsEmptySelection;
            }
            set {
                this.matrix.AllowsEmptySelection = value;

                // If empty selection is not allowed, we select the first item
                if (!value && this.SelectedIndex == -1) {
                    this.SelectedIndex = 0;
                }
            }
        }

        public ACSidebarItemCell AddItem(NSImage image, NSObject target, EventHandler e) {
            ACSidebarItemCell cell = new ACSidebarItemCell(image);
            cell.Target = target;
            cell.Activated += e;

            return cell;
        }

        public ACSidebarItemCell AddItem(NSImage image, NSImage alternateImage, NSObject target, EventHandler e) {
            ACSidebarItemCell cell = this.AddItem (image, target, e);
            cell.AlternateImage = alternateImage;

            return cell;
        }

        public ACSidebarItemCell AddItem(NSImage image) {
            ACSidebarItemCell cell = new ACSidebarItemCell (image);
            this.AddCell (cell);

            return cell;
        }

        public ACSidebarItemCell AddItem(NSImage image, NSImage alternateImage) {
            ACSidebarItemCell cell = this.AddItem (image);
            cell.AlternateImage = alternateImage;

            return cell;
        }

        private void AddCell(ACSidebarItemCell cell) {
            this.matrix.AddRowWithCells(new NSCell[]{ cell });
            this.ResizeMatrix (null);
        }
        #endregion

        #region Resizing
        public override CGRect Frame {
            get {
                return base.Frame;
            }
            set {
                base.Frame = value;
                this.ResizeMatrix (null);
            }
        }

        public void ResizeMatrix(NSNotification notification) {
            this.matrix.SizeToCells ();

            CGRect newSize = this.matrix.Frame;
            if (this.EnclosingScrollView.ContentView.Frame.Size.Height > newSize.Size.Height) {
                CGSize tmpSize = new CGSize (newSize.Size.Width, this.EnclosingScrollView.ContentView.Frame.Size.Height);
                newSize.Size = tmpSize;
            }

            this.matrix.SetFrameSize (newSize.Size);
            this.SetFrameSize (newSize.Size);
        }
        #endregion
            
        #region ACSidebar Target Action
        [Action("MatrixCallback:")]
        public void MatrixCallback(NSObject sender) {
            if (this.Target.RespondsToSelector(this.Action)) {
                this.Target.PerformSelector (this.Action, this);
            }
            else {
                this.SelectedItem.Target.PerformSelector (this.SelectedItem.Action, this.SelectedItem);
            }
        }

        private NSObject Target {
            get {
                return this._target;
            }
            set {
                this.matrix.Target = this;
                this._target = value;
            }
        }

        private Selector Action {
            get {
                return this._action;
            }
            set {
                this.matrix.Action = value;
                this._action = value;
            }
        }

        #endregion
    }
}

