using System.ComponentModel;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace Windows_Forms_Custom_Date_Picker;

/// <summary>
/// Represents a customizable Windows Forms calendar control
/// that supports culture-specific month and day names,
/// date selection, minimum and maximum dates, week numbers,
/// navigation, custom fonts and a Today button.
/// </summary>
internal partial class Windows_Forms_Custom_Month_Calendar_Control : UserControl
{
    #region Constants

    private const string DefaultCustomFormat = "    ddd,  dd. MMMM yyyy";
    private const string DefaultTodayButtonFallback = "Today";
    private const string SerbianLatinToday = "Danas";
    private const string SerbianCyrillicToday = "Данас";
    private const string PreviousButtonSymbol = "‹";
    private const string NextButtonSymbol = "›";
    private const string MonthYearHeaderFormat = "MMMM yyyy";
    private const string WeekNumberHeaderSymbol = "#";
    private const string ErrorMinGreaterThanMax = "MinDate cannot be greater than MaxDate.";
    private const string ErrorMaxLessThanMin = "MaxDate cannot be less than MinDate.";

    #endregion

    #region Fields

    /// <summary>
    /// A dictionary mapping two-letter ISO language codes to their respective "Today" translation.
    /// </summary>
    private static readonly Dictionary<string, string> TodayTranslations =
    new(StringComparer.OrdinalIgnoreCase)
    {
        ["af"] = "Vandag",     // Afrikaans
        ["am"] = "ዛሬ",        // Amharic
        ["ar"] = "اليوم",      // Arabic
        ["az"] = "Bu gün",     // Azerbaijani
        ["be"] = "Сёння",      // Belarusian
        ["bg"] = "Днес",       // Bulgarian
        ["bn"] = "आज",        // Bengali
        ["bs"] = "Danas",      // Bosnian
        ["ca"] = "Avui",       // Catalan
        ["cs"] = "Dnes",       // Czech
        ["cy"] = "Heddiw",     // Welsh
        ["da"] = "I dag",      // Danish
        ["de"] = "Heute",      // German
        ["el"] = "Σήμερα",     // Greek
        ["en"] = "Today",      // English
        ["es"] = "Hoy",        // Spanish
        ["et"] = "Täna",       // Estonian
        ["eu"] = "Gaur",       // Basque
        ["fa"] = "امروز",     // Persian
        ["fi"] = "Tänään",     // Finnish
        ["fr"] = "Aujourd'hui", // French
        ["ga"] = "Inniu",      // Irish
        ["gl"] = "Hoxe",       // Galician
        ["gu"] = "આજે",      // Gujarati
        ["he"] = "היום",      // Hebrew
        ["hi"] = "आज",        // Hindi
        ["hr"] = "Danas",      // Croatian
        ["hu"] = "Ma",         // Hungarian
        ["hy"] = "Այսօր",      // Armenian
        ["id"] = "Hari Ini",   // Indonesian
        ["is"] = "Í dag",      // Icelandic
        ["it"] = "Oggi",       // Italian
        ["ja"] = "今日",       // Japanese
        ["jv"] = "Dina iki",   // Javanese
        ["ka"] = "დღეს",       // Georgian
        ["kk"] = "Бүгін",      // Kazakh
        ["km"] = "ថ្ងៃនេះ",    // Khmer
        ["kn"] = "ಇದೊಂದು",   // Kannada (or ಇಂದು)
        ["ko"] = "오늘",       // Korean
        ["ky"] = "Бүгүн",      // Kyrgyz
        ["lo"] = "ມື້ນີ້",      // Lao
        ["lt"] = "Šiandien",   // Lithuanian
        ["lv"] = "Šodien",     // Latvian
        ["mk"] = "Денес",      // Macedonian
        ["ml"] = "ഇന്ന്",     // Malayalam
        ["mn"] = "Өнөөдөр",    // Mongolian
        ["mr"] = "आज",        // Marathi
        ["ms"] = "Hari Ini",   // Malay
        ["mt"] = "Illum",      // Maltese
        ["my"] = "ဒီနေ့",      // Burmese
        ["ne"] = "आज",        // Nepali
        ["nl"] = "Vandaag",    // Dutch
        ["no"] = "I dag",      // Norwegian
        ["pa"] = "ਅੱਜ",        // Punjabi
        ["pl"] = "Dzisiaj",    // Polish
        ["ps"] = "نن",        // Pashto
        ["pt"] = "Hoje",       // Portuguese
        ["ro"] = "Astăzi",     // Romanian
        ["ru"] = "Сегодня",    // Russian
        ["sd"] = "اڄ",        // Sindhi
        ["si"] = "අද",        // Sinhala
        ["sk"] = "Dnes",       // Slovak
        ["sl"] = "Danes",      // Slovenian
        ["so"] = "Maanta",     // Somali
        ["sq"] = "Sot",        // Albanian
        ["sr"] = "Danas",      // Serbian (Latin/General)
        ["su"] = "Kiwari",     // Sundanese
        ["sv"] = "I dag",      // Swedish
        ["sw"] = "Leo",        // Swahili
        ["ta"] = "இன்று",      // Tamil
        ["te"] = "ఈరోజు",    // Telugu
        ["tg"] = "Имрӯз",      // Tajik
        ["th"] = "วันนี้",     // Thai
        ["tk"] = "Şu gün",     // Turkmen
        ["tr"] = "Bugün",      // Turkish
        ["uk"] = "Сьогодні",   // Ukrainian
        ["ur"] = "آج",        // Urdu
        ["uz"] = "Bugun",      // Uzbek
        ["vi"] = "Hôm nay",    // Vietnamese
        ["xh"] = "Namhlanje",  // Xhosa
        ["yi"] = "היינט",     // Yiddish
        ["yo"] = "Loni",       // Yoruba
        ["zh"] = "今天",       // Chinese
        ["zu"] = "Namuhla"     // Zulu
    };

