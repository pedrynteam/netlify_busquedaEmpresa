using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MGTModel.AuthService
{
    public class UserApp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        public string IdentityId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public DateTime LastSeen { get; set; }

        public bool Active { get; set; } // To know that the user is Active or not. So dont send notifications, messages, etc, etc.

    }
}
