#region License
/*
Copyright (c) 1999 Gennadii Donchyts
All rights reserved

This library is free software; you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published
by the Free Software Foundation; either version 2.1 of the License, or
(at your option) any later version.

This library is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
or FITNESS FOR A PARTICULAR PURPOSE.
See the GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with this library; If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;

using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Serialization;

namespace xml2ikvmc
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!CheckUsage(args))
            {
                return;
            }

            string sourceXmlFileName = GenerateDependencyXml(args[0]);

            string ikvmsScriptFileName = args[1];
            string dotFileName = ikvmsScriptFileName + ".dot";

            XmlSerializer serializer = new XmlSerializer(typeof(JarAnalyzer));
            StreamReader srcReader = new StreamReader(sourceXmlFileName);
            JarAnalyzer jarAnalyzerResults = (JarAnalyzer)serializer.Deserialize(srcReader);
            srcReader.Close();

            BidirectionalGraph<string, Edge<string>> g = GenerateJarDependencyGraph(jarAnalyzerResults);
            GenerateIkvmcRunScript(g, ikvmsScriptFileName);
            GenerateDotFile(g, dotFileName);
    
            // File.Delete(sourceXmlFileName); // cleanup temporary xml file
        }

        private static string GenerateDependencyXml(string inputFolderPath)
        {
            string dependencyFilePath = Path.GetTempFileName();

            com.kirkk.analyzer.textui.XMLUISummary analyzer = new com.kirkk.analyzer.textui.XMLUISummary();
            java.io.File inputFolder = new java.io.File(inputFolderPath);
            java.io.File outputFile = new java.io.File(dependencyFilePath);
            analyzer.createSummary(inputFolder, outputFile);

            return dependencyFilePath;
        }

        private static BidirectionalGraph<string, Edge<string>> GenerateJarDependencyGraph(JarAnalyzer jars)
        {
            BidirectionalGraph<string, Edge<string>> g = new BidirectionalGraph<string, Edge<string>>(true);
            foreach(Jar jar in jars.Jars)
            {
                g.AddVertex(jar.name);
            }

            foreach (Jar jar in jars.Jars)
            {
                if (jar.Summary.OutgoingDependencies.Jar != null)
                {
                    foreach (Jar dstJar in jar.Summary.OutgoingDependencies.Jar)
                    {
                        bool exist = false;
                        foreach (IEdge<string> edge in g.InEdges(dstJar.Text[0]))
                        {
                            if (edge.Source == jar.name)
                            {
                                exist = true;
                            }
                        }

                        if (!exist)
                        {
                            g.AddEdge(new Edge<string>(dstJar.Text[0], jar.name));
                        }
                        else
                        {
                            Trace.WriteLine("Warning: loop detected, skipping dependency " + dstJar.Text[0] + " -> " + jar.name);
                        }

                        /* C# 3.5
                        if (!g.InEdges(dstJar.Text[0]).Any(v => v.Source == jar.name))
                        {
                            g.AddEdge(new Edge<string>(dstJar.Text[0], jar.name));
                        }
                        else
                        {
                            Trace.WriteLine("Warning: loop detected, skipping dependency " + dstJar.Text[0] + " -> " + jar.name);
                        }
                         */
                    }
                }
            }

            return g;
        }

        /// <summary>
        /// Generate file containing command lines to run ikvmc including names of referenced dlls.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="path"></param>
        private static void GenerateIkvmcRunScript(BidirectionalGraph<string, Edge<string>> g, string path)
        {
            StreamWriter sw = new StreamWriter(path);
            foreach (string vertex in AlgoUtility.TopologicalSort<string, Edge<string>>(g))
            {
                /* C# 3.5
                IEnumerable<string> names = g.InEdges(vertex).Select<Edge<string>, string>(item => item.Source);
                 */

                IList<string> names = new List<string>();
                foreach (IEdge<string> edge in g.InEdges(vertex))
                {
                    names.Add(edge.Source);
                }
                
                string references = "";
                foreach(string name in names)
                {
                    references += " -r:" + name.Replace(".jar", ".dll");
                }
                string commandLine = "ikvmc " + vertex + " -target:library" + references;
                sw.WriteLine(commandLine);
            }
            sw.Close();
        }


        private static void GenerateDotFile(BidirectionalGraph<string, Edge<string>> g, string path)
        {
            StreamWriter sw = new StreamWriter(path);
            sw.WriteLine("digraph G {");
            foreach (Edge<string> e in g.Edges)
            {
                sw.WriteLine("\"" + e.Target + "\" -> \"" + e.Source + "\";");
            }
            sw.WriteLine("}");
            sw.Close();
        }

        private static bool CheckUsage(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Error: number of arguments should be 2");
                PrintUsage();
                
                return false;
            }

            if (!Directory.Exists(args[0]))
            {
                Console.WriteLine("Error: input directory \"" + args[0] + "\" does not exist");
                PrintUsage();

                return false;
            }

            return true;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: jar2ikvmc <input folder containing jar files> <output script file name>");
        }
    }
}
