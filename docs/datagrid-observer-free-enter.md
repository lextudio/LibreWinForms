# Observer-free DataGridView Enter source contracts

Two canonical source cases protect the ordinary new-row Name editing path without
attaching grid/editor validation, DataError, focus or key observers. They run a
real Form and DataGridView through Application.Run, initialize the Name column
during Load and queue each pointer/key/text event independently. They require
the actual DataGridViewTextBoxEditingControl to contain Alice before Enter, then
the cell value, ended edit session and next-row position after Enter. The second
case includes a typed native-owner focus-loss/gain cycle before typing.

These controls use the original sample's Name-column policy and geometry, not
its unrelated custom masked columns. They do not simulate Linux input or qualify
the original application. The mandatory source suite includes both controls.

## Remaining native failure

The original Linux attempt retained in `librewinforms-grid-input.p0Cv34` had a
focused canonical editor containing Alice, but Return did not commit the cell.
The later attempt with extra validation observers passed. Merely attaching
CellValidating or RowValidating handlers adds a canonical CorrectFocus call;
that observer is not semantically inert. Neither result proves causality or a
repair. No validation handler, forced commit or focus repair is added here.

A separate controlled diagnostic called grid.Focus after the pointer-created
empty editor. Its expected commit failed, but the pre-Enter checkpoint showed
an empty, unfocused editor with the grid focused and the edit session active.
That is not the recorded Linux Alice/Enter failure. The diagnostic source and
both failed logs remain separate under `artifacts/dropdown-keyboard`, including
`grid-focus-diagnostic/CanonicalGridEnterCommitTests.cs`. No product change is
justified by treating those distinct observations as the same defect.

The first combined run had 392 passes and this one distinct diagnostic failure
(393 total, zero skips). A focused checkpoint run had two passes and the same
pre-Enter failure. Native Linux acceptance and the explicit grid-focus/native
reference comparison remain outstanding. No VM or native application was used
for these source checks.

The canonical DataGridView WndProc has no WM_SETFOCUS-specific implementation;
it delegates to Control. Native Control.WmSetFocus activates the containing
container before raising GotFocus. Portable FocusInternal updates its logical
focus and then the container's active control. CorrectFocus is invoked from
specific grid operations and subscribed event paths, not unconditionally from
DataGridView.OnGotFocus. This inspection does not establish which observed
explicit-focus behavior differs from Windows. A separate public Microsoft
WinForms reference is prepared under `artifacts/dropdown-keyboard/grid-focus-reference`
but has not been built or run by the author.

Only the two passing observer-free cases are admitted to source CI (minimum
392). Their outcomes were proven in both retained runs above. The final selection
then passed 392/392, zero skips, on macOS ARM64 / .NET 10.0.5 in 1.583 seconds.
The warm test-only compile completed with zero warnings/errors in 4.02 seconds,
against unchanged source-built product outputs from `8358eb96a` and the existing
ProGPU `08f4343ef` closure; no dependency graph rebuild was needed. Exact results
are retained in `grid-enter-final-build.log` and `grid-enter-final-tests.log`
under `artifacts/dropdown-keyboard/log`. No failed test or native outcome is
relabeled by this passing source gate.
