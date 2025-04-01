# BeatNet.CodeGen
**Analyses Beat Saber source code and automatically generates types for the game's network packets and structures.** 

## Requirements
- .NET 8
- dnSpy
- Beat Saber (tested on v1.38.0 - v1.40.4)

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
- `BeatmapCore.dll`
- `BeatSaber.AvatarCore.dll`

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


⚠️ Warning: The **output directory** will be wiped clean before generating the code. Any files or changes in that directory will be lost.

### Output
The following directories / namespaces will be generated:

| Namespace          | Description                                                                                                         |
|--------------------|---------------------------------------------------------------------------------------------------------------------|
| `Packet`           | Connected player manager (CPM) packets; low-level packets.                                                          |
| `Rpc`              | Remote procedure call (RPC) packets; top-level packets. Further divided into RPC manager type (`Menu`, `Gameplay`). |
| `NetSerializable`  | Game structures that are serializable over the network.                                                             |
| `Enum`             | Enumerations used in the game that are referenced by NetSerializables and packets.                                  |

### Special cases
Some types that fall into these categories will NOT be automatically generated (too complex for this dumb codegen).

You will need to provide a manual implementation for these that implements `INetSerializable`:

- `BitMaskArray`
- `BitMaskSparse`
- `ByteArrayNetSerializable`

### Expected warnings
As of v1.40.4, the following warnings may be expected:

> WARNING: Possibly incomplete type: `NoteSpawnInfoNetSerializable` - inconsistent field count (20) vs. instruction count (19)

One less instruction is expected because the `beat` field is contained in the type but not serialized.

> WARNING: Possibly incomplete type: `ObstacleSpawnInfoNetSerializable` - inconsistent field count (12) vs. instruction count (10)

Two fewer instructions are expected because the `startBeat` and `endBeat` fields are contained in the type but not serialized.

> WARNING: Possibly incomplete type: `SliderSpawnInfoNetSerializable` - inconsistent field count (27) vs. instruction count (26)

One less instruction is expected because the `headBeat` field is contained in the type but not serialized.