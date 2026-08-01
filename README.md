# Nadaked Project Setup

A reusable Unity Editor package that automates common project initialization tasks.

Instead of recreating the same folder structure, installing packages manually, importing TextMesh Pro resources, and cleaning Unity template files for every new project, you can run these operations from the Unity Editor menu.

## Features

* Creates a default `_Project` folder structure
* Moves existing `Scenes` and `Settings` folders under `_Project`
* Moves `InputSystem_Actions.inputactions` into the `Settings` folder
* Removes Unity template files such as `Readme.asset` and `TutorialInfo`
* Installs commonly used Unity packages
* Imports TextMesh Pro Essential Resources
* Provides three independent setup actions

## Requirements

* Unity 6 or newer
* Git must be installed for Git-based package dependencies
* Internet connection is required when installing packages

## Installation

### Install from Git URL

Open Unity Package Manager:

```text
Window → Package Manager
```

Click the `+` button and select:

```text
Install package from Git URL
```

Enter:

```text
https://github.com/nadaked/unity-project-setup.git
```

To install a specific release:

```text
https://github.com/nadaked/unity-project-setup.git#v1.0.1
```

### Install as a local package

Clone or download the repository.

In Unity Package Manager, select:

```text
+ → Add package from disk
```

Then select the package's `package.json` file.

## Usage

After installation, open:

```text
Tools → Setup
```

The package adds three separate setup commands.

### Create Folders

```text
Tools → Setup → Create Folders
```

Creates the following structure:

```text
Assets
└── _Project
    ├── Animations
    ├── Editor
    ├── Materials
    ├── Models
    ├── Prefabs
    ├── Resources
    ├── Scenes
    ├── ScriptableObjects
    ├── Scripts
    ├── Settings
    └── Textures
```

The command also:

* Moves `Assets/Scenes` to `Assets/_Project/Scenes`
* Moves `Assets/Settings` to `Assets/_Project/Settings`
* Moves `InputSystem_Actions.inputactions` into the Settings folder
* Deletes `Assets/Readme.asset`
* Deletes the `Assets/TutorialInfo` folder

Existing files are checked before moving or deleting them.

### Install Essential Packages

```text
Tools → Setup → Install Essential Packages
```

Installs the following packages:

* [UniTask](https://github.com/Cysharp/UniTask)
* [PrimeTween](https://github.com/KyryloKuzyk/PrimeTween)
* [Alchemy](https://github.com/annulusgames/Alchemy)
* [Unity Utils](https://github.com/adammyhre/Unity-Utils)
* [Selection History and Favorites](https://github.com/acoppes/unity-history-window)
* Unity UI

PrimeTween requires an npm scoped registry. The setup tool automatically adds the required registry to `Packages/manifest.json` when it is missing.

### Import Essential Assets

```text
Tools → Setup → Import Essential Assets
```

Imports TextMesh Pro Essential Resources by executing Unity's built-in TextMesh Pro import command.

If the resources are already present, the operation is skipped.

## Package Structure

```text
com.nadaked.project-setup
├── package.json
├── README.md
├── CHANGELOG.md
├── LICENSE
└── Editor
    ├── ProjectSetup.cs
    └── Nadaked.ProjectSetup.Editor.asmdef
```

All scripts are located under the `Editor` folder and are excluded from runtime builds.

## Notes

### Embedded PrimeTween package

If PrimeTween already exists directly inside the project's `Packages` folder, Unity treats it as an embedded package.

An embedded package cannot be updated through Unity Package Manager until the embedded folder is removed manually.

Example:

```text
Packages/com.kyrylokuzyk.primetween
```

### Selection History menu

After installation, Selection History and Favorites can be opened from:

```text
Window → Gemserk
```

## License

This project is available under the MIT License.
