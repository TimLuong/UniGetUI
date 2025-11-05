# Video Tutorial Scripts

This document contains scripts for creating video tutorials for new developers contributing to UniGetUI.

## 📹 Series Overview

**Target Audience:** New developers with basic C# knowledge  
**Format:** Screen recording with voiceover  
**Duration:** 10-15 minutes per video  
**Platform:** YouTube, GitHub video embeds

---

## Video 1: "Getting Started - Your First Build"

**Duration:** 12 minutes  
**Difficulty:** Beginner

### Script

#### [0:00 - 0:30] Introduction

> "Hi! Welcome to this video tutorial on getting started with UniGetUI development. I'm going to show you how to set up your development environment and build the project for the first time. By the end of this video, you'll have UniGetUI running on your machine and be ready to make your first contribution."

**Screen:** Show UniGetUI application running

#### [0:30 - 1:30] Prerequisites Overview

> "Before we begin, make sure you have:
> - Windows 10 version 2004 or later, or Windows 11
> - At least 10 GB of free disk space
> - A GitHub account
> 
> We'll be installing:
> - .NET 8.0 SDK
> - Visual Studio 2022
> - Git for Windows
> 
> Don't worry if you don't have these installed yet - I'll show you exactly where to get them."

**Screen:** Show checklist on screen with links

#### [1:30 - 3:00] Installing .NET 8.0 SDK

> "Let's start by installing the .NET 8.0 SDK. Open your browser and go to dotnet.microsoft.com/download/dotnet/8.0
> 
> [Navigate to website]
> 
> Click 'Download .NET 8.0 SDK' for Windows. The installer will download. Run it and follow the prompts. This should take about 2-3 minutes.
> 
> [Fast forward through installation]
> 
> Once installed, let's verify it. Open a command prompt and type: dotnet --version
> 
> [Show command prompt]
> 
> You should see version 8.0.something. Perfect!"

**Screen:** Show installation process, then terminal with version check

#### [3:00 - 5:00] Installing Visual Studio 2022

> "Next, we need Visual Studio 2022. Go to visualstudio.microsoft.com and download the Community edition - it's free!
> 
> [Navigate to website]
> 
> Run the installer. When it asks which workloads to install, select '.NET Desktop Development'. This includes everything we need.
> 
> [Show workload selection]
> 
> Under Individual Components, make sure 'Windows 10 SDK' version 10.0.26100.0 or later is selected.
> 
> [Show component selection]
> 
> This installation will take 15-30 minutes depending on your internet speed, so I'm going to fast forward through it.
> 
> [Fast forward]
> 
> Great! Visual Studio is installed."

**Screen:** Show VS installation process

#### [5:00 - 6:30] Installing Git and Cloning Repository

> "Now let's install Git. Go to git-scm.com/downloads and download Git for Windows.
> 
> [Show download]
> 
> Run the installer. The default options are fine for most people.
> 
> [Show installer]
> 
> Once installed, open Visual Studio 2022. Click 'Clone a repository'.
> 
> [Show VS start screen]
> 
> Enter this URL: https://github.com/marticliment/UniGetUI.git
> 
> Choose where you want to save it. I'll put it in my Documents folder.
> 
> Click 'Clone'. This will download the entire project."

**Screen:** Show cloning process in Visual Studio

#### [6:30 - 8:00] Opening and Building the Solution

> "Once cloning is complete, Visual Studio will automatically open the solution. If it doesn't, navigate to the 'src' folder and double-click 'UniGetUI.sln'.
> 
> [Show solution explorer]
> 
> Here's the solution with all the projects. Don't worry if this looks overwhelming - you'll get familiar with it over time.
> 
> At the top, make sure the configuration is set to 'Debug' and the platform is 'x64'.
> 
> [Point to dropdowns]
> 
> Now let's build it. Click Build > Build Solution, or press Ctrl+Shift+B.
> 
> [Show build process]
> 
> The first build takes a few minutes because it needs to download NuGet packages. Let's watch the output window...
> 
> [Show output window]
> 
> And... Build succeeded! Zero errors."

**Screen:** Show build process and success message

#### [8:00 - 9:30] Running the Application

