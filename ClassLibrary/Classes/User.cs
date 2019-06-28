using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DZ4")]

namespace ClassLibrary.Classes
{
    internal sealed class User
    {
        internal int Id { get; set; }
        internal string FirstName { get; set; }
        internal string LastName { get; set; }
        internal string Email { get; set; }
        internal string PhoneNumber { get; set; }
        internal string Login { get; set; }
        internal string Password { get; set; }
        internal UserRoles Role { get; set; }
    }
}