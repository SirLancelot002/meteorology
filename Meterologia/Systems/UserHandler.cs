using ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Program
{
    public class UserHandler
    {
        private MeasurementSystem internalSystem;
        private User myUser;
        public UserHandler() {
            internalSystem = new MeasurementSystem();
            myUser = new User(internalSystem);
        }

        public void run()
        {
            myUser.LoadData();
            int answer = 100;
            do
            {
                answer = myUser.choose();
                if (answer == 1)
                {
                    if (myUser is Admin)
                    {
                        myUser = new User(myUser);
                        Console.WriteLine("\n\tYou have logged out of Admin\n");
                    }
                    else
                    {
                        myUser = new Admin(myUser);
                        Console.WriteLine("\n\tYou have logged into Admin\n");
                    }
                }
            } while (answer != 0);
        }
    }
}
