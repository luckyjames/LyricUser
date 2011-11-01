
using NUnit.Framework;
using System;

namespace LyricUser.Test
{
    [TestFixture]
    public class SettingsTests
    {
        [TestCase]
        public void NullFails()
        {
            Assert.Catch<ArgumentNullException>( delegate { new XmlLyricsFileParser(null); } );
        }

        [TestCase]
        public void SimpleTest()
        {
            const string relativePathToTestData = @"..\..\..\TestData\SimpleSong.xml";
            XmlLyricsFileParser xmlLyricsFileParser = new XmlLyricsFileParser(relativePathToTestData);
            Assert.IsNotNullOrEmpty(xmlLyricsFileParser.Lyrics);
        }
    }
}
