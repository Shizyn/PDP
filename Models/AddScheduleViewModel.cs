using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace DP.Models
{
    public enum SlotType { Test = 1, Excursion = 2 }

    public class AddScheduleViewModel
    {
        [Required(ErrorMessage = "Выберите тип слота")]
        public SlotType Type { get; set; }

        [Required(ErrorMessage = "Выберите мероприятие")]
        public int SelectedEventId { get; set; }

        public IEnumerable<SelectListItem> Proba { get; set; }
        public IEnumerable<SelectListItem> Excursions { get; set; }

        [Required(ErrorMessage = "Укажите дату")]
        public DateTime Date { get; set; }

        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:hh\:mm}")]
        public TimeSpan FromTime { get; set; }

        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:hh\:mm}")]
        public TimeSpan ToTime { get; set; }
    }

}
