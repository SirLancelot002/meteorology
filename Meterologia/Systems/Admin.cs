using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Program
{
    public class Admin : User
    {
        public Admin(User user) : base(user.mySystem) { }
    }
}
