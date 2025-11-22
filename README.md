# Mystery Gift Injector

A Windows Forms application for injecting Mystery Gift event tickets into Pokemon Generation 3 game save files (Emerald, Fire Red, and Leaf Green).

## Features

- Inject Aurora Ticket for Pokemon Emerald
- Inject Mystic Ticket or Aurora Ticket for Pokemon Fire Red/Leaf Green
- Simple drag-and-drop interface
- Automatic CRC validation and correction
- Preserves save file integrity

## Supported Games

- Pokemon Emerald
- Pokemon Fire Red
- Pokemon Leaf Green

## Supported Mystery Gifts

### Emerald
- Aurora Ticket (access to Birth Island to catch Deoxys)

### Fire Red / Leaf Green
- Mystic Ticket (access to Navel Rock to catch Ho-Oh and Lugia)
- Aurora Ticket (access to Birth Island to catch Deoxys)

## Requirements

- Windows OS
- .NET Framework 4.0 or higher

## Usage

1. Launch the application
2. Select your game version (Emerald or Fire Red/Leaf Green)
3. Select the Mystery Gift ticket you want to inject
4. Drag and drop your save file onto the window
5. The modified save file will be created in the same directory as the application

## Build Instructions

### Prerequisites
- Visual Studio 2010 or later (or MSBuild)
- .NET Framework 4.0 SDK

### Building
1. Open `MysteryGiftInjector.sln` in Visual Studio
2. Build the solution (F6 or Build > Build Solution)
3. The executable will be in `MysteryGiftInjector/MysteryGiftInjector/bin/Release/`

Alternatively, using MSBuild from command line:
```
msbuild MysteryGiftInjector.sln /p:Configuration=Release
```

## How It Works

The application:
1. Loads embedded Wonder Card and script data for the selected event
2. Reads the save file and validates its structure
3. Injects the Wonder Card and script data at the appropriate offsets
4. Recalculates and updates the save file CRC checksum
5. Writes the modified save file

## License

This project is licensed under the GNU General Public License v3.0 - see the [LICENSE](LICENSE) file for details.

## Disclaimer

This tool is for educational and preservation purposes. Use at your own risk. Always backup your save files before modification.

## Credits

Mystery Gift event data is based on official Pokemon event distributions from the Generation 3 era.
