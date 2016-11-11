using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KerbalKonstructs.UI
{
    abstract internal class KKWindow
    {

        public virtual void Draw()
        {

        }

        public virtual void Open()
        {
            WindowManager.OpenWindow(this.Draw);
        }

        public virtual void Close()
        {
            WindowManager.CloseWindow(this.Draw);
        }

        public virtual void Toggle()
        {
            if (IsOpen())
            {
                Close();
            } else {
                Open();
            }
        }
        public virtual bool IsOpen()
        {
            return WindowManager.IsOpen(this.Draw);
        }

    }
}
