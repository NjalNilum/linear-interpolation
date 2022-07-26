using System.Runtime.InteropServices;

namespace LinearInterpolation
{
    /// <summary>
    /// 2D Point with int coordinates.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PointInt
    {
        public int x;
        public int y;

        public PointInt(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    /// <summary>
    /// 2D Point with float coordinates.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PointFloat
    {
        public float x;
        public float y;

        public PointFloat(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }

    /// <summary>
    /// Static class for dooing very easy operations on the local point structures.
    /// </summary>
    internal static class PointExtension
    {
        /// <summary>
        /// True if points are equal.
        /// </summary>
        public static bool AreEqual(PointInt p1, PointInt p2)
        {
            return p1.x == p2.x && p1.y == p2.y;
        }


        /// <summary>
        /// True if points are equal.
        /// </summary>
        public static bool AreEqual(PointFloat p1, PointFloat p2)
        {
            var pixelEpsilon = 0.001;
            return Math.Abs(p1.x - p2.x) < pixelEpsilon && Math.Abs(p1.y - p2.y) < pixelEpsilon;
        }

        /// <summary>
        /// Returns the distance between two points.
        /// </summary>
        /// <returns></returns>
        public static double GetDistance(PointInt p1, PointInt p2)
        {
            return Math.Sqrt((Math.Pow(p1.x - p2.x, 2) + Math.Pow(p1.y - p2.y, 2)));
        }
    }

    /// <summary>
    /// Wrapper for dll imports.
    /// </summary>
    internal static class PInvoke
    {
        [DllImport("User32.Dll")]
        private static extern long SetCursorPos(int x, int y);

        [DllImport("User32.Dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref PointInt pointInt);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out PointInt lpPointInt);

        [DllImport("user32.dll", SetLastError = false)]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        [Flags]
        private enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
            // Legacy flag, should not be used.
            // ES_USER_PRESENT = 0x00000004
        }

        /// <summary>
        /// Returns the current mouse position.
        /// </summary>
        /// <returns>PointInt within mouse position.</returns>
        public static PointInt GetCursorPosition()
        {
            GetCursorPos(out var cursorPosition);
            // If error handling needed
            // bool success = GetCursorPos(out lpPointInt);
            // if (!success)

            return cursorPosition;
        }

        /// <summary>
        /// Set mouse position.
        /// </summary>
        /// <param name="position">Position to set the mouse cursor on.</param>
        public static void SetCursorPosition(PointInt position)
        {
            ClientToScreen(GetDesktopWindow(), ref position);
            SetCursorPos(position.x, position.y);
        }

        /// <summary>
        /// Converts point from int to float.
        /// </summary>
        /// <returns></returns>
        public static PointFloat PointInt2Float(PointInt pointInt)
        {
            return new PointFloat(pointInt.x, pointInt.y);
        }

        /// <summary>
        /// Converts point from float to int.
        /// </summary>
        /// <param name="pointFoloat"></param>
        public static PointInt PointFloat2Int(PointFloat pointFoloat)
        {
            return new PointInt((int)pointFoloat.x, (int)pointFoloat.y);
        }

        /// <summary>
        /// Disables screensaver, automatic loggoff and what not by setting flags of the current thread executing state.
        /// </summary>
        public static void DisableScreenSaverAndLogOff()
        {
            // disable log off and screensaver and what not
            if (PInvoke.SetThreadExecutionState(PInvoke.EXECUTION_STATE.ES_CONTINUOUS
                                                | PInvoke.EXECUTION_STATE.ES_DISPLAY_REQUIRED
                                                | PInvoke.EXECUTION_STATE.ES_SYSTEM_REQUIRED
                                                | PInvoke.EXECUTION_STATE.ES_AWAYMODE_REQUIRED) == 0)
            {
                PInvoke.SetThreadExecutionState(PInvoke.EXECUTION_STATE.ES_CONTINUOUS
                                                | PInvoke.EXECUTION_STATE.ES_DISPLAY_REQUIRED
                                                | PInvoke.EXECUTION_STATE.ES_SYSTEM_REQUIRED);
            }
        }
    }
}
