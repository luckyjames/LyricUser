using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LyricUser
{
    class LyricsFileSystemNode
    {
        private string nodePath;
        public string NodePath
        {
            get
            {
                return nodePath;
            }
        }

        public LyricsFileSystemNode(string nodePath)
        {
            this.nodePath = nodePath;
        }
    }

    class LyricsFileSystem : FileSystemWatcher
    {
        private readonly LyricsFileSystemNode rootNode;

        public LyricsFileSystem(string rootPath)
        {
            this.rootNode = new LyricsFileSystemNode(rootPath);
            this.IncludeSubdirectories = true;
            this.Filter = "*.xml";
            this.Changed += new FileSystemEventHandler(LyricsFileSystem_Changed);
            this.Created += new FileSystemEventHandler(LyricsFileSystem_Changed);
            this.Deleted += new FileSystemEventHandler(LyricsFileSystem_Changed);
            this.Renamed += new RenamedEventHandler(LyricsFileSystem_Renamed);
        }

        void LyricsFileSystem_Renamed(object sender, RenamedEventArgs e)
        {
            throw new NotImplementedException(
                e.ChangeType.ToString() + " :" + e.OldFullPath + " -> " + e.FullPath);
        }

        void LyricsFileSystem_Changed(object sender, FileSystemEventArgs e)
        {
            throw new NotImplementedException(
                e.ChangeType.ToString() + " :" + e.FullPath);
        }
    }
}
