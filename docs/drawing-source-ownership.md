# Portable Drawing source ownership

The portable Drawing implementation, tests and benchmarks are owned by
LibreWinForms under `src/ProGPU.Drawing/`. The upstream Microsoft projects under
`src/System.Drawing.Common/` remain separate, unchanged reference/source inputs.
The moved implementation and test bytes match ProGPU commit
`3f1454a01dba3d6338c56fabfde7bf6a160c5c62`; its published history is preserved.

The public package remains `ProGPU.System.Drawing.Common`. Its assembly remains
`System.Drawing.Common`, version `10.0.0.0`, with public key token
`c29c9752855ee183`. Moving the source does not change drawing behavior, public
types, the API compatibility baseline or the original tests.

The portable projects use the exact ProGPU source root selected by
`LibreWinFormsProGpuSourceRoot` (or `ProGpuSourceRoot` for a direct project build).
The default is the pinned `external/ProGPU` submodule. Core rendering, text,
native runtimes and native package qualification stay in ProGPU. The common
package version still covers the full drawing runtime dependency closure.

Source and package gates select this repository's Drawing project explicitly.
They retain strong-name checks, exact source/package DLL hashes, API comparison,
the full unfiltered Drawing suite and its failure-retained EventPipe capture.
The producer may build this source through a recorded, nonrecursive framework
checkout; its manifest records the exact LibreWinForms commit. Neither a partial
source build nor the source move qualifies native payloads or desktop behavior.

For a direct build from the ProGPU SDK working directory:

```sh
dotnet test /path/to/LibreWinForms/src/ProGPU.Drawing/System.Drawing.Common.Tests/System.Drawing.Common.Tests.csproj \
  -c Release -p:ProGpuSourceRoot=/path/to/ProGPU/
```

The repository's normal source-first/package workflows remain the complete
consumer gates. Preserve published history; remove only obsolete generated
outputs after their logs and required diagnostics have been retained.