> "Now let's run it! Make sure 'UniGetUI' is set as the startup project. You can see it's bold in the Solution Explorer. If it's not, right-click it and select 'Set as Startup Project'.
> 
> [Show solution explorer]
> 
> Now press F5 or click the green play button at the top.
> 
> [Show running]
> 
> And here's UniGetUI running! It will automatically detect which package managers you have installed on your system. You can see mine found WinGet.
> 
> [Show the running application]
> 
> You can browse available packages, search for software, and even install things. But more importantly, you now have a working development environment!"

**Screen:** Show the running application, navigate through it

#### [9:30 - 10:30] Running Tests

> "Before we finish, let's make sure all the tests pass. Close the application and go to Test > Run All Tests.
> 
> [Show Test Explorer]
> 
> Visual Studio will run all the unit tests. This takes about 30 seconds.
> 
> [Show tests running]
> 
> Great! All tests passed. This confirms everything is set up correctly."

**Screen:** Show Test Explorer with passing tests

#### [10:30 - 11:30] Next Steps

> "Congratulations! You've successfully:
> - Installed all required tools
> - Cloned the repository
> - Built the project
> - Run the application
> - Verified tests pass
> 
> You're now ready to start contributing!
> 
> Next steps:
> 1. Read the Getting Started guide in the docs folder
> 2. Explore the codebase
> 3. Find a 'good first issue' on GitHub
> 4. Watch the next video: 'Making Your First Contribution'
> 
> Links to everything are in the description below."

**Screen:** Show checklist of completed items

#### [11:30 - 12:00] Outro

> "Thanks for watching! If you found this helpful, please like and subscribe. If you have questions, leave a comment or join us on GitHub Discussions. Happy coding, and I'll see you in the next video!"

**Screen:** Show end screen with links

### Video Checklist
- [ ] Record introduction
- [ ] Screen record installation processes
- [ ] Record build process
- [ ] Record running application
- [ ] Record test execution
- [ ] Add captions/subtitles
- [ ] Add chapter markers
- [ ] Create thumbnail
- [ ] Upload with description and links

---

## Video 2: "Making Your First Contribution"

**Duration:** 15 minutes  
**Difficulty:** Beginner

### Script

#### [0:00 - 0:30] Introduction

> "Welcome back! In the previous video, we got UniGetUI building on your machine. Today, I'm going to show you how to make your first contribution. We'll add a simple feature, test it, and submit a pull request. This will teach you the entire contribution workflow."

**Screen:** Show GitHub repository

#### [0:30 - 2:00] Finding a Good First Issue

> "The first step is finding something to work on. Go to the UniGetUI repository on GitHub and click on 'Issues'.
> 
> [Navigate to Issues]
> 
> In the labels filter, select 'good first issue'. These are tasks specifically chosen for new contributors - they're relatively simple and well-defined.
> 
> [Show filtered issues]
> 
> For this tutorial, we're going to add a simple setting. Let's create a new setting that shows package count in the system tray. This is perfect for learning because it touches UI, settings management, and follows the project's patterns."

**Screen:** Show GitHub issues page

#### [2:00 - 3:30] Creating a Feature Branch

> "Before making changes, we need to create a feature branch. Never work directly on the main branch.
> 
> In Visual Studio, go to the Git Changes window. If you don't see it, go to View > Git Changes.
> 
> [Show Git Changes window]
> 
> At the top, click the branch dropdown and select 'New Branch'.
> 
> [Show dialog]
> 
> Name it something descriptive like 'feature/add-package-count-setting'. Make sure 'Checkout branch' is selected, then click 'Create'.
> 
> [Show branch creation]
> 
> Now we're working on our feature branch. Any changes we make won't affect the main branch."

**Screen:** Show branch creation in Visual Studio

#### [3:30 - 6:00] Adding the Setting UI

> "Now let's add the UI for our setting. In Solution Explorer, navigate to:
> UniGetUI > Pages > SettingsPages
> 
> [Navigate in Solution Explorer]
> 
> We'll add our setting to the GeneralSettings page. Open GeneralSettingsPage.xaml.
> 
> [Open file]
> 
> Find a good spot to add our new setting - let's put it with the other display settings around line 50.
> 
> [Scroll to location]
> 
> Now I'll add a SettingsCard with a ToggleSwitch. Watch as I type this...
> 
> [Type the XAML code]
> 
> This creates a card with:
> - A header: 'Show Package Count in System Tray'
> - A description explaining what it does
> - A toggle switch to turn it on and off
> 
> IntelliSense helps us as we type. Notice how it suggests properties and closes tags."

