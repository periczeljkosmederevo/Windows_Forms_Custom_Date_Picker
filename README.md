# Windows_Forms_Custom_Date_Picker

**Windows_Forms_Custom_Date_Picker** is a specialized open-source custom control library for Windows Forms, designed to provide a highly flexible date picker component featuring an integrated, fully customizable monthly calendar.

---

### 🚀 Key Features

* **Custom Date Picker:** A robust, standalone Windows Forms control combining standard input handling with an interactive calendar drop-down.
* **Integrated Calendar Control:** Built-in monthly calendar component designed to work seamlessly out-of-the-box with the picker interface.
* **Culture & Regional Support:** Allows per-control culture configuration for tailored date and calendar display based on specific regional settings.
* **Dynamic Sizing:** The calendar control automatically adjusts its width and height based on the selected fonts, ensuring that elements like week numbers are always visible, even with larger text.
* **NuGet Ready:** Structured and packaged for quick integration and easy deployment as a reusable NuGet package.

---

### 🛠 Customization & Properties

The control offers a wide range of properties that can be configured directly through the Visual Studio Properties panel or programmatically. They are organized into logical categories:

#### Fonts

* **`Font`**: Sets the base font for the control (applied to the text box and calendar).
* **`CalendarDaysFont`**: Specifies the font used for calendar day numbers and day name headers.
* **`CalendarHeaderFont`**: Defines the font for the month and year header (including navigation buttons).
* **`CalendarTodayButtonFont`**: Sets the font for the "Today" button.

#### Appearance

* **`CalendarTodayButtonVisible`**: Toggle to display or hide the "Today" button in the calendar view.
* **`CalendarShowWeekNumbers`**: When set to `true`, the calendar displays ISO-compliant week numbers on the left side.
* **`FirstDayOfWeek`**: Defines the starting day of the week (e.g., Sunday or Monday). Default is `Monday`.
* **`CustomFormat`**: Defines the string format for the date display (e.g., `"ddd, dd. MMMM yyyy"`).

#### Behavior & Localization

* **`Value`**: Gets or sets the currently selected date.
* **`Culture`**: Sets the `CultureInfo` for the control. Automatically adjusts language-specific month/day names and logic for week numbering (ISO 8601).
* **`MinDate` / `MaxDate**`: Restricts the range of selectable dates within the calendar.
* **`CalendarTodayButtonFallbackString`**: Provides custom text for the "Today" button if the automatic culture-based translation is not desired or found.

---

### Getting Started

#### Prerequisites

* .NET 10.0 (or compatible modern .NET target supporting Windows Forms) with Visual Studio.

#### Installation

* Clone the repository: [`git clone`](https://github.com/periczeljkosmederevo/Windows_Forms_Custom_Date_Picker.git)
* Open the project solution in Visual Studio.
* Build the project or generate the NuGet package locally via the **Pack** option.

---

### Usage

1. Add the control library reference or install the package into your Windows Forms application.
2. Drag and drop the custom picker control from the toolbox onto your form layout.
3. Configure properties such as culture settings, fonts, date formats, or visual styles directly via the designer or programmatically in code.

#### Programmatic Example

* Here is an example of how you can programmatically set the culture (which affects language translations and formatting), 
* set an initial value, and retrieve the date chosen by the user:

```csharp
using System.Globalization;

// 1. Set the culture for regional formatting and translations (e.g., Serbian Latin)
customDatePicker1.Culture = new CultureInfo("sr-Latn-RS");

// 2. Set a specific date value programmatically
customDatePicker1.Value = new DateTime(2026, 8, 19);

// 3. Get the selected DateTime value chosen by the user
DateTime selectedDate = customDatePicker1.Value;

// Alternatively, get the formatted text representation based on CustomFormat and Culture
string formattedText = customDatePicker1.GetFormattedValue();

```

---

### License

* This project is licensed under the [CC0-1.0 license (Public Domain)](https://creativecommons.org/publicdomain/zero/1.0/).
* To the extent possible under law, the author(s) have dedicated all copyright and related rights to this software to the public domain worldwide.
* Feel free to use, modify, and distribute the code without any restrictions.

---

### Support

For technical inquiries or administrative follow-ups, please contact:

* **Name:** Željko Perić
* **Email:** periczeljkosmederevo@yahoo.com

---

