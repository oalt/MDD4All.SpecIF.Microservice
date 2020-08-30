using System;

namespace MDD4All.SpecIF.Apps.MongoDbInitializer
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "mongodb://localhost:27018";

            JwtConfigurationCreator jwtConfigurationCreator = new JwtConfigurationCreator();

            jwtConfigurationCreator.CreateJwtConfiguration(connectionString);

            AdminUserCreator adminUserCreator = new AdminUserCreator();

            adminUserCreator.CreateAdminUserAndRoles(connectionString);

            Console.WriteLine("Users and JWT secret generated. Press any key to exit.");

            Console.ReadLine();
        }
    }
}
