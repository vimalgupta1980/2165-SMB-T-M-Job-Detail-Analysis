using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using SysconCommon.Accounting;
using SysconCommon.Common;
using SysconCommon.Algebras.DataTables;
using SysconCommon.Common.Environment;

namespace JobDetailAnalysisDesktop
{
    public partial class CostCodeSelector : Form
    {
        public CostCodeSelector()
        {
            InitializeComponent();
        }

        private DataTable Data = null;

        public IEnumerable<decimal> NonBillableCostCodes
        {
            get
            {
                try
                {
                    List<decimal> rv = new List<decimal>();

                    if (Data != null)
                    {
                        foreach (var r in Data.Rows.ToIEnumerable())
                        {
                            if ((bool)r[2])
                                rv.Add((decimal)r[1]);
                        }

                        return rv;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        

        private void CostCodeSelector_Load(object sender, EventArgs e)
        {
            try
            {
                // get a list of cost codes
                var codes = Accounting.GetCostCodes();

                var _selected = Env.GetConfigVar("nonbillablecostcodes", "0", true).Split(',').Where(c => c != "");
                var selected = _selected.Select(c => decimal.Parse(c));
                Data = (from code in codes
                            select new
                            {
                                NonBillable = selected.Contains(code.Recnum),
                                CostCode = code.Recnum,
                                Description = code.Description,
                            }).ToDataTable("codes");

                this.dataGridView1.DataSource = Data;
                var ctemplate = new DataGridViewCheckBoxCell();
                this.dataGridView1.Columns["NonBillable"].CellTemplate = ctemplate;
                this.dataGridView1.Columns["Description"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                this.dataGridView1.AutoResizeColumns();
                this.Deactivate += new EventHandler(CostCodeSelector_Deactivate);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void CostCodeSelector_Deactivate(object sender, EventArgs e)
        {
            this.dataGridView1.CurrentCell = null;
        }
    }
}
