using Caviar.Models.SystemData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Caviar.Models.SystemData;
/// <summary>
/// 生成者：未登录用户
/// 生成时间：2021/5/28 12:37:58
/// 代码由代码生成器自动生成，更改的代码可能被进行替换
/// 可在上层目录使用partial关键字进行扩展
/// 角色模型操作器
/// </summary>
namespace Caviar.Control.Role
{
    public partial class RoleAction : SysRole
    {
        /// <summary>
        /// 添加角色
        /// </summary>
        /// <returns></returns>
        public virtual async Task<int> AddEntity()
        {
            var count = await BC.DC.AddEntityAsync(this);
            return count;
        }
        /// <summary>
        /// 删除角色
        /// </summary>
        /// <returns></returns>
        public virtual async Task<int> DeleteEntity()
        {
            var count = await BC.DC.DeleteEntityAsync(this);
            return count;
        }
        /// <summary>
        /// 修改角色
        /// </summary>
        /// <returns></returns>
        public virtual async Task<int> UpdateEntity()
        {
            var count = await BC.DC.UpdateEntityAsync(this);
            return count;
        }
        /// <summary>
        /// 获取分页数据
        /// </summary>
        /// <returns></returns>
        public virtual async Task<PageData<ViewRole>> GetPages(Expression<Func<SysRole, bool>> where,int pageIndex,int pageSize, bool isOrder = true, bool isNoTracking = false)
        {
            var pages = await BC.DC.GetPageAsync(where, u => u.Number, pageIndex, pageSize, isOrder, isNoTracking);
            var ViewPages = ModelToViewModel(pages);
            return ViewPages;
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="menus"></param>
        /// <returns></returns>
        public virtual async Task<int> DeleteEntity(List<SysRole> menus)
        {
            var count = await BC.DC.DeleteEntityAsync(menus);
            return count;
        }
        /// <summary>
        /// 批量修改
        /// </summary>
        /// <param name="menus"></param>
        /// <returns></returns>
        public virtual async Task<int> UpdateEntity(List<SysRole> menus)
        {
            var count = await BC.DC.UpdateEntityAsync(menus);
            return count;
        }

        protected delegate T Transformation<T,K>(K model);

        protected event Transformation<PageData<ViewRole>, PageData<SysRole>> TransformationEvent;

        /// <summary>
        /// 魔法转换
        /// 需要达到一个model转为viewModel效果
        /// 交给你去处理，如果你不需要处理，则自动处理，我称之为魔法转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        private PageData<ViewRole> ModelToViewModel(PageData<SysRole> model)
        {
            
            var viewModel = TransformationEvent?.Invoke(model);
            if (viewModel == null)
            {
                viewModel = CommonHelper.AToB<PageData<ViewRole>, PageData<SysRole>>(model);
            }
            return viewModel;
        }
    }
}