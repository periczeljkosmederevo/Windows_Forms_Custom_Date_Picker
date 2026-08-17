using System.ComponentModel;
using System.Globalization;

namespace Windows_Forms_Custom_Date_Picker;

/// <summary>
/// Represents a customizable Windows Forms calendar control
/// that supports culture-specific month and day names,
/// date selection, minimum and maximum dates, week numbers,
/// navigation and a Today button.
/// </summary>
public partial class Windows_Forms_Custom_Month_Calendar_Control : UserControl
{
    private CultureInfo _culture = CultureInfo.InvariantCulture;
    private DateTime _value = DateTime.Today;
    private DateTime _minDate = DateTime.MinValue.Date;
    private DateTime _maxDate = DateTime.MaxValue.Date;
    private string _customFormat = "dd.MM.yyyy";
    private DayOfWeek _firstDayOfWeek = DayOfWeek.Monday;
    private bool _showTodayButton = true;
    private bool _showWeekNumbers = false;
    private DateTime _displayedMonth;

    private bool _isInitializing = true;
    private bool _isUpdatingCalendar = false;

    private readonly Button _previousMonthButton;
    private readonly Button _nextMonthButton;
    private readonly Label _monthYearLabel;
    private readonly TableLayoutPanel _calendarTable;
    private readonly Button _todayButton;
    private readonly List<Button> _dayButtons = new();

    /// <summary>
    /// Occurs when the selected date changes.
    /// </summary>
    public event EventHandler? ValueChanged;

    /// <summary>
    /// Initializes a new instance of the calendar control.
    /// </summary>
    public Windows_Forms_Custom_Month_Calendar_Control()
    {
        DoubleBuffered = true;
        AutoScaleMode = AutoScaleMode.Font;

        MinimumSize = new Size(250, 220);
        Size = new Size(320, 280);

        BackColor = SystemColors.Window;
        ForeColor = SystemColors.WindowText;

        _displayedMonth = new DateTime(
            _value.Year,
            _value.Month,
            1);

        var headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 42,
            BackColor = SystemColors.Control
        };

        _previousMonthButton = new Button
        {
            Text = "‹",
            Dock = DockStyle.Left,
            Width = 40,
            FlatStyle = FlatStyle.Flat
        };

        _previousMonthButton.FlatAppearance.BorderSize = 0;

        _nextMonthButton = new Button
        {
            Text = "›",
            Dock = DockStyle.Right,
            Width = 40,
            FlatStyle = FlatStyle.Flat
        };

        _nextMonthButton.FlatAppearance.BorderSize = 0;

