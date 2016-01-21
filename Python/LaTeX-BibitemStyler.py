###################################################################
# LaTeX-BibitemsStyler
#
# Silvia "PchiwaN" Mur Blanch
# silvia.murblanch at gmail.com
#
# Andreea Georgescu
# andreea.i.georgescu at gmail.com
###################################################################

###################################################################
# Version 2.0 - Revised by suggestion of Olli Nummi
###################################################################
# replace '~\cite' by '\cite' in the search for cites
# use '\include' as well as '\input' to search for tex files
# search for cites in the main tex file too
# handle multiple keys inside single citation
###################################################################

###################################################################
# Version 3.0 - Python 3
###################################################################
# handles duplicate bibitems
# give file names and ordering style as sys.argv inputs
###################################################################


from __future__ import print_function
from collections import namedtuple, OrderedDict
import os
import sys

# Check default encoding
if (sys.stdout.encoding is None):            
    print >> sys.stderr, "Please set python env PYTHONIOENCODING=UTF-8" 
    exit(1)

# Define input as raw_input for Python 2.x
try: input = raw_input
except NameError: pass

bibstyles = namedtuple('bibstyles', ['PLAIN', 'ALPHA', 'UNSRT'])
bibstyles.__new__.__defaults__ = tuple([False] * len(bibstyles._fields))

