# BeatNet.CodeGen
**Analyses Beat Saber source code and automatically generates types for the game's network packets and structures.** 

## Requirements
- .NET 7
- dnSpy
- Beat Saber (tested on v1.34.6)

## Setup
Clone the project, then build with:

```bash
dotnet build
```

## Usage

### Source export
Use dnSpy to open the assemblies from `<Beat Saber>/Beat Saber_Data/Managed` that are relevant to BeatNet:

- `Main.dll`
- `BGNetCore.dll`
- `GameplayCore.dll`

For each assembly, export the source code using `File` → `Export to project`.

ℹ️ Tip: You can select multiple (or all) assemblies using <kbd>CTRL</kbd> / <kbd>Shift</kbd> and export multiple assemblies at once. 

⚠️ Note: This is a **dumb** analyzer, do not (re)format the source code and use default dnSpy settings - otherwise the code generator may not be able to parse the source code.

### Code generation
Run the code generator with the following command line arguments:

```bash
./BeatNet.CodeGen.exe <source-dir> <output-dir>
```

| Argument     | Description                                                                                                                   |
|--------------|-------------------------------------------------------------------------------------------------------------------------------|
| `source-dir` | The directory containing the exported source code. Should contain subdirectories for each assembly (e.g. a `Main` directory). |
| `output-dir` | The directory to output the generated code to.                                                                                |