Evo ažuriranog README fajla. Uključio sam ispravku za `Culture` i `FirstDayOfWeek` kako smo se dogovorili, i sredio formatiranje sekcije za podešavanja kako bi bila preglednija.

---

# Windows_Forms_Custom_Date_Picker

**Windows_Forms_Custom_Date_Picker** is a specialized open-source custom control library for Windows Forms, designed to provide a highly flexible date picker component featuring an integrated, fully customizable monthly calendar.

---

### 🚀 Key Features

* **Custom Date Picker:** A robust, standalone Windows Forms control combining standard input handling with an interactive calendar drop-down.
* **Integrated Calendar Control:** Built-in monthly calendar component designed to work seamlessly out-of-the-box with the picker interface.
* **Culture & Regional Support:** Allows per-control culture configuration for tailored date and calendar display based on specific regional settings.
* **NuGet Ready:** Structured and packaged for quick integration and easy deployment as a reusable NuGet package.

---

### 🛠 Customization & Properties

The control offers a wide range of properties that can be configured directly through the Visual Studio Properties panel or programmatically:

#### Appearance & Fonts

* **`Font`**: Sets the base font for the entire control, including the text box and calendar.
* **`CalendarDaysFont`**: Specifies the font used specifically for displaying calendar day numbers.
* **`CalendarHeaderFont`**: Defines the font for the month and year header.
* **`CalendarTodayButtonFont`**: Sets the font for the "Today" button.
* **`CalendarTodayButtonVisible`**: Toggle to display or hide the "Today" button in the calendar view.
* **`ShowWeekNumbers`**: When set to `true`, the calendar will display ISO week numbers on the left side.

#### Behavior & Localization

* **`Culture`**: Sets the `CultureInfo` for the control. This automatically adjusts language-specific month and day names for the calendar display.
* **`FirstDayOfWeek`**: Defines the starting day of the week (e.g., Sunday or Monday). By default, this is set to `Monday`, but it can be configured independently of the selected `Culture`.
* **`CustomFormat`**: Defines the string format for the date display (e.g., `"ddd, dd. MMMM yyyy"`).
* **`Value`**: Gets or sets the currently selected date.
* **`MinDate` / `MaxDate**`: Restricts the range of selectable dates within the calendar.
* **`CalendarTodayButtonFallbackString`**: Provides custom text for the "Today" button if the automatic culture-based translation is not desired or found.

---

### Getting Started

#### Prerequisites

* .NET 10.0 (or compatible modern .NET target supporting Windows Forms) with Visual Studio.

#### Installation

* Clone the repository: `git clone` [https://github.com/periczeljkosmederevo/Windows_Forms_Custom_Date_Picker.git](https://github.com/periczeljkosmederevo/Windows_Forms_Custom_Date_Picker.git)
* Open the project solution in Visual Studio.
* Build the project or generate the NuGet package locally via the **Pack** option.

---

### Usage

1. Add the control library reference or install the package into your Windows Forms application.
2. Drag and drop the custom picker control from the toolbox onto your form layout.
3. Configure properties such as culture settings, date formats, or visual styles directly via the designer or programmatically in code.

---

### License

* This project is licensed under the [CC0-1.0 license (Public Domain)](https://creativecommons.org/publicdomain/zero/1.0/).
* To the extent possible under law, the author(s) have dedicated all copyright and related rights to this software to the public domain worldwide.
* Feel free to use, modify, and distribute the code without any restrictions.
* For more details, see the [LICENSE](https://www.google.com/search?q=LICENSE.txt) file or view the full legal text at [Creative Commons CC0 1.0 Universal](https://creativecommons.org/publicdomain/zero/1.0/).

---

### Support

For technical inquiries or administrative follow-ups, please contact:

* **Name:** Željko Perić
* **Email:** periczeljkosmederevo@yahoo.com