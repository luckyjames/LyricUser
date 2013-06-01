using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LyricUser
{
    /// <summary>
    /// Creates an abstraction of the lyrics in the file system, raises pertinent events and allows fast retrieval
    ///  and manipulation. Keeps its data consistent with the state of the files on disc by using FileSystemWatcher
    /// On construction performas a lazy
    /// </summary>
    class LyricsFileSystemModel : FileSystemWatcher
    {
        class Node
        {
            private string nodePath;
            public string NodePath
            {
                get
                {
                    return nodePath;
                }
            }
    
            public Node(string nodePath)
            {
                this.nodePath = nodePath;
            }
        }
    
        private readonly Node rootNode;

        public LyricsFileSystemModel(string rootPath)
        {
            this.rootNode = new Node(rootPath);
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
