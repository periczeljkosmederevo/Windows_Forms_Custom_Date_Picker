using System.ComponentModel;
using System.Globalization;

namespace Windows_Forms_Custom_Date_Picker;

[DefaultProperty(nameof(Value))]
[DefaultEvent(nameof(ValueChanged))]
public partial class Windows_Forms_Custom_Date_Picker_Control : UserControl
{
    private readonly TextBox _textBox;
    private readonly Button _dropDownButton;
    private readonly Windows_Forms_Custom_Month_Calendar_Control _calendar;
    private readonly ToolStripDropDown _popup;

    private CultureInfo _culture = CultureInfo.CurrentCulture;
    private string _customFormat = "   ddd,  dd. MMMM yyyy";
    private DateTime _value = DateTime.Today;
    private DateTime _minDate = DateTime.MinValue;
    private DateTime _maxDate = DateTime.MaxValue;

    private DayOfWeek _firstDayOfWeek = DayOfWeek.Sunday;
    private bool _showTodayButton = true;
    private bool _showWeekNumbers;
    private bool _internalUpdate;

    public Windows_Forms_Custom_Date_Picker_Control()
    {
        InitializeComponent();

        SetStyle(ControlStyles.AllPaintingInWmPaint
            | ControlStyles.UserPaint
            | ControlStyles.OptimizedDoubleBuffer,
            true);

        MinimumSize = new Size(120, 23);
        Size = new Size(200, 29);

        _textBox = new TextBox
        {
            BorderStyle = BorderStyle.FixedSingle,
            ReadOnly = true,
            TabStop = true,
            Dock = DockStyle.Fill
        };

        _dropDownButton = new Button
        {
            Text = "▼",
            Dock = DockStyle.Right,
            Width = 30,
            TabStop = false,
            FlatStyle = FlatStyle.System
        };

        _calendar = new Windows_Forms_Custom_Month_Calendar_Control
        {
            Culture = _culture,
            CustomFormat = _customFormat,
            Value = _value,
            MinDate = _minDate,
            MaxDate = _maxDate,
            FirstDayOfWeek = _firstDayOfWeek,
            ShowTodayButton = _showTodayButton,
            ShowWeekNumbers = _showWeekNumbers
        };

        _popup = new ToolStripDropDown
        {
            AutoClose = true,
            AutoSize = true,
            Padding = Padding.Empty
        };

        var host = new ToolStripControlHost(_calendar)
        {
            Padding = Padding.Empty,
            Margin = Padding.Empty,
            AutoSize = false,
            Size = _calendar.Size
        };

        _popup.Items.Add(host);

        Controls.Add(_textBox);
        Controls.Add(_dropDownButton);

        _dropDownButton.Click += DropDownButton_Click;
        _calendar.ValueChanged += Calendar_ValueChanged;
        _textBox.KeyDown += TextBox_KeyDown;

        UpdateText();
    }

    #region Properties

