﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace GitUI
{
    public partial class FormRebase : Form
    {
        public FormRebase()
        {
            InitializeComponent();
        }

        private void FormRebase_Load(object sender, EventArgs e)
        {
            string selectedHead = GitCommands.GitCommands.GetSelectedBranch();
            Currentbranch.Text = "Current branch: " + selectedHead;

            Branches.DisplayMember = "Name";
            Branches.DataSource = GitCommands.GitCommands.GetHeads(true, true);
            Branches.Select();

            EnableButtons();
        }

        private void EnableButtons()
        {


            if (GitCommands.GitCommands.InTheMiddleOfRebase())
            {
                if (this.Height < 200)
                    this.Height = 500;

                Branches.Enabled = false;
                Ok.Enabled = false;

                AddFiles.Enabled = true;
                Resolved.Enabled = !GitCommands.GitCommands.InTheMiddleOfConflictedMerge();
                Mergetool.Enabled = GitCommands.GitCommands.InTheMiddleOfConflictedMerge();
                Skip.Enabled = true;
                Abort.Enabled = true;
            }
            else
            {
                Branches.Enabled = true;
                Ok.Enabled = true;
                AddFiles.Enabled = false;
                Resolved.Enabled = false;
                Mergetool.Enabled = false;
                Skip.Enabled = false;
                Abort.Enabled = false;
            }

            if (GitCommands.GitCommands.InTheMiddleOfConflictedMerge())
            {
                if (MessageBox.Show("There are mergeconflicts and a rebase is progress, solve conflicts?", "Solve conflics", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Mergetool_Click(null, null);
                }
            }
            else
                if (GitCommands.GitCommands.InTheMiddleOfRebase())
                {
                    if (MessageBox.Show("There are no mergeconflicts and a rebase is progress, continue rebase?\n\nIf you get this dialog a few times, choose no and read output.", "Continue rebase", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        Resolved_Click(null, null);
                    }
                }
        }

        private void Mergetool_Click(object sender, EventArgs e)
        {
            new FormResolveConflicts().ShowDialog();
            EnableButtons();
        }

        private void AddFiles_Click(object sender, EventArgs e)
        {
            FormAddFiles form = new FormAddFiles();
            form.ShowDialog();
        }

        private void Resolved_Click(object sender, EventArgs e)
        {
            //Output.Text += "\n";
            //Output.Text += GitCommands.GitCommands.ContinueRebase();
            new FormProcess(GitCommands.GitCommands.ContinueRebaseCmd());
            EnableButtons();
            patchGrid1.Initialize();
        }

        private void Skip_Click(object sender, EventArgs e)
        {
            //Output.Text += "\n";
            //Output.Text += GitCommands.GitCommands.SkipRebase();
            new FormProcess(GitCommands.GitCommands.SkipRebaseCmd());
            EnableButtons();
            patchGrid1.Initialize();
        }

        private void Abort_Click(object sender, EventArgs e)
        {
            //Output.Text += "\n";
            //Output.Text += GitCommands.GitCommands.AbortRebase();
            new FormProcess(GitCommands.GitCommands.AbortRebaseCmd());
            EnableButtons();
            patchGrid1.Initialize();
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Branches.Text))
            {
                MessageBox.Show("Please select a branch");
                return;
            }

            FormProcess form = new FormProcess(GitCommands.GitCommands.RebaseCmd(Branches.Text));
            if (form.outputString.ToString().Trim() == "Current branch a is up to date.")
                MessageBox.Show("Current branch a is up to date.\nNothing to rebase.", "Rebase");
            else
                if (string.IsNullOrEmpty(form.outputString.ToString()))
                {
                    MessageBox.Show("Done.", "Rebase");
                }

            EnableButtons();
            patchGrid1.Initialize();
        }
    }
}
