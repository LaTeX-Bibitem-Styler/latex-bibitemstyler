LaTeX Bibitem Styler
====================

This program allows you to automatically sort `\bibitems` by citation order of alphabetically.

### Description

There's a major problem in LaTeX bibliography styling/sorting when not using BibTeX.

When using BibTeX, one can specify a `\bibliographystyle`, but if we're generating our bibliography from plain `\bibitem` entries, using the `\thebibliography` command, there's no way of specifying the output style.

Given a main `.tex` file and a bibliography `.tex` file (containing only bibitems), the _LaTeX-BibitemStyler_ will go through all the input `.tex` files of your project, collect the cites in order of appearance, and allow you to generate a new bibliography `.tex` file while specifying one of the following output styles.

- **PLAIN**.
- **ALPHA**. Alphabetical order
- **UNSRT**. Cite order of appearance

This project currently has two flavors:

- Microsoft .NET application
- Python

If you found this useful, please drop us a line! Reviews and suggestions are very welcome!

---------------------------------------

### Usage: .NET application

It’s very easy to use, just fill in the textboxes…

![.NET application screenshot](C%23.NET/LaTeXBibitemStyler/screenshot.png?raw=true)

##### Main Tex File
The main file of your LaTeX project. I.e.: `thesis.tex`. See example below:

    \documentclass[letterpaper,oneside,british,english,11pt]{book}
    \usepackage[T1]{fontenc}
    \usepackage[latin1]{inputenc}
    \setcounter{secnumdepth}{3}
    \setcounter{tocdepth}{3}
    \makeatletter
    \providecommand{\tabularnewline}{\\}
    \floatstyle{ruled}
    \newfloat{algorithm}{tbp}{loa}
    \floatname{algorithm}{Algorithm}
    \makeatother
    \begin{document}
    \include{titlePage}
    \pagenumbering{roman}
    \pagestyle{plain}
    \input{abstract.tex}
    \input{acknowledgements.tex}
    \tableofcontents{}
    \listoffigures
    \pagenumbering{arabic}
    \pagestyle{fancyplain}
    \newpage
    \input{intro.tex}
    \input{chapter1.tex}
    \input{chapter2.tex}
    \input{chapter3.tex}
    \normalsize
    \nocite{*}
    % ***** This is what you would usually do to include a bibliography *****
        %\bibliographystyle{unsrt}
        %\bibliography{biblio.bib}
    % ***** But instead you must use a .tex file *****
    \input{biblio.tex}
    \addcontentsline{toc}{chapter}{\numberline{}\sf\bfseries{Bibliography}}
    \end{document}

##### Biblio Filename
The LaTeX file where you’ve written your bibliography in a plain style (not a `.bib` file, it must be a `.tex` file!). I.e.: `biblio.tex`. See example below:

    \begin{thebibliography}{100}
        \bibitem{army0} BEN KAGE. \emph{Roomba maker iRobot also developing military robots for Pentagon}. 
        [web page], 14 December 2006. NaturalNews. [Visited: 23 January 2009]. 
        URL: \url{http://www.naturalnews.com/021301.html}.
        
        \bibitem{army2} WADE ROUSH. \emph{iRobot Wins \$3.75M Army Contract to Develop Warrior Robot | Xconomy}. 
        [web page], 2 October 2008. Xconomy Boston. [Visited: 23 January 2009]. 
        URL: \url{http://www.xconomy.com/boston/2008/10/02/irobot-wins-375m-army-contractto-develop-warrior-robot/}.
        
        \bibitem{anylogic} XJ TECHNOLOGIES. \emph{AnyLogic - Xjtek}. 
        [web page], 29 December 2008. XJ Technologies Company. [Visited: 18 December 2008]. 
        URL: \url{http://www.xjtek.com/anylogic/}.
        
        \bibitem{simulink} THE MATHWORKS. \emph{Simulink - Simulation and Model-Based Design}. 
        [web page], January  2009. The Mathworks, Inc. [Visited: 18 December 2008]. 
        URL: \url{http://www.mathworks.com/products/simulink/}.
        
        \bibitem{pywin} MARK HAMMOND. \emph{SourceForge.net: Python for Windows extensions}. 
        [web page], 31 July 2008. SourceForge.net. [Visited: 14 April 2008]. 
        URL: \url{http://sourceforge.net/projects/pywin32/}.        
    \end{thebibliography}
    %%%%% CLEAR DOUBLE PAGE!
    \newpage{\pagestyle{empty}\cleardoublepage}

##### Output Filename
Name of the new bibliography file that will be generated.

##### Bibliography Style
- PLAIN. Simply rewrite the file with different preamble and postamble.
- APLHA. Generate bibliography file with cites in alphabetical order.
- UNSRT. Generate bibliography file with cites in order of appearance (goes through all your LaTeX project’s `.tex` files and fetches all cite references).

##### Preamble
You might want to change your bibliography file’s predefined preamble.

##### Postamble
You might want to change your bibliography file’s predefined postamble.


Finally, just hit **RUN!** and let the application perform its magic, it’ll be done in a second. You’ll find the newly generated bibliography file in the original file’s path.

---------------------------------------

### Usage: Python code

Run the program with command-line arguments:
- The main tex file path (use double '\' if you're on Windows)
- The input bibliography tex file name. Mind that all your latex project’s files must be in the same directory.
- The output bibliography tex file name (the name of the new bibliography file that will be generated)
- The bibliography style:
    - 0: PLAIN (Original order)
    - 1: ALPHA (Alphanumerical order)
    - 2: UNSRT (Cite order of appearance)

Example:

    python3 LaTeX-BibitemStyler.py main_file.tex bibliography_original.tex bibliography_ordered.tex 2

Then in the main tex file add

>\input{bibliography_ordered.tex}

before

>\end{document}

### License

Copyright 2015 Andreea Georgescu, Sílvia Mur

Licensed under GNU GPL version 2 or any later version

http://www.gnu.org/licenses/
