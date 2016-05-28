using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CSVUtils;
using System.Globalization;

namespace NaturalLanguageTranslator
{
    public class NLT
    {
        private static NLT defaultInstance;

        public static NLT Default {
            get { return defaultInstance; }
            set { defaultInstance = value; }
        }

        public event Action<NLT> LanguageChanged = (sender) => { };

        private FileInfo translationsFile;
        private char separator;

        private string[] availableLanguages = null;
        private string currentLanguage;
        private Dictionary<string, string> translations;

        public NLT(FileInfo translationsFile, char separator = ',')
        {
            this.translationsFile = translationsFile;
            this.separator = separator;

            foreach (string[] row in CSV.ReadFile(translationsFile, separator))
            {
                availableLanguages = row;
                break; // We only care about the first row
            }
            CurrentCulture = CultureInfo.CurrentCulture;
        }

        public string CurrentLanguage
        {
            get { return currentLanguage; }
            set
            {
                if (availableLanguages.Contains(value))
                {
                    currentLanguage = value;
                    UpdateTranslations();
                    LanguageChanged(this);
                }
            }
        }

        public CultureInfo CurrentCulture
        {
            get { return GetCulture(CurrentLanguage); }
            set { CurrentLanguage = value.Name; }
        }

        public string[] AvailableLanguages { get { return availableLanguages; } }

        public CultureInfo[] AvailableCultures
        {
            get
            {
                return AvailableLanguages
                    .Select(each => GetCulture(each))
                    .ToArray();
            }
        }

        public string Translate(string original)
        {
            if (translations.ContainsKey(original))
            {
                return translations[original];
            }
            else
            {
                return original;
            }
        }

        private void UpdateTranslations()
        {
            translations = new Dictionary<string, string>();
            int langIndex = Array.IndexOf(availableLanguages, currentLanguage);
            if (langIndex == 0) return; // Special case: "official" language
            int rowIndex = 0;
            foreach(string[] row in CSV.ReadFile(translationsFile, separator))
            {
                if (rowIndex > 0)
                {
                    translations[row[0]] = row[langIndex];
                }
                rowIndex++;
            }
        }
        
        private CultureInfo GetCulture(string language)
        {
            return CultureInfo
                .GetCultures(CultureTypes.AllCultures)
                .FirstOrDefault(c => language.Equals(c.Name));
        }
    }
}
