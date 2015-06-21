using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Xml.Serialization;
using static System.Console;

namespace SourceTreeBookmarkCreator
{
    class Program
    {
        /// <summary>
        /// Finds .git repo's on disk and creates a SourceTree bookmarks file with bookmarkfolders and bookmark to each location
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            WriteLine("Please make sure SourceTree and press any key....");
            ReadKey();
            WriteLine("Working, this might take a minute...");

            var nodes = new List<TreeViewNode>();
            var outputPath = Path.Combine(Environment.GetEnvironmentVariable("LocalAppData"), @"Atlassian\SourceTree", "bookmarks.xml");

            try
            {
                //BACKUP EXISTION BOOKMARKS FILE
                if (File.Exists(outputPath))
                {
                    File.Move(outputPath, outputPath + "_backup_" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss"));
                }

                if (args.Any())
                {
                    foreach (var arg in args)
                    {
                        if (Directory.Exists(arg))
                            nodes.AddRange(WalkTheDirectoryTree(arg));
                    }
                }
                else
                {
                    var rootPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    nodes = WalkTheDirectoryTree(rootPath);
                }

                var writer = new XmlSerializer(nodes.GetType());
                using (var file = new StreamWriter(outputPath))
                {
                    writer.Serialize(file, nodes);
                    file.Close();
                }
            }
            catch (UnauthorizedAccessException uaex) when (!Debugger.IsAttached)
            {
                ForegroundColor = ConsoleColor.Red;
                WriteLine(uaex.Message);
                ReadKey();
            }
            catch (IOException ioex) when (!Debugger.IsAttached)
            {
                ForegroundColor = ConsoleColor.Red;
                WriteLine(ioex.Message);
                ReadKey();
            }
            catch (SecurityException sex) when (!Debugger.IsAttached)
            {
                ForegroundColor = ConsoleColor.Red;
                WriteLine(sex.Message);
                ReadKey();
            }

            ForegroundColor = ConsoleColor.Green;
            WriteLine("All Done!");
            ReadKey();
        }

        /// <summary>
        /// Creates a nested bookmark object when it comes upon a .git repo in a directory
        /// </summary>
        /// <param name="subdirectory"></param>
        /// <returns></returns>
        private static List<TreeViewNode> WalkTheDirectoryTree(string subdirectory)
        {
            var nodes = new List<TreeViewNode>();

            var tldGitFolders = Directory.GetDirectories(subdirectory, ".git", SearchOption.TopDirectoryOnly);
            if (tldGitFolders.Any())
            {
                var bookmarkNode = new BookmarkNode
                {
                    Name = new DirectoryInfo(subdirectory).Name,
                    Path = subdirectory,
                    IsLeaf = true
                };
                nodes.Add(bookmarkNode);
            }

            if (Directory.GetDirectories(subdirectory, ".git", SearchOption.AllDirectories).Count() == tldGitFolders.Count())
                return nodes;

            var folderNode = new BookmarkFolderNode
            {
                Name = new DirectoryInfo(subdirectory).Name,
                IsLeaf = false
            };

            var subDirectories = Directory.GetDirectories(subdirectory);
            foreach (var subDir in subDirectories)
            {
                folderNode.Children.AddRange(WalkTheDirectoryTree(subDir));
            }
            nodes.Add(folderNode);

            return nodes;
        }

        private static bool IsGitInDirectoryTree(string path)
        {
            return Directory.GetDirectories(path, ".git", SearchOption.AllDirectories).Any();
        }
    }
}
