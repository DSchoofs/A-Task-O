using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Schoofs_ATaskO.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {


        public IndexModel()
        {

        }
        public void OnGet()
        {

        }

        public void OnPost()
        {
        }
    }
}
