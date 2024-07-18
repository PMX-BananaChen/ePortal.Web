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
    public class SchedulingController : Controller
    {
        private readonly ICommandBus commandBus;
        private readonly IEntityRepository<StaffScheduling> StaffSchedulingRepository;
        private readonly IEntityRepository<ePortal.Models.FactoryScheduling> SchedulingRepository;
        private readonly IFormsAuthentication formAuthentication;

        public SchedulingController(ICommandBus commandBus, IEntityRepository<StaffScheduling> StaffSchedulingRepository, IEntityRepository<ePortal.Models.FactoryScheduling> securityGroupRepository, IFormsAuthentication formAuthentication)
        {
            this.commandBus = commandBus;
            this.SchedulingRepository = securityGroupRepository;
            this.StaffSchedulingRepository = StaffSchedulingRepository;
            this.formAuthentication = formAuthentication;
        }
       
        [Authorizer]
        public ActionResult Index()
        {
            var SchedulingList = SchedulingRepository.GetAll();
            return View("Index", SchedulingList);
        }

        public ActionResult SchedulingList()
        {
            var SchedulingList = SchedulingRepository.GetAll();
            return PartialView("_SchedulingList", SchedulingList);
        }
        public ActionResult Edit(Guid Id)
        {

            var menu = SchedulingRepository.Get(m => m.AutoID == Id);

            var MenuModel = Mapper.Map<FactoryScheduling, FactorySchedulingModel>(menu);
          
            return PartialView("Edit", MenuModel);
        }

        [Authorizer]
        public ActionResult SchedulingEdit(ePortal.Web.Models.FactorySchedulingModel fs)
        {
            
            if (ModelState.IsValid)
            {
                var command = Mapper.Map<FactorySchedulingModel, CreateOrUpdateEntityCommand<ePortal.Models.FactoryScheduling>>(fs);
                
              
                    var result = commandBus.Submit(command, 1);

                    return Json(new { Success = result.Success });
                
                // If we got this far, something failed
                
            }

            // If we got this far, something failed
            return Json(new { errors =""  });

           
        }
        
        public ActionResult SchedulingSave(ePortal.Web.Models.FactorySchedulingModel fs)
        {

            if (ModelState.IsValid)
            {
                var command = Mapper.Map<FactorySchedulingModel, CreateOrUpdateEntityCommand<ePortal.Models.FactoryScheduling>>(fs);


                var result = commandBus.Submit(command, 0);

                return Json(new { Success = result.Success });

                // If we got this far, something failed

            }

            // If we got this far, something failed
            return Json(new { errors = "" });


        }   
       
    }

}
