using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KerbalKonstructs.UI
{
    abstract internal class KKWindow
    {

        /// <summary>
        /// Basic drawing function. Put excludes or references to other drawing functions in a override here
        /// </summary>
        public virtual void Draw()
        {

        }

        /// <summary>
        /// registers the drawing function to the windowmanager
        /// </summary>
        public virtual void Open()
        {
            WindowManager.OpenWindow(this.Draw);
        }

        /// <summary>
        /// unregisters the drawing function to the windowmanager
        /// </summary>
        public virtual void Close()
        {
            WindowManager.CloseWindow(this.Draw);
        }

        /// <summary>
        /// Switches the state of the window
        /// </summary>
        public virtual void Toggle()
        {
            if (IsOpen())
            {
                Close();
            } else {
                Open();
            }
        }

        /// <summary>
        /// return if a window is open
        /// </summary>
        /// <returns>true if window is open</returns>
        public virtual bool IsOpen()
        {
            return WindowManager.IsOpen(this.Draw);
        }

    }
}
