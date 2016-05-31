using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NaturalLanguageTranslator;
using System.IO;
using System.Globalization;
using System.Linq;
using CSVUtils;

namespace NLTTest
{
    [TestClass]
    public class NLTTest
    {
        public const string TRANSLATIONS = "translations1.csv";

        [TestInitialize]
        public void Setup()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("en");
            NLT.Default = new NLT(new FileInfo(TRANSLATIONS));
        }

        [TestMethod]
        public void CurrentLanguageIsConfiguredCorrectly()
        {
            Assert.AreEqual("en", NLT.Default.CurrentLanguage);
        }

        [TestMethod]
        public void ChangingCurrentLanguageWorksAsExpected()
        {
            NLT.Default.CurrentLanguage = "es";
            Assert.AreEqual("Hola", "Hello".Translated());
        }

        [TestMethod]
        public void RetrievingAvailableLanguagesWorks()
        {
            Assert.IsTrue(NLT.Default.AvailableLanguages.SequenceEqual(new string[] { "en", "es" }));
        }

        [TestMethod]
        public void CurrentCultureIsConfiguredCorrectly()
        {
            Assert.AreEqual(CultureInfo.GetCultureInfo("en"), NLT.Default.CurrentCulture);
        }

        [TestMethod]
        public void ChangingCurrentCultureWorksAsExpected()
        {
            NLT.Default.CurrentCulture = CultureInfo.GetCultureInfo("es");
            Assert.AreEqual("Hola", "Hello".Translated());
        }

        [TestMethod]
        public void RetrievingAvailableCulturesWorks()
        {
            Assert.IsTrue(NLT.Default.AvailableCultures
                .SequenceEqual(new CultureInfo[]
                {
                    CultureInfo.GetCultureInfo("en"),
                    CultureInfo.GetCultureInfo("es")
                }));
        }

        [TestMethod]
        public void ItShouldUseTheDefaultLanguageIfTheThreadCultureIsNotAvailable()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultures(CultureTypes.AllCultures).FirstOrDefault();

            Assert.AreEqual(CultureInfo.GetCultureInfo("en"), NLT.Default.CurrentCulture);
            Assert.AreEqual("en", NLT.Default.CurrentLanguage);
        }

        [TestMethod]
        public void NLTCanAutomaticallyCreateTheTranslationsFileIfInstructed()
        {
            string fileName = RandomFileName();
            try
            {
                NLT nlt = new NLT(fileName);
                Assert.AreEqual(1, nlt.AvailableLanguages.Length);
                Assert.AreEqual(CultureInfo.CurrentCulture, nlt.CurrentCulture);

                string[] words = new string[] { "Hello", "Hi", "Table", "Window" };
                foreach (string word in words) { nlt.Translate(word); }
                nlt.UpdateTranslationsFile();
                Assert.IsTrue(File.Exists(fileName), "Translation file doesn't exists");
                string[][] csv = CSV.ReadFile(fileName).ToJaggedArray();
                Assert.AreEqual(CultureInfo.CurrentCulture.Name, csv[0][0]);
                Assert.IsTrue(words
                    .OrderBy(each => each)
                    .SequenceEqual(csv
                        .Where((each, index) => index > 0)
                        .Select(each => each[0])
                        .OrderBy(each => each)),
                    "Translations file doesn't contain all the attempted translations");
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        [TestMethod]
        public void NLTCanUpdateTheTranslationsFileKeepingTheOldData()
        {
            string fileName = RandomFileName();
            try
            {
                File.Copy(TRANSLATIONS, fileName);
                NLT nlt = new NLT(fileName);

                string[] words = new string[] { "Hello", "Hi", "Table", "Window" };
                foreach (string word in words) { nlt.Translate(word); }
                nlt.UpdateTranslationsFile();
                string[][] csv = CSV.ReadFile(fileName).ToJaggedArray();

                Assert.IsTrue(csv.All(row => row.Length == 2));
                Assert.IsTrue(csv.Any(row => "English".Equals(row[0]) && "Inglés".Equals(row[1])));
                Assert.IsTrue(words.All(word => csv.Select(row => row[0]).Contains(word)));
                Assert.AreEqual(1, csv.Select(row => row[0]).Count(word => "Hello".Equals(word)));
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        private string RandomFileName()
        {
            string fileName;
            do
            {
                fileName = string.Format("test.{0}.csv", Guid.NewGuid());
            }
            while (File.Exists(fileName));
            return fileName;
        }
    }
}
