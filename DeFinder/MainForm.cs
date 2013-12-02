using denolk.DeFinder.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace denolk.DeFinder
{
    public partial class MainForm : Form
    {
        private FileService _searchService;

        public MainForm()
        {
            InitializeComponent();
            listBox.SelectionMode = SelectionMode.One;
            _searchService = new FileService();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F1 || keyData == Keys.F2)
            {
                txtDirectory.Focus();
                return true;
            }
            else if (keyData == Keys.F3)
            {
                txtKeyword.Focus();
                return true;
            }

            else if (keyData == Keys.F4)
            {
                listBox.Focus();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void txtKeyword_KeyUp(object sender, KeyEventArgs e)
        {
            FilterSearch(txtKeyword.Text);
        }

        private void txtDirectory_KeyUp(object sender, KeyEventArgs e)
        {
            var rebuildAllIndex = e.Modifiers == Keys.Control;
            if (e.KeyCode == Keys.Enter && txtDirectory.Text.Length > 0)
            {
                var input = txtDirectory.Text.ToLowerInvariant().Split(';').ToList();
                GetFileListAsync(input, rebuildAllIndex);
            }
        }

        private void listBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listBox.Items.Count > 0)
            {
                var selected = new List<string> { listBox.SelectedValue.ToString() };
                OpenSelectedFiles(selected, false);
            }
        }

        private void listBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (listBox.Items.Count > 0)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    var selected = listBox.SelectedItems.OfType<string>().ToList();
                    OpenSelectedFiles(selected, e.Control);
                }
            }

        }

        private void GetFileList(List<string> directories, bool rebuild)
        {
            ToggleLoading(true);
            var list = _searchService.GetFiles(directories, rebuild);
            SetData(list);
        }

        private async void GetFileListAsync(List<string> directories, bool rebuild)
        {
            ToggleLoading(true);
            var list = await _searchService.GetFilesAsync(directories, rebuild);
            SetData(list);
        }

        private void FilterSearch(string keyword)
        {
            var list = _searchService.Filter(keyword);
            SetData(list);
        }

        private void OpenSelectedFiles(List<string> filenames, bool isDirectory)
        {
            foreach (var f in filenames)
            {
                var target = f;
                if (isDirectory)
                {
                    target = Path.GetDirectoryName(f);
                }
                Process.Start(target);
            }
        }

        private void ToggleLoading(bool isVisible)
        {
            busyBox.Visible = isVisible;
            listBox.Enabled = !isVisible;
            txtDirectory.Enabled = !isVisible;
        }

        private void SetData(List<string> list)
        {
            ToggleLoading(true);
            listBox.DataSource = list;
            ToggleLoading(false);
            var seconds = _searchService.GetElapsedSeconds();
            Text = string.Format("deFinder - Result: {0} items in {1:0.000} secs.", list.Count, seconds);
        }
    }
}
