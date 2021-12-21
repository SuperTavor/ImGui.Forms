﻿using System;

namespace ImGui.Forms.Components.Layouts
{
    public class DataTableColumn<TData>
    {
        private readonly Func<TData, string> _valueGetter;

        public string Name { get; }

        public DataTableColumn(Func<TData, string> valueGetter, string name = null)
        {
            Name = name;
            _valueGetter = valueGetter;
        }

        public string Get(TData data) => _valueGetter(data);
    }
}