    private CultureInfo _culture = CultureInfo.InvariantCulture;
    private DateTime _value = DateTime.Today;
    private DateTime _minDate = DateTime.MinValue.Date;
    private DateTime _maxDate = DateTime.MaxValue.Date;
    private string _customFormat = DefaultCustomFormat;
    private DayOfWeek _firstDayOfWeek = DayOfWeek.Monday;
    private bool _showTodayButton = false;
    private string _todayButtonFallbackString = DefaultTodayButtonFallback;
    private bool _showWeekNumbers = false;
    private DateTime _displayedMonth;

    private bool _isInitializing = true;
    private bool _isUpdatingCalendar = false;

    private Font _dayFont = SystemFonts.DefaultFont;

    private Font _headerFont =
        new Font(
            SystemFonts.DefaultFont,
            FontStyle.Bold);

    private Font _todayButtonFont =
        SystemFonts.DefaultFont;

    private readonly Panel _headerPanel;
    private readonly Button _previousMonthButton;
    private readonly Button _nextMonthButton;
    private readonly Label _monthYearLabel;
    private readonly TableLayoutPanel _calendarTable;
    private readonly Button _todayButton;
    private readonly List<Button> _dayButtons = new();

    #endregion

    #region Events

    /// <summary>
    /// Occurs when the selected date changes.
    /// </summary>
    public event EventHandler? ValueChanged;

    #endregion

    #region Constructor

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

