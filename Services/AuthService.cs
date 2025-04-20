using MySqlConnector;
using HMS.Data;
using HMS.Models;

// For logging in and authenticating users
namespace HMS.Services
{
    public class AuthService : IAuthService
    {
        private readonly DatabaseConnection _dbConnection;

        public AuthService(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public User Authenticate(int id, string password, Role role)
        {
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();

                string query = "SELECT * FROM Users WHERE id = @Id AND password = @Password AND role = @Role AND status = 'Active'";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@Password", password);
                    command.Parameters.AddWithValue("@Role", role.ToString());

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            User user = role switch // Get the user role
                            {
                                Role.Patient => new Patient(),
                                Role.Doctor => new Doctor(),
                                Role.Nurse => new Nurse(),
                                Role.Cashier => new Cashier(),
                                Role.Admin => new Admin(),
                                _ => throw new InvalidOperationException("Unknown role")
                            };

                            user.Id = reader.GetInt32("id");
                            user.FirstName = reader.GetString("first_name");
                            user.LastName = reader.GetString("last_name");
                            user.Email = reader.GetString("email");
                            user.Phone = reader.GetInt64("phone");
                            user.Password = reader.GetString("password");
                            user.Gender = (Gender)Enum.Parse(typeof(Gender), reader.GetString("gender"));
                            user.BirthDate = reader.GetDateTime("birth_date");
                            user.Role = (Role)Enum.Parse(typeof(Role), reader.GetString("role"));
                            user.Status = (Status)Enum.Parse(typeof(Status), reader.GetString("status"));
                            reader.Close();

                            // Get additional information based on the role
                            if (user is Doctor doctor)
                            {
                                string doctorQuery = "SELECT specialty, level, hire_date FROM Doctors WHERE user_id = @UserId";
                                using (var doctorCommand = new MySqlCommand(doctorQuery, connection))
                                {
                                    doctorCommand.Parameters.AddWithValue("@UserId", id);
                                    using (var doctorReader = doctorCommand.ExecuteReader())
                                    {
                                        if (doctorReader.Read())
                                        {
                                            doctor.Specialty = doctorReader.GetString("specialty");
                                            doctor.Level = doctorReader.GetString("level");
                                            doctor.HireDate = doctorReader.GetDateTime("hire_date");
                                        }
                                    }
                                }
                            }
                            else if (user is Nurse nurse)
                            {
                                string nurseQuery = "SELECT level, hire_date FROM Nurses WHERE user_id = @UserId";
                                using (var nurseCommand = new MySqlCommand(nurseQuery, connection))
                                {
                                    nurseCommand.Parameters.AddWithValue("@UserId", id);
                                    using (var nurseReader = nurseCommand.ExecuteReader())
                                    {
                                        if (nurseReader.Read())
                                        {
                                            nurse.Level = nurseReader.GetString("level");
                                            nurse.HireDate = nurseReader.GetDateTime("hire_date");
                                        }
                                    }
                                }
                            }
                            else if (user is Cashier cashier)
                            {
                                string cashierQuery = "SELECT hire_date FROM Cashiers WHERE user_id = @UserId";
                                using (var cashierCommand = new MySqlCommand(cashierQuery, connection))
                                {
                                    cashierCommand.Parameters.AddWithValue("@UserId", id);
                                    using (var cashierReader = cashierCommand.ExecuteReader())
                                    {
                                        if (cashierReader.Read())
                                        {
                                            cashier.HireDate = cashierReader.GetDateTime("hire_date");
                                        }
                                    }
                                }
                            }

                            return user;
                        }
                    }
                }
            }

            // If no user is found or the credentials are invalid, throw an exception
            throw new InvalidOperationException("Invalid credentials.");
        }
    }
}