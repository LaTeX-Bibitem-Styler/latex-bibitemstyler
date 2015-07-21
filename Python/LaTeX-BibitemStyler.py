##
## Sílvia "PchiwaN" Mur Blanch
## silvia.murblanch at gmail.com
## LaTeX-BibitemsStyler 2009
################################ 

from enum import Enum
import os

BibStyles = Enum('PLAIN', 'ALPHA', 'UNSRT')

class Styler:
	def __init__(self):
		filePath = ''
        mainTexFile= ''
        bibFilename = ''
        outputBibFile = ''
        preamble = '\\begin{thebibliography}{100}'
        postamble = '\\end{thebibliography}\n\n%%%%% CLEAR DOUBLE PAGE!\n\\newpage{\\pagestyle{empty}\\cleardoublepage}'
        aTexFiles = []
        aCites = []
        aBibitems = []
        dBibitems = dict([])

	def GetInputFiles(self):
		print '... getting input files'
		try:
			f = open(self.mainTexFile, 'r')
			s = f.read() #read file to end
			#move parsing cursos to beginning of document
			s = s[s.find('\\begin{document}')+len('\\begin{document}'):len(s)]
			#parse the file looking for \input commands
			while s.find('\\input{') != -1:
				#get tex file name from \input command
				s = s[s.find('\\input{')+len('\\input{'):len(s)]
				texFile = s[0:s.find('}')]
				print '\t\t',texFile
				if texFile != self.bibFilename:
					self.aTexFiles.append(texFile);
				#move parsing cursor past the current \input command	
				s = s[s.find('}'):len(s)]
			f.close()
		except:
			print 'An error occurred while reading',self.mainTexFile

	def GetFilePath(self):
		print '... getting file path'
		self.filePath = self.mainTexFile[0:self.mainTexFile.rfind('\\') + 1]
	
	def GetTexFileCites(self):
		print '... getting tex files cites'
		try:
			for texFile in self.aTexFiles:
				f = open(self.filePath + texFile, 'r')
				s = f.read() #read file to end
				#parse current file looking for \cite commands, and store all cite keys in an array in their order of appearance
				while s.find('~\\cite{') != -1:
					s = s[s.find('~\\cite{')+len('~\\cite{'):len(s)]
					cite = s[0:s.find('}')]
					try:
						self.aCites.index(cite) #check if the cite key is already there, to avoid duplicating keys
					except:
						self.aCites.append(cite)
				f.close()			
		except:
			print 'An error occurred while reading input .tex file'


	def GetBibitems(self):
		print '... getting \\bibitems'
		try:
			f = open(self.filePath + self.bibFilename, 'r')
			s = f.read() #read to end
			#parse bibliography file and store bibitems in a dictionary					
			while s.find('\\bibitem') != -1:
				s = s[s.find('\\bibitem')+len('\\bibitem'):len(s)]
				if s.find('\\bibitem') != -1: #this is any \bibitem
					bibitem = s[0:s.find('\\bibitem')]
				else: #this is the last \bibitem of the bibliography file
					bibitem = s[0:s.find('\\end{')]
				key = bibitem[1:bibitem.find('}')] #get \bibitem key
				bibitem = bibitem.replace('{'+ key +'}', '') #remove key from the \bibitem entry
				bibitem = bibitem.strip()
				bibitem = bibitem.rstrip('\n\t') #remove trailing characters from the \bibitem entry
				self.aBibitems.append(bibitem) #we store the \bibitem in an array; we'll use it for alphabetically sorting the entries
				self.dBibitems[key] = bibitem #we store the \bibitem in a dictionary; we'll access the entry by its key
			f.close()
			#print self.aCites
			#print self.aBibitems
			#print self.dBibitems
		except:
			print 'An error has ocurred while parsing the bibliography file,',self.bibFilename

	def GetKeyToValue(self, value):
		for key in self.dBibitems.keys():
			if self.dBibitems[key] == value:
				return key
		return None

	def WriteBibFile(self):
		print '... writing bibliography file'
		try:
			f = open(self.filePath + self.outputBibFile, 'w')
			#write bibliography file preamble
			print '\t\twriting preamble'
			f.write(self.preamble + '\n\n')
			#write bibliography according to the chosen style
			if self.bibStyle == BibStyles.PLAIN: #same \bibitem order as the original bibliography file, but with specified preamble and postamble
				print '\t\twriting \\bibitems PLAIN style'
				for value in self.aBibitems:
					f.write('\t\\bibitem{'+ self.GetKeyToValue(value) +'} '+ value +'\n\n')
				
			if self.bibStyle == BibStyles.ALPHA: #\bibitem entry alphabetical order
				print '\t\twriting \\bibitems ALPHA style'
				self.aBibitems.sort()
				for value in self.aBibitems:
					f.write('\t\\bibitem{'+ self.GetKeyToValue(value) +'} '+ value +'\n\n')
				
			if self.bibStyle == BibStyles.UNSRT: #\bibitem key order of appearance in the latex project files
				print '\t\twriting \\bibitems UNSRT style'
				for key in self.aCites:
					f.write('\t\\bibitem{'+ key +'} ' + self.dBibitems[key] + '\n\n')
					self.aBibitems.remove(self.dBibitems[key]) #remove the \bibitem that we just wrote from the \bibitems array
				#at this point, the \bibitems that were cited in the latex project files have been written in the output file
				#we know proceed to write the \bibitems that were not cited in the latex project, in the same order they were read
				for value in self.aBibitems:
					f.write('\t\\bibitem{' + self.GetKeyToValue(value) +'} '+ value +'\n\n') 
			print '\t\twriting postamble'
			f.write('\n'+ self.postamble)
		except:
			print 'An error has ocurred while writing the output bibliography file,',self.outputBibFile
		else:
			f.close()
			print '\nBibliography file',self.outputBibFile,'has been successfully created!\n'

