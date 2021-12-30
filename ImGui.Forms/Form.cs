﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Controls.Menu;
using ImGui.Forms.Extensions;
using ImGui.Forms.Modals;
using ImGui.Forms.Models;
using ImGuiNET;

namespace ImGui.Forms
{
    // HINT: Does not derive from Container to not be a component and therefore nestable into other containers
    public abstract class Form
    {
        private readonly IList<Modal> _modals = new List<Modal>();

        private Image _icon;
        private bool _setIcon;

        #region Properties

        public string Title { get; set; } = string.Empty;
        public Vector2 Size { get; set; } = new Vector2(700, 400);
        public int Width => (int)Size.X;
        public int Height => (int)Size.Y;

        public Theme Theme { get; set; } = Theme.Dark;

        public Image Icon
        {
            get => _icon;
            protected set
            {
                _icon = value;
                _setIcon = true;
            }
        }

        public MainMenuBar MainMenuBar { get; protected set; }

        public Component Content { get; protected set; }

        public Vector2 Padding { get; protected set; } = new Vector2(2, 2);

        public FontResource DefaultFont { get; set; }

        #endregion

        #region Events

        public event EventHandler Load;
        public event EventHandler Resized;
        public event EventHandler<ClosingEventArgs> Closing;
        public event EventHandler Closed;

        #endregion

        public void PushModal(Modal modal)
        {
            if (_modals.Count > 0)
                _modals.Last().ChildModal = modal;

            _modals.Add(modal);
        }

        public void PopModal()
        {
            _modals.Remove(_modals.Last());

            if (_modals.Count > 0)
                _modals.Last().ChildModal = null;
        }

        public void Update()
        {
            // Set icon
            if (_setIcon)
            {
                Sdl2NativeExtensions.SetWindowIcon(Application.Instance.Window.SdlWindowHandle, (Bitmap)Icon);
                _setIcon = false;
            }

            // Set window title
            Application.Instance.Window.Title = Title;

            // Begin window
            ImGuiNET.ImGui.Begin(Title, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove);

            ImGuiNET.ImGui.SetWindowSize(Size, ImGuiCond.Always);

            ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
            ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0);
            ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);

            // Set theme colors
            switch (Theme)
            {
                case Theme.Light:
                    ImGuiNET.ImGui.StyleColorsLight();
                    break;

                case Theme.Dark:
                    ImGuiNET.ImGui.StyleColorsDark();
                    break;
            }

            // Push font to default to
            if (DefaultFont != null)
                ImGuiNET.ImGui.PushFont((ImFontPtr)DefaultFont);

            // Add menu bar
            MainMenuBar?.Update();
            var menuHeight = MainMenuBar?.Height ?? 0;

            // Add form controls
            ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Padding);
            ImGuiNET.ImGui.SetWindowPos(new Vector2(0, menuHeight));

            var contentPos = ImGuiNET.ImGui.GetCursorScreenPos();
            var contentWidth = Content?.GetWidth(Width - (int)Padding.X * 2) ?? 0;
            var contentHeight = Content?.GetHeight(Height - (int)Padding.Y * 2 - menuHeight) ?? 0;
            Content?.Update(new Veldrid.Rectangle((int)contentPos.X, (int)contentPos.Y, contentWidth, contentHeight));

            // Add modals
            var modal = _modals.Count > 0 ? _modals.First() : null;
            if (modal != null)
            {
                var modalPos = new Vector2((Width - modal.Width) / 2f - Padding.X, (Height - modal.Height) / 2f - contentPos.Y / 2f);
                var modalContentSize = new Vector2(modal.Width, modal.Height);
                var modalSize = modalContentSize + new Vector2(Padding.X * 2, Modal.HeaderHeight + Padding.Y * 2);

                ImGuiNET.ImGui.SetNextWindowPos(modalPos);
                ImGuiNET.ImGui.SetNextWindowSize(modalSize);
                modal.Update(new Veldrid.Rectangle((int)modalPos.X, (int)modalPos.Y, (int)modalContentSize.X, (int)modalContentSize.Y));
            }

            // End window
            ImGuiNET.ImGui.End();
        }

        internal void OnResized()
        {
            Resized?.Invoke(this, new EventArgs());
        }

        internal void OnLoad()
        {
            Load?.Invoke(this, new EventArgs());
        }

        internal bool OnClosing()
        {
            var args = new ClosingEventArgs();
            Closing?.Invoke(this, args);

            return args.Cancel;
        }

        internal void OnClosed()
        {
            Closed?.Invoke(this, new EventArgs());
        }
    }

    public class ClosingEventArgs : EventArgs
    {
        public bool Cancel { get; set; }
    }
}
