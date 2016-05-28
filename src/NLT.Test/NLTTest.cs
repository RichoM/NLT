using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NaturalLanguageTranslator;
using System.IO;
using System.Globalization;
using System.Linq;

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
    }
}
