using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Data.OleDb;

using SysconCommon.Common.Environment;
using SysconCommon.Common;
using SysconCommon.Algebras.DataTables;
using SysconCommon.Accounting;
using SysconCommon.GUI;
using SysconCommon.Foxpro;
using SysconCommon.Algebras.DataTables.Excel.VSTO;

using Microsoft.Office.Interop.Excel;

namespace JobDetailAnalysisDesktop
{
    public partial class JobDetailAnalysis : Form
    {
        private SysconCommon.COMMethods mbapi = new SysconCommon.COMMethods();
        public JobDetailAnalysis()
        {
            InitializeComponent();

            Env.CopyOldConfigs();

#if false
            Env.ConfigInjector = (name) =>
            {
                if (new string[] { "mbdir", "product_id", "product_version", "run_count" }.Contains(name))
                {
                    Env.SetConfigFile(Env.ConfigDataPath + "/config.xml");
                    return name;
                }
                else if (new string[] { "tmtypes", "nonbillablecostcodes" }.Contains(name))
                {
                    // return "dataset" + Env.GetConfigVar("datadir", "c:\\mb7\\sample company\\", true).GetMD5Sum() + "/" + name;
                    Env.SetConfigFile(Env.GetConfigVar("mbdir") + "/syscon_tm_analysis_config.xml");
                    return name;
                }
                else
                {
                    Env.SetConfigFile(Env.GetEXEDirectory() + "/config.xml");
                    var username = WindowsIdentity.GetCurrent().Name;
                    return "dataset" + (Env.GetConfigVar("mbdir", "c:\\mb7\\sample company\\", true) + username).GetMD5Sum() + "/" + name;
                }
            };
#else   
            Env.ConfigInjector = (name) =>
            {
                var dataset_specific = new string[] { "tmtypes", "nonbillablecostcodes" };

                if (dataset_specific.Contains(name))
                {
                    Env.SetConfigFile(Env.GetConfigVar("mbdir") + "/syscon_tm_analysis_config.xml");
                    return name;
                }
                else
                {
                    Env.SetConfigFile(Env.ConfigDataPath + "/config.xml");
                    return name;
                }
            };
#endif
        }

        private void SetupTrial(int daysLeft)
        {
            var msg = string.Format("You have {0} days left to evaluate this software", daysLeft);
            this.demoLabel.Text = msg;
            btnRunReport.Enabled = true;
        }

        private void SetupInvalid()
        {
            btnRunReport.Enabled = false;
            this.demoLabel.Text = "Your License has expired or is invalid";
        }

        private void SetupFull()
        {
            btnRunReport.Enabled = true;
            this.demoLabel.Text = "";
            this.activateToolStripMenuItem.Visible = false;
        }

        private const string DefaultTemplate = "T&M Job Detail Analysis Template v09.xlsm";

