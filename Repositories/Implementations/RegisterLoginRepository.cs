using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Implementations
{
    public class RegisterLoginRepository : IRegisterLoginInterface
    {
        private readonly NpgsqlConnection _conn;
        public RegisterLoginRepository(NpgsqlConnection conn)
        {
            _conn = conn;
        }

        public async Task<List<t_Country>> GetCountries()
        {
            List<t_Country> countries = new List<t_Country>();
            if (_conn.State != System.Data.ConnectionState.Open)
            {
                await _conn.OpenAsync();
            }
            using (var cmd = new NpgsqlCommand("SELECT * FROM t_country", _conn))
            using (var reader = await cmd.ExecuteReaderAsync())
                while (await reader.ReadAsync())
                {
                    countries.Add(new t_Country()
                    {
                        c_countryId = reader.GetInt32(0),
                        c_countryName = reader.GetString(1)
                    });
                }
            await _conn.CloseAsync();
            return countries;
        }

        public async Task<List<t_State>> GetStates(int countryId)
        {
            List<t_State> states = new List<t_State>();
            await _conn.CloseAsync();
            await _conn.OpenAsync();
            using (var cmd = new NpgsqlCommand("SELECT * FROM t_state WHERE c_countryId = @c_countryId", _conn))
            {
                cmd.Parameters.AddWithValue("@c_countryId", countryId);
                using (var reader = await cmd.ExecuteReaderAsync())
                    while (await reader.ReadAsync())
                    {
                        states.Add(new t_State()
                        {
                            c_stateId = reader.GetInt32(reader.GetOrdinal("c_stateId")),
                            c_countryId = reader.GetInt32(reader.GetOrdinal("c_countryId")),
                            c_stateName = reader.GetString(reader.GetOrdinal("c_stateName"))
                        });
                    }
            }
            await _conn.CloseAsync();
            return states;
        }

        public async Task<List<t_District>> GetDistricts(int stateId)
        {
            List<t_District> districts = new List<t_District>();
            await _conn.CloseAsync();
            await _conn.OpenAsync();
            using (var cmd = new NpgsqlCommand("SELECT * FROM t_district WHERE c_stateId = @c_stateId", _conn))
            {
                cmd.Parameters.AddWithValue("@c_stateId", stateId);
                using (var reader = await cmd.ExecuteReaderAsync())
                    while (await reader.ReadAsync())
                    {
                        districts.Add(new t_District()
                        {
                            c_districtId = reader.GetInt32(reader.GetOrdinal("c_districtId")),  // ✅ Correct
                            c_stateId = reader.GetInt32(reader.GetOrdinal("c_stateId")),        // ✅ Correct
                            c_districtName = reader.GetString(reader.GetOrdinal("c_districtName"))
                        });
                    }
            }
            await _conn.CloseAsync();
            return districts;
        }

        public async Task<List<t_City>> GetCities(int districtId)
        {
            List<t_City> cities = new List<t_City>();
            await _conn.CloseAsync();
            await _conn.OpenAsync();
            using (var cmd = new NpgsqlCommand("SELECT * FROM t_city WHERE c_districtId = @c_districtId", _conn))
            {
                cmd.Parameters.AddWithValue("@c_districtId", districtId);
                using (var reader = await cmd.ExecuteReaderAsync())
                    while (await reader.ReadAsync())
                    {
                        cities.Add(new t_City()
                        {
                            c_cityId = reader.GetInt32(reader.GetOrdinal("c_cityId")),       // ✅ Correct
                            c_districtId = reader.GetInt32(reader.GetOrdinal("c_districtId")), // ✅ Correct
                            c_cityName = reader.GetString(reader.GetOrdinal("c_cityName"))    // ✅ This is fine
                        });
                    }
            }
            await _conn.CloseAsync();
            return cities;
        }

        public async Task<int> Register(t_Register register)
        {
            try
            {
                await _conn.CloseAsync();
                await _conn.OpenAsync();

                // Check if email already exists
                string checkQuery = "SELECT COUNT(*) FROM t_task_track_pro_register_login WHERE c_email = @c_email";
                using (NpgsqlCommand comCheck = new NpgsqlCommand(checkQuery, _conn))
                {
                    comCheck.Parameters.AddWithValue("@c_email", register.c_email);
                    int existingCount = Convert.ToInt32(await comCheck.ExecuteScalarAsync());

                    if (existingCount > 0)
                    {
                        await _conn.CloseAsync();
                        return 0; // Email already exists
                    }
                }

                // Insert new user
                string insertQuery = @"
            INSERT INTO t_task_track_pro_register_login (
                c_firstName, c_middleName, c_lastName, c_email, c_password, 
                c_address, c_mobile, c_gender, c_dob, 
                c_countryId, c_stateId, c_districtId, c_cityId, c_image
            ) VALUES (
                @c_firstName, @c_middleName, @c_lastName, @c_email, @c_password, 
                @c_address, @c_mobile, @c_gender, @c_dob, 
                @c_countryId, @c_stateId, @c_districtId, @c_cityId, @c_image
            ) RETURNING c_userId"; // Return the newly inserted user ID

                using (NpgsqlCommand comInsert = new NpgsqlCommand(insertQuery, _conn))
                {
                    comInsert.Parameters.AddWithValue("@c_firstName", register.c_firstName);
                    comInsert.Parameters.AddWithValue("@c_middleName", (object?)register.c_middleName ?? DBNull.Value);
                    comInsert.Parameters.AddWithValue("@c_lastName", register.c_lastName);
                    comInsert.Parameters.AddWithValue("@c_email", register.c_email);
                    comInsert.Parameters.AddWithValue("@c_password", register.c_password); // Hash this before storing
                    comInsert.Parameters.AddWithValue("@c_address", register.c_address);
                    comInsert.Parameters.AddWithValue("@c_mobile", register.c_mobile);
                    comInsert.Parameters.AddWithValue("@c_gender", register.c_gender);
                    comInsert.Parameters.AddWithValue("@c_dob", register.c_dob);
                    comInsert.Parameters.AddWithValue("@c_countryId", register.c_countryId);
                    comInsert.Parameters.AddWithValue("@c_stateId", register.c_stateId);
                    comInsert.Parameters.AddWithValue("@c_districtId", register.c_districtId);
                    comInsert.Parameters.AddWithValue("@c_cityId", register.c_cityId);
                    comInsert.Parameters.AddWithValue("@c_image", (object?)register.c_image ?? DBNull.Value);

                    int userId = Convert.ToInt32(await comInsert.ExecuteScalarAsync());
                    await _conn.CloseAsync();
                    return userId;
                }
            }
            catch (Exception ex)
            {
                await _conn.CloseAsync();
                throw new Exception("An error occurred while registering the user: " + ex.Message);
            }
        }


                public async Task<t_Register?> Login(t_Login login)
        {
            t_Register? UserData = null;
            string qry = @"
                SELECT r.*, 
                       s.c_stateName, 
                       d.c_districtName, 
                       c.c_cityName
                FROM t_task_track_pro_register_login r
                LEFT JOIN t_state s ON r.c_stateId = s.c_stateId
                LEFT JOIN t_district d ON r.c_districtId = d.c_districtId
                LEFT JOIN t_city c ON r.c_cityId = c.c_cityId
                WHERE r.c_email = @c_email AND r.c_password = @c_password;
            ";

            try
            {
                await _conn.OpenAsync();
                using (NpgsqlCommand cmd = new NpgsqlCommand(qry, _conn))
                {
                    cmd.Parameters.AddWithValue("@c_email", login.c_email);
                    cmd.Parameters.AddWithValue("@c_password", login.c_password);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            UserData = new t_Register
                            {
                                c_userId = reader.GetInt32(reader.GetOrdinal("c_userId")),
                                c_firstName = reader.GetString(reader.GetOrdinal("c_firstName")),
                                c_middleName = reader.IsDBNull(reader.GetOrdinal("c_middleName")) ? null : reader.GetString(reader.GetOrdinal("c_middleName")),
                                c_lastName = reader.GetString(reader.GetOrdinal("c_lastName")),
                                c_email = reader.GetString(reader.GetOrdinal("c_email")),
                                c_address = reader.GetString(reader.GetOrdinal("c_address")),
                                c_mobile = reader.GetString(reader.GetOrdinal("c_mobile")),
                                c_gender = reader.GetString(reader.GetOrdinal("c_gender")),
                                c_dob = reader.GetDateTime(reader.GetOrdinal("c_dob")),
                                c_countryId = reader.GetInt32(reader.GetOrdinal("c_countryId")),
                                c_stateId = reader.GetInt32(reader.GetOrdinal("c_stateId")),
                                //c_stateName = reader.IsDBNull(reader.GetOrdinal("c_stateName")) ? null : reader.GetString(reader.GetOrdinal("c_stateName")),
                                c_districtId = reader.GetInt32(reader.GetOrdinal("c_districtId")),
                                //c_districtName = reader.IsDBNull(reader.GetOrdinal("c_districtName")) ? null : reader.GetString(reader.GetOrdinal("c_districtName")),
                                c_cityId = reader.IsDBNull(reader.GetOrdinal("c_cityId")) ? 0 : reader.GetInt32(reader.GetOrdinal("c_cityId")),
                                //c_cityName = reader.IsDBNull(reader.GetOrdinal("c_cityName")) ? null : reader.GetString(reader.GetOrdinal("c_cityName")),
                                c_image = reader.IsDBNull(reader.GetOrdinal("c_image")) ? null : reader.GetString(reader.GetOrdinal("c_image"))
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while logging in: " + ex.Message);
            }
            finally
            {
                await _conn.CloseAsync();
            }

            return UserData;
        }

        public async Task<int> ChangePassword(t_ChangePassword changePassword)
        {
            try
            {
                await _conn.OpenAsync();
                string qry = @"
                    UPDATE t_task_track_pro_register_login
                    SET c_password = @c_newPassword
                    WHERE c_userId = @c_userId AND c_password = @c_oldPassword;
                ";

                using (NpgsqlCommand cmd = new NpgsqlCommand(qry, _conn))
                {
                    cmd.Parameters.AddWithValue("@c_userId", changePassword.c_userId);
                    cmd.Parameters.AddWithValue("@c_oldPassword", changePassword.c_oldPassword);
                    cmd.Parameters.AddWithValue("@c_newPassword", changePassword.c_newPassword);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while changing the password: " + ex.Message);
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<t_UserUpdateProfile> UpdateProfile(t_UserUpdateProfile userUpdateProfile)
        {
            try
            {
                await _conn.CloseAsync();
                await _conn.OpenAsync();

                // Retrieve old image path before updating
                string selectQuery = "SELECT c_image FROM t_task_track_pro_register_login WHERE c_userId = @c_userId;";
                string oldImagePath = null;

                using (NpgsqlCommand selectCmd = new NpgsqlCommand(selectQuery, _conn))
                {
                    selectCmd.Parameters.AddWithValue("@c_userId", userUpdateProfile.c_userId);
                    var result = await selectCmd.ExecuteScalarAsync();
                    oldImagePath = result != null ? result.ToString() : null;
                }

                // Update profile with new image
                string updateQuery = "UPDATE t_task_track_pro_register_login SET c_image = @c_newImage WHERE c_userId = @c_userId;";
                using (NpgsqlCommand updateCmd = new NpgsqlCommand(updateQuery, _conn))
                {
                    updateCmd.Parameters.AddWithValue("@c_newImage", userUpdateProfile.newImage);
                    updateCmd.Parameters.AddWithValue("@c_userId", userUpdateProfile.c_userId);

                    int rowsAffected = await updateCmd.ExecuteNonQueryAsync();
                }

                await _conn.CloseAsync();
            }
            catch (Exception ex)
            {
                await _conn.CloseAsync();
                throw new Exception("An error occurred while updating the profile: " + ex.Message);
            }

            return userUpdateProfile;
        }
    }
}