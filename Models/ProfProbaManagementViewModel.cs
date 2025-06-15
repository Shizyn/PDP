using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace DP.Models
{
    public class ProfProbaManagementViewModel
    {
        [Required(ErrorMessage = "Введите название профпробы")]
        [Display(Name = "Новая профпроба")]
        public string NewProfProbaName { get; set; }

        [Required(ErrorMessage = "Выберите профпробу")]
        [Display(Name = "Профпроба")]
        public int? SelectedProfProbaId { get; set; }

        [Required(ErrorMessage = "Введите название события")]
        [Display(Name = "Название события")]
        public string NewEventName { get; set; }

        [Display(Name = "Описание события")]
        public string NewEventDescription { get; set; }

        public List<ProfProba> ProfProby { get; set; } = new List<ProfProba>();

        public List<SelectListItem> ProfProbaItems
            => ProfProby?.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name
            }).ToList();

        public Dictionary<int, List<Event>> EventsByProfProba { get; set; } = new Dictionary<int, List<Event>>();
    }
}
