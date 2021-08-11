﻿using AntDesign;
using Caviar.AntDesignUI.Helper;
using Caviar.SharedKernel;
using Caviar.SharedKernel.View;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caviar.AntDesignUI.Pages.Permission
{
    public partial class PermissionRoleTemple<TData>: DataComponentBase<TData> where TData: IBaseEntity
    {
        protected List<ViewRole> ViewRoles { get; set; }
        protected override async Task OnInitializedAsync()
        {
            await GetSelectMenus();//获取已选择数据
        }

        protected virtual async Task GetSelectMenus()
        {
            var result = await Http.GetJson<List<ViewRole>>($"{Url}?PermissionId={DataSource.Id}");
            if (result.Status != StatusCodes.Status200OK) return;
            if (result.Data != null)
            {
                ViewRoles = result.Data.ListToTree();
                StateHasChanged();
            }
        }

        public override async Task<bool> FormSubmit()
        {
            List<ViewRole> roles = new List<ViewRole>();
            ViewRoles.TreeToList(roles);
            var ids = roles.Where(u => u.IsPermission).Select(u => u.Id);
            var result = await Http.PostJson($"{Url}?PermissionId={DataSource.Id}", ids);
            if (result.Status != StatusCodes.Status200OK) return false;
            Message.Success("操作成功");
            return true;
        }
    }
}
