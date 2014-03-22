
using NUnit.Framework;
using System;
using System.Xml;
using System.Collections.Generic;

namespace LyricUser.Test
{
    /// <summary>
    /// A test fisture for XmlLyricsFileParsingStrategy
    /// </summary>
    /// <remarks>
    /// Note that these are not unit tests; they are system tests
    /// </remarks>
    [TestFixture]
    public class XmlLyricsFileParsingStrategyTests
    {
       const string relativePathToTestData = @"..\..\TestData\Simple Artist\SimpleSong.xml";

        [TestCase]
        public void NullFails()
        {
            Assert.Catch<ArgumentNullException>( delegate { new XmlLyricsFileParsingStrategy(null); } );
        }

        [TestCase]
        public void BruteForce()
        {
            IDictionary<string, string> results = XmlLyricsFileParsingStrategy.BruteForce(relativePathToTestData);
            Assert.IsNotNull(results);
            Assert.AreEqual(results.Count, 6);
        }

        [TestCase]
        public void SimpleTest()
        {
            XmlLyricsFileParsingStrategy xmlLyricsFileParser = new XmlLyricsFileParsingStrategy(relativePathToTestData);
            IDictionary<string, string> results = xmlLyricsFileParser.ReadAll();
            Assert.IsNotNull(results);
            Assert.AreEqual(6, results.Count);

            Assert.AreEqual("A Simple Song", results["title"]);
            Assert.AreEqual("Simple Artist", results["artist"]);
            Assert.AreEqual("0", results["capo"]);
            // etc..
        }

        [TestCase]
        public void GetFavourite()
        {
            XmlLyricsFileParsingStrategy xmlLyricsFileParsingStrategy = new XmlLyricsFileParsingStrategy(relativePathToTestData);
            bool favouriteValue = xmlLyricsFileParsingStrategy.ReadToFirstValue<bool>("favourite");
            Assert.AreEqual(true, favouriteValue);
        }
    }
}
