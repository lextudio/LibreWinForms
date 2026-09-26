// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Drawing;
using System.Windows.Forms;
using FluentAssertions;
using LibreWinForms.Platform;
using Xunit;

namespace LibreWinForms.CanonicalLifecycle.Tests;

public partial class CanonicalLifecycleTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void QueuedGridEnterCommitsWithoutValidationObservers(bool nativeFocusCycle)
    {
        HeadlessPlatform platform = UseHeadlessPlatform(autoCloseWindows: false);
        using Form form = new() { ClientSize = new Size(634, 515), ShowIcon = false };
        using DataGridView grid = new() { Bounds = new Rectangle(26, 119, 577, 299) };
        grid.RowTemplate.Height = 21;
        form.Controls.Add(grid);
        // Match the original sample's ordinary Name column, default edit mode,
        // empty new row and Load-time column initialization. No grid/editor
        // validation, DataError, focus or key observers are installed.
        form.Load += (_, _) => grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Name", Width = 120 });
        Queue<Action> phases = new();
        LibrePoint point = default;
        phases.Enqueue(() =>
        {
            platform.SendControlInput(form, new LibreInputEvent(LibreInputEventKind.FocusGained,
                1, LibreInputModifiers.None, LibreKey.Unknown, null, default, default, LibrePointerButton.None));
            Rectangle bounds = grid.GetCellDisplayRectangle(0, 0, cutOverflow: false);
            point = new LibrePoint(grid.Left + bounds.Left + bounds.Width / 2, grid.Top + bounds.Top + bounds.Height / 2);
        });
        EnqueueInput(LibreInputEventKind.PointerMove);
        EnqueueInput(LibreInputEventKind.PointerDown, button: LibrePointerButton.Primary);
        EnqueueInput(LibreInputEventKind.PointerUp, button: LibrePointerButton.Primary);
        if (nativeFocusCycle)
        {
            EnqueueInput(LibreInputEventKind.FocusLost);
            EnqueueInput(LibreInputEventKind.FocusGained);
        }

        EnqueueInput(LibreInputEventKind.KeyDown, LibreKey.LeftShift, modifiers: LibreInputModifiers.Shift);
        EnqueueInput(LibreInputEventKind.KeyDown, LibreKey.A, modifiers: LibreInputModifiers.Shift);
        EnqueueInput(LibreInputEventKind.TextInput, text: "A", modifiers: LibreInputModifiers.Shift);
        EnqueueInput(LibreInputEventKind.KeyUp, LibreKey.A, modifiers: LibreInputModifiers.Shift);
        EnqueueInput(LibreInputEventKind.KeyUp, LibreKey.LeftShift);
        foreach ((LibreKey key, string text) in new[] { (LibreKey.L, "l"), (LibreKey.I, "i"), (LibreKey.C, "c"), (LibreKey.E, "e") })
        {
            EnqueueInput(LibreInputEventKind.KeyDown, key);
            EnqueueInput(LibreInputEventKind.TextInput, text: text);
            EnqueueInput(LibreInputEventKind.KeyUp, key);
        }

        phases.Enqueue(() =>
        {
            grid.EditingControl.Should().BeOfType<DataGridViewTextBoxEditingControl>();
            grid.EditingControl!.Text.Should().Be("Alice",
                "the editor must receive the text before testing Enter; grid focused={0}, editor focused={1}, in edit={2}",
                grid.Focused, grid.EditingControl.Focused, grid.IsCurrentCellInEditMode);
        });
        EnqueueInput(LibreInputEventKind.KeyDown, LibreKey.Enter);
        EnqueueInput(LibreInputEventKind.KeyUp, LibreKey.Enter);
        bool completed = false;
        phases.Enqueue(() =>
        {
            grid.Rows[0].Cells[0].Value.Should().Be("Alice");
            grid.IsCurrentCellInEditMode.Should().BeFalse();
            grid.Rows[0].IsNewRow.Should().BeFalse();
            grid.CurrentCell!.RowIndex.Should().Be(1);
            completed = true;
        });
        form.Shown += (_, _) => platform.Post(Advance);
        Application.Run(form);
        completed.Should().BeTrue();

        void EnqueueInput(LibreInputEventKind kind, LibreKey key = LibreKey.Unknown, string? text = null,
            LibreInputModifiers modifiers = LibreInputModifiers.None, LibrePointerButton button = LibrePointerButton.None)
            => phases.Enqueue(() => platform.SendControlInput(form,
                new LibreInputEvent(kind, 1, modifiers, key, text, point, default, button)));

        void Advance()
        {
            try
            {
                phases.Dequeue()();
                if (phases.Count > 0)
                {
                    platform.Post(Advance);
                }
                else
                {
                    form.Close();
                }
            }
            catch
            {
                form.Close();
                throw;
            }
        }
    }
}
