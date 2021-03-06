﻿namespace SadConsole.Controls
{
    using Microsoft.Xna.Framework;
    using SadConsole.Themes;
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents a button that can be toggled on/off within a group of other buttons.
    /// </summary>
    [DataContract]
    public class RadioButton: ControlBase
    {
        /// <summary>
        /// Raised when the selected state of the radio button is changed.
        /// </summary>
        public event EventHandler IsSelectedChanged;

        [DataMember(Name="Group")]
        protected string _groupName = "";
        [DataMember(Name = "Theme")]
        protected RadioButtonTheme _theme;
        [DataMember(Name = "Text")]
        protected string _text;
        [DataMember(Name = "TextAlignment")]
        protected System.Windows.HorizontalAlignment _textAlignment;
        [DataMember(Name = "IsSelected")]
        protected bool _isSelected;
        protected bool _isMouseDown;
        protected CellAppearance _currentAppearanceButton;
        protected CellAppearance _currentAppearanceText;
        
        /// <summary>
        /// The theme of this control. If the theme is not explicitly set, the theme is taken from the library.
        /// </summary>
        public virtual RadioButtonTheme Theme
        {
            get
            {
                if (_theme == null)
                    return Library.Default.RadioButtonTheme;
                else
                    return _theme;
            }
            set
            {
                _theme = value;
            }
        }

        /// <summary>
        /// The text displayed on the control.
        /// </summary>
        public string Text
        {
            get { return _text; }
            set { _text = value; Compose(true); }
        }

        /// <summary>
        /// The alignment of the text, left, center, or right.
        /// </summary>
        public System.Windows.HorizontalAlignment TextAlignment
        {
            get { return _textAlignment; }
            set { _textAlignment = value; Compose(true); }
        }
        
        /// <summary>
        /// The group of the radio button. All radio buttons with the same group name will work together to keep one radio button selected at a time.
        /// </summary>
        public string GroupName
        {
            get { return _groupName; }
            set
            {
                _groupName = value.Trim();

                if (_groupName == null)
                    _groupName = String.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the selected state of the radio button.
        /// </summary>
        /// <remarks>Radio buttons within the same group will set their IsSelected property to the opposite of this radio button when you set this property.</remarks>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;

                    if (_isSelected)
                    {
                        if (Parent != null)
                        {
                            foreach (var child in Parent.Controls)
                            {
                                if (child is RadioButton && child != this)
                                {
                                    RadioButton button = (RadioButton)child;
                                    if (button.GroupName.Equals(this.GroupName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        button.IsSelected = false;
                                    }
                                }
                            }
                        }
                    }

                    if (IsSelectedChanged != null)
                        IsSelectedChanged(this, EventArgs.Empty);

                    //if (value)
                    //    OnAction();
                    DetermineAppearance();
                    Compose(true);
                }
            }
        }

        /// <summary>
        /// Creates a new radio button control with the specified width and height.
        /// </summary>
        /// <param name="width">Width of the control.</param>
        /// <param name="height">Height of the control.</param>
        public RadioButton(int width, int height)
        {
            base.Resize(width, height);

            DetermineAppearance();
        }

        /// <summary>
        /// Determines the appearance of the control based on its current state.
        /// </summary>
        public override void DetermineAppearance()
        {
            CellAppearance currentappearanceButton = _currentAppearanceButton;
            CellAppearance currentappearanceText = _currentAppearanceText;

            if (!_isEnabled)
            {
                _currentAppearanceButton = Theme.Button.Disabled;
                _currentAppearanceText = Theme.Disabled;
            }

            else if (!_isMouseDown && _isMouseOver)
            {
                _currentAppearanceButton = Theme.Button.MouseOver;
                _currentAppearanceText = Theme.MouseOver;
            }

            else if (!_isMouseDown && !_isMouseOver && IsFocused && Engine.ActiveConsole == _parent)
            {
                _currentAppearanceButton = Theme.Button.Focused;
                _currentAppearanceText = Theme.Focused;
            }

            else if (_isMouseDown && _isMouseOver)
            {
                _currentAppearanceButton = Theme.Button.MouseClicking;
                _currentAppearanceText = Theme.MouseClicking;
            }

            else if (_isSelected)
            {
                _currentAppearanceButton = Theme.Button.Selected;
                _currentAppearanceText = Theme.Selected;
            }

            else
            {
                _currentAppearanceButton = Theme.Button.Normal;
                _currentAppearanceText = Theme.Normal;
            }

            if (currentappearanceButton != _currentAppearanceButton ||
                currentappearanceText != _currentAppearanceText)

                this.IsDirty = true;
        }

        protected override void OnMouseIn(Input.MouseInfo info)
        {
            _isMouseOver = true;

            base.OnMouseIn(info);
        }

        protected override void OnMouseExit(Input.MouseInfo info)
        {
            _isMouseOver = false;

            base.OnMouseExit(info);
        }

        protected override void OnLeftMouseClicked(Input.MouseInfo info)
        {
            base.OnLeftMouseClicked(info);

            if (_isEnabled)
            {
                IsSelected = true;
            }
        }

        /// <summary>
        /// Called when the control should process keyboard information.
        /// </summary>
        /// <param name="info">The keyboard information.</param>
        /// <returns>True if the keyboard was handled by this control.</returns>
        public override bool ProcessKeyboard(Input.KeyboardInfo info)
        {
            if (info.IsKeyReleased(Microsoft.Xna.Framework.Input.Keys.Space) || info.IsKeyReleased(Microsoft.Xna.Framework.Input.Keys.Enter))
            {
                IsSelected = true;

                return true;
            }
            else if (Parent != null)
            {
                if (info.IsKeyReleased(Microsoft.Xna.Framework.Input.Keys.Up))
                    Parent.TabPreviousControl();

                else if (info.IsKeyReleased(Microsoft.Xna.Framework.Input.Keys.Down))
                    Parent.TabNextControl();

                return true;
            }

            return false;
        }

        public override void Compose()
        {
            if (this.IsDirty)
            {
                // If we are doing text, then print it otherwise we're just displaying the button part
                if (_width != 1)
                {
                    for (int x = 0; x < 4; x++)
			        {
			            this.SetCellAppearance(x, 0, _currentAppearanceButton);
			        }
                    this.Fill(_currentAppearanceText.Foreground, _currentAppearanceText.Background, _currentAppearanceText.CharacterIndex, null);
                    this.Print(4, 0, Text.Align(TextAlignment, this.Width - 4));
                    this.SetCharacter(0, 0, 40);
                    this.SetCharacter(2, 0, 41);

                    if (_isSelected)
                        this.SetCharacter(1, 0, Theme.CheckedIcon);
                    else
                        this.SetCharacter(1, 0, Theme.UncheckedIcon);
                }
                else
                {
                }
                

                this.IsDirty = false;
            }
        }

        [OnDeserializedAttribute]
        private void AfterDeserialized(StreamingContext context)
        {
            DetermineAppearance();
            Compose(true);
        }
    }
}
