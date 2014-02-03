using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

using System.IO;

using SysconCommon.Common;
using SysconCommon;
using SysconCommon.Foxpro;
using SysconCommon.Common.Environment;
using SysconCommon.Algebras.DataTables;
using SysconCommon.Algebras.DataTables.Excel.VSTO;

namespace JobDetailAnalysisDesktop
{
    public partial class ViewAuditLog : Form
    {
        private COMMethods MBAPI;
        private ProgramInfo ProgInfo;

        public ViewAuditLog(COMMethods mbapi)
        {
            MBAPI = mbapi;
            ProgInfo = mbapi.GetProgramInfo();

            InitializeComponent();
        }

        /// <summary>
        /// enter the table name here
        /// </summary>
        public string EmptyTableName = "syscon_tm_log";

        public string TargetEmptyTableName
        {
            get
            {
                return ProgInfo.SMBDir + "/" + EmptyTableName + ".dbf";
            }
        }

        public string SourceEmptyTableName
        {
            get
            {
                return Env.GetEXEDirectory() + "/" + EmptyTableName + ".dbf";
            }
        }

        // TODO: enter properties for each column
        class _EditRow
        {
            public long recnum { get; set; }
            public string usrnme { get; set; }
            public string chgdsc { get; set; }
            public DateTime chgdte { get; set; }
            public string chgpth { get; set; }
        }

        _EditRow[] EditItems = null;

        private void _Load(object sender, EventArgs e)
        {
            using (var con = Connections.GetOLEDBConnection())
            {
                // make sure the setup table exists
                if (!File.Exists(TargetEmptyTableName))
                {
                    File.Copy(SourceEmptyTableName, TargetEmptyTableName);
                    File.Copy(Env.GetEXEDirectory() + "/" + EmptyTableName + ".fpt", ProgInfo.SMBDir + "/" + EmptyTableName + ".fpt");
                }

                // TODO: remove deleted items
                // don't have to do this

                // TODO: make sure all the items exist
                // don't have to do this

                // TODO: load the EditItems array
                var EditItems_dt = con.GetDataTable("Edit Items", "select * from syscon_tm_log");
                EditItems = EditItems_dt.ToList<_EditRow>().ToArray();

                this.grdItems.DataSource = EditItems;
                this.txtFilter.KeyPress += new KeyPressEventHandler(txtFilter_KeyPress);

                this.grdItems.CellDoubleClick += new DataGridViewCellEventHandler(grdItems_CellDoubleClick);
            }
        }

        void grdItems_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // throw new NotImplementedException();
            if (e.ColumnIndex == grdItems.Columns["chgpth"].Index)
            {
                try
                {
                    var filename = grdItems[e.ColumnIndex, e.RowIndex].Value.ToString();
                    var book = ExcelAddinUtil.getWorkbook(filename);
                    book.Application.Visible = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not load audit log in excel. See error log for details.", "Error", MessageBoxButtons.OK);
                    Env.Log("{0}\r\n{1}", ex.Message, ex.StackTrace);
                }
            }
        }

        void txtFilter_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
                btnFilter_Click(null, null);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SaveChanges()
        {
            throw new NotImplementedException();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            // SaveChanges();
            this.Close();
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            if (txtFilter.Text.Trim() == "")
            {
                this.grdItems.DataSource = EditItems;
                return;
            }

            List<_EditRow> filteredItems = new List<_EditRow>();
            foreach (var i in EditItems)
            {
                foreach (var pi in typeof(_EditRow).GetProperties())
                {
                    string cell_value = pi.GetValue(i, null).ToString().Trim();
                    if (cell_value.ToUpper().Contains(txtFilter.Text.ToUpper().Trim()))
                        filteredItems.Add(i);
                }
            }

            this.grdItems.DataSource = filteredItems.ToArray();
        }
    }
}
