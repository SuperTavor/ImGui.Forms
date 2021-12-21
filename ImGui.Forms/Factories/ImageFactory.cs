﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Veldrid;

namespace ImGui.Forms.Factories
{
    public class ImageFactory
    {
        private readonly GraphicsDevice _gd;
        private readonly ImGuiRenderer _controller;

        private readonly IDictionary<object, IntPtr> _inputPointers;
        private readonly IDictionary<IntPtr, Texture> _ptrTextures;

        internal ImageFactory(GraphicsDevice gd, ImGuiRenderer controller)
        {
            _gd = gd;
            _controller = controller;
            _inputPointers = new Dictionary<object, IntPtr>();
            _ptrTextures = new Dictionary<IntPtr, Texture>();
        }

        internal IntPtr LoadImage(Stream tex)
        {
            return LoadImage((Bitmap)Image.FromStream(tex));
        }

        internal IntPtr LoadImage(string path)
        {
            return LoadImage((Bitmap)Image.FromFile(path));
        }

        internal IntPtr LoadImage(Bitmap img)
        {
            if (_inputPointers.ContainsKey(img))
                return _inputPointers[img];

            var ptr = LoadImageInternal(img);
            _inputPointers[img] = ptr;

            return ptr;
        }

        public void UnloadImage(IntPtr ptr)
        {
            if (!_ptrTextures.ContainsKey(ptr))
                return;

            _controller.RemoveImGuiBinding(_ptrTextures[ptr]);
        }

        private IntPtr LoadImageInternal(Bitmap image)
        {
            var texture = _gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
                (uint)image.Width, (uint)image.Height, 1, 1, Veldrid.PixelFormat.B8_G8_R8_A8_UNorm, TextureUsage.Sampled));

            var data = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            _gd.UpdateTexture(texture, data.Scan0, (uint)(4 * image.Width * image.Height),
                    0, 0, 0, (uint)image.Width, (uint)image.Height, 1, 0, 0);

            image.UnlockBits(data);

            // Add image pointer to cache
            var imgPtr = _controller.GetOrCreateImGuiBinding(_gd.ResourceFactory, texture);
            _ptrTextures[imgPtr] = texture;

            return imgPtr;
        }
    }
}
