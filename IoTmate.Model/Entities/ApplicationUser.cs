using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace IoTmate.Model.Entities
{
    public class ApplicationUser : IdentityUser
    {
        // Room where user is
        public ICollection<RoomMember> RoomMemberships { get; set; } = [];
    }
}
