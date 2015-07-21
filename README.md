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

---------------------------------------
Andreea Georgescu

Sílvia Mur