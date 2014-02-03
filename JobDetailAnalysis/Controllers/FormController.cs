using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using SysconCommon.Common.Environment;
using SysconCommon.Algebras.JSON;
using SysconCommon.Common.Validity;
using SysconCommon.Algebras.DataTables;

namespace JobDetailAnalysis.Controllers
{
    [SecureParameters]
    public class FormController : Controller
    {
        //
        // GET: /Form/

        public ActionResult Form()
        {
#if true
            var json = FormBuilder.GetForm();
            Response.ContentType = "application/json";
            HttpContext.Response.Write(json);
            return new EmptyResult();
#else
            return Json(new string[] { "b" }, JsonRequestBehavior.AllowGet);
#endif
        }

        public ActionResult Submit(UnsafeString startperiod, UnsafeString endperiod, UnsafeString joblist, UnsafeString tmtable)
        {
            var _startperiod = int.Parse(startperiod.getValue(@"^\s*\d+\s*$"));
            var _endperiod = int.Parse(endperiod.getValue(@"^\s*\d+\s*$"));
            var _jobnum = int.Parse(joblist.getValue(@"^\s*\d+\s*$"));
            var _tmtable = tmtable.getValue("^(YES|NO)$");
            Env.SetConfigVar("userdefined/startperiod", _startperiod);
            Env.SetConfigVar("userdefined/endperiod", _endperiod);
            Env.SetConfigVar("userdefined/joblist", _jobnum);
            Env.SetConfigVar("userdefined/tmtable", _tmtable);
            var dt = Env.RunSqlStatement(Connections.Connection, "costsummary");
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment; filename=costsummary.csv");
            Response.Write(dt.AsCSVText());
            return new EmptyResult();
        }

        public ActionResult Tmeqln()
        {
            var dt = Connections.Connection.RunSqlStatement("tmeqln");
            HttpContext.Response.Write(dt.ToJSON());
            return new EmptyResult();
        }
    }
}
