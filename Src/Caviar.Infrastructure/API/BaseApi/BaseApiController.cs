﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Caviar.Core.Services;
using Caviar.Core.Interface;
using Caviar.SharedKernel.Entities.View;
using Caviar.Core;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Caviar.SharedKernel.Entities;
using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Caviar.Infrastructure.API.BaseApi
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BaseApiController: Controller
    {
        protected Interactor Interactor;

        protected ILanguageService LanguageService { get; set; }

        protected UserServices<ApplicationUser> UserServices { get; set; }

        void SetLanguage(string acceptLanguage)
        {
            if (string.IsNullOrEmpty(acceptLanguage)) acceptLanguage = CurrencyConstant.DefaultLanguage;
            var culture = CultureInfo.GetCultureInfo(acceptLanguage);
            LanguageService = CreateService<ILanguageService>();
            LanguageService.SetLanguage(culture);
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            Interactor = CreateService<Interactor>();
            UserServices = CreateService<UserServices<ApplicationUser>>();

            Interactor.Stopwatch.Start();

            //获取ip地址
            Interactor.Current_Ipaddress = context.HttpContext.GetUserIp();
            //获取完整Url
            Interactor.Current_AbsoluteUri = context.HttpContext.Request.GetAbsoluteUri();
            //获取请求路径
            Interactor.Current_Action = context.HttpContext.Request.Path.Value;
            //请求上下文
            Interactor.HttpContext = HttpContext;
            //请求参数
            Interactor.ActionArguments = context.ActionArguments;
            //用户信息
            Interactor.User = context.HttpContext.User;
            //设置语言信息
            var acceptLanguage = context.HttpContext.Request.Cookies.SingleOrDefault(c => c.Key == CurrencyConstant.LanguageHeader).Value;
            SetLanguage(acceptLanguage);

            await base.OnActionExecutionAsync(context, next);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var result = context.Result;
            var resultScanner = CreateService<ResultScannerServices>();
            //赋值字段权限
            resultScanner.PermissionFieldss = UserServices.GetPermissions(u => u.PermissionType == PermissionType.RoleFields).Result;
            var resultMsg = resultScanner.ResultHandle(result);
            if (resultMsg != null)
            {
                resultMsg.Title = LanguageService[$"{CurrencyConstant.ResuleMsg}.{resultMsg.Title}"];
                ModificationTips(resultMsg);
                context.Result = base.Ok(resultMsg);
            }
            Interactor.Stopwatch.Stop();
            base.OnActionExecuted(context);
        }

        protected virtual IActionResult Ok(HttpStatusCode status = HttpStatusCode.OK,string title = "Succeeded",string url = null,string detail = null,object data = null)
        {
            var resultMst = new ResultMsg<object>
            {
                Status = status,
                Title = title,
                Url = url,
                Detail = detail,
                Data = data
            };
            return Ok(resultMst);
        }

        protected virtual void ModificationTips(ResultMsg resultMsg)
        {
            resultMsg.Title = LanguageService[$"{CurrencyConstant.ResuleMsg}.Title.{resultMsg.Title}"];
        }

        protected virtual T CreateService<T>()
        {
            T service = HttpContext.RequestServices.GetRequiredService<T>();
            return service;
        }


        /// <summary>
        /// 只能获取自身字段
        /// </summary>
        /// <param name="modelName"></param>
        /// <returns></returns>
        protected virtual async Task<List<ViewFields>> GetFields<T>() where T:IUseEntity
        {
            var permissionServices = CreateService<RoleFieldServices>();
            var fieldName = typeof(T).Name;
            var fullName = typeof(T).FullName;
            var fields = FieldScannerServices.GetClassFields(fieldName, fullName, LanguageService);
            var roles = await UserServices.GetRoles();
            fields = await permissionServices.GetRoleFields(fields, fullName, roles);
            fields = fields.OrderBy(u => u.Entity.Number).ToList();
            return fields;
        }

    }


    public class EasyBaseApiController<Vm, T>: BaseApiController where T : class, IUseEntity, new() where Vm : IView<T>,new()
    {
        IEasyBaseServices<T> _service;
        protected IEasyBaseServices<T> Service
        {
            get
            {
                if (_service == null)
                {
                    _service = CreateService<IEasyBaseServices<T>>();
                }
                return _service;
            }
            set
            {
                _service = value;
            }
        }

        /// <summary>
        /// 只能获取自身字段
        /// </summary>
        /// <param name="modelName"></param>
        /// <returns></returns>
        [HttpGet]
        public virtual async Task<IActionResult> GetFields()
        {
            var fields = await GetFields<T>();
            return Ok(fields);
        }

        [HttpPost]
        public virtual async Task<IActionResult> CreateEntity(Vm vm)
        {
            var id = await Service.CreateEntity(vm.Entity);
            return Ok(id);
        }

        [HttpPost]
        public virtual async Task<IActionResult> UpdateEntity(Vm vm)
        {
            await Service.UpdateEntity(vm.Entity);
            return Ok();
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteEntity(Vm vm)
        {
            await Service.DeleteEntity(vm.Entity);
            return Ok();
        }

        [HttpGet]
        public virtual async Task<IActionResult> GetEntity(int id)
        {
            var entity = await Service.GetEntity(id);
            var entityVm = ToView(entity);
            return Ok(entityVm);
        }

        [HttpGet]
        public virtual async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 10, bool isOrder = true, bool isNoTracking = true)
        {
            var entity = await Service.GetPages(null, pageIndex, pageSize, isOrder, isNoTracking);
            var entityVm = ToView(entity);
            return Ok(entityVm);
        }

        protected virtual Vm ToView(T entity)
        {
            var vm = new Vm() {  Entity = entity};
            return vm;
        }

        protected virtual List<Vm> ToView(List<T> entitys)
        {
            if (entitys == null) return null;
            entitys = Sort(entitys);
            return entitys.Select(x => new Vm() { Entity = x }).ToList();
        }

        protected virtual List<T> Sort(List<T> entitys)
        {
            return entitys.OrderBy(u => u.Number).ToList();
        }

        protected virtual PageData<Vm> ToView(PageData<T> page)
        {

            var pageVm = new PageData<Vm>()
            {
                Rows = ToView(page.Rows),
                PageIndex = page.PageIndex,
                PageSize = page.PageSize,
                Total = page.Total
            };
            return pageVm;
        }

        protected virtual List<T> ToEntity(List<Vm> vm)
        {
            if (vm == null) return null;
            return vm.Select(v => v.Entity).ToList();
        }
    }
}
