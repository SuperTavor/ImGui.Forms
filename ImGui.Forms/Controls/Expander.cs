﻿using System;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Models;
using ImGuiNET;
using Veldrid;

namespace ImGui.Forms.Controls
{
    public class Expander : Component
    {
        public string Caption { get; set; } = string.Empty;

        public Component Content { get; set; }

        public int ContentHeight { get; set; } = 200;

        public bool Expanded { get; set; }

        #region Events

        public event EventHandler ExpandedChanged;

        #endregion

        public override Size GetSize()
        {
            var size = ImGuiNET.ImGui.CalcTextSize(Caption);
            return new Size(1f, (int)((int)Math.Ceiling(size.Y) + (Expanded ? ImGuiNET.ImGui.GetStyle().ItemSpacing.X + ContentHeight : 0)));
        }

        protected override void UpdateInternal(Rectangle contentRect)
        {
            var expanded = Expanded;
            var flags = expanded ? ImGuiTreeNodeFlags.DefaultOpen : ImGuiTreeNodeFlags.None;

            expanded = ImGuiNET.ImGui.CollapsingHeader(Caption ?? string.Empty, flags);
            if (expanded)
                Content?.Update(new Rectangle(contentRect.X, contentRect.Y, contentRect.Width, contentRect.Height));

            if (Expanded != expanded)
            {
                Expanded = expanded;
                OnExpandedChanged();
            }
        }

        private void OnExpandedChanged()
        {
            ExpandedChanged?.Invoke(this, new EventArgs());
        }
    }
}
