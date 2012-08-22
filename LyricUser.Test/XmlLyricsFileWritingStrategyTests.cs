
using NUnit.Framework;
using System;
using System.Xml;
using System.Collections.Generic;

namespace LyricUser.Test
{
    [TestFixture]
    public class XmlLyricsFileWritingStrategyTests
    {
        const string relativePathToTestData = @"..\..\TestData\SimpleSong.xml";

        [TestCase]
        public void SimpleCheck()
        {
            var dictionary = new Dictionary<string, string>();
            dictionary["yes"] = "no";
            string result = XmlLyricsFileWritingStrategy.WriteToString(dictionary);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("<yes>no</yes>"));
        }
    }
}