class Styler:
    def __init__(self, mainTexFile='', bibFilename='', outputBibFile='', style_index=0):
        self.mainTexFile = mainTexFile
        self.bibFilename = bibFilename
        self.outputBibFile = outputBibFile
        self.bibStyle = bibstyles(**{bibstyles._fields[style_index]: True})
        self.preamble = '\\begin{thebibliography}{100}'
        self.postamble = '\\end{thebibliography}\n\n%%%%% CLEAR DOUBLE PAGE!\n' \
            + '\\newpage{\\pagestyle{empty}\\cleardoublepage}'
        self.aTexFiles = []
        self.aCites = []
        self.dBibitems = OrderedDict()

    def GetInputFiles(self):
        '''Read main tex file and get the content of all \input tags
        '''
        print('... getting input files')
        try:
            f = open(self.mainTexFile, 'r')
            s = f.read()  # read file to end
            # move parsing cursors to beginning of document
            s = s[s.find('\\begin{document}')+len('\\begin{document}'):]
            # parse the file looking for \input or \include commands
            while len(s) > 0:
                if s.find('\\input{') == -1 and s.find('\\include{') == -1:
                    break
                else:
                    if s.find('\\input{') != -1 and s.find('\\include{') != -1:
                        # there are both commands in the tex file
                        if s.find('\\input{') < s.find('\\include{'):
                            # \input command comes before \include command
                            s = s[s.find('\\input{')+len('\\input{'):]
                        else:
                            s = s[s.find('\\include{')+len('\\include{'):]
                    else:
                        if s.find('\\input{') != -1:
                            s = s[s.find('\\input{')+len('\\input{'):]
                        else:
                            s = s[s.find('\\include{')+len('\\include{'):]

                    # get file name from command
                    texFile = s[:s.find('}')]
                    print('\t\t', texFile)
                    if texFile != self.bibFilename:
                        self.aTexFiles.append(texFile)
                    # move parsing cursor past the current \input command
                    s = s[s.find('}'):]
            f.close()
        except:
            print('An error occurred while reading', self.mainTexFile)
            raise

    def GetFilePath(self):
        '''Extract main file path from main tex file
        '''
        print('... getting file path')
        self.filePath = self.mainTexFile[:self.mainTexFile.rfind('\\') + 1]

    def GetMainTexFileCites(self):
        '''Search for cites in the project's main tex file
        '''
        print('... getting main tex file cites')
        try:
            f = open(self.filePath + self.mainTexFile, 'r')
            s = f.read()  # read file to end
            # parse current file looking for \cite commands, and store all cite keys in
            # an array in their order of appearance
            while s.find('\\cite{') != -1:
                s = s[s.find('\\cite{')+len('\\cite{'):]
                temp = s[:s.find('}')]
                # handle multiple keys inside single citation
                cites = temp.split(',')
                for c in cites:
                    cite = c.strip()  # clear leading and trailing whitespaces
                    try:
                        self.aCites.index(cite)
                        # check if the cite key is already there,
                        # to avoid duplicating keys
                    except:
                        self.aCites.append(cite)
            f.close()
        except:
            print('An error occurred while reading main .tex file')
            raise

    def GetTexFileCites(self):
        '''Read all the project's tex files and get the contents of all \cite tags,
        with no repetition
        '''
        print('... getting tex files cites')
        try:
            for texFile in self.aTexFiles:
                f = open(self.filePath + texFile, 'r')
                s = f.read()  # read file to end
                # parse current file looking for \cite commands, and store all cite keys
                # in an array in their order of appearance
                while s.find('\\cite{') != -1:
                    s = s[s.find('\\cite{')+len('\\cite{'):]
                    temp = s[:s.find('}')]
                    # handle multiple keys inside single citation
                    cites = temp.split(',')
                    for c in cites:
                        cite = c.strip()  # clear leading and trailing whitespaces
                        try:
                            # check if the cite key is already there,
                            # to avoid duplicating keys
                            self.aCites.index(cite)
                        except:
                            self.aCites.append(cite)
                f.close()
        except:
            print('An error occurred while reading input .tex file')
            raise

    def GetBibitems(self):
        '''Read bibliography files and get all \bibitems
        '''
        print('... getting \\bibitems')
        try:
            with open(self.filePath + self.bibFilename, 'r') as f:
                s = f.read()  # read to end
            # parse bibliography file and store bibitems in a dictionary
            while s.find('\\bibitem') != -1:
                s = s[s.find('\\bibitem')+len('\\bibitem'):]
                if s.find('\\bibitem') != -1:  # this is any \bibitem
                    bibitem = s[:s.find('\\bibitem')]
                else:  # this is the last \bibitem of the bibliography file
                    bibitem = s[:s.find('\\end{')]
                if s.find('%\\cite{') != -1:
                    # remove comment of form '%\cite{}' from future bibitem
                    bibitem = bibitem[:s.find('%\\cite{')]
                key = bibitem[1:bibitem.find('}')]  # get \bibitem key
                # remove key from the \bibitem entry
                bibitem = bibitem.replace('{' + key + '}', '')
                bibitem = bibitem.strip()
                # remove trailing characters from the \bibitem entry
                bibitem = bibitem.rstrip('\n\t')
                # we store the \bibitem in a dictionary;
                # we'll access the entry by its key
                self.dBibitems[key] = bibitem
            f.close()
        except:
            print('An error has ocurred while parsing the bibliography file,',
                  self.bibFilename)
            raise

    def WriteBibFile(self):
        '''Write output bibliography tex file, according to the specified sorting method
        '''
        print('... writing bibliography file')
        try:
            f = open(self.filePath + self.outputBibFile, 'w')
            # write bibliography file preamble
            print('\t\twriting preamble')
            f.write(self.preamble + '\n\n')
            # write bibliography according to the chosen style
            if self.bibStyle.PLAIN:
                # same \bibitem order as the original bibliography file,
                # but with specified preamble and postamble
                print('\t\twriting \\bibitems PLAIN style')
                for key in self.dBibitems:
                    f.write('\\bibitem{' + key + '} ' + self.dBibitems[key] + '\n\n')

            elif self.bibStyle.ALPHA:  # \bibitem entry alphabetical order
                print('\t\twriting \\bibitems ALPHA style')
                for key in sorted(self.dBibitems, key=self.dBibitems.get):
                    f.write('\\bibitem{' + key + '} ' + self.dBibitems[key] + '\n\n')

            elif self.bibStyle.UNSRT:
                # \bibitem key order of appearance in the latex project files
                print('\t\twriting \\bibitems UNSRT style')
                for key in self.aCites:
                    try:
                        f.write('\\bibitem{' + key + '} ' + self.dBibitems[key] + '\n\n')
                        # remove the \bibitem that we just wrote from the \bibitems array
                        del self.dBibitems[key]
                    except KeyError:
                        print('KeyError: ' + key + ' not found in dBibitems')
                # At this point, the \bibitems that were cited in the latex project files
                # have been written in the output file.
                if self.dBibitems:  # there are still bibitems left that were not cited
                    print('\n+ There are bibitems that were not cited in the latex '
                          'project. \nDo you want to include them in the same order '
                          'they were read? (y/N)')
                    if input() == 'y':
                       for key in self.dBibitems:
                           f.write('\\bibitem{' + key + '} ' + self.dBibitems[key] +
                                   '\n\n')
            print('\t\twriting postamble')
            f.write('\n' + self.postamble)
        except:
            print('An error has occurred while writing the output bibliography file,',
                  self.outputBibFile)
            raise
        else:
            f.close()
            print('\nBibliography file', self.outputBibFile,
                  'has been successfully created!\n')

    def StyleBibitems(self):
        '''Main function, sorts the bibitems
        '''
        print('\n\n')
        print('########################################################')
        print('######### LaTeX-BibitemsStyler by Pchiwan 2009 #########')
        print('########################################################\n\n')

        print('+ Main Tex file path (use double \'\\\' if on Windows)\n')
        print(self.mainTexFile)
        if os.path.exists(self.mainTexFile):

            print('\n+ Bibliography Tex file name\n')
            print(self.bibFilename)

            print('\n+ Output bibliography Tex file name\n')
            print(self.outputBibFile)

            print('\n+ This is the default bibliography preamble\n\n', self.preamble)
            print('\n\n+ Do you want to change it? (y/N)')
            if input() == 'y':
               print('\n+ Enter preamble (type \'\\q\' to submit)\n')
               k = ''
               self.preamble = ''
               while k != '\q':
                   k = input()
                   if k != '\q':
                       self.preamble += k

            print('\n+ This is the default bibliography postamble\n\n', self.postamble)
            print('\n\n+ Do you want to change it? (y/N)')
            if input() == 'y':
               print('\n+ Enter postamble (type \'\\q\' to submit)\n')
               k = ''
               self.postamble = ''
               while k != '\q':
                   k = input()
                   if k != '\q':
                       self.postamble += k

            print('\n+ Bibliography style\n')
            if self.bibStyle.PLAIN:
                print('0 - PLAIN')
            if self.bibStyle.ALPHA:
                print('1 - ALPHA (Alphanumerical order)')
            if self.bibStyle.UNSRT:
                print('2 - UNSRT (Cite order of appearance)')
            print('\n\n')

            self.GetInputFiles()
            self.GetFilePath()
            self.GetMainTexFileCites()
            self.GetTexFileCites()
            self.GetBibitems()
            self.WriteBibFile()
        else:
            print('Please enter a valid main Tex file path')


def main():
    try:
        styler = Styler(mainTexFile=sys.argv[1],
                        bibFilename=sys.argv[2],
                        outputBibFile=sys.argv[3],
                        style_index=int(sys.argv[4]))
    except:
        print('Please run the program with 4 arguments:')
        print('    - The main tex file path (use double \'\\\' if on Windows)')
        print('    - The bibliography Tex file name')
        print('    - The output bibliography Tex file name')
        print('    - The bibliography style:')
        print('0 - PLAIN (Original order)')
        print('1 - ALPHA (Alphanumerical order)')
        print('2 - UNSRT (Cite order of appearance)')
        print()
        raise
    styler.StyleBibitems()


if __name__ == '__main__':
    main()