        _monthYearLabel = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font(
                Font.FontFamily,
                Font.Size,
                FontStyle.Bold)
        };

        headerPanel.Controls.Add(_monthYearLabel);
        headerPanel.Controls.Add(_previousMonthButton);
        headerPanel.Controls.Add(_nextMonthButton);

        _calendarTable = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 7,
            RowCount = 7,
            Padding = new Padding(4),
            CellBorderStyle =
                TableLayoutPanelCellBorderStyle.None
        };

        _todayButton = new Button
        {
            Text = "Today",
            Dock = DockStyle.Bottom,
            Height = 32,
            FlatStyle = FlatStyle.Flat
        };

        _todayButton.FlatAppearance.BorderSize = 0;

        Controls.Add(_calendarTable);
        Controls.Add(_todayButton);
        Controls.Add(headerPanel);

        _previousMonthButton.Click += PreviousMonthButton_Click;
        _nextMonthButton.Click += NextMonthButton_Click;
        _todayButton.Click += TodayButton_Click;

        Resize += CustomMonthCalendar_Resize;
        FontChanged += CustomMonthCalendar_FontChanged;

        _isInitializing = false;

        UpdateCalendar();
    }

    // ============================================================
    // CULTURE
    // ============================================================

    /// <summary>
    /// Gets or sets the culture used to display month and day names.
    /// </summary>
    [Category("Localization")]
    [Description("Culture used to display month and day names.")]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Visible)]
    public CultureInfo Culture
    {
        get => _culture;

        set
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (_culture.Equals(value))
                return;

            _culture = value;

            if (_isInitializing)
                return;

            UpdateCalendar();
        }
    }

    // ============================================================
    // CUSTOM FORMAT
    // ============================================================

    /// <summary>
    /// Gets or sets the format used when the selected date
    /// is represented as text.
    /// </summary>
    [Category("Appearance")]
    [Description(
        "Format used when the selected date is represented as text.")]
    [DefaultValue("dd.MM.yyyy")]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Visible)]
    public string CustomFormat
    {
        get => _customFormat;

        set
        {
            _customFormat =
                string.IsNullOrWhiteSpace(value)
                    ? "dd.MM.yyyy"
                    : value;

            Invalidate();
        }
    }

    // ============================================================
    // VALUE
    // ============================================================

    /// <summary>
    /// Gets or sets the currently selected date.
    /// </summary>
    [Category("Behavior")]
    [Description("Currently selected date.")]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Visible)]
    public DateTime Value
    {
        get => _value;

        set
        {
            DateTime newValue = value.Date;

            if (newValue < MinDate)
                newValue = MinDate;

            if (newValue > MaxDate)
                newValue = MaxDate;

            if (_value == newValue)
                return;

            _value = newValue;

            _displayedMonth = new DateTime(
                _value.Year,
                _value.Month,
                1);

            UpdateCalendar();

            ValueChanged?.Invoke(
                this,
                EventArgs.Empty);
        }
    }

    // ============================================================
    // MIN DATE
    // ============================================================

    /// <summary>
    /// Gets or sets the minimum selectable date.
    /// </summary>
    [Category("Behavior")]
    [Description("Minimum selectable date.")]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Visible)]
    public DateTime MinDate
    {
        get => _minDate;

        set
        {
            DateTime newValue = value.Date;

            if (newValue > _maxDate)
                throw new ArgumentException(
                    "MinDate cannot be greater than MaxDate.");

            _minDate = newValue;

            if (_value < _minDate)
                Value = _minDate;
            else
                UpdateCalendar();
        }
    }

    // ============================================================
    // MAX DATE
    // ============================================================

    /// <summary>
    /// Gets or sets the maximum selectable date.
    /// </summary>
    [Category("Behavior")]
    [Description("Maximum selectable date.")]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Visible)]
    public DateTime MaxDate
    {
        get => _maxDate;

        set
        {
            DateTime newValue = value.Date;

            if (newValue < _minDate)
                throw new ArgumentException(
                    "MaxDate cannot be less than MinDate.");

            _maxDate = newValue;

            if (_value > _maxDate)
                Value = _maxDate;
            else
                UpdateCalendar();
        }
    }

    // ============================================================
    // FIRST DAY OF WEEK
    // ============================================================

    /// <summary>
    /// Gets or sets the first day of the week displayed
    /// in the calendar.
    /// </summary>
    [Category("Appearance")]
    [Description(
        "First day of the week displayed in the calendar.")]
    [DefaultValue(DayOfWeek.Monday)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Visible)]
    public DayOfWeek FirstDayOfWeek
    {
        get => _firstDayOfWeek;

        set
        {
            if (_firstDayOfWeek == value)
                return;

            _firstDayOfWeek = value;

            UpdateCalendar();
        }
    }

    // ============================================================
    // SHOW TODAY BUTTON
    // ============================================================

    /// <summary>
    /// Gets or sets a value indicating whether the Today button
    /// is displayed.
    /// </summary>
    [Category("Appearance")]
    [Description(
        "Determines whether the Today button is displayed.")]
    [DefaultValue(true)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Visible)]
    public bool ShowTodayButton
    {
        get => _showTodayButton;

        set
        {
            _showTodayButton = value;

            if (_todayButton != null)
                _todayButton.Visible = value;
        }
    }

    // ============================================================
    // SHOW WEEK NUMBERS
    // ============================================================

    /// <summary>
    /// Gets or sets a value indicating whether week numbers
    /// are displayed.
    /// </summary>
    [Category("Appearance")]
    [Description(
        "Determines whether week numbers are displayed.")]
    [DefaultValue(false)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Visible)]
    public bool ShowWeekNumbers
    {
        get => _showWeekNumbers;

        set
        {
            if (_showWeekNumbers == value)
                return;

            _showWeekNumbers = value;

            UpdateCalendar();
        }
    }

    // ============================================================
    // VALUE TEXT
    // ============================================================

    /// <summary>
    /// Gets the currently selected date formatted according
    /// to the CustomFormat and Culture properties.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Hidden)]
    public string ValueText =>
        _value.ToString(
            _customFormat,
            _culture);

    // ============================================================
    // PREVIOUS MONTH
    // ============================================================

    /// <summary>
    /// Moves the displayed month one month backward,
    /// if the previous month is within the allowed date range.
    /// </summary>
    private void PreviousMonthButton_Click(
        object? sender,
        EventArgs e)
    {
        DateTime previousMonth;

        try
        {
            previousMonth =
                _displayedMonth.AddMonths(-1);
        }
        catch
        {
            return;
        }

        DateTime lastDayOfPreviousMonth =
            new DateTime(
                previousMonth.Year,
                previousMonth.Month,
                DateTime.DaysInMonth(
                    previousMonth.Year,
                    previousMonth.Month));

        if (lastDayOfPreviousMonth < MinDate)
            return;

        _displayedMonth = previousMonth;

        UpdateCalendar();
    }

    // ============================================================
    // NEXT MONTH
    // ============================================================

    /// <summary>
    /// Moves the displayed month one month forward,
    /// if the next month is within the allowed date range.
    /// </summary>
    private void NextMonthButton_Click(
        object? sender,
        EventArgs e)
    {
        DateTime nextMonth;

        try
        {
            nextMonth =
                _displayedMonth.AddMonths(1);
        }
        catch
        {
            return;
        }

        DateTime firstDayOfNextMonth =
            new DateTime(
                nextMonth.Year,
                nextMonth.Month,
                1);

        if (firstDayOfNextMonth > MaxDate)
            return;

        _displayedMonth = nextMonth;

        UpdateCalendar();
    }

    // ============================================================
    // TODAY
    // ============================================================

    /// <summary>
    /// Selects today's date and displays the corresponding month.
    /// </summary>
    private void TodayButton_Click(
        object? sender,
        EventArgs e)
    {
        DateTime today = DateTime.Today;

        if (today < MinDate)
            today = MinDate;

        if (today > MaxDate)
            today = MaxDate;

        bool valueChanged =
            _value != today;

        _value = today;

        _displayedMonth =
            new DateTime(
                today.Year,
                today.Month,
                1);

        UpdateCalendar();

        if (valueChanged)
        {
            ValueChanged?.Invoke(
                this,
                EventArgs.Empty);
        }
    }

    // ============================================================
    // UPDATE CALENDAR
    // ============================================================

    /// <summary>
    /// Rebuilds the calendar contents according to the current
    /// culture, selected date, displayed month and display settings.
    /// </summary>
    private void UpdateCalendar()
    {
        if (_isInitializing)
            return;

        if (_isUpdatingCalendar)
            return;

        if (_calendarTable == null)
            return;

        _isUpdatingCalendar = true;

        try
        {
            _calendarTable.SuspendLayout();

            // ----------------------------------------------------
            // Remove all previously created calendar controls.
            // ----------------------------------------------------

            foreach (Control control in
                     _calendarTable.Controls)
            {
                control.Dispose();
            }

            _calendarTable.Controls.Clear();
            _dayButtons.Clear();

            // ----------------------------------------------------
            // Determine the number of columns.
            // ----------------------------------------------------

            int columnCount =
                _showWeekNumbers ? 8 : 7;

            _calendarTable.ColumnCount =
                columnCount;

            _calendarTable.RowCount = 7;

            _calendarTable.ColumnStyles.Clear();
            _calendarTable.RowStyles.Clear();

            // ----------------------------------------------------
            // Configure the seven calendar rows.
            // ----------------------------------------------------

            for (int i = 0; i < 7; i++)
            {
                _calendarTable.RowStyles.Add(
                    new RowStyle(
                        SizeType.Percent,
                        100f / 7f));
            }

            // ----------------------------------------------------
            // Calculate the required column widths.
            // ----------------------------------------------------

            AdjustDayColumnWidths();

            // ----------------------------------------------------
            // Display the current month and year.
            // ----------------------------------------------------

            _monthYearLabel.Text =
                _displayedMonth.ToString(
                    "MMMM yyyy",
                    _culture);

            // ----------------------------------------------------
            // Create the day-of-week header.
            // ----------------------------------------------------

            CreateDayHeaders();

            // ----------------------------------------------------
            // Create the calendar day buttons.
            // ----------------------------------------------------

            CreateDays();

            // ----------------------------------------------------
            // Update month navigation buttons.
            // ----------------------------------------------------

            UpdateNavigationButtons();
        }
        finally
        {
            _calendarTable.ResumeLayout(true);

            _isUpdatingCalendar = false;
        }
    }

    // ============================================================
    // AUTOMATIC DAY COLUMN WIDTH CALCULATION
    // ============================================================

    /// <summary>
    /// Calculates the required width of the calendar columns
    /// based on the longest localized day name.
    /// </summary>
    private void AdjustDayColumnWidths()
    {
        string[] dayNames = GetDayNames();

        using Font headerFont = new Font(
            Font.FontFamily,
            Font.Size,
            FontStyle.Bold);

        // ------------------------------------------------------------
        // Find the widest localized day name.
        // ------------------------------------------------------------

        int maxWidth = 0;

        foreach (string dayName in dayNames)
        {
            Size measuredSize = TextRenderer.MeasureText(
                dayName,
                headerFont);

            maxWidth = Math.Max(
                maxWidth,
                measuredSize.Width);
        }

        // Add extra horizontal space around the text.
        int minimumDayColumnWidth = maxWidth + 12;

        // Width reserved for the week number column.
        int weekNumberWidth = _showWeekNumbers ? 40 : 0;

        // ------------------------------------------------------------
        // Calculate the minimum required width of the calendar.
        // ------------------------------------------------------------

        int requiredWidth =
            weekNumberWidth +
            (minimumDayColumnWidth * 7) +
            _calendarTable.Padding.Left +
            _calendarTable.Padding.Right;

        // The control must never be narrower than 250 pixels.
        requiredWidth = Math.Max(250, requiredWidth);

        // ------------------------------------------------------------
        // The minimum width follows the CURRENT language.
        //
        // This allows:
        //
        // Kiswahili -> wider control
        // Serbian   -> narrower control
        // ------------------------------------------------------------

        MinimumSize = new Size(
            requiredWidth,
            MinimumSize.Height);

        // ------------------------------------------------------------
        // Change the actual control width.
        //
        // Without this, after switching from Kiswahili to Serbian,
        // the control would remain unnecessarily wide.
        // ------------------------------------------------------------

        if (Width != requiredWidth)
        {
            Width = requiredWidth;
        }

        // ------------------------------------------------------------
        // After changing Width, the calendar table has its
        // actual available width.
        // ------------------------------------------------------------

        int availableWidth =
            _calendarTable.ClientSize.Width
            - _calendarTable.Padding.Left
            - _calendarTable.Padding.Right
            - weekNumberWidth;

        if (availableWidth <= 0)
            return;

        // ------------------------------------------------------------
        // Do not allow the columns to become narrower than required
        // by the longest localized day name.
        // ------------------------------------------------------------

        int minimumTotalDayWidth =
            minimumDayColumnWidth * 7;

        if (availableWidth < minimumTotalDayWidth)
        {
            availableWidth = minimumTotalDayWidth;
        }

        // ------------------------------------------------------------
        // Calculate the base width of all seven day columns.
        // ------------------------------------------------------------

        int baseColumnWidth =
            availableWidth / 7;

        // Calculate the remainder caused by integer division.
        int remainder =
            availableWidth % 7;

        // ------------------------------------------------------------
        // Create the column styles.
        // ------------------------------------------------------------

        _calendarTable.ColumnStyles.Clear();

        // Week number column.
        if (_showWeekNumbers)
        {
            _calendarTable.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    weekNumberWidth));
        }

        // Seven day columns.
        //
        // The last column receives the remainder so that
        // no unnecessary empty space remains at the end.
        // ------------------------------------------------------------

        for (int i = 0; i < 7; i++)
        {
            int columnWidth = baseColumnWidth;

            if (i == 6)
            {
                columnWidth += remainder;
            }

            _calendarTable.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    columnWidth));
        }
    }

    // ============================================================
    // DAY HEADERS
    // ============================================================

    /// <summary>
    /// Creates the localized day-of-week header labels.
    /// </summary>
    private void CreateDayHeaders()
    {
        int offset =
            _showWeekNumbers ? 1 : 0;

        if (_showWeekNumbers)
        {
            var weekHeader = new Label
            {
                Text = "#",
                Dock = DockStyle.Fill,
                TextAlign =
                    ContentAlignment.MiddleCenter,
                Font = new Font(
                    Font,
                    FontStyle.Bold),
                AutoEllipsis = false
            };

            _calendarTable.Controls.Add(
                weekHeader,
                0,
                0);
        }

        string[] dayNames =
            GetDayNames();

        for (int i = 0; i < 7; i++)
        {
            var label = new Label
            {
                Text = dayNames[i],
                Dock = DockStyle.Fill,

                // Keep the header text on a single line.
                AutoSize = false,

                // Do not truncate the localized day name.
                AutoEllipsis = false,

                TextAlign =
                    ContentAlignment.MiddleCenter,

                Font = new Font(
                    Font,
                    FontStyle.Bold),

                Margin = new Padding(0)
            };

            _calendarTable.Controls.Add(
                label,
                i + offset,
                0);
        }
    }

    // ============================================================
    // DAY NAMES
    // ============================================================

    /// <summary>
    /// Returns the localized abbreviated names of the seven
    /// days of the week in the configured order.
    /// </summary>
    private string[] GetDayNames()
    {
        string[] result =
            new string[7];

        DateTimeFormatInfo dtfi =
            _culture.DateTimeFormat;

        for (int i = 0; i < 7; i++)
        {
            DayOfWeek day =
                (DayOfWeek)
                (((int)_firstDayOfWeek + i) % 7);

            result[i] =
                dtfi.GetAbbreviatedDayName(day);
        }

        return result;
    }

    // ============================================================
    // CREATE DAYS
    // ============================================================

    /// <summary>
    /// Creates the buttons representing the days
    /// of the currently displayed month.
    /// </summary>
    private void CreateDays()
    {
        int offset =
            _showWeekNumbers ? 1 : 0;

        DateTime firstOfMonth =
            new DateTime(
                _displayedMonth.Year,
                _displayedMonth.Month,
                1);

        int firstDayOffset =
            GetDayOffset(
                firstOfMonth.DayOfWeek);

        int daysInMonth =
            DateTime.DaysInMonth(
                _displayedMonth.Year,
                _displayedMonth.Month);

        for (int day = 1;
             day <= daysInMonth;
             day++)
        {
            DateTime date =
                new DateTime(
                    _displayedMonth.Year,
                    _displayedMonth.Month,
                    day);

            int position =
                firstDayOffset + day - 1;

            int row =
                position / 7 + 1;

            int column =
                position % 7 + offset;

            Button button =
                CreateDayButton(date);

            _calendarTable.Controls.Add(
                button,
                column,
                row);

            _dayButtons.Add(button);
        }

        if (_showWeekNumbers)
        {
            CreateWeekNumbers(
                firstDayOffset,
                daysInMonth);
        }
    }

    // ============================================================
    // DAY OFFSET
    // ============================================================

    /// <summary>
    /// Calculates the zero-based column offset of a day
    /// according to the configured first day of the week.
    /// </summary>
    private int GetDayOffset(
        DayOfWeek day)
    {
        int first =
            (int)_firstDayOfWeek;

        int current =
            (int)day;

        return
            (current - first + 7) % 7;
    }

    // ============================================================
    // DAY BUTTON
    // ============================================================

    /// <summary>
    /// Creates a button representing a specific calendar date.
    /// </summary>
    private Button CreateDayButton(
        DateTime date)
    {
        bool isSelected =
            date.Date == Value.Date;

        bool isToday =
            date.Date == DateTime.Today;

        bool enabled =
            date >= MinDate &&
            date <= MaxDate;

        var button = new Button
        {
            Text =
                date.Day.ToString(
                    CultureInfo.InvariantCulture),

            Dock = DockStyle.Fill,

            Margin =
                new Padding(1),

            FlatStyle =
                FlatStyle.Flat,

            Enabled = enabled,

            Tag = date,

            TabStop = false
        };

        button.FlatAppearance.BorderSize =
            isSelected ? 1 : 0;

        if (isToday && !isSelected)
        {
            button.Font =
                new Font(
                    button.Font,
                    FontStyle.Bold);
        }

        if (isSelected)
        {
            button.BackColor =
                SystemColors.Highlight;

            button.ForeColor =
                SystemColors.HighlightText;
        }

        button.Click += DayButton_Click;

        return button;
    }

    // ============================================================
    // DAY CLICK
    // ============================================================

    /// <summary>
    /// Handles selection of a calendar day.
    /// </summary>
    private void DayButton_Click(
        object? sender,
        EventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.Tag is not DateTime date)
            return;

        Value = date;
    }

    // ============================================================
    // WEEK NUMBERS
    // ============================================================

    /// <summary>
    /// Creates the week number labels when week numbers are enabled.
    /// </summary>
    private void CreateWeekNumbers(
        int firstDayOffset,
        int daysInMonth)
    {
        DateTime firstOfMonth =
            new DateTime(
                _displayedMonth.Year,
                _displayedMonth.Month,
                1);

        int numberOfWeeks =
            (firstDayOffset +
             daysInMonth +
             6) / 7;

        for (int week = 0;
             week < numberOfWeeks;
             week++)
        {
            DateTime weekDate =
                firstOfMonth.AddDays(
                    week * 7 -
                    firstDayOffset);

            int weekNumber =
                GetWeekOfYear(weekDate);

            var label = new Label
            {
                Text =
                    weekNumber.ToString(
                        CultureInfo.InvariantCulture),

                Dock = DockStyle.Fill,

                TextAlign =
                    ContentAlignment.MiddleCenter,

                ForeColor =
                    SystemColors.GrayText
            };

            _calendarTable.Controls.Add(
                label,
                0,
                week + 1);
        }
    }

    // ============================================================
    // WEEK NUMBER CALCULATION
    // ============================================================

    /// <summary>
    /// Calculates the week number for the specified date
    /// using the calendar and week rules of the current culture.
    /// </summary>
    private int GetWeekOfYear(
        DateTime date)
    {
        Calendar calendar =
            _culture.Calendar;

        CalendarWeekRule rule =
            _culture.DateTimeFormat.CalendarWeekRule;

        DayOfWeek firstDay =
            _culture.DateTimeFormat.FirstDayOfWeek;

        return calendar.GetWeekOfYear(
            date,
            rule,
            firstDay);
    }

    // ============================================================
    // NAVIGATION
    // ============================================================

    /// <summary>
    /// Updates the enabled state of the previous and next
    /// month navigation buttons.
    /// </summary>
    private void UpdateNavigationButtons()
    {
        DateTime firstDayOfDisplayedMonth =
            new DateTime(
                _displayedMonth.Year,
                _displayedMonth.Month,
                1);

        DateTime lastDayOfDisplayedMonth =
            firstDayOfDisplayedMonth
                .AddMonths(1)
                .AddDays(-1);

        _previousMonthButton.Enabled =
            lastDayOfDisplayedMonth >= MinDate;

        _nextMonthButton.Enabled =
            firstDayOfDisplayedMonth <= MaxDate;
    }

    // ============================================================
    // RESIZE
    // ============================================================

    /// <summary>
    /// Rebuilds the calendar when the control is resized.
    /// </summary>
    private void CustomMonthCalendar_Resize(
        object? sender,
        EventArgs e)
    {
        if (_isInitializing ||
            _isUpdatingCalendar)
            return;

        UpdateCalendar();
    }

    // ============================================================
    // FONT CHANGED
    // ============================================================

    /// <summary>
    /// Rebuilds the calendar when the control font changes.
    /// </summary>
    private void CustomMonthCalendar_FontChanged(
        object? sender,
        EventArgs e)
    {
        if (_isInitializing ||
            _isUpdatingCalendar)
            return;

        UpdateCalendar();
    }

    // ============================================================
    // FORMATTED VALUE
    // ============================================================

    /// <summary>
    /// Returns the selected date formatted according to
    /// the current CustomFormat and Culture settings.
    /// </summary>
    public string GetFormattedValue() =>
        Value.ToString(
            CustomFormat,
            Culture);

    // ============================================================
    // GO TO MONTH
    // ============================================================

    /// <summary>
    /// Displays the specified month and year if they are
    /// within the configured minimum and maximum date range.
    /// </summary>
    public void GoToMonth(
        int year,
        int month)
    {
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(
                nameof(month));

        DateTime requestedMonth =
            new DateTime(
                year,
                month,
                1);

        if (requestedMonth
                .AddMonths(1)
                .AddDays(-1) < MinDate)
        {
            return;
        }

        if (requestedMonth > MaxDate)
            return;

        _displayedMonth =
            requestedMonth;

        UpdateCalendar();
    }

    // ============================================================
    // GO TO SELECTED DATE
    // ============================================================

    /// <summary>
    /// Displays the month containing the currently selected date.
    /// </summary>
    public void GoToSelectedDate()
    {
        _displayedMonth =
            new DateTime(
                Value.Year,
                Value.Month,
                1);

        UpdateCalendar();
    }
}