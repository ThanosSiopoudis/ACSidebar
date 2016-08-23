using System;
using AppKit;
using Foundation;
using CoreGraphics;

namespace ACSidebar
{
    [Register("ACSidebarItemCell")]
    public class ACSidebarItemCell : NSButtonCell
    {
        private static float kSelectionCornerRadius = 5.0f;
        private static float kSelectionWidth = 2.0f;
        private static NSColor kSelectionColour = NSColor.FromCalibratedRgba(0.12f, 0.49f, 0.93f, 1.0f);
        private static NSColor kSelectionHighlightColour = NSColor.FromCalibratedRgba(0.12f, 0.49f, 0.93f, 0.7f);
        public NSShadow shadow {
            get {
                NSShadow shadow = new NSShadow ();
                shadow.ShadowOffset = new CGSize (0, -1);
                shadow.ShadowColor = NSColor.Black;
                shadow.ShadowBlurRadius = 3.0f;

                return shadow;
            }
        }

        public event EventHandler itemSelected;
        public virtual void OnItemSelected(EventArgs e) {
            EventHandler handler = itemSelected;
            if (handler != null) {
                handler(this, e);
            }
        }

        public ACSidebarItemCell (NSImage image) : base (image)
        {}

        public ACSidebarItemCell (IntPtr handle) : base(handle)
        {}

        public ACSidebarItemCell (NSCoder coder) : base(coder)
        {}

        public ACSidebarItemCell () : base ()
        {}

        public void DrawImageWithFrame(CGRect frame, NSView controlView) {
            NSImage image;
            if ((this.Highlighted || this.State == NSCellStateValue.On) && this.AlternateImage != null) {
                image = this.AlternateImage;
            }
            else {
                image = this.Image;
            }
            this.DrawImage (image, frame, controlView);
        }

        public override void DrawImage (NSImage image, CGRect frame, NSView controlView)
        {
            NSGraphicsContext.GlobalSaveGraphicsState ();
            {
                this.shadow.Set ();
                CGRect imgRect = frame.Inset ((frame.Size.Width - image.Size.Width) / 2.0f, (frame.Size.Height - image.Size.Height) / 2.0f);
                image.Flipped = true;
                image.DrawInRect (imgRect, CGRect.Empty, NSCompositingOperation.SourceOver, 1.0f);
            }
            NSGraphicsContext.GlobalRestoreGraphicsState ();
        }

        public override void DrawInteriorWithFrame (CGRect cellFrame, NSView inView)
        {
            NSGraphicsContext.GlobalSaveGraphicsState ();
            {
                this.shadow.Set();
                NSColor.White.Set();

                base.DrawTitle(this.AttributedTitle, cellFrame, inView);
                this.DrawImageWithFrame(cellFrame, inView);
            }
            NSGraphicsContext.GlobalRestoreGraphicsState();
        }

        public void DrawBackground(CGRect frame, NSView inView) {
            // We do nothing for this example here
        }

        public void DrawSelection(CGRect frame, NSView inView) {
            if (this.State == NSCellStateValue.On) {
                kSelectionColour.Set ();
            }
            else {
                kSelectionHighlightColour.Set ();
            }

            CGRect strokeRect = frame.Inset (10, 10);
            NSBezierPath path = NSBezierPath.FromRoundedRect (strokeRect, kSelectionCornerRadius, kSelectionCornerRadius);
            path.LineWidth = kSelectionWidth;
            path.Stroke ();
        }

        public override void DrawWithFrame (CGRect cellFrame, NSView inView)
        {
            NSGraphicsContext.GlobalSaveGraphicsState ();
            {
                this.DrawBackground (cellFrame, inView);
                this.DrawInteriorWithFrame (cellFrame, inView);

                if (this.State == NSCellStateValue.On || this.Highlighted) {
                    this.DrawSelection (cellFrame, inView);
                }
            }
            NSGraphicsContext.GlobalRestoreGraphicsState ();
        }
    }
}

