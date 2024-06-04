using Microsoft.AspNetCore.Identity;

namespace Bumbo.ErrorDescriber
{
    internal class DutchIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError PasswordRequiresDigit()
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresDigit),
                Description = "Wachtwoord moet minimaal één cijfer ('0'-'9') bevatten."
            };
        }
        public override IdentityError PasswordRequiresNonAlphanumeric()
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresNonAlphanumeric),
                Description = "Wachtwoord moet minimaal één niet-alfanumeriek karakter bevatten."
            };
        }
    }
}