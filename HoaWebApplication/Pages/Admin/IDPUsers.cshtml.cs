using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using HoaCommon.Extensions.UserClaims;
using HoaEntities.Models.OutputModels;
using HoaEntities.Models.UpdateModels;
using HoaWebApplication.Models.PaginationModels;
using HoaWebApplication.Services.IdentityServer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HoaWebApplication.Pages.Admin
{
    public class IDPUsersModel : PageModel
    {
        private IIDPUserService _iIDPUserService;

        //property that will be used for paginating the user data
        public XPaginationHeaderDto XPaginationDto;

        public IEnumerable<IDPUserViewDto> Users { get; set; }

        [BindProperty(SupportsGet = true)]
        public IDPUserUpdateDto UserToEdit { get; set; }

        //page number, page size, filters, and sorts that will be used for pagination
        [BindProperty(SupportsGet = true)]
        public PageParametersDto PageParameters { get; set; }

        //set the access level of the user to either read only or CRUD access
        [BindProperty(SupportsGet = true)]
        [Display(Name = "Read Only Access?")]
        public bool ReadOnlyAccess { get; set; }

        //set the role of the user
        [BindProperty(SupportsGet = true)]
        public string Role { get; set; }

        //set if user can create posts or not
        [BindProperty(SupportsGet = true)]
        [Display(Name = "Post Creation Access?")]
        public bool PostAccess { get; set; }

        public IDPUsersModel(IIDPUserService iIDPUserService)
        {
            _iIDPUserService = iIDPUserService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.IsAdministrator())
            {
                return RedirectToPage("/Account/AccessDenied");
            }

            //set page size temporarily
            PageParameters.PageSize = 5;

            (Users, XPaginationDto) = await _iIDPUserService.GetAllUsersAsync(PageParameters.PageNum, PageParameters.PageSize);

            return Page();
        }

        public IActionResult OnGetFetchEditIDPUsersPartial()
        {
            //see startup in AddMvc for how this partial is fetched
            //this was an answer found at https://softdevpractice.com/blog/asp-net-core-mvc-ajax-modals/
            return Partial("_EditIDPUserPartial", this);
        }

        public async Task<IActionResult> OnPostEditIDPUserAsync()
        {
            if (!ModelState.IsValid)
            {
                return Partial("_EditIDPUserPartial", this);
            }

            await _iIDPUserService.UpdateUser(ReadOnlyAccess, Role, PostAccess, UserToEdit.Id);

            return Partial("_EditIDPUserPartial", this);
        }
    }
}