**Screen:** Show typing XAML in Visual Studio

#### [6:00 - 8:00] Implementing the Logic

> "Now we need to make this toggle actually do something. Open the code-behind file: GeneralSettingsPage.xaml.cs
> 
> [Open code-behind]
> 
> First, let's add a method to load the setting when the page opens. I'll add this to the constructor...
> 
> [Type code]
> 
> This reads the saved setting value and sets the toggle accordingly.
> 
> Now we need to save the setting when the user toggles it. Let's add an event handler...
> 
> [Type event handler]
> 
> This is called whenever the toggle changes. It saves the new value using the Settings.Set method.
> 
> Notice how we're following the naming conventions - camelCase for the method parameter, PascalCase for the method name."

**Screen:** Show typing C# code

#### [8:00 - 9:30] Building and Testing

> "Let's make sure our code compiles. Press Ctrl+Shift+B to build.
> 
> [Show build output]
> 
> Build succeeded! Now let's test it. Press F5 to run the application.
> 
> [Show application starting]
> 
> Navigate to Settings, and scroll down to find our new setting.
> 
> [Show the new setting]
> 
> There it is! Let's toggle it on and off. Notice how it works smoothly.
> 
> [Toggle the switch]
> 
> Now let's verify persistence. Close the application and reopen it.
> 
> [Close and reopen]
> 
> Navigate back to Settings... and our toggle is still on! The setting persisted. Perfect!"

**Screen:** Show testing the new feature

#### [9:30 - 11:00] Committing Changes

> "Now we need to commit our changes. Go back to the Git Changes window.
> 
> [Show Git Changes]
> 
> You can see our modified files listed here. Review the changes by clicking on each file.
> 
> [Show diff view]
> 
> The green lines are our additions. Everything looks good.
> 
> Now let's write a commit message. A good commit message has:
> - A brief summary on the first line
> - An empty line
> - A detailed description
> 
> [Type commit message]
> 
> Click 'Commit All'. Our changes are now saved to our local branch."

**Screen:** Show committing in Visual Studio

#### [11:00 - 12:30] Pushing and Creating a Pull Request

> "Now we need to push our branch to GitHub. In the Git Changes window, click 'Push'.
> 
> [Show push]
> 
> Now open your browser and go to the UniGetUI repository on GitHub.
> 
> [Navigate to GitHub]
> 
> GitHub detected our new branch! Click 'Compare & pull request'.
> 
> [Show PR creation page]
> 
> The title is pre-filled from our commit message. Let's add a description:
> - What we added
> - Why it's useful
> - How to test it
> 
> [Fill in PR template]
> 
> Scroll down to review the changes one more time...
> 
> [Show diff]
> 
> Everything looks good. Click 'Create pull request'."

**Screen:** Show PR creation on GitHub

#### [12:30 - 13:30] What Happens Next

> "Your pull request is now open! Here's what happens next:
> 
> 1. Automated checks will run - builds and tests
> 2. A maintainer will review your code
> 3. They might request changes or ask questions
> 4. Once approved, your code will be merged!
> 
> [Show PR page]
> 
> You can see the checks are running here. When you get feedback, be responsive and courteous. Code review is a conversation, not a criticism.
> 
> Remember:
> - Respond to feedback promptly
> - Ask questions if something is unclear
> - Make requested changes
> - Learn from the review process"

**Screen:** Show PR with checks running

#### [13:30 - 14:30] Recap and Next Steps

> "Let's recap what we did:
> 1. Found a good first issue
> 2. Created a feature branch
> 3. Added a new setting with UI and logic
> 4. Tested our changes locally
> 5. Committed with a good message
> 6. Created a pull request
> 
> You've just completed the full contribution workflow! This is exactly how all contributions work, whether it's fixing a bug or adding a major feature.
> 
> Next steps:
> - Watch for feedback on your PR
> - Find another issue to work on
> - Watch the next video: 'Understanding the Package Manager Architecture'"

