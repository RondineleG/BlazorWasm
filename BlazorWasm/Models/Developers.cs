using System.ComponentModel.DataAnnotations;

namespace BlazorWasm.Models
{
    public class Developers
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "O campo Nome e obrigatorio!")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "O campo Sobrenome e obrigatorio!")]
        public string LastName { get; set; }
        
        [Required(ErrorMessage = "O campo Email e obrigatorio!")]
        public string Email { get; set; }
        [Required]
        public decimal Experience { get; set; } = 1;
    }
}

