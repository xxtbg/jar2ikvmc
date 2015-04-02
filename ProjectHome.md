## Summary ##
The tool helps converting big bundles of Java jar files to .NET dlls using [IKVM.NET](http://www.ikvm.net).

Please ask questions and feature requests in the followin **Google Group:** http://groups.google.com/group/jar2ikvmc

**Jar2ikvmc** uses [JarAnalyser](http://www.kirkk.com/main/Main/JarAnalyzer) to detect dependencies between jar files and then generates command-line script for ikvmc.exe.

See **[Documentation](Documentation.md)** page for more information about internals of the tool.

Information about releases, recent news and plans can be found on my blog:  **http://don.env.com.ua**.

## Example ##
Here is sample results of program running using jar files for JFreeChart:

### Generated Script ###
```
ikvmc swtgraphics2d.jar -target:library
ikvmc servlet.jar -target:library
ikvmc junit.jar -target:library
ikvmc jcommon-1.0.10.jar -target:library
ikvmc gnujaxp.jar -target:library
ikvmc jfreechart-1.0.6.jar -target:library -r:jcommon-1.0.10.dll -r:servlet.dll -r:gnujaxp.dll
ikvmc jfreechart-1.0.6-swt.jar -target:library -r:jfreechart-1.0.6.dll -r:jcommon-1.0.10.dll
ikvmc jfreechart-1.0.6-experimental.jar -target:library -r:jfreechart-1.0.6.dll -r:jcommon-1.0.10.dll
ikvmc itext-2.0.2.jar -target:library -r:gnujaxp.dll
```

### Dependencies ###
![http://don.env.com.ua/files/jar2ikvm/JFreeChart-dependency-diagram.png](http://don.env.com.ua/files/jar2ikvm/JFreeChart-dependency-diagram.png)