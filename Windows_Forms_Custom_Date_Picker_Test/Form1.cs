using System.Globalization;

namespace Windows_Forms_Custom_Date_Picker_Test;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
    }

    private void LanguageRadioButton_CheckedChanged(object? sender, EventArgs e)
    {
        if (sender is RadioButton rb && rb.Checked)
        {
            string langCode = "en";

            if (rb == ArabicLanguageRadioButton)
                langCode = "ar";
            else if (rb == BengaliLanguageRadioButton)
                langCode = "bn";
            else if (rb == DeutschDeutschLanguageRadioButton)
                langCode = "de";
            else if (rb == GreekLanguageRadioButton)
                langCode = "el";
            else if (rb == EnglishLanguageRadioButton)
                langCode = "en";
            else if (rb == SpanishLanguageRadioButton)
                langCode = "es";
            else if (rb == FrenchLanguageRadioButton)
                langCode = "fr";
            else if (rb == HindiLanguageRadioButton)
                langCode = "hi";
            else if (rb == IndonesianLanguageRadioButton)
                langCode = "id";
            else if (rb == ItalianLanguageRadioButton)
                langCode = "it";
            else if (rb == JapaneseLanguageRadioButton)
                langCode = "ja";
            else if (rb == KoreanLanguageRadioButton)
                langCode = "ko";
            else if (rb == DutchLanguageRadioButton)
                langCode = "nl";
            else if (rb == PolishLanguageRadioButton)
                langCode = "pl";
            else if (rb == PortugueseLanguageRadioButton)
                langCode = "pt";
            else if (rb == RussianLanguageRadioButton)
                langCode = "ru";
            else if (rb == SrpskiCyrlLanguageRadioButton)
                langCode = "sr-Cyrl";
            else if (rb == SrpskiLatnLanguageRadioButton16)
                langCode = "sr-Latn";
            else if (rb == SwahiliLanguageRadioButton)
                langCode = "sw";
            else if (rb == TurkishLanguageRadioButton)
                langCode = "tr";
            else if (rb == VietnameseLanguageRadioButton)
                langCode = "vi";
            else if (rb == ChineseLanguageRadioButton)
                langCode = "zh";

            ApplyLanguageSettings(langCode);
        }
    }

    private void ApplyLanguageSettings(string langCode)
    {
        // OVDE se automatski postavlja kultura na vaš picker:
        windows_Forms_Custom_Date_Picker_Control1.Culture = new CultureInfo(langCode);
    }

    private void EnglishButton_Click(object sender, EventArgs e)
    {
        windows_Forms_Custom_Date_Picker_Control1.Culture = new CultureInfo("en-US");
    }

    private void SerbianButton_Click(object sender, EventArgs e)
    {
        windows_Forms_Custom_Date_Picker_Control1.Culture = new CultureInfo("sr-Latn-RS");
    }
}