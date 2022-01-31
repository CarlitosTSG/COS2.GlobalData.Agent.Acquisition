using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Conflux.Database.Model
{
    public class DxWebUser : IdentityUser
    {
        // Linked Entity
        public long EntityId { get; set; }

        public bool Active { get; set; }
    }
}
