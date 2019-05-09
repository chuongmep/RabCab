using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace RabCab.Entities.Controls
{
    public class SplitButton : Button
    {
        private const int PushButtonWidth = 14;
        private static readonly int BorderSize = SystemInformation.Border3DSize.Width * 2;

        private Rectangle _dropDownRectangle;

        private bool _showSplit = true;

        private bool _skipNextOpen;
        private PushButtonState _state;


        public SplitButton()
        {
            AutoSize = true;
        }

        public sealed override bool AutoSize
        {
            get => base.AutoSize;
            set => base.AutoSize = value;
        }

        public sealed override Color BackColor
        {
            get => base.BackColor;
            set => base.BackColor = value;
        }

        public sealed override Color ForeColor
        {
            get => base.ForeColor;
            set => base.ForeColor = value;
        }

        [DefaultValue(true)]
        public bool ShowSplit
        {
            set
            {
                if (value != _showSplit)
                {
                    _showSplit = value;

                    Invalidate();

                    Parent?.PerformLayout();
                }
            }
        }


        private PushButtonState State
        {
            get => _state;
            set
            {
                if (!_state.Equals(value))
                {
                    _state = value;
                    Invalidate();
                }
            }
        }


        public override Size GetPreferredSize(Size proposedSize)
        {
            var preferredSize = base.GetPreferredSize(proposedSize);

            if (_showSplit && !string.IsNullOrEmpty(Text) &&
                TextRenderer.MeasureText(Text, Font).Width + PushButtonWidth > preferredSize.Width)
                return preferredSize + new Size(PushButtonWidth + BorderSize * 2, 0);

            return preferredSize;
        }


        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData.Equals(Keys.Down) && _showSplit)
                return true;

            return base.IsInputKey(keyData);
        }


        protected override void OnGotFocus(EventArgs e)
        {
            if (!_showSplit)
            {
                base.OnGotFocus(e);
                return;
            }


            if (!State.Equals(PushButtonState.Pressed) && !State.Equals(PushButtonState.Disabled))
                State = PushButtonState.Default;
        }


        protected override void OnKeyDown(KeyEventArgs kevent)
        {
            if (_showSplit)
            {
                if (kevent.KeyCode.Equals(Keys.Down))
                    ShowContextMenuStrip();

                else if (kevent.KeyCode.Equals(Keys.Space) && kevent.Modifiers == Keys.None)
                    State = PushButtonState.Pressed;
            }


            base.OnKeyDown(kevent);
        }


        protected override void OnKeyUp(KeyEventArgs kevent)
        {
            if (kevent.KeyCode.Equals(Keys.Space))
                if (MouseButtons == MouseButtons.None)
                    State = PushButtonState.Normal;

            base.OnKeyUp(kevent);
        }


        protected override void OnLostFocus(EventArgs e)
        {
            if (!_showSplit)
            {
                base.OnLostFocus(e);

                return;
            }

            if (!State.Equals(PushButtonState.Pressed) && !State.Equals(PushButtonState.Disabled))
                State = PushButtonState.Normal;
        }


        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!_showSplit)
            {
                base.OnMouseDown(e);

                return;
            }


            if (_dropDownRectangle.Contains(e.Location))
                ShowContextMenuStrip();

            else
                State = PushButtonState.Pressed;
        }


        protected override void OnMouseEnter(EventArgs e)
        {
            if (!_showSplit)
            {
                base.OnMouseEnter(e);

                return;
            }


            if (!State.Equals(PushButtonState.Pressed) && !State.Equals(PushButtonState.Disabled))
                State = PushButtonState.Hot;
        }


        protected override void OnMouseLeave(EventArgs e)
        {
            if (!_showSplit)
            {
                base.OnMouseLeave(e);

                return;
            }


            if (!State.Equals(PushButtonState.Pressed) && !State.Equals(PushButtonState.Disabled))
                State = Focused ? PushButtonState.Default : PushButtonState.Normal;
        }


        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            if (!_showSplit)
            {
                base.OnMouseUp(mevent);

                return;
            }


            if (ContextMenuStrip == null || !ContextMenuStrip.Visible)
            {
                SetButtonDrawState();

                if (Bounds.Contains(Parent.PointToClient(Cursor.Position)) &&
                    !_dropDownRectangle.Contains(mevent.Location)) OnClick(new EventArgs());
            }
        }


        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);


            if (!_showSplit) return;


            var g = pevent.Graphics;

            var bounds = ClientRectangle;


            // draw the button background as according to the current state.

            if (State != PushButtonState.Pressed && IsDefault && !Application.RenderWithVisualStyles)
            {
                var backgroundBounds = bounds;

                backgroundBounds.Inflate(-1, -1);

                ButtonRenderer.DrawButton(g, backgroundBounds, State);

                // button renderer doesnt draw the black frame when themes are off =(
                g.DrawRectangle(SystemPens.WindowFrame, 0, 0, bounds.Width - 1, bounds.Height - 1);
            }

            else
            {
                ButtonRenderer.DrawButton(g, bounds, State);
            }


            var backBrush = new SolidBrush(BackColor);
            g.FillRectangle(backBrush, 1, 1, bounds.Width - 2, bounds.Height - 2);


            // calculate the current dropdown rectangle.

            _dropDownRectangle = new Rectangle(bounds.Right - PushButtonWidth - 1, BorderSize, PushButtonWidth,
                bounds.Height - BorderSize * 2);


            var internalBorder = BorderSize;

            var focusRect =
                new Rectangle(internalBorder,
                    internalBorder,
                    bounds.Width - _dropDownRectangle.Width - internalBorder,
                    bounds.Height - internalBorder * 2);


            var drawSplitLine = State == PushButtonState.Hot || State == PushButtonState.Pressed ||
                                !Application.RenderWithVisualStyles;


            if (RightToLeft == RightToLeft.Yes)
            {
                _dropDownRectangle.X = bounds.Left + 1;

                focusRect.X = _dropDownRectangle.Right;

                if (drawSplitLine)
                {
                    // draw two lines at the edge of the dropdown button

                    g.DrawLine(SystemPens.ButtonShadow, bounds.Left + PushButtonWidth, BorderSize,
                        bounds.Left + PushButtonWidth, bounds.Bottom - BorderSize);

                    g.DrawLine(SystemPens.ButtonFace, bounds.Left + PushButtonWidth + 1, BorderSize,
                        bounds.Left + PushButtonWidth + 1, bounds.Bottom - BorderSize);
                }
            }

            else
            {
                if (drawSplitLine)
                {
                    // draw two lines at the edge of the dropdown button

                    g.DrawLine(SystemPens.ButtonShadow, bounds.Right - PushButtonWidth, BorderSize,
                        bounds.Right - PushButtonWidth, bounds.Bottom - BorderSize);

                    g.DrawLine(SystemPens.ButtonFace, bounds.Right - PushButtonWidth - 1, BorderSize,
                        bounds.Right - PushButtonWidth - 1, bounds.Bottom - BorderSize);
                }
            }


            // Draw an arrow in the correct location

            PaintArrow(g, _dropDownRectangle);


            // Figure out how to draw the text

            var formatFlags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;


            // If we dont' use mnemonic, set formatFlag to NoPrefix as this will show ampersand.

            if (!UseMnemonic)
                formatFlags = formatFlags | TextFormatFlags.NoPrefix;

            else if (!ShowKeyboardCues) formatFlags = formatFlags | TextFormatFlags.HidePrefix;


            if (!string.IsNullOrEmpty(Text))
                TextRenderer.DrawText(g, Text, Font, focusRect, ForeColor, formatFlags);


            // draw the focus rectangle.


            if (State != PushButtonState.Pressed && Focused) ControlPaint.DrawFocusRectangle(g, focusRect);
        }


        private void PaintArrow(Graphics g, Rectangle dropDownRect)
        {
            var middle = new Point(Convert.ToInt32(dropDownRect.Left + dropDownRect.Width / 2),
                Convert.ToInt32(dropDownRect.Top + dropDownRect.Height / 2));


            //if the width is odd - favor pushing it over one pixel right.

            middle.X += dropDownRect.Width % 2;


            Point[] arrow =
            {
                new Point(middle.X - 2, middle.Y - 1), new Point(middle.X + 3, middle.Y - 1),
                new Point(middle.X, middle.Y + 2)
            };


            g.FillPolygon(SystemBrushes.ControlText, arrow);
        }


        private void ShowContextMenuStrip()
        {
            if (_skipNextOpen)
            {
                // we were called because we're closing the context menu strip

                // when clicking the dropdown button.

                _skipNextOpen = false;

                return;
            }

            State = PushButtonState.Pressed;


            if (ContextMenuStrip != null)
            {
                ContextMenuStrip.Closing += ContextMenuStrip_Closing;

                ContextMenuStrip.Show(this, new Point(0, Height), ToolStripDropDownDirection.BelowRight);
            }
        }


        private void ContextMenuStrip_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            if (sender is ContextMenuStrip cms) cms.Closing -= ContextMenuStrip_Closing;


            SetButtonDrawState();


            if (e.CloseReason == ToolStripDropDownCloseReason.AppClicked)
                _skipNextOpen = _dropDownRectangle.Contains(PointToClient(Cursor.Position));
        }


        private void SetButtonDrawState()
        {
            if (Bounds.Contains(Parent.PointToClient(Cursor.Position)))
                State = PushButtonState.Hot;

            else if (Focused)
                State = PushButtonState.Default;

            else
                State = PushButtonState.Normal;
        }
    }
}