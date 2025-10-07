# Simple Thunderbird Share Target

A simple Windows application that adds a "Share" target for Mozilla Thunderbird, allowing you to easily attach files to a new email.

## Features

*   Integrates with the modern Windows Share UI (Windows 10 and 11).
*   Allows sharing one or more files directly to a new Thunderbird email composition window.
*   Simple and lightweight.

## Prerequisites

*   Windows 10 or Windows 11.
*   **Mozilla Thunderbird must be installed.**

## Installation

You can install this application by downloading the latest release package from the project's GitHub page and running the installer.

## How to Use

Once installed, the application works seamlessly with the Windows Share feature:

1.  In File Explorer, select one or more files you want to attach to an email.
2.  Right-click on the selected files and choose **Share**.
3.  In the Share dialog that appears, select **Simple Thunderbird Share Target** from the list of applications.
4.  A new Thunderbird compose window will open with your selected files already attached.

That's it! There is no main application window for normal use; it runs only when you share something to it.

## Configuration

For this version, the application works out-of-the-box with **no configuration required**.

It assumes Thunderbird is installed in its default directory:
`C:\Program Files\Mozilla Thunderbird\thunderbird.exe`

Support for custom installation paths (including portable versions) is not yet implemented. If you need this feature, please open an issue on GitHub to request it.

## License

This project is licensed under the GNU Lesser General Public License v2.1. See the LICENSE.txt file for details.

## Contact & Support

If you encounter any issues or have suggestions for improvement, please open an issue on the project's GitHub page https://github.com/felipec84/SimpleThunderbirdShareTarget.
