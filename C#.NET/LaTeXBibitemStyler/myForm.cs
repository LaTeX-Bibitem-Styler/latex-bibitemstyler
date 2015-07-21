using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using LaTeXBibitemStyler;

/************************************************************************
 * Version 5.0 - Revised by suggestion of Oleg O. Kit                   *
 ************************************************************************
 * v5-01: add support for tags \citenum and \nocite                     *
 * v5-02: ignore commented out lines (those beginning with %)           *
 * Extra: took the chance to implement big refactors/improvements       * 
 ************************************************************************/

/************************************************************************
 * Version 4.0 - Revised by suggestion of Nuno Costa                    *
 ************************************************************************
 * v4-01: revise UNSRT generation methods, be careful when a cite has   *
 *   no matching bibitem in the bibitems hashtable!                     *
 ************************************************************************/

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
        private Operations op;

        public myForm()
        {
            InitializeComponent();

            op = new Operations();
        }

        #region [Events]

        private void myForm_Load(object sender, EventArgs e)
        {
            cbxBibStyle.DataSource = Enum.GetNames(typeof(BibStyles));
            this.Top = Screen.PrimaryScreen.Bounds.Height / 2 - this.Height / 2;
            this.Left = Screen.PrimaryScreen.Bounds.Width / 2 - this.Width / 2;

            rtxPreamble.Text = "\\begin{thebibliography}{100}";
            rtxPostamble.Text = "\\end{thebibliography}\n\n%%%%% CLEAR DOUBLE PAGE!\n\\newpage{\\pagestyle{empty}\\cleardoublepage}";
        }

        private void SearchMainTexFile_Click(object sender, EventArgs e)
        {
            lblResult.Text = "";
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = Environment.SpecialFolder.Desktop.ToString();
            ofd.Filter = "TEX Files (*.tex)|*.tex";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txbMainTexFile.Text = ofd.FileName;
                txbMainTexFile.SelectAll();
            }
        }

        private void SearchBiblioFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = Environment.SpecialFolder.Desktop.ToString();
            ofd.Filter =  "TEX Files (*.tex)|*.tex";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txbBibFilename.Text = ofd.FileName;
                txbBibFilename.SelectAll();

                string filename = txbBibFilename.Text.Substring(txbBibFilename.Text.LastIndexOf('\\') + 1);
                if (filename.EndsWith(".tex"))
                    filename = filename.Replace(".tex", "_new.tex");
                else
                    filename += "_new";

                txbOutputFilename.Text = filename;
            }
        }

        private void Run_Click(object sender, EventArgs e)
        {
            //start styling process; 
            //check if the specified main tex file exists 
            if (txbMainTexFile.Text != "" && File.Exists(txbMainTexFile.Text))
            {
                op.mainTexFile = txbMainTexFile.Text;

                if (txbBibFilename.Text != "")
                {
                    op.bibFilename = txbBibFilename.Text;
                    if (!op.bibFilename.EndsWith(".tex"))
                        op.bibFilename += ".tex";

                    if (txbOutputFilename.Text != "")
                    {
                        op.outputBibFile = txbOutputFilename.Text;
                        if (!op.outputBibFile.EndsWith(".tex"))
                            op.outputBibFile += ".tex";

                        op.preamble = rtxPreamble.Text;
                        op.postamble = rtxPostamble.Text;

                        op.bibStyle = (BibStyles)cbxBibStyle.SelectedIndex;

                        op.GetInputFiles();
                        op.GetFilePath();
                        op.GetMainTexFileCites();
                        op.GetTexFileCites();
                        op.GetBibitems();
                        if (op.WriteBibFile())
                        {
                            lblResult.Text = "Yay! Made it!";
                            lblResult.ForeColor = System.Drawing.Color.Green;
                        }
                        else
                        {
                            lblResult.Text = "Oh! Something went wrong while generating the output file! :(";
                            lblResult.ForeColor = System.Drawing.Color.Red;
                        }
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

        #endregion

       
    }
}