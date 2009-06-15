using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;

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

                //parse document looking for \input tags, and we'll store the enclosed tex file names in an arraylist
                while (sr.Peek() >= 0)
                {
                    string l = sr.ReadLine();
                    if (l.Contains("\\input{"))
                    {
                        int i = l.IndexOf('{') + 1;
                        int c = l.IndexOf('}') - i;
                        string texFile = l.Substring(i, c);
                        if (texFile != bibFilename) aTexFiles.Add(texFile);
                    }
                }
                sr.Close();
            }
            catch { }
        }

        /// <summary>
        /// extract main file path from main tex file
        /// </summary>
        private void GetFilePath()
        {
            filePath = mainTexFile.Substring(0, mainTexFile.LastIndexOf('\\') + 1);
        }

        /// <summary>
        /// read all the project's tex files and get the contents of all \cite tags, with no repetition
        /// </summary>
        private void GetTexFileCites()
        {
            try
            {
                StreamReader sr;
                aCites = new ArrayList();

                //read through all tex files looking for \cite tags
                foreach (string texFile in aTexFiles)
                {
                    sr = new StreamReader(filePath + texFile);
                    string s = sr.ReadToEnd();
                    //parse the document looking for \cite tags, we'll store the contents in an arraylist so
                    //the global order of appearance is kept, and no repetition is allowed
                    while (s.IndexOf("~\\cite{") != -1)
                    {
                        s = s.Substring(s.IndexOf("~\\cite{") + 7);
                        string cite = s.Substring(0, s.IndexOf('}'));
                        if (!aCites.Contains(cite)) aCites.Add(cite); //así evitamos duplicados
                    }
                    sr.Close();
                }
            }
            catch { }
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
            catch { }
        }

        private string GetKeyToValue(string value)
        {
            foreach (string key in hBibitems.Keys)
            {
                if (hBibitems[key].ToString() == value) return key;
            }
            return "";
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

                //write bibliography in the same order it was previously read
                //it may seem quite dumb but it can be used to write the bibliography enclosed by pre and post ambles
                if (bibStyle == BibStyles.PLAIN)
                {
                    //we simply write the \bibitems in the file
                    foreach (string key in hBibitems.Keys)
                    {
                        sw.Write("\t\\bibitem{" + key + "} " + hBibitems[key].ToString() + "\n\n");
                    }
                }

                //write bibliography in alphabetical order of the content of the \bibitems
                if (bibStyle == BibStyles.ALPHA)
                {
                    aBibitems.Sort(); //default sorting of the arraylist is alphabetical asc
                    for (int i = 0; i < aBibitems.Count; i++)
                    {
                        string value = aBibitems[i].ToString();
                        sw.Write("\t\\bibitem{" + GetKeyToValue(value) + "} " + value + "\n\n");
                    }
                }

                //write bibliography in the order of the appearance of cites
                if (bibStyle == BibStyles.UNSRT)
                {
                    //write \bibitem in the order of appearance of cites 
                    for (int i = 0; i < aCites.Count; i++)
                    {
                        if (hBibitems.Contains(aCites[i].ToString()))
                        {
                            sw.Write("\t\\bibitem{" + aCites[i].ToString() + "} " + hBibitems[aCites[i].ToString()].ToString() + "\n\n");
                            hBibitems.Remove(aCites[i].ToString());
                        }
                    }

                    //when we've written all the cited \bibitems, there might still be some \bibitems to write (these
                    //have not been cited in document). We then write the left \bibtems in the order they were previously read
                    foreach (string key in hBibitems.Keys)
                    {
                        sw.Write("\t\\bibitem{" + key + "} " + hBibitems[key].ToString() + "\n\n");
                    }
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