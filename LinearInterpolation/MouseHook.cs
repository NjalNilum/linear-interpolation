namespace LinearInterpolation
{
    /// <summary>
    /// MouseHook doesn't work on my machine or in .net 6.0 or what ever. It doesn't work.
    /// I made a polling thing on my own.
    /// If you want to set the mouse cursor in code, use the method in here, otherwise this may cause some strange behavior.
    /// </summary>
    public class MouseHook
    {
        public delegate void MouseHookEventHandler(object sender, MouseHookEventArgs e);
        private MouseHookEventHandler? mouseMove;
        private PointInt lastMousePosition;
        private bool punning;

        /// <summary>
        /// Event is called every time when mouse was moved with a maximum rate of 30Hz.
        /// </summary>
        public event MouseHookEventHandler MouseMove
        {
            add
            {
                this.mouseMove += value;
                PollPositionAsync();
            }
            remove
            {
                this.mouseMove -= value;
                this.punning = false;
            }
        }

        protected virtual void OnMouseMove(MouseHookEventArgs e)
        {
            var mouseHookEventHandler = this.mouseMove;
            mouseHookEventHandler?.Invoke(this, e);
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        public MouseHook()
        {
            this.lastMousePosition = PInvoke.GetCursorPosition();
            this.punning = true;
        }

        /// <summary>
        /// Avoid a mouse move event when manually set the mouse cursor.
        /// </summary>
        public void SetMouseCursorManual(PointInt newPosition)
        {
            lock (this)
            {
                this.lastMousePosition = newPosition;
                PInvoke.SetCursorPosition(newPosition);
            }
        }

        /// <summary>
        /// Poll mouse position.
        /// </summary>
        /// <returns></returns>
        private async void PollPositionAsync()
        {
            await Task.Run(() =>
                           {
                               while (this.punning)
                               {
                                   lock (this)
                                   {
                                       Thread.CurrentThread.Priority = ThreadPriority.Highest;
                                       var actualPosition = PInvoke.GetCursorPosition();

                                       if (!PointExtension.AreEqual(actualPosition, this.lastMousePosition))
                                       {
                                           OnMouseMove(new MouseHookEventArgs(this.lastMousePosition, actualPosition));
                                           this.lastMousePosition = actualPosition;
                                       }
                                   }

                                   Thread.Sleep(33); // FPS = 30
                               }
                           });
        }
    }

    /// <summary>
    /// </summary>
    public class MouseHookEventArgs : EventArgs
    {
        public PointInt OldPosition;
        public PointInt NewPosition;

        public MouseHookEventArgs(PointInt oldPos, PointInt newPos)

        {
            this.OldPosition = oldPos;
            this.NewPosition = newPos;
        }
    }
}