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
            try
            {
                foreach (string[] row in CSV.ReadFile(translationsFile, separator))
                {
                    availableLanguages = row;
                    break; // We only care about the first row
                }
            }
            catch
            {
                // Something happened, use the default culture as available language
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
            Dictionary<string, List<string>> updatedTranslations = new Dictionary<string, List<string>>();
            NLT nlt = new NLT(translationsFile, separator);
            foreach (string language in availableLanguages)
            {
                nlt.CurrentLanguage = language;
                List<string> languageTranslations = new List<string>();
                foreach (string text in sortedUsed)
                {
                    languageTranslations.Add(nlt.Translate(text));
                }
                updatedTranslations[language] = languageTranslations;
            }
            
            using (StreamWriter writer = new StreamWriter(translationsFile.FullName, false))
            {
                // Write header
                for (int i = 0; i < availableLanguages.Length; i++)
                {
                    if (i > 0)
                    {
                        writer.Write(separator);
                    }
                    writer.Write(availableLanguages[i]);
                }
                writer.WriteLine();

                // Now write the translations
                for(int i = 0; i < sortedUsed.Length; i++)
                {
                    for (int j = 0; j < availableLanguages.Length; j++)
                    {
                        if (j > 0)
                        {
                            writer.Write(separator);
                        }
                        string lang = availableLanguages[j];
                        string text = updatedTranslations[lang][i];
                        text = text.Replace("\"", "\"\""); // Escape quotes, if any
                        bool enclose = text.Contains(separator)
                            || text.Contains('"')
                            || text.Contains('\n')
                            || text.Contains('\r');
                        if (enclose) { writer.Write('"'); }
                        writer.Write(text);
                        if (enclose) { writer.Write('"'); }
                    }
                    writer.WriteLine();
                }
                writer.Flush();
            }
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
