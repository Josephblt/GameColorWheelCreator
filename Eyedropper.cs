using System;
using System.Drawing;
using System.Runtime.InteropServices;

sealed class Eyedropper
{
    #region External Methods

    [DllImport("user32.dll")]
    static extern IntPtr GetDC(IntPtr hwnd);

    [DllImport("user32.dll")]
    static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc);

    [DllImport("gdi32.dll")]
    static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

    #endregion

    #region Public Methods

    public static Color GetPixelColor(Point position)
    {
        IntPtr hdc = GetDC(IntPtr.Zero);
        uint pixel = GetPixel(hdc, position.X, position.Y);
        ReleaseDC(IntPtr.Zero, hdc);
        Color color = Color.FromArgb((int)(pixel & 0x000000FF),
                                     (int)(pixel & 0x0000FF00) >> 8,
                                     (int)(pixel & 0x00FF0000) >> 16);
        return color;
    }

    #endregion
}