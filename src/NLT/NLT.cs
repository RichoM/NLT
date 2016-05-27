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
                if (availableLanguages == null)
                {
                    availableLanguages = row;
                    break;
                }
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
            get
            {
                return CultureInfo.GetCultures(CultureTypes.AllCultures)
                    .FirstOrDefault(c => CurrentLanguage.Equals(c.Name));
            }
            set { CurrentLanguage = value.Name; }
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
    }
}
