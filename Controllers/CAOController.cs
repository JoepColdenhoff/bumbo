using Bumbo.ExceptionClasses;
using Bumbo.Models;
using Bumbo.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bumbo.Controllers
{
    public class CAOController : Controller
    {
        private BumboContext _context;
        private RoosterController _roosterController;

        private List<Beschikbaarheid> geselecteerdeTijd { get; set; }

        public CAOController(BumboContext context, RoosterController rooster)
        {
            _context = context;
            geselecteerdeTijd = new List<Beschikbaarheid> { };
            _roosterController = rooster;
        }

        private List<Diensten> GetShifts(Medewerker medewerker)
        {
            return _context.Dienstens
                            .Where(d => d.Medewerker == medewerker)
                            .ToList();
        }

        //methode dat checkt of CAO-regels toegepast worden per leeftijd adhv beschikbaarheid wat geselecteerd wordt
        public CAOErrorViewModel CheckAgeShiftRules(int medewerkerId, int dienstId)
        {
            var medewerker = _context.Medewerkers
                .Include(m => m.Beschikbaarheids)
                .Include(m => m.Dienstens)
                    .ThenInclude(d => d.Beschikbaarheid)
                    .FirstOrDefault(m => m.MedewerkerId == medewerkerId);

            int age = CalculateAge(medewerker.Geboortedatum);
            List<Beschikbaarheid> availability = (List<Beschikbaarheid>)medewerker.Beschikbaarheids;
            List<Diensten> shifts = GetShifts(medewerker);

            foreach (Diensten diensten in shifts)
            {
                int totalHours = (int)(diensten.EindTijd - diensten.StartTijd).TotalHours;

                if (age < 16)
                {
                    bool moreThan8 = totalHours > 8;
                    bool worksAfter19 = diensten.EindTijd.Hours > 19;

                    if (moreThan8 || worksAfter19)
                    {
                        _roosterController.DeleteFromRoster(dienstId);

                        if (moreThan8)
                        {
                            _roosterController.ReturnWithError("CAOError", "Medewerker mag onder de 16 niet meer dan 8 uur maken.");
                        }
                        else
                        {
                            _roosterController.ReturnWithError("CAOError", "Medewerker jonger dan 16 mag niet na 19:00 werken.");
                        }
                    }

                    int schoolHours = diensten.Beschikbaarheid.SchoolUren ?? 0;
                    if (totalHours + schoolHours > 12 && schoolHours > 0)
                    {
                        _roosterController.DeleteFromRoster(dienstId);
                        _roosterController.ReturnWithError("CAOError", "Medewerker jonger dan 16 mag in combinatie met school niet meer dan 12 uur werken.");
                    }
                    bool workedOver40Hours = CalculateTotalWeeklyHours(shifts) > 40;
                    if(workedOver40Hours) 
                    {
                        _roosterController.ReturnWithError("CAOError", "Medewerker heeft wordt hierdoor meer dan 4*40 uur per maand ingepland"); 
                    }

                }
                else if (age == 16 || age == 17)
                {
                    bool tooManyHoursWithSchool = totalHours > 9;
                    

                    if (tooManyHoursWithSchool)
                    {
                        _roosterController.DeleteFromRoster(dienstId);

                        _roosterController.ReturnWithError("CAOError", "Medewerker werkt hierdoor te veel i.c.m. school");
                    }

                    bool over45Hours = CalculateTotalWeeklyHours(shifts) > 45;
                }
                else if (age >= 18)
                {
                    bool over12Hours = totalHours > 12;
                    bool over60HoursInWeek = CalculateTotalWeeklyHours(shifts) > 60;

                    if (over12Hours || over60HoursInWeek)
                    {
                        _roosterController.DeleteFromRoster(dienstId);
                        if (over12Hours)
                        {
                            _roosterController.ReturnWithError("CAOError", "Medewerker werkt meer dan 12 uur.");
                        }
                        else
                        {
                            _roosterController.ReturnWithError("CAOError", "Medewerker werkt hierdoor meer dan 60 uur deze week.");
                        }
                    }
                }
            }

            if (CalculateTotalWeeklyHours(shifts) > 60)
            {
                throw new CAOException(new CAOErrorViewModel
                {
                    Exceeds60Hours = true
                });
            }
            return new CAOErrorViewModel { Passed = true };
        }




        private int CalculateTotalWeeklyHours(List<Diensten> shifts)
        {
            int totalHours = 0;

            foreach (var shift in shifts)
            {
                int shiftHours = (int)(shift.EindTijd - shift.StartTijd).TotalHours;

                totalHours += shiftHours;
            }

            return totalHours;
        }

        public int CalculateAge(DateTime birthDate)
        {
            DateTime currentDate = DateTime.Now;
            int age = currentDate.Year - birthDate.Year;

            if (birthDate.Date > currentDate.AddYears(-age))
            {
                age--;
            }

            return age;
        }
    }


}