**Screen:** Show checklist

#### [14:30 - 15:00] Outro

> "Congratulations on your first contribution! You're now officially a UniGetUI contributor. Thanks for watching, and I'll see you in the next video where we dive deeper into how package managers work in UniGetUI."

**Screen:** Show end screen

---

## Video 3: "Understanding Package Manager Architecture"

**Duration:** 15 minutes  
**Difficulty:** Intermediate

### Script

#### [0:00 - 0:45] Introduction

> "Welcome! In this video, we're going to understand how package managers work in UniGetUI. This is more advanced than the previous videos, but understanding this architecture is key to contributing effectively to the project. We'll look at the IPackageManager interface, explore how WinGet is implemented, and understand the helper pattern."

**Screen:** Show architecture diagram

#### [0:45 - 2:30] The Big Picture

> "UniGetUI supports multiple package managers - WinGet, Scoop, Chocolatey, Pip, and more. But they all look the same to the rest of the application. How?
> 
> [Show diagram]
> 
> The answer is abstraction through interfaces. Let's open the IPackageManager interface.
> 
> [Navigate to file]
> 
> Here's the interface that all package managers must implement. Look at these key methods:
> - FindPackages: Search for packages
> - GetInstalledPackages: List what's installed
> - GetAvailableUpdates: Check for updates
> 
> And these helpers:
> - DetailsHelper: Gets package information
> - OperationHelper: Handles install/update/uninstall
> - SourcesHelper: Manages repositories
> 
> This design means we can add new package managers without changing anything else in the app."

**Screen:** Show IPackageManager interface file

#### [2:30 - 5:00] Exploring a Real Implementation

> "Let's look at how WinGet implements this interface. Navigate to:
> UniGetUI.PackageEngine.Managers.WinGet > WinGet.cs
> 
> [Open file]
> 
> See how it inherits from PackageManager and implements the interface? Let's look at the constructor...
> 
> [Scroll to constructor]
> 
> It defines:
> 1. Capabilities - what can this manager do?
> 2. Properties - name, description, icons
> 3. Helpers - creates the helper classes
> 
> Let's look at capabilities first...
> 
> [Highlight capabilities]
> 
> These tell the UI what features to enable. WinGet can run as admin, supports custom versions, parallel operations, and more.
> 
> Now let's look at how packages are found..."

**Screen:** Show WinGet.cs implementation

#### [5:00 - 8:00] The Helper Pattern

> "Instead of one giant class doing everything, UniGetUI uses the Helper pattern. Let's open WinGetOperationHelper.cs
> 
> [Open file]
> 
> This class focuses on one thing: building command-line arguments for operations. Look at this method...
> 
> [Show _getOperationParameters]
> 
> It takes:
> - The package to operate on
> - User options (run as admin, skip hash check, etc.)
> - The operation type (install, update, uninstall)
> 
> And returns an array of command-line arguments. Let's trace through an installation...
> 
> [Step through code]
> 
> If we're installing Visual Studio Code as admin with a custom location, it builds:
> 'install --id Microsoft.VisualStudioCode --admin --location C:\\Apps'
> 
> This separation makes the code:
> - Easier to test
> - Easier to maintain  
> - More organized"

**Screen:** Show OperationHelper implementation

#### [8:00 - 10:30] Following a Package Installation

> "Let's trace a complete installation from UI to completion. I'll set a breakpoint and walk through it...
> 
> [Set breakpoint]
> 
> Run the application and click Install on a package.
> 
> [Show hitting breakpoint]
> 
> We're now in the UI code. It creates an InstallationOptions object with the user's choices. Step into the Install method...
> 
> [Step through debugger]
> 
> Now we're in the PackageManager base class. It delegates to the OperationHelper. Step in again...
> 
> [Step through]
> 
> Now we're in WinGetOperationHelper building the command. It constructs the argument array. Step in...
> 
> [Step through]
> 
> And now it executes the actual winget command. It monitors the output, reports progress, and notifies when complete.
> 
> [Show output]
> 
> This layered approach keeps each piece focused and testable."

**Screen:** Show debugging session

