using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LaTeXBibitemStyler
{
    public class Operations
    {
        ArrayList aTexFiles;
        ArrayList aCites;
        ArrayList aBibitems;
        Hashtable hBibitems;
        public string filePath;
        public string mainTexFile;
        public string bibFilename;
        public string outputBibFile;
        public string preamble;
        public string postamble;
        public BibStyles bibStyle;

        /// <summary>
        /// read main tex file and get the content of all \input tags
        /// </summary>
        public void GetInputFiles()
        {
            try
            {
                StreamReader sr = new StreamReader(mainTexFile);
                aTexFiles = new ArrayList();

                //search for beginning of document
                while (sr.ReadLine() != "\\begin{document}") ;

                //parse document looking for \input or \include tags, and we'll store the enclosed tex file names in an arraylist //v2-02
                while (sr.Peek() >= 0)
                {
                    string l = sr.ReadLine();
                    l = l.TrimStart();
                    if (l.StartsWith("%") || string.IsNullOrEmpty(l)) //current line is commented out or empty //v5-02
                        continue;

                    if (l.Contains("\\input{") || l.Contains("\\include{")) //v2-02 
                    {
                        int i = l.IndexOf("{") + 1;
                        int c = l.IndexOf("}") - i;
                        //get file name from command
                        string texFile = l.Substring(i, c);
                        if (texFile != bibFilename) aTexFiles.Add(texFile);
                    }
                }
                sr.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// extract main file path from main tex file
        /// </summary>
        public void GetFilePath()
        {
            filePath = mainTexFile.Substring(0, mainTexFile.LastIndexOf('\\') + 1);
        }

        /// <summary>
        /// search for cites in the project's main tex file  //v2-03
        /// </summary>
        public void GetMainTexFileCites() //v2-03
        {
            StreamReader sr;
            CiteTypes type;
            aCites = new ArrayList();

            try
            {                                
                sr = new StreamReader(mainTexFile);
                while (sr.Peek() >= 0)
                {
                    string s = sr.ReadLine();
                    s = s.TrimStart();
                    if (s.StartsWith("%") || string.IsNullOrEmpty(s)) //current line is commented out or empty //v5-02
                        continue;

                    //parse the document looking for tags
                    // * \cite
                    // * \citenum
                    // * \nocite
                    //we'll store the contents in an arraylist so
                    //the global order of appearance is kept, and no repetition is allowed
                    while ((type = GetNextCiteType(s)) != CiteTypes.END) //v2-01
                    {
                        s = ParseCite(s, type);
                    }
                }
                sr.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// read all the project's tex files and get the contents of all \cite tags, with no repetition
        /// </summary>
        public void GetTexFileCites()
        {
            try
            {
                StreamReader sr;
                CiteTypes type;
                //aCites = new ArrayList(); //v2-03

                //read through all tex files looking for \cite tags
                foreach (string texFile in aTexFiles)
                {
                    var filename = filePath + texFile + (!texFile.EndsWith(".tex") ? ".tex" : "");
                    if (!File.Exists(filename))
                        continue;
                    
                    sr = new StreamReader(filename);
                    while (sr.Peek() >= 0)
                    {
                        string s = sr.ReadLine();
                        s = s.TrimStart();
                        if (s.StartsWith("%") || string.IsNullOrEmpty(s)) //current line is commented out or empty //v5-02
                            continue;

                        //parse the document looking for tags
                        // * \cite
                        // * \citenum
                        // * \nocite
                        //we'll store the contents in an arraylist so
                        //the global order of appearance is kept, and no repetition is allowed
                        while ((type = GetNextCiteType(s)) != CiteTypes.END) //v2-01
                        {
                            s = ParseCite(s, type);
                        }
                    }
                    sr.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// get next \cite in string
        /// </summary>
        public CiteTypes GetNextCiteType(string s) //v4-01
        {
            var index1 = s.IndexOf("\\cite{");
            var index2 = s.IndexOf("\\citenum{");
            var index3 = s.IndexOf("\\nocite{");

            if (index1 == -1 && index2 == -1 && index3 == -1)
                return CiteTypes.END;

            //put all values in integer list in order to find the mininum one
            var lIndex = new List<int>();
            if (index1 > -1) lIndex.Add(index1);
            if (index2 > -1) lIndex.Add(index2);
            if (index3 > -1) lIndex.Add(index3);
            
            if (lIndex.Min() == index1)
                return CiteTypes.CITE;

            if (lIndex.Min() == index2)
                return CiteTypes.CITENUM;

            //default case
            return CiteTypes.NOCITE;
        }

        /// <summary>
        /// parse a \cite tag
        /// </summary>
        public string ParseCite(string s, CiteTypes type) //v4-01
        {
            string sTag = "";
            int iOffset = 0;

            switch (type)
            {
                case CiteTypes.CITE:
                    sTag = "\\cite{";
                    iOffset = 6;
                    break;

                case CiteTypes.CITENUM:
                    sTag = "\\citenum{";
                    iOffset = 9;
                    break;

                default: //CiteTypes.NOCITE
                    sTag = "\\nocite{";
                    iOffset = 8;
                    break;
            }

            s = s.Substring(s.IndexOf(sTag) + iOffset); //v2-01
            string temp = s.Substring(0, s.IndexOf('}'));
            //v2-04: handle multiple keys inside single citation
            string[] cites = temp.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string c in cites)
            {
                string cite = c.TrimEnd().TrimStart(); //clear leading and trailing whitespaces
                aCites.Add(cite);
            }
            return s;
        }

        /// <summary>
        /// read bibliography files and get all \bibitems
        /// </summary>
        public void GetBibitems()
        {
            try
            {
                StreamReader sr = new StreamReader(bibFilename);
                aBibitems = new ArrayList();
                hBibitems = new Hashtable();

                //read through the bibliography file and store \bibtems in a hashtable and an arraylist
                //in the hashtable we'll store {key, value} as {\bibitem label, \bibitem content}
                //in the arraylist we'll store only the content of the \bibitem
                string s = sr.ReadToEnd();
                while (s.IndexOf("\\bibitem") != -1)
                {
                    s = s.Substring(s.IndexOf("\\bibitem") + 8);
                    string bibitem;
                    if (s.IndexOf("\\bibitem") != -1) //this is any \bibitem in the file
                        bibitem = s.Substring(0, s.IndexOf("\\bibitem"));
                    else //this is the last \bibitem in the file
                        bibitem = s.Substring(0, s.IndexOf("\\end{"));

                    bibitem = bibitem.Trim();
                    string key = bibitem.Substring(1, bibitem.IndexOf('}') - 1);
                    bibitem = bibitem.Replace("{" + key + "}", "").Trim().TrimEnd(new char[] { '\n', '\t' });
                    //we store the \bibitems in both a hashtable and an arraylist because we'll use the arraylist 
                    //to sort the \bibitems alphabetically
                    aBibitems.Add(bibitem);
                    hBibitems.Add(key, bibitem);
                }
                sr.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// obtain hashtable's entry key from its value
        /// </summary>
        public string GetKeyToValue(string value)
        {
            foreach (string key in hBibitems.Keys)
            {
                if (hBibitems[key].ToString() == value) return key;
            }
            return "";
        }

        //v3-01
        /// <summary>
        /// write bibitems in arraylist to the output bibliography tex file
        /// </summary>
        public void WriteBibitems(StreamWriter sw)
        {
            for (int i = 0; i < aBibitems.Count; i++)
            {
                string value = aBibitems[i].ToString();
                sw.Write("\t\\bibitem{" + GetKeyToValue(value) + "} " + value + "\n\n");
            }
        }

        /// <summary>
        /// write output bibliography tex file, according to the specified sorting method
        /// </summary>
        public bool WriteBibFile()
        {
            StreamWriter sw = new StreamWriter(filePath + outputBibFile, false, Encoding.Default);

            try
            {
                //write document's preamble
                sw.Write(preamble + "\n\n");

                switch (bibStyle)
                {
                    //write bibliography in the same order it was previously read
                    //it may seem quite dumb but it can be used to write the bibliography 
                    //enclosed by different pre and post ambles
                    case BibStyles.PLAIN:
                        //we simply write the \bibitems in the file
                        WriteBibitems(sw); //v3-01
                        break;

                    //write bibliography in alphabetical order of the content of the \bibitems
                    case BibStyles.ALPHA:
                        //sort bibitems and write them in the file
                        aBibitems.Sort();
                        WriteBibitems(sw); //v3-01
                        break;

                    //write bibliography in the order of the appearance of cites
                    case BibStyles.UNSRT:
                        //write \bibitems in the order of appearance of cites
                        //remember cites can be repeated must \bibitem must not be written more than once even
                        for (int i = 0; i < aCites.Count; i++)
                        {
                            try
                            {
                                var key = aCites[i].ToString();
                                if (hBibitems.ContainsKey(key)) //v4-01
                                {
                                    string value = hBibitems[key].ToString();
                                    if (aBibitems.Contains(value))
                                    {
                                        sw.Write("\t\\bibitem{" + aCites[i].ToString() + "} " + value + "\n\n");
                                        aBibitems.Remove(value);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }

                        //when we've written all the cited \bibitems, there might still be some \bibitems to write (these
                        //have not been cited in document). We then write the left \bibtems in the order they were previously read
                        WriteBibitems(sw); //v3-01
                        break;
                }

                //write document's postamble
                sw.Write("\n" + postamble);
                sw.Close();

                System.Diagnostics.Process.Start("wordpad", "\"" + filePath + outputBibFile + "\"");
                return true;                
            }
            catch (Exception ex)
            {
                sw.Write("\n" + postamble);
                sw.Close();
            }
            return false;
        }
    }
}
