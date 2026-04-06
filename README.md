# 🧮 Calculator WinForms

A modern, glassmorphism Calculator built entirely in C# and Windows Forms!

![Platform](https://img.shields.io/badge/Platform-Windows-blue?style=flat) ![.NET](https://img.shields.io/badge/.NET-6%2B-purple?style=flat) ![License](https://img.shields.io/badge/License-MIT-green?style=flat) ![Language](https://img.shields.io/badge/Language-C%23-blue?style=flat)

## Preview

![Calculator Preview](assets/preview.png)
> *Replace with an actual screenshot after first run*

## Features

- **Glassmorphism UI**: Beautiful semi-transparent glass panels and gradient backgrounds.
- **Chained Arithmetic**: Evaluate multi-step calculations continuously with correct mathematical behavior.
- **Keyboard Support**: Full numpad and keyboard support seamlessly mapped to operations.
- **Borderless Drag Window**: Click anywhere on the form to drag and reposition.
- **Divide-by-zero Guard**: Safe handling of zero division safely displaying "Cannot divide by 0".
- **Precision Display**: High precision calculation using double types and the `G15` format specifier.

## Getting Started

### Prerequisites

- Windows 10+
- .NET 6 SDK

### To Run

```bash
git clone https://github.com/Sjjcnr/calculator-win-forms
cd calculator-win-forms
dotnet run
```

## Project Structure

```text
calculator-win-forms/
├── Form1.cs
├── GlassButton.cs
├── Program.cs
├── calculator-win-forms.csproj
├── README.md
├── .gitignore
└── assets/
    └── preview.png
```

## Keyboard Shortcuts

| Key | Action |
| --- | --- |
| `0`–`9` | Input Digits |
| `+`, `−`, `×`, `÷` | Mathematical Operations |
| `Enter` / `=` | Calculate / Equals |
| `Escape` | Clear All (C) |
| `Backspace` | Remove last character (⌫) |
| `.` | Decimal point |

## Tech Stack

- C# 10
- .NET 6
- Windows Forms
- GDI+ (System.Drawing)

## License

This project is licensed under the MIT License.
