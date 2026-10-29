// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if LIBREWINFORMS_PORTABLE
using System.Drawing;
using LibreWinForms.Platform;

namespace System.Windows.Forms;

/// <summary>
/// Lets another UI framework host a LibreWinForms control tree that has no window of its own: the host
/// draws the tree into its own surface and forwards input to it. The WPF <c>WindowsFormsHost</c> is the
/// client; controls other than <see cref="Form"/> carry logical handles, so there is no child HWND to parent.
/// </summary>
public static class LibreControlHosting
{
    /// <summary>
    /// Paints <paramref name="root"/> and its descendants with the control's origin at the graphics origin,
    /// restricted to <paramref name="clip"/> (in the control's own coordinates).
    /// </summary>
    public static void Paint(Control root, Graphics graphics, Rectangle clip)
    {
        ArgumentNullException.ThrowIfNull(root);
        root.PaintPortableTreeInto(graphics, clip);
    }

    /// <summary>
    /// Delivers a normalized input event to the tree rooted at <paramref name="root"/>. Positions are in the
    /// root's own coordinates.
    /// </summary>
    public static void DispatchInput(Control root, in LibreInputEvent inputEvent)
    {
        ArgumentNullException.ThrowIfNull(root);
        root.DispatchPortableInput(inputEvent);
    }
}
#endif
