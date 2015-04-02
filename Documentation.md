## Summary ##
jar2ikvmc is a small tool which helps to convert a set of Java jar files into .NET dlls.

It can be usefull when number of original jar files to be converted to .NET is substantial.
Additionally to ikvmc command-line script it generates [Graphviz](http://www.graphviz.org/) file containing dependencies diagram.

## Feedback ##

Please post you feedback on Comments on my [blog](http://don.env.com.ua).

You can also Submit an Issue to report bug or suggest an improvement.

## Itroduction ##

After using [IKVM.NET](http://www.ikvm.net/) to convert some Java programs to .NET I decided to write a small program that generates command-line script for ikvmc.exe program. It works perfect when original Java program consists only from one or few jar files. In this case command-line will look like:

` $ ikvmc junit.jar -target:library `

And as result we have JUnit library as a .NET assembly called junit.dll. But what if program consists of 20 or 100 input jar files? Detecting dependencies between all of them and writing manually all those _-r:<lib1.dll> -r:<lib2.dll> ..._ will not be so easy.

First I searched for a programs that can analyze dependencies between jars and found [JarAnalyzer](http://www.kirkk.com/main/Main/JarAnalyzer). It was able to detect dependencies between all jar files in provided folder and then write them into xml:

```

<Jar name="bcel-5.2.jar">
  <Summary>
    <OutgoingDependencies>
    </OutgoingDependencies>
    <IncomingDependencies>
      <Jar>jaranalyzer-1.2.jar</Jar>
    </IncomingDependencies>
  </Summary>
</Jar>
<Jar name="jaranalyzer-1.2.jar">
  <Summary>
    <OutgoingDependencies>
      <Jar>bcel-5.2.jar</Jar>
      <Jar>ant.jar</Jar>
    </OutgoingDependencies>
    <IncomingDependencies>
    </IncomingDependencies>
  </Summary>
</Jar>

```

Then I decided to put all those dependencies into a graph structure so that it would be possible to enumerate via it's nodes in a proper order. [QuickGraph](http://www.codeproject.com/cs/miscctrl/quickgraph.asp) was a good choice to solve that task:
```
private static void GenerateIkvmcRunScript(BidirectionalGraph<string, Edge<string>> g, string path)
{
  StreamWriter sw = new StreamWriter(path);
  foreach (string vertex in AlgoUtility.TopologicalSort<string, Edge<string>>(g))
  {
    IEnumerable<string> names;
    names = g.InEdges(vertex).Select<Edge<string>, string>(item => item.Source);

    string references = "";
    foreach (string name in names)
    {
      references += " -r:" + name.Replace(".jar", ".dll");
    }

    string commandLine = "ikvmc " + vertex + " -target:library" + references;
    sw.WriteLine(commandLine);
  }
  sw.Close();
}
```

In order to have everything in one program I also converted JarAnalyzer itself using IKVM.NET :). As result both jar dependency analysis and generation of ikvmc script were embedded in one executable. However size of a program increased a little due to ikvm-ed JDK libraries.

```
bin/
  jar2ikvmc.exe
  QuickGraph.dll
  bcel-5.2.dll
  jaranalyzer-1.2.dll
  IKVM.OpenJDK.ClassLibrary.dll
  IKVM.Runtime.dll

$ jar2ikvmc

Usage: jar2ikvmc <input folder containing jar files> <output script file name>
```

## Example (JFreeChart) ##
```
$ ls JFreeChart/
gnujaxp.jar
itext-2.0.2.jar
jcommon-1.0.10.jar
jfreechart-1.0.6-experimental.jar
jfreechart-1.0.6-swt.jar
jfreechart-1.0.6.jar
junit.jar servlet.jar
swtgraphics2d.jar

$ jar2ikvmc.exe JFreeChart JFreeChart2Net.cmd

$ cat JFreeChart2Net.cmd
ikvmc swtgraphics2d.jar -target:library
ikvmc servlet.jar -target:library
ikvmc junit.jar -target:library
ikvmc jcommon-1.0.10.jar -target:library
ikvmc gnujaxp.jar -target:library
ikvmc jfreechart-1.0.6.jar -target:library -r:jcommon-1.0.10.dll -r:servlet.dll -r:gnujaxp.dll
ikvmc jfreechart-1.0.6-swt.jar -target:library -r:jfreechart-1.0.6.dll -r:jcommon-1.0.10.dll
ikvmc jfreechart-1.0.6-experimental.jar -target:library -r:jfreechart-1.0.6.dll -r:jcommon-1.0.10.dll
ikvmc itext-2.0.2.jar -target:library -r:gnujaxp.dll

$ cat JFreeChart2Net.cmd.dot
digraph G {
"gnujaxp.jar" -> "itext-2.0.2.jar";
"gnujaxp.jar" -> "jfreechart-1.0.6.jar";
"jcommon-1.0.10.jar" -> "jfreechart-1.0.6-experimental.jar";
"jcommon-1.0.10.jar" -> "jfreechart-1.0.6-swt.jar";
"jcommon-1.0.10.jar" -> "jfreechart-1.0.6.jar";
"jfreechart-1.0.6.jar" -> "jfreechart-1.0.6-experimental.jar";
"jfreechart-1.0.6.jar" -> "jfreechart-1.0.6-swt.jar";
"servlet.jar" -> "jfreechart-1.0.6.jar";
}
```
First file contains command lines for ikvmc and the second file is a dot file in a [Graphviz](http://www.graphviz.org/) format showing all dependencies on a diagram.

The last file can be converted to a png format using the following command line:
```
$ dot -Tpng -o"JFreeChart2Net.png" JFreeChart2Net.cmd.dot
```
Which will produce the following diagram:

![http://don.env.com.ua/files/jar2ikvm/JFreeChart-dependency-diagram.png](http://don.env.com.ua/files/jar2ikvm/JFreeChart-dependency-diagram.png)

The same code for [GeoTools](http://www.geotools.org) will look like [this](http://don.env.com.ua/files/jar2ikvm/ikvmc_Geotools.txt). Imagine creating it by hands :). Here is a graph of it: [GeoTools.png](http://don.env.com.ua/files/jar2ikvm/GeoTools.png)