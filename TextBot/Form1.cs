//MIT License
//
//Copyright(c) 2018 XWolf Override
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using FastColoredTextBoxNS;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TextBot
{
    public partial class Form1 : Form
    {
        private string filename;
        private bool changed = false;

        public Form1()
        {
            InitializeComponent();
            // Processor Input
            Processor.doClearInput = pClearInput;
            Processor.doSetInput = pSetInput;
            Processor.doWriteInput = pWriteInput;
            Processor.doReadInput = pReadInput;
            // Processor Output
            Processor.doClear = pClear;
            Processor.doWrite = pWrite;
            Processor.doSetOutput = pSetOutput;
            Text += ' '+Program.VERSION;

            tbCode.ServiceColors = new ServiceColors();
            tbCode.AddStyle(new MarkerStyle(new SolidBrush(Color.FromArgb(40, Color.Gray))));
            tbCode.ClearStylesBuffer();
            tbCode.Range.ClearStyle(StyleIndex.All);
            tbCode.Language = Language.CSharp;
            NewCode();

            DoArgs(Environment.GetCommandLineArgs());
        }

        #region INPUT
        private void pClearInput()
        {
            tbInput.Clear();
        }

        private void pSetInput(string s)
        {
            tbInput.Text = s;
        }

        private void pWriteInput(string s)
        {
            tbInput.SelectionStart = tbInput.Text.Length;
            tbInput.SelectedText = s;
            tbInput.SelectionStart = tbInput.Text.Length;
        }

        private string pReadInput()
        {
            return tbInput.Text;
        }
        #endregion

        #region OUTPUT
        private void pClear()
        {
            tbOutput.Clear();
        }

        private void pWrite(string s)
        {
            tbOutput.SelectionStart = tbOutput.Text.Length;
            tbOutput.SelectedText = s;
            tbOutput.SelectionStart = tbOutput.Text.Length;
        }

        private void pSetOutput(string s)
        {
            tbOutput.Text = s;
        }
        #endregion

        private void DoArgs(string[] args)
        {
            if (args.Length > 1)
                try
                {
                    tbCode.Text = File.ReadAllText(args[1]);
                    filename = args[1];
                    changed = false;
                }
                catch
                {
                    MessageBox.Show("Can't open source file", "Error opening code", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
        }

        private void NewCode()
        {
            if (CloseCode())
            {
                tbCode.Text = Properties.Resources.DefaultCode;
                changed = false;
            }
        }

        private void OpenCode()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (filename != null)
                ofd.FileName = filename;
            ofd.Filter = "Text processor code script|*." + Program.PROCESSOR_C_SHARP + "|All files|*.*";
            if (ofd.ShowDialog() == DialogResult.OK && CloseCode())
            {
                filename = ofd.FileName;
                tbCode.Text = File.ReadAllText(ofd.FileName);
                changed = false;
            }
        }

        private bool CloseCode()
        {
            if (!changed)
                return true;
            switch (MessageBox.Show("Save last changes?", "Save", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
            {
                case DialogResult.Yes:
                    return SaveCode();
                case DialogResult.No:
                    return true;
                case DialogResult.Cancel:
                    return false;
            }
            return false;
        }

        private bool SaveCode()
        {
            if (filename == null)
                return SaveCodeAs();
            else
            {
                File.WriteAllText(filename, tbCode.Text);
                changed = false;
                return true;
            }
        }

        private bool SaveCodeAs()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (filename != null)
                sfd.FileName = filename;
            sfd.Filter = "Text processor code script|*." + Program.PROCESSOR_C_SHARP + "|All files|*.*";
            sfd.DefaultExt = Program.PROCESSOR_C_SHARP;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                filename = sfd.FileName;
                return SaveCode();
            }
            return false;
        }

        private void tsbPlay_Click(object sender, EventArgs e)
        {
            Processor.Execute(tbCode.Text);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            tbCode.Focus();
            tbCode.SelectionLength = 0;
            tbCode.SelectionStart = 0;
        }

        private void tsbNew_Click(object sender, EventArgs e)
        {
            NewCode();
        }

        private void tsbOpen_Click(object sender, EventArgs e)
        {
            OpenCode();
        }

        private void tsbSave_Click(object sender, EventArgs e)
        {
            SaveCode();
        }

        private void tsbSaveAs_Click(object sender, EventArgs e)
        {
            SaveCodeAs();
        }

        private void tbCode_TextChanged(object sender, EventArgs e)
        {
            changed = true;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!CloseCode())
                e.Cancel = true;
        }

        private void tsbHelp_Click(object sender, EventArgs e)
        {
            FHelp.Execute();
        }
    }
}