#### [10:30 - 12:00] Adding a New Package Manager

> "Now you understand the architecture, let's see what's needed to add a new package manager. Here's a checklist:
> 
> [Show checklist on screen]
> 
> 1. Create new project: UniGetUI.PackageEngine.Managers.YourManager
> 2. Create main class implementing IPackageManager
> 3. Define capabilities and properties
> 4. Create OperationHelper to build CLI commands
> 5. Create DetailsHelper to fetch package info
> 6. Create SourcesHelper to manage repositories
> 7. Register your manager in the initialization code
> 8. Add icons and localization
> 9. Write tests
> 10. Document your manager
> 
> Following this pattern, you can add support for any package manager that has a CLI!"

**Screen:** Show checklist

#### [12:00 - 13:30] Best Practices

> "When working with package managers, follow these best practices:
> 
> 1. Fail-safe methods: Return empty lists instead of throwing exceptions
> [Show example]
> 
> 2. Async/await: Package operations are I/O-bound
> [Show example]
> 
> 3. Progress reporting: Keep users informed
> [Show example]
> 
> 4. Error handling: CLI commands can fail for many reasons
> [Show example]
> 5. Logging: Help debug issues
> [Show example]
> 
> These patterns are used consistently throughout the codebase."

**Screen:** Show code examples

#### [13:30 - 14:30] Recap and Resources

> "Let's recap what we learned:
> - The IPackageManager interface abstracts all package managers
> - The Helper pattern separates concerns
> - Package operations flow from UI through helpers to CLI
> - Fail-safe design prevents crashes
> 
> Resources for deeper learning:
> - Tutorial 2: Understanding Package Managers
> - Adding Features Guide
> - Explore the codebase - read implementations
> - Ask questions in GitHub Discussions
> 
> With this knowledge, you can now contribute to package manager integrations or even add support for a new one!"

**Screen:** Show resources list

#### [14:30 - 15:00] Outro

> "Thanks for watching this deeper dive into UniGetUI's architecture! Understanding these patterns will make you a more effective contributor. Next video, we'll look at working with WinUI 3 and creating custom controls. See you then!"

**Screen:** Show end screen

---

## Video Production Notes

### Equipment Needed
- Screen recording software (OBS Studio, Camtasia, or similar)
- Good quality microphone
- Quiet recording environment
- Video editing software

### Recording Tips
1. **Prepare:** Have the script ready and practice
2. **Record in chunks:** Easier to edit mistakes
3. **Use a clean desktop:** Close unnecessary applications
4. **Zoom in:** Make text readable
5. **Speak clearly:** Pace yourself, don't rush
6. **Include pauses:** Makes editing easier

### Editing Checklist
- [ ] Remove long pauses and mistakes
- [ ] Add intro/outro graphics
- [ ] Add chapter markers for key sections
- [ ] Include captions/subtitles for accessibility
- [ ] Add text overlays for important points
- [ ] Add zoom effects for detailed code
- [ ] Balance audio levels
- [ ] Export in 1080p minimum

### Publishing Checklist
- [ ] Create eye-catching thumbnail
- [ ] Write descriptive title
- [ ] Write detailed description with timestamps
- [ ] Add links to docs and resources
- [ ] Add tags for searchability
- [ ] Add to appropriate playlists
- [ ] Enable comments for questions
- [ ] Share in GitHub Discussions

---

## Additional Video Ideas

### Video 4: "Working with WinUI 3 and XAML"
- Creating UI pages
- Data binding
- Event handling
- Custom controls

### Video 5: "Writing Effective Tests"
- xUnit basics
- Arrange-Act-Assert pattern
- Mocking dependencies
- Running tests in Visual Studio

### Video 6: "Code Review Best Practices"
- What to look for
- Giving constructive feedback
- Responding to feedback
- Common pitfalls

### Video 7: "Advanced: Adding a New Package Manager"
- End-to-end implementation
- Testing strategies
- Integration with existing code
- Submitting for review

### Video 8: "Debugging Techniques"
- Using breakpoints effectively
- Inspecting variables
- Call stack navigation
- Common debugging scenarios

---

**Note:** Update these scripts as the codebase evolves. Keep videos current with the latest practices and patterns.
