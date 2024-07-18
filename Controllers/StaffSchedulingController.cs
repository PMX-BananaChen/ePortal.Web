using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ePortal.Web.ViewModels;
using ePortal.Domain.Commands;
using ePortal.Web.Core.Models;
using ePortal.CommandProcessor.Dispatcher;
using ePortal.Data.Repositories;
using ePortal.Core.Common;
using ePortal.Web.Core.Extensions;
using ePortal.Web.Core.Authentication;
using ePortal.Models;
using ePortal.Web.Core.ActionFilters;
using AutoMapper;
using ePortal.Web.Models;
using System.Data.SqlClient;
namespace ePortal.Web.Controllers
{
    public class StaffSchedulingController : Controller
    {
        private readonly ICommandBus commandBus;
        private readonly IEntityRepository<StaffScheduling> StaffSchedulingRepository;
        private readonly IFormsAuthentication formAuthentication;

        public StaffSchedulingController(ICommandBus commandBus, IEntityRepository<StaffScheduling> StaffSchedulingRepository, IFormsAuthentication formAuthentication)
        {
            this.commandBus = commandBus;
            this.StaffSchedulingRepository = StaffSchedulingRepository;
            this.formAuthentication = formAuthentication;
        }
       
        [Authorizer]
        public ActionResult Index()
        {
            var SchedulingList = StaffSchedulingRepository.GetAll();
            return View("Index", SchedulingList);
        }


        public ActionResult Edit(Guid Id)
        {

            var menu = StaffSchedulingRepository.Get(m => m.AutoID == Id);

           // var MenuModel = Mapper.Map<FactoryScheduling, FactorySchedulingModel>(menu);
          
            return PartialView("Edit");
        }

        [Authorizer]
        public ActionResult Scheduling(ePortal.Web.Models.FactorySchedulingModel fs)
        {
            var SchedulingList = StaffSchedulingRepository.GetAll();
            if (ModelState.IsValid)
            {
                var command = new CreateOrUpdateEntityCommand<ePortal.Models.FactoryScheduling>
                {
                    Entity = new ePortal.Models.FactoryScheduling
                    {
                        AutoID = Guid.NewGuid(),
                        SchedulingNo = fs.SchedulingNo,
                        Sync_Time = DateTime.Now,
                        Break_Begin_A=fs.Break_Begin_A,
                        Break_Begin_B = fs.Break_Begin_B,
                        Break_Begin_C=fs.Break_Begin_C,
                        Break_Begin_D=fs.Break_Begin_D,
                        Break_End_A=fs.Break_End_A,
                        Break_End_B=fs.Break_End_B,
                        Break_End_C=fs.Break_End_C,
                        Break_End_D=fs.Break_End_D,
                        Carding_activate=fs.Carding_activate,
                        Carding_Type=fs.Carding_Type,
                        Update_Time = DateTime.Now
                       
                       
                    }
                };
              
                    var result = commandBus.Submit(command, 0);

                    return Json(new { Success = result.Success });
                
                // If we got this far, something failed
                
            }

            // If we got this far, something failed
            return Json(new { errors =""  });

           
        }

     


       

       
      
      
     

     
       
    }
}