################### MAIN LOOP


styler = Styler()

print '\n\n'
print '########################################################'
print '######### LaTeX-BibitemsStyler by Pchiwan 2009 #########'
print '########################################################\n\n'

print '+ Enter main Tex file path (use double \'\\\')\n'
styler.mainTexFile = raw_input() 
if os.path.exists(styler.mainTexFile):
	
	print '\n+ Enter bibliography Tex file name\n'
	styler.bibFilename = raw_input() 
		
	print '\n+ Enter output bibliography Tex file name\n'
	styler.outputBibFile = raw_input() 

	print '\n+ This is the default bibliography preamble\n\n',styler.preamble
	print '\n\n+ Do you want to change it? (y/n)'
	if raw_input() == 'y':
		print '\n+ Enter preamble (type \'\\q\' to submit)\n'
		k = ''
		styler.preamble = ''
		while k != '\q':
			k = raw_input()
			if k != '\q':
				styler.preamble += k

	print '\n+ This is the default bibliography postamble\n\n',styler.postamble
	print '\n\n+ Do you want to change it? (y/n)'
	if raw_input() == 'y':	
		print '\n+ Enter postamble (type \'\\q\' to submit)\n'
		k = ''
		styler.postamble = ''
		while k != '\q':
			k = raw_input()
			if k != '\q':
				styler.postamble += k

	print '\n+ Select a bibliography style\n'
	print '0 - PLAIN'
	print '1 - ALPHA (Alphanumerical order)'
	print '2 - UNSRT (Cite order of appearance)'
	styler.bibStyle = BibStyles[int(raw_input())]
	print '\n\n'
	
	styler.GetInputFiles()
	styler.GetFilePath()
	styler.GetTexFileCites()
	styler.GetBibitems()
	styler.WriteBibFile()
else:
	print 'Please enter a valid main Tex file path'
        

        