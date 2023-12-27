using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Utils.Paths
{
    public class AddUser
    {
        public string? UserName { get; set; }
        public string? Color { get; set; }

        public AddUser(string userName, string color)
        {
            UserName = userName;
            Color = color;
        }

        public AddUser() { }
    }
}
