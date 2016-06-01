using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CSVUtils;
using System.Globalization;
using System.Text.RegularExpressions;

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
        private Dictionary<string, int> used = new Dictionary<string, int>();

        public NLT(string fileName, char separator = ',') : this(new FileInfo(fileName), separator)
        { }

        public NLT(FileInfo translationsFile, char separator = ',')
        {
            this.translationsFile = translationsFile;
            this.separator = separator;

            InitializeAvailableLanguages();
            InitializeCurrentLanguage();
        }

        private void InitializeAvailableLanguages()
        {
            if (translationsFile.Exists)
            {
                foreach (string[] row in CSV.ReadFile(translationsFile, separator))
                {
                    availableLanguages = row;
                    break; // We only care about the first row
                }
            }
            else
            {
                // The file doesn't exists, use the default culture as available language
                availableLanguages = new string[] { CultureInfo.CurrentCulture.TwoLetterISOLanguageName };
            }
        }

        private void InitializeCurrentLanguage()
        {
            // We try to use the default culture
            CurrentCulture = CultureInfo.CurrentCulture;
            // But if we can't we just use the first available language
            if (string.IsNullOrEmpty(CurrentLanguage))
            {
                CurrentLanguage = availableLanguages.FirstOrDefault();
            }
        }

        public string CurrentLanguage
        {
            get { return currentLanguage; }
            set
            {
                if (availableLanguages.Contains(value)
                    && !string.Equals(currentLanguage, value))
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
            set { CurrentLanguage = GetLanguage(value); }
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
            RegisterUsage(original);
            if (translations.ContainsKey(original))
            {
                return translations[original];
            }
            else
            {
                return original;
            }
        }

        public void UpdateTranslationsFile()
        {
            string[] sortedUsed = used.OrderBy(each => each.Value).Select(each => each.Key).ToArray();
            string[] sortedExisting = translations.Keys.Except(sortedUsed).ToArray();
            string[] sortedAll = sortedUsed.Concat(sortedExisting).ToArray();
            string[,] updatedTranslations = new string[sortedAll.Length + 1, availableLanguages.Length];
            
            NLT nlt = new NLT(translationsFile, separator);
            for (int j = 0; j < availableLanguages.Length; j++)
            {
                string language = availableLanguages[j];
                nlt.CurrentLanguage = language;
                updatedTranslations[0, j] = language;
                for (int i = 0; i < sortedAll.Length; i++)
                {
                    updatedTranslations[i + 1, j] = nlt.Translate(sortedAll[i]);
                }
            }
            
            CSV.WriteFile(updatedTranslations.ToEnumerable(), translationsFile, separator);
        }
        
        private void RegisterUsage(string text)
        {
            if (!used.ContainsKey(text))
            {
                used[text] = 1;
            }
            else
            {
                used[text]++;
            }
        }

        private void UpdateTranslations()
        {
            translations = new Dictionary<string, string>();
            if (!translationsFile.Exists) return;
            int langIndex = Array.IndexOf(availableLanguages, currentLanguage);
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

        private string GetLanguage(CultureInfo culture)
        {
            string candidate = availableLanguages
                .Where(lang => Regex.IsMatch(culture.Name, lang))
                .OrderByDescending(lang => lang.Length)
                .FirstOrDefault();
            return candidate ?? availableLanguages.FirstOrDefault();
        }
    }
}
