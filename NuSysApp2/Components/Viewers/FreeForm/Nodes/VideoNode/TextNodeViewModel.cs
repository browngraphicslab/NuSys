﻿using System;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class TextNodeViewModel : ElementViewModel
    {
        #region Private Members
        private string _rtf = string.Empty;

        #endregion Private Members

        public string Text { get; set; }

        public delegate void TextBindingChangedHandler(object source, string text);
        public event TextBindingChangedHandler TextBindingChanged;

        public TextNodeViewModel(ElementController controller) : base(controller)
        {           
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 164, 220, 255));
            Text = controller.LibraryElementModel?.Data;
            ((TextNodeController) controller).TextChanged += TextChanged;

            controller.Disposed += ControllerOnDisposed;

        }

        private void ControllerOnDisposed(object source)
        {
            ((TextNodeController)Controller).TextChanged -= TextChanged;
            Controller.Disposed -= ControllerOnDisposed;
        }

        private void TextChanged (object sender, string text, ElementViewModel originalSenderViewModel)
        {
            if (originalSenderViewModel != this)
            {
                TextBindingChanged?.Invoke(this, text);
                Text = text;
            }
        }
        #region Public Properties     

        public string RtfText
        {
            get
            {
                return _rtf;
            }

            set
            {
                _rtf = value;
                RaisePropertyChanged("RtfText");
            }

        }

        #endregion Public Properties
    }
}