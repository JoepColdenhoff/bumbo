using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Bumbo.Models;

namespace Bumbo.ViewModels;

public partial class Account
{
    public string? functie { get; set; }
 
    [Required(ErrorMessage = "Email is verplicht.")]
    [StringLength(45, ErrorMessage = "Email mag maximaal 45 tekens bevatten.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Wachtwoord is verplicht.")]
    [StringLength(45, ErrorMessage = "Wachtwoord mag maximaal 45 tekens bevatten.")]
    public string Wachtwoord { get; set; } = null!;



    public virtual ICollection<Medewerker> Medewerkers { get; set; } = new List<Medewerker>();
}
