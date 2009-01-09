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

            //txbMainTexFile.Text = "D:\\# MCGILL\\MSDL\\Project\\thesis_PFC.tex";
            //txbBibFilename.Text = "biblio.tex";
            //txbOutputFilename.Text = "newbib.tex";
            rtxPreamble.Text = "\\begin{thebibliography}{100}";
            rtxPostamble.Text = "\\end{thebibliography}\n\n%%%%% CLEAR DOUBLE PAGE!\n\\newpage{\\pagestyle{empty}\\cleardoublepage}";
            //bibStyle = BibStyles.UNSRT;
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

        private void GetInputFiles()
        {
            try
            {
                StreamReader sr = new StreamReader(mainTexFile);
                aTexFiles = new ArrayList();

                //buscar el inicio de documento
                while (sr.ReadLine() != "\\begin{document}") ;

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

        private void GetFilePath()
        {
            filePath = mainTexFile.Substring(0, mainTexFile.LastIndexOf('\\') + 1);
        }

        private void GetTexFileCites()
        {
            try
            {
                StreamReader sr;
                aCites = new ArrayList();

                //recorrer todos los archivos tex en busca de citas
                foreach (string texFile in aTexFiles)
                {
                    sr = new StreamReader(filePath + texFile);
                    string s = sr.ReadToEnd();
                    //buscar todas las citas del documento tex y guardarlas en un arraylist en su orden de aparición
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

        private void GetBibitems()
        {
            try
            {
                StreamReader sr = new StreamReader(filePath + bibFilename);
                aBibitems = new ArrayList();
                hBibitems = new Hashtable();

                //recorrer el documento de bibliografía y guardar los bibitems en una tabla de hash
                string s = sr.ReadToEnd();
                while(s.IndexOf("\\bibitem") != -1)
                {
                    s = s.Substring(s.IndexOf("\\bibitem") + 8);
                    string bibitem;
                    if(s.IndexOf("\\bibitem") != -1) //estamos tratando un bibitem cualquier
                        bibitem = s.Substring(0, s.IndexOf("\\bibitem"));
                    else //estamos tratando el último bibitem del documento 
                        bibitem = s.Substring(0, s.IndexOf("\\end{"));

                    string key = bibitem.Substring(1, bibitem.IndexOf('}') - 1);
                    bibitem = bibitem.Replace("{" + key + "}", "").Trim().TrimEnd(new char[] { '\n', '\t' });
                    aBibitems.Add(bibitem); //el arraylist es para ordenar alfabéticamente
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

        private void WriteBibFile()
        {
            try
            {
                StreamWriter sw = new StreamWriter(filePath + outputBibFile, false, Encoding.UTF8);

                //escribir preámbulo del documento de bibliografía
                sw.Write(preamble + "\n\n");

                //escribir la bibliografía sin orden (tal y como estaba en el documento original) --> ES TONTERÍA!
                if (bibStyle == BibStyles.PLAIN)
                {
                    //escribimos todos los bibitems a pelo
                    foreach (string key in hBibitems.Keys)
                    {
                        sw.Write("\t\\bibitem{" + key + "} " + hBibitems[key].ToString() + "\n\n");
                    }
                }

                //escribir la bibliografía en orden alfabético de los bibitems
                if (bibStyle == BibStyles.ALPHA)
                {
                    aBibitems.Sort();
                    for (int i = 0; i < aBibitems.Count; i++)
                    {
                        string value = aBibitems[i].ToString();
                        sw.Write("\t\\bibitem{" + GetKeyToValue(value) + "} " + value + "\n\n");
                    }
                }

                //escribir la bibliografía en el orden de aparición de las citas
                if (bibStyle == BibStyles.UNSRT)
                {
                    //escribir primero los bibitems en el orden de aparición de las citas
                    for (int i = 0; i < aCites.Count; i++)
                    {
                        sw.Write("\t\\bibitem{" + aCites[i].ToString() + "} " + hBibitems[aCites[i].ToString()].ToString() + "\n\n");
                        hBibitems.Remove(aCites[i].ToString());
                    }

                    //cuando se terminen las citas, escribir el resto de bibitems en el orden en que se leyeron del documento original
                    foreach (string key in hBibitems.Keys)
                    {
                        sw.Write("\t\\bibitem{" + key + "} " + hBibitems[key].ToString() + "\n\n");
                    }
                }

                sw.Write("\n" + postamble);
                sw.Close();

                System.Diagnostics.Process.Start("wordpad", "\"" + filePath + outputBibFile + "\"");
                lblResult.Text = "Yay! Made it!";
                lblResult.ForeColor = System.Drawing.Color.Green;
            }
            catch { }
        }
    }
}