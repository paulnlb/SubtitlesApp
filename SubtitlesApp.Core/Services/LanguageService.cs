using SubtitlesApp.Core.Constants;
using SubtitlesApp.Core.Models;

namespace SubtitlesApp.Core.Services;

public class LanguageService
{
    private readonly List<Language> _languages =
    [
        new()
        {
            Id = 1,
            Code = LanguageCodes.Auto,
            NativeName = LanguageNativeNames.Auto,
            Name = "Auto",
        },
        new()
        {
            Id = 2,
            Code = LanguageCodes.Afrikaans,
            NativeName = LanguageNativeNames.Afrikaans,
            Name = "Afrikaans",
        },
        new()
        {
            Id = 3,
            Code = LanguageCodes.Arabic,
            NativeName = LanguageNativeNames.Arabic,
            Name = "Arabic",
        },
        new()
        {
            Id = 4,
            Code = LanguageCodes.Armenian,
            NativeName = LanguageNativeNames.Armenian,
            Name = "Armenian",
        },
        new()
        {
            Id = 5,
            Code = LanguageCodes.Azerbaijani,
            NativeName = LanguageNativeNames.Azerbaijani,
            Name = "Azerbaijani",
        },
        new()
        {
            Id = 6,
            Code = LanguageCodes.Belarusian,
            NativeName = LanguageNativeNames.Belarusian,
            Name = "Belarusian",
        },
        new()
        {
            Id = 7,
            Code = LanguageCodes.Bosnian,
            NativeName = LanguageNativeNames.Bosnian,
            Name = "Bosnian",
        },
        new()
        {
            Id = 8,
            Code = LanguageCodes.Bulgarian,
            NativeName = LanguageNativeNames.Bulgarian,
            Name = "Bulgarian",
        },
        new()
        {
            Id = 9,
            Code = LanguageCodes.Catalan,
            NativeName = LanguageNativeNames.Catalan,
            Name = "Catalan",
        },
        new()
        {
            Id = 10,
            Code = LanguageCodes.Chinese,
            NativeName = LanguageNativeNames.Chinese,
            Name = "Chinese",
        },
        new()
        {
            Id = 11,
            Code = LanguageCodes.Croatian,
            NativeName = LanguageNativeNames.Croatian,
            Name = "Croatian",
        },
        new()
        {
            Id = 12,
            Code = LanguageCodes.Czech,
            NativeName = LanguageNativeNames.Czech,
            Name = "Czech",
        },
        new()
        {
            Id = 13,
            Code = LanguageCodes.Danish,
            NativeName = LanguageNativeNames.Danish,
            Name = "Danish",
        },
        new()
        {
            Id = 14,
            Code = LanguageCodes.Dutch,
            NativeName = LanguageNativeNames.Dutch,
            Name = "Dutch",
        },
        new()
        {
            Id = 15,
            Code = LanguageCodes.English,
            NativeName = LanguageNativeNames.English,
            Name = "English",
        },
        new()
        {
            Id = 16,
            Code = LanguageCodes.Estonian,
            NativeName = LanguageNativeNames.Estonian,
            Name = "Estonian",
        },
        new()
        {
            Id = 17,
            Code = LanguageCodes.Finnish,
            NativeName = LanguageNativeNames.Finnish,
            Name = "Finnish",
        },
        new()
        {
            Id = 18,
            Code = LanguageCodes.French,
            NativeName = LanguageNativeNames.French,
            Name = "French",
        },
        new()
        {
            Id = 19,
            Code = LanguageCodes.Galician,
            NativeName = LanguageNativeNames.Galician,
            Name = "Galician",
        },
        new()
        {
            Id = 20,
            Code = LanguageCodes.German,
            NativeName = LanguageNativeNames.German,
            Name = "German",
        },
        new()
        {
            Id = 21,
            Code = LanguageCodes.Greek,
            NativeName = LanguageNativeNames.Greek,
            Name = "Greek",
        },
        new()
        {
            Id = 22,
            Code = LanguageCodes.Hebrew,
            NativeName = LanguageNativeNames.Hebrew,
            Name = "Hebrew",
        },
        new()
        {
            Id = 23,
            Code = LanguageCodes.Hindi,
            NativeName = LanguageNativeNames.Hindi,
            Name = "Hindi",
        },
        new()
        {
            Id = 24,
            Code = LanguageCodes.Hungarian,
            NativeName = LanguageNativeNames.Hungarian,
            Name = "Hungarian",
        },
        new()
        {
            Id = 25,
            Code = LanguageCodes.Icelandic,
            NativeName = LanguageNativeNames.Icelandic,
            Name = "Icelandic",
        },
        new()
        {
            Id = 26,
            Code = LanguageCodes.Indonesian,
            NativeName = LanguageNativeNames.Indonesian,
            Name = "Indonesian",
        },
        new()
        {
            Id = 27,
            Code = LanguageCodes.Italian,
            NativeName = LanguageNativeNames.Italian,
            Name = "Italian",
        },
        new()
        {
            Id = 28,
            Code = LanguageCodes.Japanese,
            NativeName = LanguageNativeNames.Japanese,
            Name = "Japanese",
        },
        new()
        {
            Id = 29,
            Code = LanguageCodes.Kannada,
            NativeName = LanguageNativeNames.Kannada,
            Name = "Kannada",
        },
        new()
        {
            Id = 30,
            Code = LanguageCodes.Kazakh,
            NativeName = LanguageNativeNames.Kazakh,
            Name = "Kazakh",
        },
        new()
        {
            Id = 31,
            Code = LanguageCodes.Korean,
            NativeName = LanguageNativeNames.Korean,
            Name = "Korean",
        },
        new()
        {
            Id = 32,
            Code = LanguageCodes.Latvian,
            NativeName = LanguageNativeNames.Latvian,
            Name = "Latvian",
        },
        new()
        {
            Id = 33,
            Code = LanguageCodes.Lithuanian,
            NativeName = LanguageNativeNames.Lithuanian,
            Name = "Lithuanian",
        },
        new()
        {
            Id = 34,
            Code = LanguageCodes.Macedonian,
            NativeName = LanguageNativeNames.Macedonian,
            Name = "Macedonian",
        },
        new()
        {
            Id = 35,
            Code = LanguageCodes.Malay,
            NativeName = LanguageNativeNames.Malay,
            Name = "Malay",
        },
        new()
        {
            Id = 36,
            Code = LanguageCodes.Marathi,
            NativeName = LanguageNativeNames.Marathi,
            Name = "Marathi",
        },
        new()
        {
            Id = 37,
            Code = LanguageCodes.Maori,
            NativeName = LanguageNativeNames.Maori,
            Name = "Maori",
        },
        new()
        {
            Id = 38,
            Code = LanguageCodes.Nepali,
            NativeName = LanguageNativeNames.Nepali,
            Name = "Nepali",
        },
        new()
        {
            Id = 39,
            Code = LanguageCodes.Norwegian,
            NativeName = LanguageNativeNames.Norwegian,
            Name = "Norwegian",
        },
        new()
        {
            Id = 40,
            Code = LanguageCodes.Persian,
            NativeName = LanguageNativeNames.Persian,
            Name = "Persian",
        },
        new()
        {
            Id = 41,
            Code = LanguageCodes.Polish,
            NativeName = LanguageNativeNames.Polish,
            Name = "Polish",
        },
        new()
        {
            Id = 42,
            Code = LanguageCodes.Portuguese,
            NativeName = LanguageNativeNames.Portuguese,
            Name = "Portuguese",
        },
        new()
        {
            Id = 43,
            Code = LanguageCodes.Romanian,
            NativeName = LanguageNativeNames.Romanian,
            Name = "Romanian",
        },
        new()
        {
            Id = 44,
            Code = LanguageCodes.Serbian,
            NativeName = LanguageNativeNames.Serbian,
            Name = "Serbian",
        },
        new()
        {
            Id = 45,
            Code = LanguageCodes.Slovak,
            NativeName = LanguageNativeNames.Slovak,
            Name = "Slovak",
        },
        new()
        {
            Id = 46,
            Code = LanguageCodes.Slovenian,
            NativeName = LanguageNativeNames.Slovenian,
            Name = "Slovenian",
        },
        new()
        {
            Id = 47,
            Code = LanguageCodes.Spanish,
            NativeName = LanguageNativeNames.Spanish,
            Name = "Spanish",
        },
        new()
        {
            Id = 48,
            Code = LanguageCodes.Swahili,
            NativeName = LanguageNativeNames.Swahili,
            Name = "Swahili",
        },
        new()
        {
            Id = 49,
            Code = LanguageCodes.Swedish,
            NativeName = LanguageNativeNames.Swedish,
            Name = "Swedish",
        },
        new()
        {
            Id = 50,
            Code = LanguageCodes.Tagalog,
            NativeName = LanguageNativeNames.Tagalog,
            Name = "Tagalog",
        },
        new()
        {
            Id = 51,
            Code = LanguageCodes.Tamil,
            NativeName = LanguageNativeNames.Tamil,
            Name = "Tamil",
        },
        new()
        {
            Id = 52,
            Code = LanguageCodes.Thai,
            NativeName = LanguageNativeNames.Thai,
            Name = "Thai",
        },
        new()
        {
            Id = 53,
            Code = LanguageCodes.Turkish,
            NativeName = LanguageNativeNames.Turkish,
            Name = "Turkish",
        },
        new()
        {
            Id = 54,
            Code = LanguageCodes.Ukrainian,
            NativeName = LanguageNativeNames.Ukrainian,
            Name = "Ukrainian",
        },
        new()
        {
            Id = 55,
            Code = LanguageCodes.Urdu,
            NativeName = LanguageNativeNames.Urdu,
            Name = "Urdu",
        },
        new()
        {
            Id = 56,
            Code = LanguageCodes.Vietnamese,
            NativeName = LanguageNativeNames.Vietnamese,
            Name = "Vietnamese",
        },
        new()
        {
            Id = 57,
            Code = LanguageCodes.Welsh,
            NativeName = LanguageNativeNames.Welsh,
            Name = "Welsh",
        },
    ];

    public List<Language> GetAllLanguages()
    {
        return _languages.ToList(); // copy to new list
    }

    public List<Language> GetLanguages(Func<Language, bool> predicate)
    {
        return _languages.Where(predicate).ToList(); // copy to new list
    }

    public Language GetDefaultLanguage()
    {
        return _languages[0];
    }

    public Language? GetLanguageById(int id)
    {
        return _languages.Find(l => l.Id == id);
    }

    public Language? GetLanguageByCode(string code)
    {
        return _languages.Find(l => l.Code == code);
    }
}