        _headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 42,
            BackColor = SystemColors.Control
        };

        _previousMonthButton = new Button
        {
            Text = PreviousButtonSymbol,
            Dock = DockStyle.Left,
            Width = 40,
            FlatStyle = FlatStyle.Flat,
            Font = _headerFont
        };

        _previousMonthButton.FlatAppearance.BorderSize = 0;

        _nextMonthButton = new Button
        {
            Text = NextButtonSymbol,
            Dock = DockStyle.Right,
            Width = 40,
            FlatStyle = FlatStyle.Flat,
            Font = _headerFont
        };

        _nextMonthButton.FlatAppearance.BorderSize = 0;

        _monthYearLabel = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,

            // Use the dedicated header font.
            Font = _headerFont
        };

        _headerPanel.Controls.Add(_monthYearLabel);
        _headerPanel.Controls.Add(_previousMonthButton);
        _headerPanel.Controls.Add(_nextMonthButton);

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
            Text = GetLocalizedTodayText(),
            Dock = DockStyle.Bottom,
            Height = 32,
            FlatStyle = FlatStyle.Flat,

            // Use the dedicated Today button font.
            Font = _todayButtonFont
        };

        _todayButton.FlatAppearance.BorderSize = 0;

        Controls.Add(_calendarTable);
        Controls.Add(_todayButton);
        Controls.Add(_headerPanel);

        _previousMonthButton.Click +=
            PreviousMonthButton_Click;

        _nextMonthButton.Click +=
            NextMonthButton_Click;

        _todayButton.Click +=
            TodayButton_Click;

        Resize +=
            CustomMonthCalendar_Resize;

        FontChanged +=
            CustomMonthCalendar_FontChanged;

        _isInitializing = false;

        UpdateCalendar();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the base font of the calendar control.
    /// </summary>
    [Category("Fonts")]
    [Description("Base font for the control.")]
    [Browsable(true)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Visible)]
    [AllowNull]
    public override Font Font
    {
        get => base.Font;

        set
        {
            base.Font = value;

            Font effectiveFont =
            value ?? SystemFonts.DefaultFont;

            if (_monthYearLabel != null)
            {
                _monthYearLabel.Font =
                    new Font(
                        effectiveFont,
                        FontStyle.Bold);
            }

            if (_todayButton != null)
            {
                _todayButton.Font =
                    effectiveFont;
            }

            UpdateCalendar();
        }
    }

    /// <summary>
    /// Gets or sets the font used for calendar day numbers
    /// and abbreviated day names.
    /// </summary>
    [Category("Fonts")]
    [Description(
        "Font used for the calendar days.")]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Visible)]
    public Font CalendarDaysFont
    {
        get => _dayFont;

        set
        {
            _dayFont =
                value ?? SystemFonts.DefaultFont;

            UpdateCalendar();
        }
    }

    /// <summary>
    /// Gets or sets the font used for the month and year header.
    /// </summary>
    [Category("Fonts")]
    [Description(
        "Font used for the month and year header.")]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Visible)]
    public Font CalendarHeaderFont
    {
        get => _headerFont;

        set
        {
            _headerFont =
                value ??
                new Font(
                    SystemFonts.DefaultFont,
                    FontStyle.Bold);

            if (_monthYearLabel != null)
            {
                _monthYearLabel.Font =
                    _headerFont;
            }

            if (_previousMonthButton != null)
                _previousMonthButton.Font = _headerFont;

            if (_nextMonthButton != null)
                _nextMonthButton.Font = _headerFont;


            // Recalculate the calendar height
            // according to the new header font.
            UpdateCalendar();
        }
    }

    /// <summary>
    /// Gets or sets the font used for the Today button.
    /// </summary>
    [Category("Fonts")]
    [Description(
        "Font used for the Today button.")]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Visible)]
    public Font CalendarTodayButtonFont
    {
        get => _todayButtonFont;

        set
        {
            _todayButtonFont =
                value ?? SystemFonts.DefaultFont;

            if (_todayButton != null)
            {
                _todayButton.Font =
                    _todayButtonFont;
            }

            // Recalculate the calendar height
            // according to the new Today button font.
            UpdateCalendar();
        }
    }

    /// <summary>
    /// Gets or sets the culture used to display
    /// month and day names.
    /// </summary>
    [Category("Localization")]
    [Description(
        "Culture used to display month and day names.")]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Visible)]
    public CultureInfo Culture
    {
        get => _culture;

        set
        {
            if (value == null)
                throw new ArgumentNullException(
                    nameof(value));

            if (_culture.Equals(value))
                return;

            _culture = value;

            if (_todayButton != null)
            {
                _todayButton.Text = GetLocalizedTodayText();
            }

            if (_isInitializing)
                return;

            UpdateCalendar();
        }
    }

    /// <summary>
    /// Gets or sets the format used when the selected
    /// date is represented as text.
    /// </summary>
    [Category("Appearance")]
    [Description(
        "Format used when the selected date is represented as text.")]
    [DefaultValue(DefaultCustomFormat)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Visible)]
    public string CustomFormat
    {
        get => _customFormat;

        set
        {
            _customFormat =
                string.IsNullOrWhiteSpace(value)
                    ? DefaultCustomFormat
                    : value;

            Invalidate();
        }
    }

    /// <summary>
    /// Gets or sets the currently selected date.
    /// </summary>
    [Category("Behavior")]
    [Description(
        "Currently selected date.")]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Visible)]
    public DateTime Value
    {
        get => _value;

        set
        {
            DateTime newValue =
                value.Date;

            if (newValue < MinDate)
                newValue = MinDate;

            if (newValue > MaxDate)
                newValue = MaxDate;

            if (_value == newValue)
                return;

            _value = newValue;

            _displayedMonth =
                new DateTime(
                    _value.Year,
                    _value.Month,
                    1);

            UpdateCalendar();

            ValueChanged?.Invoke(
                this,
                EventArgs.Empty);
        }
    }

    /// <summary>
    /// Gets or sets the minimum selectable date.
    /// </summary>
    [Category("Behavior")]
    [Description(
        "Minimum selectable date.")]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Visible)]
    public DateTime MinDate
    {
        get => _minDate;

        set
        {
            DateTime newValue =
                value.Date;

            if (newValue > _maxDate)
            {
                throw new ArgumentException(
                    ErrorMinGreaterThanMax);
            }

            _minDate = newValue;

            if (_value < _minDate)
                Value = _minDate;
            else
                UpdateCalendar();
        }
    }

    /// <summary>
    /// Gets or sets the maximum selectable date.
    /// </summary>
    [Category("Behavior")]
    [Description(
        "Maximum selectable date.")]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Visible)]
    public DateTime MaxDate
    {
        get => _maxDate;

        set
        {
            DateTime newValue =
                value.Date;

            if (newValue < _minDate)
            {
                throw new ArgumentException(
                    ErrorMaxLessThanMin);
            }

            _maxDate = newValue;

            if (_value > _maxDate)
                Value = _maxDate;
            else
                UpdateCalendar();
        }
    }

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

    /// <summary>
    /// Gets or sets a value indicating whether the Today button
    /// is displayed.
    /// </summary>
    [Category("Appearance")]
    [Description(
        "Determines whether the Today button is displayed.")]
    [DefaultValue(false)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Visible)]
    public bool CalendarTodayButtonVisible
    {
        get => _showTodayButton;

        set
        {
            if (_showTodayButton == value)
                return;

            _showTodayButton = value;

            if (_todayButton != null)
            {
                _todayButton.Visible =
                    value;
            }

            UpdateCalendar();
        }
    }

    /// <summary>
    /// Gets or sets the fallback text for the Today button.
    /// </summary>
    [Category("Localization")]
    [Description("Optional fallback text for the Today button if no translation is found.")]
    [DefaultValue(DefaultTodayButtonFallback)]
    public string CalendarTodayButtonFallbackString
    {
        get => _todayButtonFallbackString;
        set => _todayButtonFallbackString = string.IsNullOrEmpty(value) ? DefaultTodayButtonFallback : value;
    }

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

    /// <summary>
    /// Gets the selected date formatted according to
    /// the current CustomFormat and Culture settings.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Hidden)]
    public string ValueText =>
        _value.ToString(
            _customFormat,
            _culture);

    #endregion

    #region Methods

    /// <summary>
    /// Returns the selected date formatted according to
    /// the current CustomFormat and Culture settings.
    /// </summary>
    public string GetFormattedValue() =>
        Value.ToString(
            CustomFormat,
            Culture);

    /// <summary>
    /// Displays the specified month and year if they are
    /// within the configured minimum and maximum date range.
    /// </summary>
    public void GoToMonth(
        int year,
        int month)
    {
        if (month < 1 || month > 12)
        {
            throw new ArgumentOutOfRangeException(
                nameof(month));
        }

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

    /// <summary>
    /// Retrieves the localized text for the "Today" button based on the current culture.
    /// Supports specific regional variants (e.g., Serbian Cyrillic/Latin) and 
    /// looks up standard language codes in the TodayTranslations dictionary, 
    /// defaulting to the fallback string for unhandled cultures.
    /// </summary>
    private string GetLocalizedTodayText()
    {
        string cultureName = _culture.Name.ToLowerInvariant();
        if (cultureName.Contains("sr-latn") || cultureName == "sr-latn")
            return SerbianLatinToday;
        if (cultureName.Contains("sr-cyrl") || cultureName == "sr-cyrl")
            return SerbianCyrillicToday;

        string languageCode = _culture.TwoLetterISOLanguageName.ToLowerInvariant();
        if (TodayTranslations.TryGetValue(languageCode, out string? translation))
        {
            return translation;
        }

        // Return the user-defined fallback string if it exists, otherwise the default fallback constant
        return !string.IsNullOrEmpty(_todayButtonFallbackString)
            ? _todayButtonFallbackString
            : DefaultTodayButtonFallback;
    }

    #endregion

    #region Event Handlers

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

        _displayedMonth =
            previousMonth;

        UpdateCalendar();
    }

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

        _displayedMonth =
            nextMonth;

        UpdateCalendar();
    }

    private void TodayButton_Click(
        object? sender,
        EventArgs e)
    {
        DateTime today =
            DateTime.Today;

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

    private void CustomMonthCalendar_Resize(
        object? sender,
        EventArgs e)
    {
        if (_isInitializing ||
            _isUpdatingCalendar)
        {
            return;
        }

        UpdateCalendar();
    }

    private void CustomMonthCalendar_FontChanged(
        object? sender,
        EventArgs e)
    {
        if (_isInitializing ||
            _isUpdatingCalendar)
        {
            return;
        }

        UpdateCalendar();
    }

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

    #endregion

    #region Calendar Internal Logic

    /// <summary>
    /// Rebuilds the calendar and recalculates its dimensions
    /// according to the current culture, fonts and settings.
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

            // Remove all previously generated controls.

            foreach (Control control in
                     _calendarTable.Controls)
            {
                control.Dispose();
            }

            _calendarTable.Controls.Clear();
            _dayButtons.Clear();

            // Determine the number of columns.

            int columnCount =
                _showWeekNumbers ? 8 : 7;

            _calendarTable.ColumnCount =
                columnCount;

            _calendarTable.RowCount = 7;

            _calendarTable.ColumnStyles.Clear();
            _calendarTable.RowStyles.Clear();

            // Calculate the required calendar width.

            AdjustDayColumnWidths();

            // Calculate the required calendar height.

            AdjustCalendarHeight();

            // Display the current month and year.

            _monthYearLabel.Text =
                _displayedMonth.ToString(
                    MonthYearHeaderFormat,
                    _culture);

            // Make sure the dedicated fonts
            // are applied to the existing controls.

            _monthYearLabel.Font =
                _headerFont;

            _todayButton.Font =
                _todayButtonFont;

            // Create the day-name header row.

            CreateDayHeaders();

            // Create the calendar day buttons.

            CreateDays();

            // Update the month navigation buttons.

            UpdateNavigationButtons();
        }
        finally
        {
            _calendarTable.ResumeLayout(true);

            _isUpdatingCalendar = false;
        }
    }

    /// <summary>
    /// Calculates and applies the required width of the calendar
    /// according to the longest abbreviated day name and week numbers.
    /// </summary>
    private void AdjustDayColumnWidths()
    {
        string[] dayNames =
            GetDayNames();

        using Font headerFont =
            new Font(
                _dayFont.FontFamily,
                _dayFont.Size,
                FontStyle.Bold);

        // Find the widest abbreviated day name.

        int maxWidth = 0;

        foreach (string dayName in dayNames)
        {
            Size measuredSize =
                TextRenderer.MeasureText(
                    dayName,
                    headerFont);

            maxWidth = Math.Max(
                maxWidth,
                measuredSize.Width);
        }

        // Add horizontal space around the text.
        int minimumDayColumnWidth =
            maxWidth + 12;

        // Dynamically calculate the week number column width based on the font and number size.
        int weekNumberWidth = 0;
        if (_showWeekNumbers)
        {
            Size weekMeasuredSize =
                TextRenderer.MeasureText(
                    "##", // Measure with two characters to ensure enough room for two-digit week numbers.
                    headerFont);

            weekNumberWidth = Math.Max(40, weekMeasuredSize.Width + 12);
        }

        // Calculate the minimum required control width.
        int requiredWidth =
            weekNumberWidth +
            (minimumDayColumnWidth * 7) +
            _calendarTable.Padding.Left +
            _calendarTable.Padding.Right;

        // Keep the control at least 250 pixels wide.
        requiredWidth =
            Math.Max(
                250,
                requiredWidth);

        // The minimum width follows the current language
        // and the current calendar-day font.
        MinimumSize = new Size(
            requiredWidth,
            MinimumSize.Height);

        // Resize the actual control as well.
        if (Width != requiredWidth)
        {
            Width = requiredWidth;
        }

        // Calculate the available width for the seven
        // calendar-day columns.
        int availableWidth =
            _calendarTable.ClientSize.Width
            - _calendarTable.Padding.Left
            - _calendarTable.Padding.Right
            - weekNumberWidth;

        if (availableWidth <= 0)
            return;

        int minimumTotalDayWidth =
            minimumDayColumnWidth * 7;

        if (availableWidth <
            minimumTotalDayWidth)
        {
            availableWidth =
                minimumTotalDayWidth;
        }

        int baseColumnWidth =
            availableWidth / 7;

        int remainder =
            availableWidth % 7;

        _calendarTable.ColumnStyles.Clear();

        if (_showWeekNumbers)
        {
            _calendarTable.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    weekNumberWidth));
        }

        for (int i = 0; i < 7; i++)
        {
            int columnWidth =
                baseColumnWidth;

            if (i == 6)
            {
                columnWidth +=
                    remainder;
            }

            _calendarTable.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    columnWidth));
        }
    }

    /// <summary>
    /// Calculates and applies the required height of the calendar
    /// according to the configured header, day and Today button fonts.
    /// </summary>
    private void AdjustCalendarHeight()
    {
        // Measure the month and year header font.

        int headerTextHeight =
            TextRenderer.MeasureText(
                "Ag",
                _headerFont).Height;

        // Add vertical padding around the header text.

        int headerHeight =
            headerTextHeight + 16;

        // Keep a reasonable minimum header height.

        headerHeight =
            Math.Max(
                42,
                headerHeight);

        // Measure the Today button font.

        int todayButtonHeight = 0;

        if (_showTodayButton)
        {
            int todayTextHeight =
                TextRenderer.MeasureText(
                    "Ag",
                    _todayButtonFont).Height;

            // Add vertical padding around the Today text.

            todayButtonHeight =
                todayTextHeight + 12;

            // Keep a reasonable minimum button height.

            todayButtonHeight =
                Math.Max(
                    32,
                    todayButtonHeight);
        }

        // Measure the calendar day font.

        int dayTextHeight =
            TextRenderer.MeasureText(
                "Ag",
                _dayFont).Height;

        // Add vertical padding around the day number.

        int dayRowHeight =
            dayTextHeight + 8;

        // Keep a reasonable minimum row height.

        dayRowHeight =
            Math.Max(
                28,
                dayRowHeight);

        // The calendar table contains seven rows:
        //
        // Row 0 = day-name header
        // Rows 1-6 = calendar weeks
        //
        // All seven rows use the calculated row height.

        int calendarGridHeight =
            dayRowHeight * 7;

        // Add the table padding.

        int tablePaddingHeight =
            _calendarTable.Padding.Top +
            _calendarTable.Padding.Bottom;

        // Calculate the total required height.

        int requiredHeight =
            headerHeight +
            todayButtonHeight +
            calendarGridHeight +
            tablePaddingHeight;

        // Keep the control at least 220 pixels high.

        requiredHeight =
            Math.Max(
                220,
                requiredHeight);

        // Update the minimum control height.

        MinimumSize = new Size(
            MinimumSize.Width,
            requiredHeight);

        // Resize the actual control.
        //
        // This is important when a smaller font replaces
        // a previously larger font.

        if (Height != requiredHeight)
        {
            Height = requiredHeight;
        }

        // Apply the calculated header height.

        _headerPanel.Height =
            headerHeight;

        // Apply the calculated Today button height.

        _todayButton.Height =
            todayButtonHeight;

        // Configure all seven calendar rows.

        _calendarTable.RowStyles.Clear();

        for (int i = 0; i < 7; i++)
        {
            _calendarTable.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    dayRowHeight));
        }
    }

    /// <summary>
    /// Creates the abbreviated day-name headers
    /// according to the current culture and first day of week.
    /// </summary>
    private void CreateDayHeaders()
    {
        int offset =
            _showWeekNumbers ? 1 : 0;

        // Create the week-number header.

        if (_showWeekNumbers)
        {
            var weekHeader = new Label
            {
                Text = WeekNumberHeaderSymbol,
                Dock = DockStyle.Fill,
                TextAlign =
                    ContentAlignment.MiddleCenter,

                Font =
                    new Font(
                        _dayFont,
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

        // Create the seven abbreviated day-name labels.

        for (int i = 0; i < 7; i++)
        {
            var label = new Label
            {
                Text = dayNames[i],
                Dock = DockStyle.Fill,

                // Keep the label on one line.

                AutoSize = false,

                // Do not display an ellipsis.

                AutoEllipsis = false,

                TextAlign =
                    ContentAlignment.MiddleCenter,

                Font =
                    new Font(
                        _dayFont,
                        FontStyle.Bold),

                Margin =
                    new Padding(0)
            };

            _calendarTable.Controls.Add(
                label,
                i + offset,
                0);
        }
    }

    /// <summary>
    /// Returns the seven abbreviated day names according
    /// to the selected culture and first day of week.
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

    /// <summary>
    /// Creates the day buttons for the currently displayed month.
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
                firstDayOffset +
                day -
                1;

            int row =
                position / 7 + 1;

            int column =
                position % 7 +
                offset;

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

    /// <summary>
    /// Calculates the zero-based calendar column offset
    /// of the specified day.
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

    /// <summary>
    /// Creates a button representing a single calendar day.
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

            Dock =
                DockStyle.Fill,

            Margin =
                new Padding(1),

            FlatStyle =
                FlatStyle.Flat,

            Enabled =
                enabled,

            Tag =
                date,

            TabStop =
                false,

            Font =
                _dayFont
        };

        button.FlatAppearance.BorderSize =
            isSelected ? 1 : 0;

        // Highlight today's date with a bold font
        // when it is not the selected date.

        if (isToday && !isSelected)
        {
            button.Font =
                new Font(
                    _dayFont,
                    FontStyle.Bold);
        }

        // Highlight the selected date.

        if (isSelected)
        {
            button.BackColor =
                SystemColors.Highlight;

            button.ForeColor =
                SystemColors.HighlightText;
        }

        button.Click +=
            DayButton_Click;

        return button;
    }

    /// <summary>
    /// Creates the week-number labels when week numbers
    /// are enabled.
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
                GetWeekOfYear(
                    weekDate);

            var label = new Label
            {
                Text =
                    weekNumber.ToString(
                        CultureInfo.InvariantCulture),

                Dock =
                    DockStyle.Fill,

                TextAlign =
                    ContentAlignment.MiddleCenter,

                ForeColor =
                    SystemColors.GrayText,

                Font =
                    _dayFont
            };

            _calendarTable.Controls.Add(
                label,
                0,
                week + 1);
        }
    }

    /// <summary>
    /// Calculates the week number according to the current culture.
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

    #endregion
}