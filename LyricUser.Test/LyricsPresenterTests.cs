
using NUnit.Framework;
using System;

namespace LyricUser.Test
{
    [TestFixture]
    class LyricsPresenterTests
    {
        const string relativePathToTestData = @"..\..\..\TestData\SimpleSong.xml";

        [TestCase]
        public void NullConstruction()
        {
            Assert.Catch<ArgumentNullException>(delegate { new LyricsPresenter(null); });
        }

        [TestCase]
        public void ValidDataPresentsLyrics()
        {
            // No Mocking library yet so..
            XmlLyricsFileParser xmlLyricsFileParser = new XmlLyricsFileParser(relativePathToTestData);

            LyricsPresenter lyricsPresenter = new LyricsPresenter(xmlLyricsFileParser);

            Assert.AreEqual("Simple\r\nSimple\r\nSimple\r\nSimple Song", lyricsPresenter.Lyrics);
        }

        [TestCase]
        public void ValidDataPresentsMetadata()
        {
            // No Mocking library yet so..
            XmlLyricsFileParser xmlLyricsFileParser = new XmlLyricsFileParser(relativePathToTestData);

            LyricsPresenter lyricsPresenter = new LyricsPresenter(xmlLyricsFileParser);

            Assert.AreEqual(5, lyricsPresenter.Metadata.Count);
        }
    }
}
