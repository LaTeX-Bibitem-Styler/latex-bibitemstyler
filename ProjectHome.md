There's a major problem in LaTeX bibliography styling/sorting when not using BibTeX.

When using BibTeX, one can specify a <i>\bibliographystyle</i>, but if we're generating our bibliography from plain <i>\bibitem</i> entries, using the <i>\thebibliography</i> command, there's no way of specifying the output style.

Given a main .tex file and a bibliography .tex file (containing only <i>bibitems</i>), the <a href='http://code.google.com/p/latex-bibitemstyler/'>LaTeX-BibitemStyler</a> will go through all the input .tex files of your project, collect the cites in order of appearance, and allow you to generate a new bibliography .tex file while specifying an output style.

<ul>
<li>PLAIN.</li>
<li>ALPHA. Alphabetical order</li>
<li>UNSRT. Cite order of appearance</li>
</ul>

Microsoft.NET Framework is required to run the <a href='http://code.google.com/p/latex-bibitemstyler/'>LaTeX-BibitemStyler</a>.

If you found this useful, please drop me a line! Reviews and suggestions are very welcome!

++Updated: January 14th, 2014

As from January 15th, 2014, Google Code does not support creating new downloads so I cannot upload newer versions of the Microsoft.NET application. Therefore, you can now download it from my Google Drive. Here it is:

<a href='https://drive.google.com/file/d/0B1Y6bHNjGjMcM1VlWUh5eWtmeDA'><a href='https://drive.google.com/file/d/0B1Y6bHNjGjMcM1VlWUh5eWtmeDA'>https://drive.google.com/file/d/0B1Y6bHNjGjMcM1VlWUh5eWtmeDA</a></a>