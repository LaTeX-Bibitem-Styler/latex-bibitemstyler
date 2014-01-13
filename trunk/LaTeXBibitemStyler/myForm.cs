using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;

/************************************************************************
 * Version 3.0 - Revised by suggestion of Günther Lientschnig           *
 ************************************************************************
 * v3-01: revise PLAIN and UNSRT generation methods, they were not   	*
 *   working propery!                                                   *
 ************************************************************************/

/************************************************************************
 * Version 2.0 - Revised by suggestion of Olli Nummi                    *
 ************************************************************************
 * v2-01: replace '~\cite' by '\cite' in the search for cites			*
 * v2-02: use '\include' as well as '\input' to search for tex files    *
 * v2-03: search for cites in the main tex file too                     *
 * v2-04: handle multiple keys inside single citation                   *
 ************************************************************************/

namespace LaTeXBibitemsStyler
{
    public partial class myForm : Form
    {
        enum BibStyles { PLAIN, ALPHA, UNSRT };
        ArrayList aTexFiles;
        ArrayList aCites;
        ArrayList aBibitems;
        Hashtable hBibitems;
        string filePath;
        string mainTexFile;
        string bibFilename;
        string outputBibFile;
        string preamble;
        string postamble;
        BibStyles bibStyle;

        public myForm()
        {
            InitializeComponent();
        }

        private void myForm_Load(object sender, EventArgs e)
        {
            cbxBibStyle.DataSource = Enum.GetNames(typeof(BibStyles));
            this.Top = Screen.PrimaryScreen.Bounds.Height / 2 - this.Height / 2;
            this.Left = Screen.PrimaryScreen.Bounds.Width / 2 - this.Width / 2;

            rtxPreamble.Text = "\\begin{thebibliography}{100}";
            rtxPostamble.Text = "\\end{thebibliography}\n\n%%%%% CLEAR DOUBLE PAGE!\n\\newpage{\\pagestyle{empty}\\cleardoublepage}";
        }

        private void Search_Click(object sender, EventArgs e)
        {
            lblResult.Text = "";
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = Environment.SpecialFolder.Desktop.ToString();
            ofd.Filter = "TEX Files (*.tex)|*.tex";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txbMainTexFile.Text = ofd.FileName;
            }
        }

        private void Run_Click(object sender, EventArgs e)
        {
            //start styling process; 
            //check if the specified main tex file exists 
            if (txbMainTexFile.Text != "" && File.Exists(txbMainTexFile.Text))
            {
                mainTexFile = txbMainTexFile.Text;

                if (txbBibFilename.Text != "")
                {
                    bibFilename = txbBibFilename.Text;

                    if (txbOutputFilename.Text != "")
                    {
                        outputBibFile = txbOutputFilename.Text;

                        preamble = rtxPreamble.Text;
                        postamble = rtxPostamble.Text;

                        bibStyle = (BibStyles)cbxBibStyle.SelectedIndex;

                        GetInputFiles();
                        GetFilePath();
                        GetMainTexFileCites();
                        GetTexFileCites();
                        GetBibitems();
                        WriteBibFile();
                        //this.Dispose();
                    }
                    else 
                    {
                        lblResult.Text = "Hey, I need an output filename to continue!";
                        lblResult.ForeColor = System.Drawing.Color.Red;
                    }
                }
                else
                {
                    lblResult.Text = "Hey, I need a bibliography file to continue!";
                    lblResult.ForeColor = System.Drawing.Color.Red;
                }
            }
            else
            {
                lblResult.Text = "Ooops! Please write a valid TEX file path.";
                lblResult.ForeColor = System.Drawing.Color.Red;
            }
        }

        /// <summary>
        /// read main tex file and get the content of all \input tags
        /// </summary>
        private void GetInputFiles()
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
        private void GetFilePath()
        {
            filePath = mainTexFile.Substring(0, mainTexFile.LastIndexOf('\\') + 1);
        }

        /// <summary>
        /// search for cites in the project's main tex file  //v2-03
        /// </summary>
        private void GetMainTexFileCites() //v2-03
        {
            StreamReader sr;
            aCites = new ArrayList();

            try
            {
                sr = new StreamReader(mainTexFile);
                string s = sr.ReadToEnd();
                //parse the document looking for \cite tags, we'll store the contents in an arraylist so
                //the global order of appearance is kept, and no repetition is allowed
                while (s.IndexOf("\\cite{") != -1) //v2-01
                {
                    s = s.Substring(s.IndexOf("\\cite{") + 6); //v2-01
                    string temp = s.Substring(0, s.IndexOf('}'));
                    //v2-04: handle multiple keys inside single citation
                    string[] cites = temp.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach(string c in cites)
                    {
                        string cite = c.TrimEnd().TrimStart(); //clear leading and trailing whitespaces
                        aCites.Add(cite);
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
        private void GetTexFileCites()
        {
            try
            {
                StreamReader sr;
                //aCites = new ArrayList(); //v2-03

                //read through all tex files looking for \cite tags
                foreach (string texFile in aTexFiles)
                {
                    sr = new StreamReader(filePath + texFile + (!texFile.EndsWith(".tex") ? ".tex" : ""));
                	string s = sr.ReadToEnd();
	                //parse the document looking for \cite tags, we'll store the contents in an arraylist so
	                //the global order of appearance is kept, and no repetition is allowed
	                while (s.IndexOf("\\cite{") != -1) //v2-01
	                {
	                    s = s.Substring(s.IndexOf("\\cite{") + 6); //v2-01
	                    string temp = s.Substring(0, s.IndexOf('}'));
	                    //v2-04: handle multiple keys inside single citation
	                    string[] cites = temp.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
	                    foreach(string c in cites)
	                    {
	                        string cite = c.TrimEnd().TrimStart(); //clear leading and trailing whitespaces
	                        aCites.Add(cite);
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
        /// read bibliography files and get all \bibitems
        /// </summary>
        private void GetBibitems()
        {
            try
            {
                StreamReader sr = new StreamReader(filePath + bibFilename);
                aBibitems = new ArrayList();
                hBibitems = new Hashtable();

                //read through the bibliography file and store \bibtems in a hashtable and an arraylist
                //in the hashtable we'll store {key, value} as {\bibitem label, \bibitem content}
                //in the arraylist we'll store only the content of the \bibitem
                string s = sr.ReadToEnd();
                while(s.IndexOf("\\bibitem") != -1)
                {
                    s = s.Substring(s.IndexOf("\\bibitem") + 8);
                    string bibitem;
                    if(s.IndexOf("\\bibitem") != -1) //this is any \bibitem in the file
                        bibitem = s.Substring(0, s.IndexOf("\\bibitem"));
                    else //this is the last \bibitem in the file
                        bibitem = s.Substring(0, s.IndexOf("\\end{"));

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
        private string GetKeyToValue(string value)
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
        private void WriteBibitems(StreamWriter sw)
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
        private void WriteBibFile()
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
                            string value = hBibitems[aCites[i].ToString()].ToString();
                            if (aBibitems.Contains(value))
                            {
                                sw.Write("\t\\bibitem{" + aCites[i].ToString() + "} " + value + "\n\n");
                                aBibitems.Remove(value);
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
                lblResult.Text = "Yay! Made it!";
                lblResult.ForeColor = System.Drawing.Color.Green;
            }
            catch 
            {
                sw.Write("\n" + postamble);
                sw.Close();
            }
        }
    }
}