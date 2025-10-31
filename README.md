## Veritool Collector

Self-contained .NET utility that replaces `veritool.bat` with a configurable data collection workflow for TRI field engineers.

### Features
- Captures OS, RAM, and GPU details into `system_info.txt`.
- Copies TRI artifacts (TriMotion config, calibration and gray level logs, TRI_AXI library) into a timestamped `artifacts/` folder.
- Exports `HKEY_LOCAL_MACHINE\SOFTWARE\TRI\TR7600` registry keys via `reg.exe`.
- Updates the `FAE Check List` workbook (or writes CSV fallback) using bindings defined in `veritool.config.json`.
- Emits a machine-readable CSV snapshot and structured log file for traceability.

### Configure
Edit `veritool.config.json` to point at site-specific file paths, registry keys, and checklist cell bindings. The shipped defaults mirror the legacy batch script and populate cells on the `FAE Check List` sheet (`C3`–`F24`).

### Build & Publish
1. Install the .NET 8 SDK on a Windows machine.
2. Restore dependencies:
   ```
   dotnet restore src/VeritoolCollector/VeritoolCollector.csproj
   ```
3. Publish a single-file, self-contained executable (example for 64-bit Windows):
   ```
   dotnet publish src/VeritoolCollector/VeritoolCollector.csproj \
     -c Release \
     -r win-x64 \
     --self-contained true \
     -p:PublishSingleFile=true \
     -p:PublishTrimmed=true
   ```
   The resulting `VeritoolCollector.exe` in `bin/Release/net8.0/win-x64/publish/` runs without a pre-installed runtime.

### Run
```
VeritoolCollector.exe \
  --dest D:\\TRI\\Backups\\2025-08-07 \
  --checklist C:\\Path\\To\\Checklist_T2003040941-003.xlsx \
  --meta FAE=Maxym --meta Customer=IE001 --meta MachineModel=TR7600LLSIII \
  --meta SerialNumber=T2003040941-003 --meta TubeSerial=NM2030 \
  --meta CameraSerial="021988/021989/022006/022007/022008" \
  --meta ResolutionSpec="30/25/20/15/10/5" --meta Height="157.577 mm (Reg: Src2Img)" \
  --meta AxiVersion=3.6.76.2 --meta PlcVersion=2.18
```

Outcome:
- `system_info.txt` with device details.
- `artifacts/` containing copied files, directories, and registry export.
- Updated checklist workbook (`Checklist_YYYYMMDD_HHMMSS.xlsx`) plus `checklist_snapshot.csv` summarising values and artifact status.
- `logs/collector.log` with a full run trace.

### Notes
- Run from an elevated prompt if `reg.exe export` or protected folders require admin rights.
- Nonexistent artifacts record `Not Found` while keeping the rest of the run intact.
- Extend with future PySide6 UI by wrapping command invocation and reusing the JSON configuration.