        private string Template
        {
            get
            {
                var template = Env.GetConfigVar("template", Env.GetEXEDirectory() + @"\" + DefaultTemplate, true);
                if (!File.Exists(template))
                {
                    var dflt = Env.GetEXEDirectory() + @"\" + DefaultTemplate;
                    if (!File.Exists(dflt))
                    {
                        Env.Log("Error: Default Template ({0}) does not exist.", dflt);
                    }
                    else
                    {
                        Env.SetConfigVar("template", dflt);
                    }

                    selectTemplateToolStripMenuItem_Click(null, null);

                    // recursive call
                    return Template;
                }

                return template;
            }
            set
            {
                Env.SetConfigVar("template", value);
            }
        }

        bool loaded = false;

        private void JobDetailAnalysis_Load(object sender, EventArgs e)
        {
            // resets it everytime it is run so that the user can't just change to a product they already have a license for
            Env.SetConfigVar("product_id", 178504);
            
            var product_id = Env.GetConfigVar("product_id", 0, false);
            var product_version = "2.1.2.0";
            bool require_login = false;

            if (!loaded)
            {
                require_login = true;
                loaded = true;
                this.Text += " (version " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + ")";
            }

            try
            {
                var license = SysconCommon.Protection.ProtectionInfo.GetLicense(product_id, product_version, 15751);


                if (license.IsTrial)
                {
                    if (!license.IsValid())
                    {
                        SetupInvalid();
                    }
                    else
                    {
                        var l = license as SysconCommon.Protection.TrialLicense;
                        SetupTrial(l.DaysLeft);
                    }
                }
                else
                {
                    SetupFull();
                }
            }
            catch
            {
                SetupInvalid();
            }
            
            // var datadir = Env.GetConfigVar("mbdir", "c:\\mb7\\sample company\\", true);

            //SysconCommon.Accounting.MasterBuilder.Job.SetCache("select * from actrec");

            txtDataDir.TextChanged +=new EventHandler(txtDataDir_TextChanged);

            if (require_login)
            {
                mbapi.smartGetSMBDir();

                if (mbapi.RequireSMBLogin() == null)
                    this.Close();
            }

            txtDataDir.Text = mbapi.smartGetSMBDir();
        }


        private void LoadValues()
        {
            cmbEndPeriod.SelectItem<string>(p => p == Env.GetConfigVar("endperiod", "12", true));
            cmbStartingPeriod.SelectItem<string>(p => p == Env.GetConfigVar("startperiod", "0", true));
            chkUnbilled.Checked = Env.GetConfigVar("ExportUnbilledOnly", false, true);
        }

        private void txtDataDir_TextChanged(object sender, EventArgs e)
        {
            // Env.SetConfigVar("datadir", txtDataDir.Text);
            SysconCommon.Common.Environment.Connections.SetOLEDBFreeTableDirectory(txtDataDir.Text);
            LoadValues();
        }

        private void btnSelectDir_Click(object sender, EventArgs e)
        {

        }

        private void PopulateTemplate(string template, long[] jobnums, int strprd, int endprd, DateTime trndate, decimal[] nonbillablecstcdes)
        {
            using (var con = SysconCommon.Common.Environment.Connections.GetOLEDBConnection())
            {
                using (Env.TempDBFPointer
                    _jobcst = con.GetTempDBF(),
                    _jobnums = con.GetTempDBF(),
                    _blgqty = con.GetTempDBF(),
                    _nobill = con.GetTempDBF(),
                    _sources = con.GetTempDBF(),
                    _csttyps = con.GetTempDBF(),
                    _phases = con.GetTempDBF())
                {
                    using (var progress = new ProgressDialog(9))
                    {
                        progress.Text = "Select Job Cost Records";
                        progress.Show();

                        // first create a table of job numbers to join against
                        con.ExecuteNonQuery("create table {0} (jobnum n(10,0) not null)", _jobnums);
                        foreach(var j in jobnums) 
                        {
                            con.ExecuteNonQuery("insert into {0} (jobnum) values ({1})", _jobnums, j);
                        }

                        con.ExecuteNonQuery("select"
                                + " jobcst.recnum"
                                + ", actrec.estemp as estnum"
                                + ", jobcst.empnum"
                                + ", jobcst.vndnum"
                                + ", jobcst.eqpnum"
                                + ", 000000 as tmemln"
                                + ", 000000 as tmeqln"
                                + ", jobcst.jobnum"
                                + ", actrec.jobnme"
                                + ", alltrim(reccln.clnnme) as clnnme"
                                + ", jobcst.trnnum"
                                + ", jobcst.dscrpt"
                                + ", jobcst.trndte"
                                + ", jobcst.actprd"
                                + ", jobcst.srcnum"
                                + ", jobcst.status"
                                + ", jobcst.bllsts"
                                + ", jobcst.phsnum"
                                + ", jobphs.phsnme"
                                + ", jobcst.cstcde"
                                + ", cstcde.cdenme"
                                + ", jobcst.csttyp"
                                + ", jobcst.csthrs"
                                + ", IIF(jobcst.acrinv = 0 AND jobcst.bllsts = 1 AND jobcst.csttyp = 2"
                                    + ", jobcst.blgqty"
                                    + ", 0000000000) as blgpnd"
                                + ", jobcst.eqpqty"
                                + ", jobcst.blgqty"
                                + ", jobcst.paytyp"
                                + ", jobcst.cstamt"
                                + ", jobcst.blgttl"
                                + ", jobcst.acrinv"
                                + ", IIF("
                                    + "jobcst.csttyp = 2, 'Employee    ',"
                                    + "IIF(jobcst.csttyp = 3, 'Equipment    ',"
                                    + "IIF(jobcst.empnum <> 0, 'Employee    ',"
                                    + "IIF(jobcst.eqpnum <> 0, 'Equipment   ',"
                                    + "IIF(jobcst.vndnum <> 0, 'Vendor      ',"
                                    + "'None        '))))) as empeqpvnd"
                                + ", [                                                        ] as ename"
                                + ", 00000000.00000000 as rate01"
                                + ", 00000000.00000000 as rate02"
                                + ", 00000000.00000000 as rate03"
                                + ", 00000000.00000000 as minhrs"
                                + ", 00000001.00000000 as markup"
                                + ", 0000000000000.00000000 as estbll"
                                + ", jobcst.eqpnum"
                                + ", eqpmnt.eqpnme"
                                + ", jobcst.blgunt"
                                + ", jobcst.empnum"
                                + ", employ.fullst"
                                + ", jobcst.blgamt"
                                + ", jobcst.ntetxt"
                                + ", estmtr.fullst as estnme"
                                + ", nvl(sprvsr.fullst, [No Supervisor               ]) as sprvsr"
                            + " from jobcst"
                            + " join actrec on jobcst.jobnum = actrec.recnum"
                            + " join reccln on reccln.recnum = actrec.clnnum"
                            + " join employ estmtr on actrec.estemp = estmtr.recnum"
                            + " left join employ sprvsr on actrec.sprvsr = sprvsr.recnum"
                            + " left join cstcde on cstcde.recnum = jobcst.cstcde"
                            + " join {0} _jobnums on _jobnums.jobnum = jobcst.jobnum"
                            + " left join eqpmnt on eqpmnt.recnum = jobcst.eqpnum"
                            + " left join employ on employ.recnum = jobcst.empnum"
                            + " left join jobphs on jobphs.recnum = jobcst.jobnum and jobphs.phsnum = jobcst.phsnum"
                            // + " and between(jobcst.actprd, {1}, {2})"
                            + " where jobcst.actprd >= {1}"
                            + (chkUnbilled.Checked ? " and jobcst.bllsts = 1" : "")
                            + " and jobcst.actprd <= {2}"
                            + " and jobcst.status <> 2"
                            + " and jobcst.trndte <= {3}"
                            + " order by jobcst.recnum"
                            + " into table {4}"
                            , _jobnums, strprd, endprd, trndate.ToFoxproDate(), _jobcst);

                        progress.Tick();
                        progress.Text = "Selecting Names";

                        con.ExecuteNonQuery("update _jobcst set ename = alltrim(str(empnum)) + [ - ] + alltrim(employ.fullst)"
                            + " from {0} _jobcst"
                            + " join employ on _jobcst.empnum = employ.recnum"
                            + " where empeqpvnd = 'Employee'"
                            , _jobcst);

                        con.ExecuteNonQuery("update _jobcst set ename = alltrim(str(vndnum)) + [ - ] + alltrim(actpay.vndnme)"
                            + " from {0} _jobcst"
                            + " join actpay on _jobcst.vndnum = actpay.recnum"
                            + " where empeqpvnd = 'Vendor'"
                            , _jobcst);

                        con.ExecuteNonQuery("update _jobcst set ename = alltrim(str(eqpnum)) + [ - ] + alltrim(eqpmnt.eqpnme)"
                            + " from {0} _jobcst"
                            + " join eqpmnt on _jobcst.eqpnum = eqpmnt.recnum"
                            + " where empeqpvnd = 'Equipment'"
                            , _jobcst);

                        progress.Tick();
                        progress.Text = "Locating T&M Line items";

                        con.ExecuteNonQuery("update _jobcst set _jobcst.tmemln = tmemln.linnum"
                            + " from {0} _jobcst"
                            + " join timmat on timmat.recnum = _jobcst.jobnum"
                            + " join tmemln on"
                                + " tmemln.recnum = timmat.emptbl"
                                + " and timmat.emptbl <> 0"
                                + " and tmemln.empnum = 0"
                                + " and tmemln.cstcde = _jobcst.cstcde"
                            + " where _jobcst.csttyp = 2"
                            , _jobcst);

                        con.ExecuteNonQuery("update _jobcst set _jobcst.tmemln = tmemln.linnum"
                            + " from {0} _jobcst"
                            + " join timmat on timmat.recnum = _jobcst.jobnum"
                            + " join tmemln on"
                                + " tmemln.recnum = timmat.emptbl"
                                + " and timmat.emptbl <> 0"
                                + " and tmemln.empnum = _jobcst.empnum"
                                + " and tmemln.cstcde = _jobcst.cstcde"
                            + " where _jobcst.empnum <> 0 AND _jobcst.csttyp = 2"
                            , _jobcst);

                        con.ExecuteNonQuery("update _jobcst set _jobcst.tmeqln = tmeqln.linnum"
                            + " from {0} _jobcst"
                            + " join timmat on timmat.recnum = _jobcst.jobnum"
                            + " join tmeqln on tmeqln.recnum = timmat.eqptbl"
                            , _jobcst);

                        progress.Tick();
                        progress.Text = "Loading T&M Rates";

                        con.ExecuteNonQuery("update _jobcst set"
                                + " _jobcst.rate01 = tmeqln.oprrte"
                                + ", _jobcst.rate02 = tmeqln.stdrte"
                                + ", _jobcst.rate03 = tmeqln.idlrte"
                            + " from {0} _jobcst"
                            + " join timmat on timmat.recnum = _jobcst.jobnum"
                            + " join tmeqln on tmeqln.recnum = timmat.eqptbl and tmeqln.linnum = _jobcst.tmeqln"
                            + " where _jobcst.tmeqln <> 0"
                            , _jobcst);

                        con.ExecuteNonQuery("update _jobcst set"
                                + " _jobcst.rate01 = tmemln.rate01"
                                + ", _jobcst.rate02 = tmemln.rate02"
                                + ", _jobcst.rate03 = tmemln.rate03"
                            + " from {0} _jobcst"
                            + " join timmat on timmat.recnum = _jobcst.jobnum"
                            + " join tmemln on tmemln.recnum = timmat.emptbl and tmemln.linnum = _jobcst.tmemln"
                            + " where _jobcst.tmemln <> 0"
                            , _jobcst);

                        progress.Tick();
                        progress.Text = "Loading Minimum Hours";

                        con.ExecuteNonQuery("update _jobcst set _jobcst.minhrs = tmeqln.minhrs"
                            + " from {0} _jobcst"
                            + " join timmat on timmat.recnum = _jobcst.jobnum"
                            + " join tmeqln on tmeqln.recnum = timmat.eqptbl and tmeqln.linnum = _jobcst.tmeqln"
                            + " where _jobcst.tmeqln <> 0"
                            , _jobcst);

                        con.ExecuteNonQuery("update _jobcst set _jobcst.minhrs = tmemln.minhrs"
                            + " from {0} _jobcst"
                            + " join timmat on timmat.recnum = _jobcst.jobnum"
                            + " join tmemln on tmemln.recnum = timmat.emptbl and tmemln.linnum = _jobcst.tmemln"
                            + " where _jobcst.tmemln <> 0"
                            , _jobcst);

                        progress.Tick();
                        progress.Text = "Selecting Markup Values";

                        con.ExecuteNonQuery("update _jobcst set _jobcst.markup ="
                                + " (1.00 + (timmat.mtrhdn / 100.00)) *"
                                + " (1.00 + (timmat.mtrshw / 100.00)) *"
                                + " (1.00 + (timmat.mtrovh / 100.00)) *"
                                + " (1.00 + (timmat.mtrpft / 100.00))"
                            + " from {0} _jobcst"
                            + " join timmat on timmat.recnum = _jobcst.jobnum"
                            + " where _jobcst.csttyp = 1"
                            , _jobcst);

                        con.ExecuteNonQuery("update _jobcst set _jobcst.markup ="
                                + " (1.00 + (timmat.labhdn / 100.00)) *"
                                + " (1.00 + (timmat.labshw / 100.00)) *"
                                + " (1.00 + (timmat.labovh / 100.00)) *"
                                + " (1.00 + (timmat.labpft / 100.00))"
                            + " from {0} _jobcst"
                            + " join timmat on timmat.recnum = _jobcst.jobnum"
                            + " where _jobcst.csttyp = 2"
                            , _jobcst);

                        con.ExecuteNonQuery("update _jobcst set _jobcst.markup ="
                                + " (1.00 + (timmat.eqphdn / 100.00)) *"
                                + " (1.00 + (timmat.eqpshw / 100.00)) *"
                                + " (1.00 + (timmat.eqpovh / 100.00)) *"
                                + " (1.00 + (timmat.eqppft / 100.00))"
                            + " from {0} _jobcst"
                            + " join timmat on timmat.recnum = _jobcst.jobnum"
                            + " where _jobcst.csttyp = 3"
                            , _jobcst);

                        con.ExecuteNonQuery("update _jobcst set _jobcst.markup ="
                                + " (1.00 + (timmat.subhdn / 100.00)) *"
                                + " (1.00 + (timmat.subshw / 100.00)) *"
                                + " (1.00 + (timmat.subovh / 100.00)) *"
                                + " (1.00 + (timmat.subpft / 100.00))"
                            + " from {0} _jobcst"
                            + " join timmat on timmat.recnum = _jobcst.jobnum"
                            + " where _jobcst.csttyp = 4"
                            , _jobcst);

                        con.ExecuteNonQuery("update _jobcst set _jobcst.markup ="
                                + " (1.00 + (timmat.otrhdn / 100.00)) *"
                                + " (1.00 + (timmat.otrshw / 100.00)) *"
                                + " (1.00 + (timmat.otrovh / 100.00)) *"
                                + " (1.00 + (timmat.otrpft / 100.00))"
                            + " from {0} _jobcst"
                            + " join timmat on timmat.recnum = _jobcst.jobnum"
                            + " where _jobcst.csttyp = 5"
                            , _jobcst);

                        con.ExecuteNonQuery("update _jobcst set _jobcst.markup ="
                                + " (1.00 + (timmat.cs6hdn / 100.00)) *"
                                + " (1.00 + (timmat.cs6shw / 100.00)) *"
                                + " (1.00 + (timmat.cs6ovh / 100.00)) *"
                                + " (1.00 + (timmat.cs6pft / 100.00))"
                            + " from {0} _jobcst"
                            + " join timmat on timmat.recnum = _jobcst.jobnum"
                            + " where _jobcst.csttyp = 6"
                            , _jobcst);

                        con.ExecuteNonQuery("update _jobcst set _jobcst.markup ="
                                + " (1.00 + (timmat.cs7hdn / 100.00)) *"
                                + " (1.00 + (timmat.cs7shw / 100.00)) *"
                                + " (1.00 + (timmat.cs7ovh / 100.00)) *"
                                + " (1.00 + (timmat.cs7pft / 100.00))"
                            + " from {0} _jobcst"
                            + " join timmat on timmat.recnum = _jobcst.jobnum"
                            + " where _jobcst.csttyp = 7"
                            , _jobcst);

                        con.ExecuteNonQuery("update _jobcst set _jobcst.markup ="
                                + " (1.00 + (timmat.cs8hdn / 100.00)) *"
                                + " (1.00 + (timmat.cs8shw / 100.00)) *"
                                + " (1.00 + (timmat.cs8ovh / 100.00)) *"
                                + " (1.00 + (timmat.cs8pft / 100.00))"
                            + " from {0} _jobcst"
                            + " join timmat on timmat.recnum = _jobcst.jobnum"
                            + " where _jobcst.csttyp = 8"
                            , _jobcst);

                        // BUG in SMB database, cs9pft is type C!!!!
                        con.ExecuteNonQuery("update _jobcst set _jobcst.markup ="
                                + " (1.00 + (timmat.cs9hdn / 100.00)) *"
                                + " (1.00 + (timmat.cs9shw / 100.00)) *"
                                + " (1.00 + (timmat.cs9ovh / 100.00)) *"
                                + " (1.00 + (val(timmat.cs9pft) / 100.00))"
                            + " from {0} _jobcst"
                            + " join timmat on timmat.recnum = _jobcst.jobnum"
                            + " where _jobcst.csttyp = 9"
                            , _jobcst);

                        progress.Tick();
                        progress.Text = "Selecting Billing Quantities";

                        con.ExecuteNonQuery("select _jobcst.recnum, 00000000.00000000 as hrs, cast(null as n(3)) as typ from {0} _jobcst into table {1}", _jobcst, _blgqty);

                        con.ExecuteNonQuery("update blgqty set hrs = _jobcst.csthrs, typ = _jobcst.paytyp"
                            + " from {0} blgqty"
                            + " join {1} _jobcst on blgqty.recnum = _jobcst.recnum"
                            + " where _jobcst.csttyp = 2"
                            , _blgqty, _jobcst);

                        con.ExecuteNonQuery("update blgqty set hrs = _jobcst.blgqty, typ = _jobcst.paytyp"
                            + " from {0} blgqty"
                            + " join {1} _jobcst on blgqty.recnum = _jobcst.recnum"
                            + " where _jobcst.csttyp = 2 and blgqty.hrs = 0 and _jobcst.bllsts = 1"
                            , _blgqty, _jobcst);

                        con.ExecuteNonQuery("update blgqty set hrs = _jobcst.eqpqty, typ = eqpmnt.eqptyp"
                            + " from {0} blgqty"
                            + " join {1} _jobcst on blgqty.recnum = _jobcst.recnum"
                            + " join eqpmnt on _jobcst.eqpnum = eqpmnt.recnum"
                            + " where _jobcst.csttyp = 3 and _jobcst.eqpnum <> 0 and eqpmnt.eqptyp <> 0"
                            , _blgqty, _jobcst);

                        progress.Tick();
                        progress.Text = "Calculating Billing Amounts";

                        con.ExecuteNonQuery("update _jobcst set _jobcst.estbll = "
                                + "IIF(typ = 1 and rate01 <> 0, hrs*rate01,"
                                + "IIF(typ = 2 and rate02 <> 0, hrs*rate02,"
                                + "IIF(typ = 3 and rate03 <> 0, hrs*rate03,"
                                + "cstamt)))"
                            + " from {0} _jobcst"
                            + " join {1} blgqty on blgqty.recnum = _jobcst.recnum"
                            , _jobcst, _blgqty);

                        con.ExecuteNonQuery("update _jobcst set estbll = cstamt"
                            + " from {0} _jobcst"
                            + " join eqpmnt on eqpmnt.recnum = _jobcst.eqpnum"
                            + " where _jobcst.csttyp = 3 and (isnull(eqpmnt.eqptyp) or eqpmnt.eqptyp = 0)"
                            , _jobcst);

                        con.ExecuteNonQuery("update {0} set estbll = estbll * markup", _jobcst);

                        con.ExecuteNonQuery("update {0} set estbll = 0 where bllsts = 2", _jobcst);

                        con.ExecuteNonQuery("update {0} set estbll = blgttl where bllsts = 3", _jobcst);

                        con.ExecuteNonQuery("update {0} set estbll = blgamt where estbll = 0", _jobcst);

                        // build a list of nonbillable cost codes
                        con.ExecuteNonQuery("create table {0} (cstcde n(18,3) not null)", _nobill);

                        foreach (var cstcde in nonbillablecstcdes)
                        {
                            con.ExecuteNonQuery("insert into {0} (cstcde) values ({1})", _nobill, cstcde);
                        }

                        // make sure those cost codes are not billed
                        con.ExecuteNonQuery("update _jobcst set estbll = 0 from {0} _jobcst join {1} _nobill on _nobill.cstcde = _jobcst.cstcde" 
                            + " where _jobcst.acrinv = 0" 
                            , _jobcst, _nobill);
                        // set their bllsts to unbillable cost code
                        // v2.1.2 BUT only for items that have not already been invoiced
                        con.ExecuteNonQuery("update _jobcst set bllsts = 4 from {0} _jobcst join {1} _nobill on _nobill.cstcde = _jobcst.cstcde" 
                            + " where _jobcst.acrinv = 0"
                            , _jobcst, _nobill);

                        progress.Tick();
                        progress.Text = "Selecting References";

                        con.ExecuteNonQuery("select distinct _jobcst.srcnum as recnum, source.srcnme, source.srcdsc"
                            + " from {0} _jobcst"
                            + " left join source on source.recnum = _jobcst.srcnum"
                            + " into table {1}"
                            , _jobcst, _sources);

                        con.ExecuteNonQuery("select distinct _jobcst.csttyp as recnum, csttyp.typnme"
                            + " from {0} _jobcst"
                            + " left join csttyp on csttyp.recnum = _jobcst.csttyp"
                            + " into table {1}"
                            , _jobcst, _csttyps);

                        con.ExecuteNonQuery("select distinct _jobcst.phsnum, jobphs.phsnme, alltrim(str(_jobcst.phsnum)) + [ - ] + alltrim(jobphs.phsnme) as phsdsc"
                            + " from {0} _jobcst"
                            + " join jobphs on jobphs.recnum = _jobcst.jobnum and jobphs.phsnum = _Jobcst.phsnum"
                            + " into table {1}"
                            , _jobcst, _phases);

                        con.ExecuteNonQuery("insert into {0} (phsnum, phsnme, phsdsc) values (0, [None], [0 - None])", _phases);

                        /* if we want a summary, do the totals now */
                        var is_summary = chkSumCustomer.Checked || chkSumJob.Checked || chkSumPeriod.Checked;
                        Env.TempDBFPointer _jobcstsum = null;

                        if (is_summary)
                        {
                            var groupCols = new List<string>();
                            if(chkSumCustomer.Checked) groupCols.Add("clnnme");
                            if (chkSumJob.Checked) { groupCols.Add("jobnum"); groupCols.Add("jobnme"); groupCols.Add("clnnme"); }
                            if(chkSumPeriod.Checked) groupCols.Add("actprd");

                            var sum_cols = new string[] 
                            {
                                "cstamt", "blgttl", "estbll", "blgqty", "blgpnd", "blgamt"
                            };

                            _jobcstsum = con.Summarize(_jobcst, groupCols.Uniq().ToArray(), sum_columns: sum_cols);
                        }

                        progress.Tick();
                        progress.Text = "Loading Excel Sheet";

                        ExcelAddinUtil.UseNewApp();
                        
                        try
                        {
                            var details_dt = con.GetDataTable("Details", "select * from {0}", _jobcstsum != null ? _jobcstsum : _jobcst);
                            details_dt.ConfigurableWriteToExcel(template, "ImportData", "DetailData");

                            var sources_dt = con.GetDataTable("Sources", "select * from {0}", _sources);
                            sources_dt.ConfigurableWriteToExcel(template, "References", "Sources");

                            var csttyps_dt = con.GetDataTable("Cost Types", "select * from {0}", _csttyps);
                            csttyps_dt.ConfigurableWriteToExcel(template, "References", "CostTypes");

                            var jobphs_dt = con.GetDataTable("Phases", "select * from {0}", _phases);
                            jobphs_dt.ConfigurableWriteToExcel(template, "References", "JobPhases");

                            ExcelAddinUtil.app.Visible = true;

                            // auto-fit row heights
                            ExcelAddinUtil.app.ActiveWorkbook.getWorksheet("Job Detail").Cells.Rows.AutoFit();

                            // if the template has the unbilled by job pivot table, go ahead and set it's settings
                            try
                            {
                                if (_jobcstsum == null)
                                {
                                    /* set the pivot table options and refresh data for the unbilled by job sheet */
                                    var unbilled_ptable = ExcelAddinUtil.app.ActiveWorkbook.getWorksheet("Unbilled By Job").PivotTables("ByJob");
                                    unbilled_ptable.PivotCache.Refresh();

                                    /* set the billing status filter to 'Unbilled' */
                                    unbilled_ptable.PivotFields("Billing Status").ClearAllFilters();
                                    unbilled_ptable.PivotFields("Billing Status").CurrentPage = "Unbilled";

                                    /* sort the pivot table by date */
                                    unbilled_ptable.PivotFields("Date").AutoSort(1, "Date");

                                    /* hide the "Job" that shows up for unpopulated rows in the source data */
                                    var missing = Missing.Value;
                                    unbilled_ptable.PivotFields("Job").PivotFilters.Add(16, missing, "0 - 0 - 0 - 0", missing, missing, missing, missing, missing);

                                    /* collapse to dates */
                                    unbilled_ptable.PivotFields("Date").ShowDetail = false;

                                    /* format numbers in the pivot table */
                                    Worksheet ws = ExcelAddinUtil.app.ActiveWorkbook.getWorksheet("Unbilled By Job");
                                    Range r = ws.get_Range("B:H");
                                    r.NumberFormat = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                                    r = ws.get_Range("D:D,B:B");
                                    r.NumberFormat = "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)";
                                }
                            }
                            catch { }
                        }
                        finally
                        {
                            ExcelAddinUtil.app.Visible = true;
                        }
                    }
                }
            }
        }

        private void btnRunReport_Click(object sender, EventArgs e)
        {
            if (!File.Exists(Template))
            {
                selectTemplateToolStripMenuItem_Click(null, null);
            }

            SysconCommon.Common.Environment.Connections.SetOLEDBFreeTableDirectory(txtDataDir.Text);

            var nonbillable = Env.GetConfigVar("nonbillablecostcodes", "0", true)
                .Split(',')
                .Where(i => i != "")
                .Select(i => decimal.Parse(i))
                .ToArray();

            var validjobtypes_delim = Env.GetConfigVar<string>("tmtypes", "", true);
            var validjobtypes_strs = validjobtypes_delim.Split(',');
            var validjobtypes = validjobtypes_delim.Trim() == "" ? new long[] { } : validjobtypes_strs.Select(i => Convert.ToInt64(i));

            using (var con = SysconCommon.Common.Environment.Connections.GetOLEDBConnection())
            {
                long[] jobs = null;

                if (this.radioShowTMJobs.Checked)
                {
                    using (var jobtyps = con.GetTempDBF())
                    {
                        con.ExecuteNonQuery("create table {0} (jobtyp n(3, 0))", jobtyps);
                        foreach (var jt in validjobtypes)
                        {
                            con.ExecuteNonQuery("insert into {0} (jobtyp) values ({1})", jobtyps, jt);
                        }

                        jobs = (from x in con.GetDataTable("Jobnums", "select actrec.recnum from actrec join {0} jobtypes on actrec.jobtyp = jobtypes.jobtyp", jobtyps).Rows
                                select Convert.ToInt64(x["recnum"])).ToArray();
                    }
                }

                var job_selector = new MultiJobSelector(jobs);

                job_selector.ShowDialog();

                var jobnums = job_selector.SelectedJobNumbers.ToArray();
                if (jobnums.Length > 0)
                {
                    PopulateTemplate(Template, jobnums,
                        Convert.ToInt32(cmbStartingPeriod.SelectedItem), Convert.ToInt32(cmbEndPeriod.SelectedItem), dteTransactionDate.Value, nonbillable);
                }
                else
                {
                    MessageBox.Show("No jobs selected.", "Error", MessageBoxButtons.OK);
                }
            }
        }
        

        private void cmbEndPeriod_SelectedIndexChanged(object sender, EventArgs e)
        {
            Env.SetConfigVar("endperiod", cmbEndPeriod.SelectedItem);
        }

        private void cmbStartingPeriod_SelectedIndexChanged(object sender, EventArgs e)
        {
            Env.SetConfigVar("startperiod", cmbStartingPeriod.SelectedItem);
        }



        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var frm = new About();
            frm.ShowDialog();
        }

