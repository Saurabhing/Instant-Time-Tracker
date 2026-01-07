⏱️ Instant Time Tracker
A lightweight, high-performance desktop time tracker for Windows. Instant Time Tracker automatically logs active window usage, organizes time by custom categories (like "Study" or "Gaming"), and provides a persistent, always-on-top widget for real-time focus.

Built with C# (WPF) for maximum efficiency and zero lag.

(Replace this link with a screenshot of your actual app!)

✨ Features
🖥️ Smart Desktop Widget
Always-on-Top: A sleek, transparent floating widget keeps your session time visible without blocking your work.

Quick Mode Switching: Switch between "Everything" (Total Uptime) and "Study" (Focus Mode) instantly from the dropdown.

Pause/Resume: Taking a break? Pause tracking with one click to keep your stats accurate.

Live App Detection: Shows the icon and name of the currently active window in real-time.

📊 Detailed Dashboard
Daily Timeline: See exactly how you spent every second of your day.

Custom Modes: Create your own tracking categories (e.g., Coding, Gaming, Reading) directly from the dashboard.

Filter & Focus: View stats for "Everything" or drill down into specific categories.

Mode Management: Add new modes or delete old ones with ease.

Visual Stats: Progress bars show which apps are consuming the most time.

⚙️ Under the Hood
Privacy First: All data is stored locally (JSON). No cloud, no sign-ups, no data tracking.

Auto-Start: Option to launch automatically with Windows.

System Tray: Minimizes quietly to the system tray to save taskbar space.

Dual Tracking Logic:

Everything Mode: Tracks total system uptime since launch.

Custom Modes: Tracks cumulative time for that specific category across the entire day.

🚀 Installation
Go to the [https://github.com/Saurabhing/Instant-Time-Tracker/releases/tag/v2.0] tab (on the right).

Download the latest InstantTimeTracker.exe.

Run the .exe file.

(Optional) In the Dashboard, check "Launch on Startup" to have it run every day automatically.

🛠️ How to Build from Source
Requirements: Visual Studio 2022 with .NET Desktop Development workload.

Clone the repository:

Bash

git clone https://github.com/Saurabhing/Instant-Time-Tracker.git
Open InstantTimeTracker.sln in Visual Studio.

Press Ctrl + Shift + B to Build.

Press F5 to Run.

To publish a single standalone .exe:

PowerShell

dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./Publish
🎮 How to Use
Open the App: The widget appears in the bottom-right corner.

Select a Mode:

Select "Everything" to see how long your PC has been running.

Select "Study" (or create a custom mode) to track focused work.

View Dashboard: Right-click the widget and select "Open Dashboard".

Add/Delete Modes:

Type a name in the top bar and click + to add a mode.

Select a mode and click - (Trash Icon) to delete it.

🤝 Contributing
Contributions are welcome! If you have an idea for a feature (e.g., weekly graphs, pomodoro timer), feel free to fork the repo and submit a Pull Request.

📄 License
This project is open-source and available under the Apache License 2.0. See the LICENSE file for more details.

Made with ❤️ using C# & WPF.
