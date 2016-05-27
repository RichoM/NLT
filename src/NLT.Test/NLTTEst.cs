using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NaturalLanguageTranslator;
using System.IO;
using System.Globalization;

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
    }
}
