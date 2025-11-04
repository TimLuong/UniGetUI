# Repository Overview

## Project Purpose
UniGetUI (formerly WingetUI) is a unified graphical user interface application for Windows 10 and 11 that consolidates multiple command-line package managers into a single, intuitive interface. The project aims to simplify software management on Windows by providing a centralized platform where users can discover, install, update, and uninstall software packages from various package managers including WinGet, Scoop, Chocolatey, Pip, Npm, .NET Tool, and PowerShell Gallery.

The application serves as a bridge between users and CLI-based package management systems. It eliminates the need to memorize complex command-line syntax while leveraging the power and breadth of multiple package ecosystems simultaneously.

## Problem Statement
Windows users face several challenges when managing software:

1. **Fragmented Package Management**: Different package managers (WinGet, Scoop, Chocolatey, etc.) require users to learn and use separate command-line tools with different syntax and commands.

2. **Command-Line Complexity**: Many users, especially those not familiar with terminal environments, find CLI tools intimidating or difficult to use effectively.

3. **Manual Update Management**: Keeping track of updates across multiple package sources requires checking each package manager individually, which is time-consuming and error-prone.

4. **Limited Discoverability**: Discovering new software packages through command-line interfaces is challenging compared to visual browsing experiences.

5. **Installation Configuration Complexity**: Customizing installation options, architectures, and switches requires detailed knowledge of each package manager's specific syntax.

UniGetUI addresses these pain points by providing a unified, user-friendly graphical interface that abstracts away the complexity of multiple package management systems while maintaining their full functionality.

## Target Audience
UniGetUI is designed for:

- **Windows Power Users**: Users who want efficient software management without constantly switching between different package managers or memorizing CLI commands.

- **System Administrators**: IT professionals who need to manage software installations across multiple machines and want to streamline package deployment and maintenance.

- **Developers**: Software developers who work with various development tools and dependencies across different ecosystems (Python, Node.js, .NET, etc.) and need a centralized management solution.

- **Windows Enthusiasts**: Users who prefer package managers over traditional installer downloads but want a more accessible interface than command-line tools.

- **New Package Manager Users**: Individuals who are new to package management concepts and need an approachable entry point to these powerful tools.

- **Anyone Managing Multiple Software Installations**: Users who regularly install, update, and maintain multiple software applications and want to automate or simplify these processes.

## Key Features

- **Unified Package Management**: Combine packages from WinGet, Chocolatey, Scoop, Pip, Npm, .NET Tool, and PowerShell Gallery in a single interface, allowing installation, update, and removal with one click.

- **Package Discovery**: Browse and search for new packages across all supported package managers with filtering capabilities to easily find desired software.

- **Detailed Package Information**: View comprehensive metadata about packages including publisher information, download URLs, package sizes, and descriptions before installation.

- **Bulk Operations**: Select and perform operations on multiple packages simultaneously for efficient bulk installation, updates, or uninstallation.

- **Automatic Updates**: Configure automatic package updates or receive notifications when updates become available, with granular control to skip versions or ignore updates on a per-package basis.

- **Widget Integration**: Manage available updates directly from the Windows Widgets pane or Dev Home with the companion "Widgets for UniGetUI" application.

- **System Tray Integration**: Access package information and perform quick updates or removals directly from the system tray icon.

- **Custom Installation Options**: Customize installation parameters, select different versions, choose architecture (32-bit/64-bit), and specify custom switches, with settings automatically saved for future updates.

- **Package Sharing**: Generate shareable links to recommend software packages to others.

- **Import/Export Functionality**: Export package lists with custom installation parameters to set up new machines or share configurations, and backup installed packages locally for easy recovery during system migrations.

- **Multi-language Support**: Available in 50+ languages with active community translation efforts.

- **Built-in Auto-updater**: Keep UniGetUI itself up-to-date automatically without manual intervention.

## Quick Start

### Installation Options

**Microsoft Store (Recommended)**
- Install directly from the [Microsoft Store](https://apps.microsoft.com/detail/xpfftq032ptphf) for automatic updates and easy installation.

**Direct Download**
- Download the installer from the [latest release](https://github.com/marticliment/UniGetUI/releases/latest/download/UniGetUI.Installer.exe) and run it.

**Via WinGet**
```cmd
winget install --exact --id MartiCliment.UniGetUI --source winget
```

**Via Scoop**
```cmd
scoop bucket add extras
scoop install extras/unigetui
```

**Via Chocolatey**
```cmd
choco install wingetui
```

### Getting Started
1. Install UniGetUI using your preferred method from the options above.
2. Launch the application - it will automatically detect available package managers on your system.
3. Browse available packages in the "Discover Packages" section.
4. Install software by selecting packages and clicking the install button.
5. Check for updates in the "Updates" section and apply them as needed.
6. Manage installed packages through the "Installed Packages" section.

### System Requirements
- Windows 10 or Windows 11
- At least one supported package manager (WinGet, Scoop, Chocolatey, Pip, Npm, .NET Tool, or PowerShell Gallery)

### Important Notes
- UniGetUI is an unofficial, independent project not affiliated with any of the supported package managers.
- The official website is [https://www.marticliment.com/unigetui/](https://www.marticliment.com/unigetui/) - any other site should be considered unofficial.
- Users are responsible for the software they download through the application.
- For security issues, report them via the [disclosure program](https://whitehub.net/programs/unigetui/).
