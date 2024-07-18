using ePortal.CommandProcessor.Dispatcher;
using ePortal.Data;
using ePortal.Data.Infrastructure;
using ePortal.Data.Repositories;
using ePortal.Models;
using ePortal.Web.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace ePortal.Web.Controllers
{
    public class AuditController : Controller
    {
        private PortalDataContext db = new PortalDataContext();

        private readonly ICommandBus commandBus;
        private readonly IEntityRepository<Employee_Full> employeeRepository;
        private readonly IEntityRepository<Absence> absenceRepository;
        private readonly IEntityRepository<Overtime> overtimeRepository;
        private readonly IEntityRepository<Scheduling> schedulingRepository;
        private readonly IEntityRepository<OthersFormMode> OthersFormModelRepository;
        private readonly IEntityRepository<AccessFormMode> AccessFormModelRepository;
        private readonly IEntityRepository<AccessRecord> AccessRecordRepository;
        private readonly IEntityRepository<Bide> BideRepository;
        private readonly IEntityRepository<MailingList> mailingListRepository;
        private readonly IUnitOfWork UnitOfWork;
        private readonly IEntityRepository<AbnormalAnalysis> analysisRepository;
        private readonly IEntityRepository<HCP> HCPRepository;

        public AuditController(ICommandBus commandBus, IEntityRepository<Employee_Full> employeeRepository, IEntityRepository<Scheduling> schedulingRepository, IEntityRepository<OthersFormMode> OthersFormModelRepository, IEntityRepository<AccessFormMode> AccessFormModelRepository, IEntityRepository<HCP> HCPRepository, IEntityRepository<Absence> absenceRepository, IEntityRepository<Overtime> overtimeRepository, IEntityRepository<AbnormalAnalysis> analysisRepository, IEntityRepository<AccessRecord> AccessRecordRepository, IEntityRepository<Bide> BideRepository, IEntityRepository<MailingList> mailingListRepository, IUnitOfWork UnitOfWork)
        {
            this.UnitOfWork = UnitOfWork;
            this.BideRepository = BideRepository;
            this.commandBus = commandBus;
            this.absenceRepository = absenceRepository;
            this.AccessRecordRepository = AccessRecordRepository;
            this.mailingListRepository = mailingListRepository;
            this.overtimeRepository = overtimeRepository;
            this.employeeRepository = employeeRepository;
            this.analysisRepository = analysisRepository;
            this.schedulingRepository = schedulingRepository;
            this.OthersFormModelRepository = OthersFormModelRepository;
            this.AccessFormModelRepository = AccessFormModelRepository;
            this.HCPRepository = HCPRepository;
        }


        public ActionResult Index()
        {

            return View();
        }


        public ActionResult Audit(string types, string star, string end)
        {

            var dt = analysisRepository.ProcQuery("Audit_Report", new SqlParameter[]
                {
                    new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.String,Value=types,ParameterName="cc"},
                      new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.String,Value=star,ParameterName="stardate"},
                        new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.String,Value=end,ParameterName="enddate"}
                });

            string s = DTtoJSON(dt);


            return Content(Convert.ToString(s));


        }


        public static string DTtoJSON(DataTable dt)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            ArrayList dic = new ArrayList();
            foreach (DataRow row in dt.Rows)
            {
                Dictionary<string, object> drow = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    drow.Add(col.ColumnName, row[col.ColumnName]);
                }
                dic.Add(drow);
            }
            return jss.Serialize(dic);
        }


    }
}
