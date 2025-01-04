﻿using SubtitlesApp.Core.Constants;
using SubtitlesApp.Core.Models;

namespace SubtitlesApp.Core.Services;

public class LanguageService
{
    private readonly List<Language> _languages =
    [
        new() { Id = 1, Code = LanguageCodes.Auto, Name = LanguageNames.Auto, EnglishName = "Auto" },
        new() { Id = 2, Code = LanguageCodes.Afrikaans, Name = LanguageNames.Afrikaans, EnglishName = "Afrikaans" },
        new() { Id = 3, Code = LanguageCodes.Arabic, Name = LanguageNames.Arabic, EnglishName = "Arabic" },
        new() { Id = 4, Code = LanguageCodes.Armenian, Name = LanguageNames.Armenian, EnglishName = "Armenian" },
        new() { Id = 5, Code = LanguageCodes.Azerbaijani, Name = LanguageNames.Azerbaijani, EnglishName = "Azerbaijani" },
        new() { Id = 6, Code = LanguageCodes.Belarusian, Name = LanguageNames.Belarusian, EnglishName = "Belarusian" },
        new() { Id = 7, Code = LanguageCodes.Bosnian, Name = LanguageNames.Bosnian, EnglishName = "Bosnian" },
        new() { Id = 8, Code = LanguageCodes.Bulgarian, Name = LanguageNames.Bulgarian, EnglishName = "Bulgarian" },
        new() { Id = 9, Code = LanguageCodes.Catalan, Name = LanguageNames.Catalan, EnglishName = "Catalan" },
        new() { Id = 10, Code = LanguageCodes.Chinese, Name = LanguageNames.Chinese, EnglishName = "Chinese" },
        new() { Id = 11, Code = LanguageCodes.Croatian, Name = LanguageNames.Croatian, EnglishName = "Croatian" },
        new() { Id = 12, Code = LanguageCodes.Czech, Name = LanguageNames.Czech, EnglishName = "Czech" },
        new() { Id = 13, Code = LanguageCodes.Danish, Name = LanguageNames.Danish, EnglishName = "Danish" },
        new() { Id = 14, Code = LanguageCodes.Dutch, Name = LanguageNames.Dutch, EnglishName = "Dutch" },
        new() { Id = 15, Code = LanguageCodes.English, Name = LanguageNames.English, EnglishName = "English" },
        new() { Id = 16, Code = LanguageCodes.Estonian, Name = LanguageNames.Estonian, EnglishName = "Estonian" },
        new() { Id = 17, Code = LanguageCodes.Finnish, Name = LanguageNames.Finnish, EnglishName = "Finnish" },
        new() { Id = 18, Code = LanguageCodes.French, Name = LanguageNames.French, EnglishName = "French" },
        new() { Id = 19, Code = LanguageCodes.Galician, Name = LanguageNames.Galician, EnglishName = "Galician" },
        new() { Id = 20, Code = LanguageCodes.German, Name = LanguageNames.German, EnglishName = "German" },
        new() { Id = 21, Code = LanguageCodes.Greek, Name = LanguageNames.Greek, EnglishName = "Greek" },
        new() { Id = 22, Code = LanguageCodes.Hebrew, Name = LanguageNames.Hebrew, EnglishName = "Hebrew" },
        new() { Id = 23, Code = LanguageCodes.Hindi, Name = LanguageNames.Hindi, EnglishName = "Hindi" },
        new() { Id = 24, Code = LanguageCodes.Hungarian, Name = LanguageNames.Hungarian, EnglishName = "Hungarian" },
        new() { Id = 25, Code = LanguageCodes.Icelandic, Name = LanguageNames.Icelandic, EnglishName = "Icelandic" },
        new() { Id = 26, Code = LanguageCodes.Indonesian, Name = LanguageNames.Indonesian, EnglishName = "Indonesian" },
        new() { Id = 27, Code = LanguageCodes.Italian, Name = LanguageNames.Italian, EnglishName = "Italian" },
        new() { Id = 28, Code = LanguageCodes.Japanese, Name = LanguageNames.Japanese, EnglishName = "Japanese" },
        new() { Id = 29, Code = LanguageCodes.Kannada, Name = LanguageNames.Kannada, EnglishName = "Kannada" },
        new() { Id = 30, Code = LanguageCodes.Kazakh, Name = LanguageNames.Kazakh, EnglishName = "Kazakh" },
        new() { Id = 31, Code = LanguageCodes.Korean, Name = LanguageNames.Korean, EnglishName = "Korean" },
        new() { Id = 32, Code = LanguageCodes.Latvian, Name = LanguageNames.Latvian, EnglishName = "Latvian" },
        new() { Id = 33, Code = LanguageCodes.Lithuanian, Name = LanguageNames.Lithuanian, EnglishName = "Lithuanian" },
        new() { Id = 34, Code = LanguageCodes.Macedonian, Name = LanguageNames.Macedonian, EnglishName = "Macedonian" },
        new() { Id = 35, Code = LanguageCodes.Malay, Name = LanguageNames.Malay, EnglishName = "Malay" },
        new() { Id = 36, Code = LanguageCodes.Marathi, Name = LanguageNames.Marathi, EnglishName = "Marathi" },
        new() { Id = 37, Code = LanguageCodes.Maori, Name = LanguageNames.Maori, EnglishName = "Maori" },
        new() { Id = 38, Code = LanguageCodes.Nepali, Name = LanguageNames.Nepali, EnglishName = "Nepali" },
        new() { Id = 39, Code = LanguageCodes.Norwegian, Name = LanguageNames.Norwegian, EnglishName = "Norwegian" },
        new() { Id = 40, Code = LanguageCodes.Persian, Name = LanguageNames.Persian, EnglishName = "Persian" },
        new() { Id = 41, Code = LanguageCodes.Polish, Name = LanguageNames.Polish, EnglishName = "Polish" },
        new() { Id = 42, Code = LanguageCodes.Portuguese, Name = LanguageNames.Portuguese, EnglishName = "Portuguese" },
        new() { Id = 43, Code = LanguageCodes.Romanian, Name = LanguageNames.Romanian, EnglishName = "Romanian" },
        new() { Id = 44, Code = LanguageCodes.Serbian, Name = LanguageNames.Serbian, EnglishName = "Serbian" },
        new() { Id = 45, Code = LanguageCodes.Slovak, Name = LanguageNames.Slovak, EnglishName = "Slovak" },
        new() { Id = 46, Code = LanguageCodes.Slovenian, Name = LanguageNames.Slovenian, EnglishName = "Slovenian" },
        new() { Id = 47, Code = LanguageCodes.Spanish, Name = LanguageNames.Spanish, EnglishName = "Spanish" },
        new() { Id = 48, Code = LanguageCodes.Swahili, Name = LanguageNames.Swahili, EnglishName = "Swahili" },
        new() { Id = 49, Code = LanguageCodes.Swedish, Name = LanguageNames.Swedish, EnglishName = "Swedish" },
        new() { Id = 50, Code = LanguageCodes.Tagalog, Name = LanguageNames.Tagalog, EnglishName = "Tagalog" },
        new() { Id = 51, Code = LanguageCodes.Tamil, Name = LanguageNames.Tamil, EnglishName = "Tamil" },
        new() { Id = 52, Code = LanguageCodes.Thai, Name = LanguageNames.Thai, EnglishName = "Thai" },
        new() { Id = 53, Code = LanguageCodes.Turkish, Name = LanguageNames.Turkish, EnglishName = "Turkish" },
        new() { Id = 54, Code = LanguageCodes.Ukrainian, Name = LanguageNames.Ukrainian, EnglishName = "Ukrainian" },
        new() { Id = 55, Code = LanguageCodes.Urdu, Name = LanguageNames.Urdu, EnglishName = "Urdu" },
        new() { Id = 56, Code = LanguageCodes.Vietnamese, Name = LanguageNames.Vietnamese, EnglishName = "Vietnamese" },
        new() { Id = 57, Code = LanguageCodes.Welsh, Name = LanguageNames.Welsh, EnglishName = "Welsh" },
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