    [Category("Behavior")]
    [Description("Culture used by the date picker.")]
    [DefaultValue(typeof(CultureInfo), "")]
    public CultureInfo Culture
    {
        get => _culture;
        set
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (Equals(_culture, value))
                return;

            _culture = value;
            _calendar.Culture = _culture;
            UpdateText();
        }
    }

    [Category("Appearance")]
    [Description("Format used to display the selected date.")]
    [DefaultValue("d")]
    public string CustomFormat
    {
        get => _customFormat;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("CustomFormat cannot be null or empty.", nameof(value));

            if (_customFormat == value)
                return;

            _customFormat = value;
            _calendar.CustomFormat = _customFormat;
            UpdateText();
        }
    }

    [Category("Behavior")]
    [Description("Currently selected date.")]
    [DefaultValue(typeof(DateTime), "")]
    public DateTime Value
    {
        get => _value;
        set
        {
            DateTime newValue = value;

            if (newValue < _minDate)
                newValue = _minDate;

            if (newValue > _maxDate)
                newValue = _maxDate;

            newValue = newValue.Date;

            if (_value == newValue)
            {
                UpdateText();
                return;
            }

            _value = newValue;
            UpdateCalendarValue();
            UpdateText();
            OnValueChanged(EventArgs.Empty);
        }
    }

    [Category("Behavior")]
    [Description("Minimum selectable date.")]
    [DefaultValue(typeof(DateTime), "")]
    public DateTime MinDate
    {
        get => _minDate;
        set
        {
            if (value > _maxDate)
                throw new ArgumentException("MinDate cannot be greater than MaxDate.", nameof(value));

            _minDate = value.Date;
            _calendar.MinDate = _minDate;

            if (_value < _minDate)
                Value = _minDate;
        }
    }

    [Category("Behavior")]
    [Description("Maximum selectable date.")]
    [DefaultValue(typeof(DateTime), "")]
    public DateTime MaxDate
    {
        get => _maxDate;
        set
        {
            if (value < _minDate)
                throw new ArgumentException("MaxDate cannot be less than MinDate.", nameof(value));

            _maxDate = value.Date;
            _calendar.MaxDate = _maxDate;

            if (_value > _maxDate)
                Value = _maxDate;
        }
    }

    [Category("Appearance")]
    [Description("First day of the week displayed by the calendar.")]
    [DefaultValue(DayOfWeek.Sunday)]
    public DayOfWeek FirstDayOfWeek
    {
        get => _firstDayOfWeek;
        set
        {
            if (_firstDayOfWeek == value)
                return;

            _firstDayOfWeek = value;
            _calendar.FirstDayOfWeek = value;
        }
    }

    [Category("Appearance")]
    [Description("Determines whether the calendar displays the Today button.")]
    [DefaultValue(true)]
    public bool ShowTodayButton
    {
        get => _showTodayButton;
        set
        {
            if (_showTodayButton == value)
                return;

            _showTodayButton = value;
            _calendar.ShowTodayButton = value;
        }
    }

    [Category("Appearance")]
    [Description("Determines whether week numbers are displayed.")]
    [DefaultValue(false)]
    public bool ShowWeekNumbers
    {
        get => _showWeekNumbers;
        set
        {
            if (_showWeekNumbers == value)
                return;

            _showWeekNumbers = value;
            _calendar.ShowWeekNumbers = value;
        }
    }

    [Browsable(false)]
    public Windows_Forms_Custom_Month_Calendar_Control Calendar => _calendar;

    #endregion

    #region Events

    [Category("Property Changed")]
    [Description("Occurs when the selected date changes.")]
    public event EventHandler? ValueChanged;

    protected virtual void OnValueChanged(EventArgs e)
    {
        ValueChanged?.Invoke(this, e);
    }

    #endregion

    #region Calendar

    private void DropDownButton_Click(object? sender, EventArgs e)
    {
        ShowCalendar();
    }

    private void ShowCalendar()
    {
        if (_popup.Visible)
        {
            _popup.Close();
            return;
        }

        UpdateCalendarValue();
        Point location = PointToScreen(new Point(0, Height));
        _popup.Show(location);
    }

    private void Calendar_ValueChanged(object? sender, EventArgs e)
    {
        if (_internalUpdate)
            return;

        DateTime selectedDate = _calendar.Value.Date;

        if (selectedDate < _minDate || selectedDate > _maxDate)
        {
            return;
        }

        _value = selectedDate;
        UpdateText();
        OnValueChanged(EventArgs.Empty);
        _popup.Close();
    }

    private void UpdateCalendarValue()
    {
        if (!_calendar.IsHandleCreated)
            return;

        _internalUpdate = true;

        try
        {
            if (_value >= _minDate && _value <= _maxDate)
            {
                _calendar.Value = _value;
            }
        }
        finally
        {
            _internalUpdate = false;
        }
    }

    #endregion

    #region Text

    private void UpdateText()
    {
        if (_textBox == null)
            return;

        try
        {
            _textBox.Text = _value.ToString(_customFormat, _culture);
        }
        catch (FormatException)
        {
            _textBox.Text = _value.ToString("d", _culture);
        }
    }

    #endregion

    #region Keyboard

    private void TextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space || e.KeyCode == Keys.Down)
        {
            ShowCalendar();
            e.Handled = true;
            e.SuppressKeyPress = true;
        }
    }

    #endregion

    #region Layout

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        if (_dropDownButton != null)
        {
            _dropDownButton.Width = Math.Max(25, Height);
        }
    }

    #endregion
}