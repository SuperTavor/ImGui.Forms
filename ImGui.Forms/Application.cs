﻿using System;
using System.Drawing;
using System.Numerics;
using ImGui.Forms.Factories;
using ImGui.Forms.Localization;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Rectangle = Veldrid.Rectangle;

namespace ImGui.Forms
{
    public class Application
    {
        private readonly ExecutionContext _executionContext;

        private DragDropEventEx _dragDropEvent;
        private bool _frameHandledDragDrop;

        public static Application Instance { get; private set; }

        #region Factories

        public IdFactory IdFactory { get; }

        public ImageFactory ImageFactory { get; }

        public FontFactory FontFactory { get; }

        #endregion

        public Form MainForm => _executionContext.MainForm;

        public ILocalizer Localizer { get; }

        private Application(Form mainForm, GraphicsDevice gd, Sdl2Window window, ILocalizer localizer)
        {
            _executionContext = new ExecutionContext(mainForm, gd, window);

            IdFactory = new IdFactory();
            ImageFactory = new ImageFactory(gd, _executionContext.Renderer);
            FontFactory = new FontFactory(ImGuiNET.ImGui.GetIO(), _executionContext.Renderer);

            Localizer = localizer;
        }

        public static Application Create(Form form, ILocalizer localizer = null)
        {
            if (Instance != null)
                throw new InvalidOperationException("There already is an application created.");

            VeldridStartup.CreateWindowAndGraphicsDevice(
                new WindowCreateInfo(20, 20, form.Width, form.Height, WindowState.Normal, form.Title),
                new GraphicsDeviceOptions(true, null, true, ResourceBindingModel.Improved, true, true),
                out var window,
                out var gd);

            return Instance = new Application(form, gd, window, localizer);
        }

        public void Execute()
        {
            _executionContext.Window.Resized += Window_Resized;
            _executionContext.Window.DragDrop += Window_DragDrop;

            var cl = _executionContext.GraphicsDevice.ResourceFactory.CreateCommandList();

            // Main application loop
            while (_executionContext.Window.Exists)
            {
                // TODO: Remove drag drop events after n seconds to not fill it up endlessly if no control could handle them
                _dragDropEvent = default;
                _frameHandledDragDrop = false;

                // Snapshot current machine state
                var snapshot = _executionContext.Window.PumpEvents();
                if (!_executionContext.Window.Exists)
                    break;

                _executionContext.Renderer.Update(1f / 60f, snapshot);

                // Update main form
                _executionContext.MainForm.Update();

                // Update frame buffer
                cl.Begin();
                cl.SetFramebuffer(_executionContext.GraphicsDevice.MainSwapchain.Framebuffer);
                cl.ClearColorTarget(0, new RgbaFloat(SystemColors.Control.R, SystemColors.Control.G, SystemColors.Control.B, 1f));
                _executionContext.Renderer.Render(_executionContext.GraphicsDevice, cl);
                cl.End();
                _executionContext.GraphicsDevice.SubmitCommands(cl);
                _executionContext.GraphicsDevice.SwapBuffers(_executionContext.GraphicsDevice.MainSwapchain);
            }

            // Clean up Veldrid resources
            _executionContext.GraphicsDevice.WaitForIdle();

            _executionContext.Renderer.Dispose();
            cl.Dispose();

            _executionContext.GraphicsDevice.Dispose();
        }

        private void Window_Resized()
        {
            _executionContext.GraphicsDevice.MainSwapchain.Resize((uint)_executionContext.Window.Width, (uint)_executionContext.Window.Height);
            _executionContext.Renderer.WindowResized(_executionContext.Window.Width, _executionContext.Window.Height);

            _executionContext.MainForm.Width = _executionContext.Window.Width;
            _executionContext.MainForm.Height = _executionContext.Window.Height;

            _executionContext.MainForm.OnResized();
        }

        private void Window_DragDrop(DragDropEvent obj)
        {
            _dragDropEvent = new DragDropEventEx(obj, ImGuiNET.ImGui.GetMousePos());
        }

        internal bool TryGetDragDrop(Rectangle controlRect, out DragDropEventEx obj)
        {
            obj = _dragDropEvent;

            // Try get drag drop event
            if (_frameHandledDragDrop || _dragDropEvent.MousePosition == default)
                return false;

            // Check if control contains dropped element
            return _frameHandledDragDrop = controlRect.Contains(new Veldrid.Point((int)obj.MousePosition.X, (int)obj.MousePosition.Y));
        }
    }

    class ExecutionContext
    {
        public Form MainForm { get; }

        public GraphicsDevice GraphicsDevice { get; }

        public Sdl2Window Window { get; }

        public ImGuiRenderer Renderer { get; }

        public ExecutionContext(Form mainForm, GraphicsDevice gd, Sdl2Window window)
        {
            MainForm = mainForm;
            GraphicsDevice = gd;
            Window = window;

            Renderer = new ImGuiRenderer(gd, gd.MainSwapchain.Framebuffer.OutputDescription, mainForm.Width, mainForm.Height);
        }
    }

    struct DragDropEventEx
    {
        public DragDropEvent Event { get; }
        public Vector2 MousePosition { get; }

        public DragDropEventEx(DragDropEvent evt, Vector2 mousePos)
        {
            Event = evt;
            MousePosition = mousePos;
        }
    }
}
