using System.ComponentModel;
using System.Globalization;

namespace Windows_Forms_Custom_Date_Picker;

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

    private readonly Button _previousMonthButton;
    private readonly Button _nextMonthButton;
    private readonly Label _monthYearLabel;
    private readonly TableLayoutPanel _calendarTable;
    private readonly Button _todayButton;
    private readonly List<Button> _dayButtons = new();

    public event EventHandler? ValueChanged;

    public Windows_Forms_Custom_Month_Calendar_Control()
    {
        DoubleBuffered = true;
        AutoScaleMode = AutoScaleMode.Font;
        MinimumSize = new Size(250, 220);
        Size = new Size(320, 280);

        BackColor = SystemColors.Window;
        ForeColor = SystemColors.WindowText;

        _displayedMonth = new DateTime(_value.Year, _value.Month, 1);

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
            Font = new Font(Font.FontFamily, Font.Size, FontStyle.Bold)
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
            CellBorderStyle = TableLayoutPanelCellBorderStyle.None
        };

        for (int i = 0; i < 7; i++)
        {
            _calendarTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 7f));
            _calendarTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 7f));
        }

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

        UpdateCalendar();
    }

    [Category("Localization")]
    [Description("Culture used to display month and day names.")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
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
            UpdateCalendar();
        }
    }

    [Category("Appearance")]
    [Description("Format used when the selected date is represented as text.")]
    [DefaultValue("dd.MM.yyyy")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public string CustomFormat
    {
        get => _customFormat;
        set
        {
            _customFormat = string.IsNullOrWhiteSpace(value) ? "dd.MM.yyyy" : value;
            Invalidate();
        }
    }

    [Category("Behavior")]
    [Description("Currently selected date.")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
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
            _displayedMonth = new DateTime(_value.Year, _value.Month, 1);

            UpdateCalendar();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    [Category("Behavior")]
    [Description("Minimum selectable date.")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public DateTime MinDate
    {
        get => _minDate;
        set
        {
            DateTime newValue = value.Date;

            if (newValue > _maxDate)
                throw new ArgumentException("MinDate cannot be greater than MaxDate.");

            _minDate = newValue;

            if (_value < _minDate)
                Value = _minDate;
            else
                UpdateCalendar();
        }
    }

    [Category("Behavior")]
    [Description("Maximum selectable date.")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public DateTime MaxDate
    {
        get => _maxDate;
        set
        {
            DateTime newValue = value.Date;

            if (newValue < _minDate)
                throw new ArgumentException("MaxDate cannot be less than MinDate.");

            _maxDate = newValue;

            if (_value > _maxDate)
                Value = _maxDate;
            else
                UpdateCalendar();
        }
    }

    [Category("Appearance")]
    [Description("First day of the week displayed in the calendar.")]
    [DefaultValue(DayOfWeek.Monday)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
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

    [Category("Appearance")]
    [Description("Determines whether the Today button is displayed.")]
    [DefaultValue(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public bool ShowTodayButton
    {
        get => _showTodayButton;
        set
        {
            _showTodayButton = value;
            _todayButton.Visible = value;
        }
    }

    [Category("Appearance")]
    [Description("Determines whether week numbers are displayed.")]
    [DefaultValue(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
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

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string ValueText => _value.ToString(_customFormat, _culture);

    private void PreviousMonthButton_Click(object? sender, EventArgs e)
    {
        DateTime previousMonth;
        try
        {
            previousMonth = _displayedMonth.AddMonths(-1);
        }
        catch
        {
            return;
        }

        DateTime lastDayOfPreviousMonth = new DateTime(
            previousMonth.Year,
            previousMonth.Month,
            DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month));

        if (lastDayOfPreviousMonth < MinDate)
            return;

        _displayedMonth = previousMonth;
        UpdateCalendar();
    }

    private void NextMonthButton_Click(object? sender, EventArgs e)
    {
        DateTime nextMonth;
        try
        {
            nextMonth = _displayedMonth.AddMonths(1);
        }
        catch
        {
            return;
        }

        DateTime firstDayOfNextMonth = new DateTime(nextMonth.Year, nextMonth.Month, 1);

        if (firstDayOfNextMonth > MaxDate)
            return;

        _displayedMonth = nextMonth;
        UpdateCalendar();
    }

    private void TodayButton_Click(object? sender, EventArgs e)
    {
        DateTime today = DateTime.Today;

        if (today < MinDate)
            today = MinDate;

        if (today > MaxDate)
            today = MaxDate;

        _value = today;
        _displayedMonth = new DateTime(today.Year, today.Month, 1);

        UpdateCalendar();
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateCalendar()
    {
        if (_calendarTable == null)
            return;

        _calendarTable.SuspendLayout();
        try
        {
            _calendarTable.Controls.Clear();
            _dayButtons.Clear();

            int columnCount = _showWeekNumbers ? 8 : 7;
            _calendarTable.ColumnCount = columnCount;
            _calendarTable.ColumnStyles.Clear();

            if (_showWeekNumbers)
            {
                _calendarTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40f));
            }

            for (int i = 0; i < 7; i++)
            {
                _calendarTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 7f));
            }

            _calendarTable.RowCount = 7;
            _monthYearLabel.Text = _displayedMonth.ToString("MMMM yyyy", _culture);

            CreateDayHeaders();
            CreateDays();
            UpdateNavigationButtons();
        }
        finally
        {
            _calendarTable.ResumeLayout();
        }
    }

    private void CreateDayHeaders()
    {
        int offset = _showWeekNumbers ? 1 : 0;

        if (_showWeekNumbers)
        {
            var weekHeader = new Label
            {
                Text = "#",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(Font, FontStyle.Bold)
            };
            _calendarTable.Controls.Add(weekHeader, 0, 0);
        }

        string[] dayNames = GetDayNames();

        for (int i = 0; i < 7; i++)
        {
            var label = new Label
            {
                Text = dayNames[i],
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(Font, FontStyle.Bold)
            };
            _calendarTable.Controls.Add(label, i + offset, 0);
        }
    }

    private string[] GetDayNames()
    {
        string[] result = new string[7];
        DateTimeFormatInfo dtfi = _culture.DateTimeFormat;

        for (int i = 0; i < 7; i++)
        {
            DayOfWeek day = (DayOfWeek)(((int)_firstDayOfWeek + i) % 7);
            result[i] = dtfi.GetAbbreviatedDayName(day);
        }

        return result;
    }

    private void CreateDays()
    {
        int offset = _showWeekNumbers ? 1 : 0;
        DateTime firstOfMonth = new DateTime(_displayedMonth.Year, _displayedMonth.Month, 1);
        int firstDayOffset = GetDayOffset(firstOfMonth.DayOfWeek);
        int daysInMonth = DateTime.DaysInMonth(_displayedMonth.Year, _displayedMonth.Month);

        for (int day = 1; day <= daysInMonth; day++)
        {
            DateTime date = new DateTime(_displayedMonth.Year, _displayedMonth.Month, day);
            int position = firstDayOffset + day - 1;
            int row = position / 7 + 1;
            int column = position % 7 + offset;

            var button = CreateDayButton(date);
            _calendarTable.Controls.Add(button, column, row);
            _dayButtons.Add(button);
        }

        if (_showWeekNumbers)
        {
            CreateWeekNumbers(firstDayOffset, daysInMonth);
        }
    }

    private int GetDayOffset(DayOfWeek day)
    {
        int first = (int)_firstDayOfWeek;
        int current = (int)day;
        return (current - first + 7) % 7;
    }

    private Button CreateDayButton(DateTime date)
    {
        bool isSelected = date.Date == Value.Date;
        bool isToday = date.Date == DateTime.Today;
        bool enabled = date >= MinDate && date <= MaxDate;

        var button = new Button
        {
            Text = date.Day.ToString(CultureInfo.InvariantCulture),
            Dock = DockStyle.Fill,
            Margin = new Padding(1),
            FlatStyle = FlatStyle.Flat,
            Enabled = enabled,
            Tag = date,
            TabStop = false
        };

        button.FlatAppearance.BorderSize = isSelected ? 1 : 0;

        if (isToday && !isSelected)
        {
            button.Font = new Font(button.Font, FontStyle.Bold);
        }

        if (isSelected)
        {
            button.BackColor = SystemColors.Highlight;
            button.ForeColor = SystemColors.HighlightText;
        }

        button.Click += DayButton_Click;
        return button;
    }

    private void DayButton_Click(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.Tag is not DateTime date)
            return;

        Value = date;
    }

    private void CreateWeekNumbers(int firstDayOffset, int daysInMonth)
    {
        DateTime firstOfMonth = new DateTime(_displayedMonth.Year, _displayedMonth.Month, 1);
        int numberOfWeeks = (firstDayOffset + daysInMonth + 6) / 7;

        for (int week = 0; week < numberOfWeeks; week++)
        {
            DateTime weekDate = firstOfMonth.AddDays(week * 7 - firstDayOffset);
            int weekNumber = GetWeekOfYear(weekDate);

            var label = new Label
            {
                Text = weekNumber.ToString(CultureInfo.InvariantCulture),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = SystemColors.GrayText
            };

            _calendarTable.Controls.Add(label, 0, week + 1);
        }
    }

    private int GetWeekOfYear(DateTime date)
    {
        Calendar calendar = _culture.Calendar;
        CalendarWeekRule rule = _culture.DateTimeFormat.CalendarWeekRule;
        DayOfWeek firstDay = _culture.DateTimeFormat.FirstDayOfWeek;

        return calendar.GetWeekOfYear(date, rule, firstDay);
    }

    private void UpdateNavigationButtons()
    {
        DateTime firstDayOfDisplayedMonth = new DateTime(_displayedMonth.Year, _displayedMonth.Month, 1);
        DateTime lastDayOfDisplayedMonth = firstDayOfDisplayedMonth.AddMonths(1).AddDays(-1);

        _previousMonthButton.Enabled = lastDayOfDisplayedMonth >= MinDate;
        _nextMonthButton.Enabled = firstDayOfDisplayedMonth <= MaxDate;
    }

    private void CustomMonthCalendar_Resize(object? sender, EventArgs e)
    {
        UpdateCalendar();
    }

    private void CustomMonthCalendar_FontChanged(object? sender, EventArgs e)
    {
        UpdateCalendar();
    }

    public string GetFormattedValue() => Value.ToString(CustomFormat, Culture);

    public void GoToMonth(int year, int month)
    {
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month));

        DateTime requestedMonth = new DateTime(year, month, 1);

        if (requestedMonth.AddMonths(1).AddDays(-1) < MinDate)
            return;

        if (requestedMonth > MaxDate)
            return;

        _displayedMonth = requestedMonth;
        UpdateCalendar();
    }

    public void GoToSelectedDate()
    {
        _displayedMonth = new DateTime(Value.Year, Value.Month, 1);
        UpdateCalendar();
    }
}