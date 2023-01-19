﻿using System;
using System.Numerics;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Localization;
using ImGui.Forms.Models;
using ImGui.Forms.Resources;
using ImGuiNET;
using Veldrid;

namespace ImGui.Forms.Controls
{
    public class TextBox : Component
    {
        private bool _activePreviousFrame;
        private string _text=string.Empty;

        /// <summary>
        /// The text that was set or changed in this component.
        /// </summary>
        public string Text
        {
            get => _text;
            set
            {
                _text = value ?? string.Empty;
                OnTextChanged();
            }
        }

        /// <summary>
        /// The width of this component. Is set to 100% by default.
        /// </summary>
        public SizeValue Width { get; set; } = SizeValue.Relative(1f);

        /// <summary>
        /// The distance between the border of the component and the text.
        /// </summary>
        public Vector2 Padding { get; set; } = new Vector2(2, 2);

        /// <summary>
        /// The font to use for the text. Uses the default font, if not set explicitly.
        /// </summary>
        public FontResource Font { get; set; }

        /// <summary>
        /// Masks the input text, for security reasons. Eg passwords.
        /// </summary>
        public bool IsMasked { get; set; }

        /// <summary>
        /// Marks the input as read-only.
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Get or set the restriction on input characters in the text box.
        /// </summary>
        public CharacterRestriction AllowedCharacters { get; set; }

        /// <summary>
        /// Get or set the max count of characters allowed in the text box.
        /// </summary>
        public uint MaxCharacters { get; set; } = 256;

        /// <summary>
        /// The text hint shown, when no text is set.
        /// </summary>
        public LocalizedString Placeholder { get; set; }

        #region Events

        public event EventHandler TextChanged;
        public event EventHandler FocusLost;

        #endregion

        public override Size GetSize()
        {
            ApplyStyles();

            var textSize = FontResource.MeasureText(_text);
            SizeValue width = Width.IsContentAligned ? (int)Math.Ceiling(textSize.X) + (int)Padding.X * 2 : Width;
            var height = (int)Padding.Y * 2 + (int)Math.Ceiling(textSize.Y);

            RemoveStyles();

            return new Size(width, height);
        }

        protected override void UpdateInternal(Rectangle contentRect)
        {
            var flags = ImGuiInputTextFlags.None;
            if (IsMasked) flags |= ImGuiInputTextFlags.Password;
            if (IsReadOnly) flags |= ImGuiInputTextFlags.ReadOnly;

            switch (AllowedCharacters)
            {
                case CharacterRestriction.Decimal:
                    flags |= ImGuiInputTextFlags.CharsDecimal;
                    break;

                case CharacterRestriction.Hexadecimal:
                    flags |= ImGuiInputTextFlags.CharsHexadecimal;
                    break;
            }

            ImGuiNET.ImGui.SetNextItemWidth(contentRect.Width);

            if (!string.IsNullOrEmpty(Placeholder))
            {
                if (ImGuiNET.ImGui.InputTextWithHint($"##{Id}", Placeholder, ref _text, MaxCharacters, flags))
                    OnTextChanged();

                return;
            }

            if (ImGuiNET.ImGui.InputText($"##{Id}", ref _text, MaxCharacters, flags))
                OnTextChanged();

            // Check if InputText is active and lost focus
            if (!ImGuiNET.ImGui.IsItemActive() && _activePreviousFrame)
                OnFocusLost();

            _activePreviousFrame = ImGuiNET.ImGui.IsItemActive();
        }

        protected override void ApplyStyles()
        {
            if (Font != null)
                ImGuiNET.ImGui.PushFont((ImFontPtr)Font);

            ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Padding);
        }

        protected override void RemoveStyles()
        {
            ImGuiNET.ImGui.PopStyleVar();

            if (Font != null)
                ImGuiNET.ImGui.PopFont();
        }

        private void OnTextChanged()
        {
            TextChanged?.Invoke(this, new EventArgs());
        }

        private void OnFocusLost()
        {
            FocusLost?.Invoke(this, new EventArgs());
        }
    }

    public enum CharacterRestriction
    {
        Any,
        Decimal,
        Hexadecimal
    }
}
