﻿
using NUnit.Framework;
using System;
using System.Xml;

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
        const string relativePathToTestData = @"..\..\TestData\SimpleSong.xml";

        [TestCase]
        public void NullFails()
        {
            Assert.Catch<ArgumentNullException>( delegate { new XmlLyricsFileParsingStrategy(null); } );
        }

        [TestCase]
        public void SimpleTest()
        {
            XmlLyricsFileParsingStrategy xmlLyricsFileParser = new XmlLyricsFileParsingStrategy(relativePathToTestData);
            Assert.IsNotNull(xmlLyricsFileParser.DataPairs);
            Assert.AreEqual(6, xmlLyricsFileParser.DataPairs.Count);

            Assert.AreEqual("A Simple Song", xmlLyricsFileParser.DataPairs["title"]);
            Assert.AreEqual("Simple Artist", xmlLyricsFileParser.DataPairs["artist"]);
            Assert.AreEqual("0", xmlLyricsFileParser.DataPairs["capo"]);
            // etc..
        }

        [TestCase]
        public void GetFavourite()
        {
            bool favouriteValue = XmlLyricsFileParsingStrategy.ReadValue<bool>(relativePathToTestData, "favourite");
            Assert.AreEqual(true, favouriteValue);
        }
    }
}