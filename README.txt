Summary
=======

Program that generates command lines file for ikvmc.exe in a correct order and with correct references based on
the dependencies between original jar files.

Usage
=====

jar2ikvmc <input folder containing jar files> <output script file name>


Example (JFreeChart)
====================

$ jar2ikvmc.exe ./JFreeChart/ JFreeChart2Net.cmd

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


Another generated file is a Graphviz .dot diagram showing dependencies:

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


The diagram can be generated as a png file using the following command line (Graphviz should be installed):

$ dot -Tpng -o"JFreeChart2Net.png" JFreeChart2Net.cmd.dot



Author
======

Gennadii Donchyts, don@env.com.ua