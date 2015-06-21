using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SourceTreeBookmarkCreator
{
    [XmlInclude(typeof(BookmarkNode))]
    [XmlInclude(typeof(BookmarkFolderNode))]
    [Serializable]
    public class TreeViewNode
    {
        public int Level { get; set; }
        public bool IsExpanded { get; set; }
        public bool IsLeaf { get; set; } = true;
        public string Name { get; set; }

        public List<TreeViewNode> Children { get; } = new List<TreeViewNode>();
    }

    public class BookmarkNode : TreeViewNode
    {
        public string RepoType { get; set; } = "Git";
        public string Path { get; set; }

        public override string ToString() => $"Name: {Name} Path: {Path}";
        
    }

    public class BookmarkFolderNode : TreeViewNode
    {
        public override string ToString() => $"FolderName: {Name}";
    }
}
