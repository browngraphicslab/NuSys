using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    /// <summary>
    /// this class should replace CanvasBitmap in NuSys.
    /// We will use this to track how many classes are currently using this exact bitmap.
    /// The point of this is to reduce Async Calls, Memory usage, and dispose calls.
    /// </summary>
    public class CanvasBitmapHolder
    {
        /// <summary>
        /// The bitmap used to render.  This should never actually be publicly disposed, only the Holder Class should be disposed
        /// </summary>
        public CanvasBitmap Bitmap { get; private set; }

        /// <summary>
        /// Returns whether the canvas bitmap has already been disposed.  This should never be true for an active image
        /// </summary>
        public bool IsDisposed { get; private set; } = false;

        /// <summary>
        /// bool showing if this should ever be disposed
        /// </summary>
        public bool Disposable { get; private set; } = true;

        /// <summary>
        /// The number of class instances currently using this class
        /// </summary>
        public int References { get; private set; }

        /// <summary>
        /// this bitmap Id will be the URL of the bitmap itself.  This will be used to identify the image at hand
        /// </summary>
        private string _bitmapId;

        /// <summary>
        /// The constructor should only be getting called in the MediaUtil function.
        /// </summary>
        /// <param name="bitmap"></param>
        public CanvasBitmapHolder(CanvasBitmap bitmap, string id, bool disposable = true)
        {
            Debug.Assert(bitmap?.Device != null);
            Debug.Assert(!string.IsNullOrEmpty(id));
            Disposable = disposable;
            Bitmap = bitmap;
            _bitmapId = id;
            References = 1;
        }

        /// <summary>
        /// The constructor should only be getting called in the MediaUtil function.
        /// </summary>
        /// <param name="bitmap"></param>
        public CanvasBitmapHolder(string id, bool disposable = true)
        {
            Debug.Assert(!string.IsNullOrEmpty(id));
            Disposable = disposable;
            _bitmapId = id;
            References = 1;
        }


        /// <summary>
        /// this hacky method should only be called fromt he mediaUtil function.  Here to solve async issues
        /// </summary>
        /// <param name="bitmap"></param>
        public void SetBitmap(CanvasBitmap bitmap)
        {
            Debug.Assert(bitmap?.Device != null);
            Bitmap = bitmap;
        }

        /// <summary>
        /// Public method called to tell this holder class that one more class instance is using this bitmap.
        /// In other words, wait for an additional dispose call before removing/disposing the actual bitmap
        /// </summary>
        /// <param name="incrementBy"></param>
        public void IncrementReferences(int incrementBy = 1)
        {
            References += incrementBy;
        }

        /// <summary>
        /// public method to tell this holder class that one fewer classes needs this image.  
        /// This will also be called by the public 'Dispose' method
        /// </summary>
        /// <param name="decrementBy"></param>
        public void DecrementReferences(int decrementBy = 1)
        {
            References -= decrementBy;
            if (References <= 0)
            {
                PrivateDispose();
            }
        }

        /// <summary>
        /// This dipose method doesn't actually dispose of the bitmap itself.
        /// Rather this will decrement the number of references and dispose if no item is pointing to this holder instance.
        /// </summary>
        public void Dispose()
        {
            DecrementReferences();
        }

        /// <summary>
        /// private method to actually dipose of the image.  
        /// This should also remove it from the MediaUtil's dictionary
        /// </summary>
        private void PrivateDispose()
        {
            if (IsDisposed || !Disposable)
            {
                return;
            }
            IsDisposed = true;
            MediaUtil.RemoveBitmapByUrl(_bitmapId);
            Bitmap?.Dispose();
        }
    }
}
