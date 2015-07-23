LaTeX Bibitem Styler
====================

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


---------------------------------------
Andreea Georgescu | Sílvia Mur | 2015