        private void onlineHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://syscon-inc.com/product-support/2165/support.asp");
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void activateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var product_id = Env.GetConfigVar("product_id", 0, false);
            var product_version = Env.GetConfigVar("product_version", "0.0.0.0", false);

            var frm = new SysconCommon.Protection.ProtectionPlusOnlineActivationForm(product_id, product_version);
            frm.ShowDialog();
            this.OnLoad(null);
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void selectNonBillableCostCodesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var frm = new CostCodeSelector();
            frm.ShowDialog();
            var nonbillable = frm.NonBillableCostCodes.Select(i => i.ToString()).ToArray();
            var nonbillablecostcodes = string.Join(",", nonbillable);
            Env.SetConfigVar("nonbillablecostcodes", nonbillablecostcodes);
        }

        private void selectTMJobTypesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var frm = new TMTypes();
            frm.ShowDialog();
            var tmtypes = frm.TimeAndMaterialTypes;
            var tmtypes_delimited = string.Join(",", tmtypes);
            Env.SetConfigVar("tmtypes", tmtypes_delimited);

            // make sure the correct jobs show up now
            LoadValues();
        }

        private void selectMBDirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new FolderBrowserDialog();
            dlg.SelectedPath = Env.GetConfigVar("mbdir");
            var rslt = dlg.ShowDialog();
            if (rslt == DialogResult.OK)
            {
                var dir = dlg.SelectedPath + "\\";
                if (!File.Exists(dir + "actrec.dbf"))
                {
                    MessageBox.Show("Please choose a valid MB7 Path");
                }
                else
                {
                    this.txtDataDir.Text = dir;
                }
            }
        }

        private void selectTemplateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var current_template = Template;
            if (!File.Exists(current_template))
            {
                current_template = Env.GetEXEDirectory() + @"\" + DefaultTemplate;
            }

            var current_path = Path.GetDirectoryName(current_template);
            var dlg = new OpenFileDialog();
            dlg.FileName = Path.GetFileName(current_template);
            dlg.DefaultExt = Path.GetExtension(current_template);
            dlg.InitialDirectory = current_path;
            dlg.Title = "Please select a template";
            dlg.Multiselect = false;
            //dlg.Filter = "Excel 2003 (*.xls)|*.xls|Excel 2007-2010 (*.xlsx)|*.xlsx|Excel Macro Workbook (*.xlsm)|*.xlsm";
            dlg.Filter = "Excel Files (*.xls, *.xlsx, *.xlsm)|*.xls;*.xlsx;*.xlsm";
            var rslt = dlg.ShowDialog();
            if (rslt == System.Windows.Forms.DialogResult.OK)
            {
                Template = dlg.FileName;
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var frm = new Settings();
            frm.ShowDialog();
        }

        private void importFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            // dlg.InitialDirectory = current_path;
            dlg.Multiselect = false;
            dlg.Filter = "Excel Macro Workbook (*.xlsm,*.xlsx)|*.xlsm;*.xlsx";
            var rslt = dlg.ShowDialog();
            if (rslt == System.Windows.Forms.DialogResult.OK)
            {
                import_file(dlg.FileName);
            }
        }

        private void import_file(string filename)
        {
            using (var progress = new ProgressDialog(3))
            {
                progress.Show();
                progress.Text = "Copying Required Files";
                progress.Tick();

                // first, make sure the audit log exists
                if (!File.Exists(mbapi.smartGetSMBDir() + "/syscon_tm_log.DBF"))
                {
                    File.Copy(Env.GetEXEDirectory() + "/syscon_tm_log.DBF", mbapi.smartGetSMBDir() + "/syscon_tm_log.DBF");
                    File.Copy(Env.GetEXEDirectory() + "/syscon_tm_log.FPT", mbapi.smartGetSMBDir() + "/syscon_tm_log.FPT");
                }

                // make a copy of the import file into Datadir + /Syscon TM Audit/<date>.xlsm
                try
                {
                    if (!Directory.Exists(mbapi.smartGetSMBDir() + "/Syscon TM Audit"))
                        Directory.CreateDirectory(mbapi.smartGetSMBDir() + "/Syscon TM Audit");

                    var today = DateTime.Today;
                    var dtestr = string.Format("{0}-{1}-{2}", today.Year, today.Month, today.Day);

                    int index = 1;
                    var ext = Path.GetExtension(filename);
                    var new_filename = string.Format("{0}/Syscon TM Audit/{1}_{2}{3}", mbapi.smartGetSMBDir(), dtestr, index, ext);
                    while (File.Exists(new_filename))
                    {
                        new_filename = string.Format("{0}/Syscon TM Audit/{1}_{2}{3}", mbapi.smartGetSMBDir(), dtestr, ++index, ext);
                    }

                    File.Copy(filename, new_filename);
                    filename = new_filename;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not copy the file for auditing purposes, not updating SMB.\r\nPlease ensure the file is closed and try again.", "Error", MessageBoxButtons.OK);
                    Env.Log("{0}\r\n{1}", ex.Message, ex.StackTrace);
                    return;
                }

                var import_errors = false;

                using (var excel_con = SysconCommon.Common.Environment.Connections.GetInMemoryDB())
                {
                    using (var mb7_con = SysconCommon.Common.Environment.Connections.GetOLEDBConnection())
                    {
                        progress.Text = "Loading Excel (May take a while)";
                        progress.Tick();

                        // load the excel_con with values
                        ExcelAddinUtil.UseNewApp(_readonly: true);
                        try
                        {
                            var tempdt = ExcelAddinUtil.GetNamedRangeData(filename, "Change Summary", "ChangeSummary", true);
                            excel_con.LoadDataTable(tempdt);
                        }
                        finally
                        {
                            ExcelAddinUtil.CloseApp(false);
                        }

                        progress.Text = "Loading data into SMB";
                        progress.Tick();
                        // wrap it all in a transaction, so that if one thing fails, the db is not left in a corrupted state
                        // ARGHHH!!! Stupid SMB uses free tables, (no .dbc) so the rollback() seems to succeed but doesn't actually
                        // do anything.... god i hate Sage sometimes
                        try
                        {
                            // make sure there are approved changes to update
                            var chngrows_count = excel_con.GetScalar<int>("select count(*) from ChangeSummary where invalid = 0 and apprvd = 'Yes'");
                            if (chngrows_count == 0)
                            {
                                MessageBox.Show("No approved changes in spreadsheet.", "Error", MessageBoxButtons.OK);
                                return;
                            }

                            Func<string, long, bool> auditlog = (string msg, long recnum) =>
                            {
                                try
                                {
                                    mb7_con.ExecuteNonQuery("insert into syscon_tm_log (recnum, usrnme, chgdsc, chgdte, chgpth) values ({0}, {1}, {2}, {3}, {4})"
                                        , recnum
                                        , mbapi.LoggedInUser.FoxproQuote()
                                        , msg.FoxproQuote()
                                        , DateTime.Today.ToFoxproDate()
                                        , filename.FoxproQuote());

                                    return true;
                                }
                                catch (Exception ex)
                                {
                                    Env.Log("{0}\r\n{1}", ex.Message, ex.StackTrace);
                                    import_errors = true;
                                    return false;
                                }
                            };

                            // update billing status
                            var data_dt = excel_con.GetDataTable("Billing Status", "select recnum, bllsts from ChangeSummary where mbllsts = 1 and invalid = 0 and apprvd = 'Yes'");
                            foreach (var row in data_dt.Rows.ToIEnumerable())
                            {


                                try
                                {
                                    var bllsts = 0;
                                    switch (row["bllsts"].ToString().Trim().ToUpper())
                                    {
                                        case "UNBILLED":
                                            bllsts = 1;
                                            break;
                                        case "NON-BILLABLE":
                                            bllsts = 2;
                                            break;
                                        case "BILLED":
                                            bllsts = 3;
                                            break;
                                        case "UNBILLABLE COST CODE":
                                            bllsts = 2;
                                            break;
                                        default:
                                            throw new SysconException("Invalid Billing Status ({0}) for jobcst record {1}", row["bllsts"], row["recnum"]);
                                    }

                                    mb7_con.ExecuteNonQuery("update jobcst set bllsts = {0} where recnum = {1}", bllsts, row["recnum"]);
                                    auditlog("Updating Billing Status", Convert.ToInt64(row["recnum"]));
                                }
                                catch (Exception ex)
                                {
                                    import_errors = true;
                                    Env.Log("Error updating Jobcost {2}: {0}\r\n{1}", ex.Message, ex.StackTrace, row["recnum"]);
                                }
                            }

                            // update billing overrides
                            data_dt = excel_con.GetDataTable("Billing Overrides", "select recnum, bllamt from ChangeSummary where mbllovrrde = 1 and invalid = 0 and apprvd = 'Yes'");
                            foreach (var row in data_dt.Rows.ToIEnumerable())
                            {
                                try
                                {
                                    mb7_con.ExecuteNonQuery("update jobcst set ovrrde = 1, blgamt = {0} where recnum = {1}", row["bllamt"], row["recnum"]);
                                    auditlog("updated billing amount", Convert.ToInt64(row["recnum"]));
                                }
                                catch (Exception ex)
                                {
                                    import_errors = true;
                                    Env.Log("Error updating Jobcst {2}: {0}\r\n{1}", ex.Message, ex.StackTrace, row["recnum"]);
                                }
                            }

                            // update cost codes
                            data_dt = excel_con.GetDataTable("Cost Codes", "select recnum, cstcde from ChangeSummary where mcstcde = 1 and invalid = 0 and apprvd = 'Yes'");
                            foreach (var row in data_dt.Rows.ToIEnumerable())
                            {
                                try
                                {
                                    mb7_con.ExecuteNonQuery("update jobcst set cstcde = {0} where recnum = {1}", row["cstcde"], row["recnum"]);
                                    auditlog("updated cost code", Convert.ToInt64(row["recnum"]));
                                }
                                catch (Exception ex)
                                {
                                    import_errors = true;
                                    Env.Log("Error updating Jobcst {0}: {1}\r\n{2}", row["recnum"], ex.Message, ex.StackTrace);
                                }
                            }

                            // update notes
                            data_dt = excel_con.GetDataTable("Notes", "select recnum, newntes from ChangeSummary where mnotes = 1 and invalid = 0 and apprvd = 'Yes'");
                            foreach (var row in data_dt.Rows.ToIEnumerable())
                            {
                                try
                                {
                                    var new_ntetxt = row["newntes"].ToString().Trim();
                                    var old_ntetxt = mb7_con.GetScalar<string>("select ntetxt from jobcst where recnum = {0}", row["recnum"]);

                                    new_ntetxt = new_ntetxt.Replace("\r\n", new string(new char[] { (char)124 }));
                                    new_ntetxt += new string(new char[] { (char)124 });
                                    var ntetxt = new_ntetxt + old_ntetxt;

                                    mb7_con.ExecuteNonQuery("update jobcst set ntetxt = {0} where recnum = {1}", ntetxt.FoxproQuote(), row["recnum"]);
                                    auditlog("Updated Notes", Convert.ToInt64(row["recnum"]));
                                }
                                catch (Exception ex)
                                {
                                    import_errors = true;
                                    Env.Log("Error updating Jobcst {2}: {0}\r\n{1}", ex.Message, ex.StackTrace, row["recnum"]);
                                }
                            }

                            // update partial billings - this must happen last so that any other changes are copied to the new record correctly
                            data_dt = excel_con.GetDataTable("Partial Billings", "select recnum, originalbllqty, bllqty from ChangeSummary where mbllhours = 1 and invalid = 0 and apprvd = 'Yes'");
                            foreach (var row in data_dt.Rows.ToIEnumerable())
                            {
                                try
                                {
                                    // as an extra-check so this doesn't happen twice because that would be detrimental, make sure
                                    // that the original billing quantity matches that that is in SMB currently
                                    var smb_bllqty = mb7_con.GetScalar<decimal>("select blgqty from jobcst where recnum = {0}", row["recnum"]);
                                    var orig_bllqty = Convert.ToDecimal(row["originalbllqty"]);

                                    if (smb_bllqty != orig_bllqty)
                                    {
                                        throw new SysconException("SMB billing quantity ({0}) does not match original billing quantity ({1})", smb_bllqty, orig_bllqty);
                                    }

                                    // create a new jobcst record with the difference
                                    var jobcst_row = mb7_con.GetDataTable("Job Cost", "select * from jobcst where recnum = {0}", row["recnum"]).Rows[0];
                                    jobcst_row["cstamt"] = 0.0m;
                                    jobcst_row["payrec"] = 0;
                                    jobcst_row["csthrs"] = 0;
                                    jobcst_row["paytyp"] = 0;
                                    jobcst_row["blgqty"] = orig_bllqty - Convert.ToDecimal(row["bllqty"]);
                                    jobcst_row["srcnum"] = Env.GetConfigVar<int>("NewJobcostSource", 31, true);
                                    jobcst_row["usrnme"] = jobcst_row["usrnme"].ToString().Trim() + "-Import";
                                    jobcst_row["ntetxt"] = string.Format("{0}: Split from original billing record #{1}", DateTime.Now, row["recnum"]);
                                    jobcst_row["recnum"] = mb7_con.GetScalar<long>("select max(recnum) + 1 from jobcst");
                                    // jobcst_row["ovrrde"] = 1;

                                    // insert the record
                                    var sql = jobcst_row.FoxproInsertString("jobcst");
                                    mb7_con.ExecuteNonQuery(sql);
                                    auditlog(string.Format("Split from {0}", row["recnum"]), Convert.ToInt64(jobcst_row["recnum"]));

                                    // update the billing quantity
                                    mb7_con.ExecuteNonQuery("update jobcst set blgqty = {0}, ntetxt = {2} + ntetxt where recnum = {1}", row["bllqty"], row["recnum"],
                                        string.Format("{0} - Partial Billing, new record (#{1}) added{2}", DateTime.Now, jobcst_row["recnum"], (char)124).FoxproQuote());
                                    auditlog("Set to partial billing", Convert.ToInt64(row["recnum"]));
                                }
                                catch (Exception ex)
                                {
                                    import_errors = true;
                                    Env.Log("Error updating Jobcst {2}: {0}\r\n{1}", ex.Message, ex.StackTrace, row["recnum"]);
                                }
                            }

                            if (import_errors)
                            {
                                throw new SysconException("Error Importing Data.");
                            }

                            MessageBox.Show("File imported successfully.", "Success", MessageBoxButtons.OK);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error updating SMB data, see logfile for details");
                            Env.Log(string.Format("Error updating SMB data: {0}\r\n{1}", ex.Message, ex.StackTrace));
                        }
                    }
                }
            }
        }

        private void chkUnbilled_CheckedChanged(object sender, EventArgs e)
        {
            Env.SetConfigVar("ExportUnbilledOnly", chkUnbilled.Checked);
        }

        private void btnSMBDir_Click(object sender, EventArgs e)
        {
            // Env.SetConfigFile(Env.GetEXEDirectory() + "/config.xml");
            mbapi.smartSelectSMBDirByGUI();
            var usr = mbapi.RequireSMBLogin();
            if (usr != null)
            {
                txtDataDir.Text = mbapi.smartGetSMBDir();
            }
        }

        private void viewAuditLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var frm = new ViewAuditLog(mbapi);
            frm.ShowDialog();
        }
    }
}
