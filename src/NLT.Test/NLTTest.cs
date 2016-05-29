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
        [TestInitialize]
        public void Setup()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("en");
            NLT.Default = new NLT(new FileInfo("translations1.csv"));
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
            string fileName;
            do
            {
                fileName = string.Format("test.{0}.csv", Guid.NewGuid());
            }
            while (File.Exists(fileName));

            NLT nlt = new NLT(fileName);
            Assert.AreEqual(1, nlt.AvailableLanguages.Length);
            Assert.AreEqual(CultureInfo.CurrentCulture, nlt.CurrentCulture);

            string[] words = new string[] { "Hello", "Hi", "Table", "Window" };
            foreach(string word in words) { nlt.Translate(word); }
            nlt.UpdateTranslationsFile();
            Assert.IsTrue(File.Exists(fileName), "Translation file doesn't exists");
            string[][] csv = CSV.ReadFile(fileName).ToArray();
            Assert.AreEqual(CultureInfo.CurrentCulture.Name, csv[0][0]);
            Assert.IsTrue(words
                .OrderBy(each => each)
                .SequenceEqual(csv
                    .Where((each, index) => index > 0)
                    .Select(each => each[0])
                    .OrderBy(each => each)),
                "Translations file doesn't contain all the attempted translations");
        }
    }
}
