// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if LIBREWINFORMS_PORTABLE
using System.Drawing;

namespace System.Windows.Forms;

/// <summary>
///  Portable TabControl state. On Windows the control wraps comctl32's tab control and asks it
///  (TCM_GETCURSEL, TCM_GETITEMRECT, TCM_ADJUSTRECT, ...) for its selection and geometry. A
///  portable handle has no USER32 window behind it, so the selection lives in the managed
///  <see cref="_selectedIndex"/> field and the geometry below reproduces comctl32's default single
///  row layout: a strip of tab headers along <see cref="Alignment"/>, the page area inset from it.
/// </summary>
public partial class TabControl
{
    // comctl32 insets the page area this far from the control's other three edges.
    private const int PortablePageInset = 4;

    // First header's offset along the strip.
    private const int PortableFirstTabOffset = 2;

    private bool IsPortableVertical => Alignment is TabAlignment.Left or TabAlignment.Right;

    /// <summary>The strip's thickness across <see cref="Alignment"/>: the item height.</summary>
    private int GetPortableTabStripThickness()
        => ShouldSerializeItemSize() && _itemSize.Height > 0
            ? _itemSize.Height
            : FontHeight + (2 * _padding.Y);

    private int GetPortableTabLength(int index)
    {
        if (SizeMode == TabSizeMode.Fixed && ShouldSerializeItemSize() && _itemSize.Width > 0)
        {
            return _itemSize.Width;
        }

        TabPage page = _tabPages[index];
        int length = TextRenderer.MeasureText(page.Text ?? string.Empty, Font).Width + (2 * _padding.X);
        if (_imageList is not null && page.ImageIndexer.ActualIndex >= 0)
        {
            length += _imageList.ImageSize.Width + _padding.X;
        }

        return Math.Max(length, 2 * _padding.X);
    }

    /// <summary>comctl32 selects the first item inserted into an empty, created control.</summary>
    private void SelectPortableFirstInsertedTab(int index)
    {
        if (IsHandleCreated && _selectedIndex == -1 && index >= 0)
        {
            _selectedIndex = index;
        }
    }

    /// <summary>The header rectangle of tab <paramref name="index"/> (TCM_GETITEMRECT).</summary>
    private RECT GetPortableTabRect(int index)
    {
        int thickness = GetPortableTabStripThickness();
        int offset = PortableFirstTabOffset;
        for (int i = 0; i < index; i++)
        {
            offset += GetPortableTabLength(i);
        }

        int length = GetPortableTabLength(index);
        Size size = ClientSize;
        Rectangle bounds = Alignment switch
        {
            TabAlignment.Bottom => new Rectangle(offset, size.Height - thickness, length, thickness),
            TabAlignment.Left => new Rectangle(0, offset, thickness, length),
            TabAlignment.Right => new Rectangle(size.Width - thickness, offset, thickness, length),
            _ => new Rectangle(offset, 0, length, thickness),
        };

        return bounds;
    }

    /// <summary>The page area (TCM_ADJUSTRECT with fLarger = FALSE), in client coordinates.</summary>
    private Rectangle GetPortableDisplayRectangle()
    {
        int strip = TabCount > 0 ? GetPortableTabStripThickness() + 1 : PortablePageInset;
        Size size = ClientSize;
        int acrossStrip = IsPortableVertical ? size.Width : size.Height;
        int alongStrip = IsPortableVertical ? size.Height : size.Width;
        int acrossLength = Math.Max(0, acrossStrip - strip - PortablePageInset);
        int alongLength = Math.Max(0, alongStrip - (2 * PortablePageInset));

        return Alignment switch
        {
            TabAlignment.Bottom => new Rectangle(PortablePageInset, PortablePageInset, alongLength, acrossLength),
            TabAlignment.Left => new Rectangle(strip, PortablePageInset, acrossLength, alongLength),
            TabAlignment.Right => new Rectangle(PortablePageInset, PortablePageInset, acrossLength, alongLength),
            _ => new Rectangle(PortablePageInset, strip, alongLength, acrossLength),
        };
    }
}
#endif
