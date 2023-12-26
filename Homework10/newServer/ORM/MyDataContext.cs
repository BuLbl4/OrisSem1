using System.Reflection;
using System.Text;
using Npgsql;
namespace newServer.ORM;

public class MyDataContext 
{
    private static string connectionString => "Host=localhost;Port=5433;Username=postgres;Password=Bulat2004;Database=ForOris";
    public bool Add<T>(T entity)
    {
        string tableName = typeof(T).Name;
        var type = entity?.GetType();
        var properties = type?.GetProperties(BindingFlags.Instance | BindingFlags.Public) 
            .ToList();
        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                var columns = string.Join(", ",typeof(T).GetProperties().Select(p => p.Name));
                var values = string.Join(", ", typeof(T).GetProperties().Select(p => "@" + p.Name));
                command.CommandText = $"INSERT INTO {tableName} ({columns}) VALUES ({values})";
                 
                foreach (var property in properties)
                {
                    command.Parameters.Add(new NpgsqlParameter("@" + property.Name, property.GetValue(entity)));
                }
                
                var number = command.ExecuteNonQuery();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("The adding method worked successfully {0}", number);
                Console.ResetColor();
                return true;
                
            }
        }
        
    }   

    public bool Update<T>(T entity)
    {
        var type = entity?.GetType();
        var tableName = typeof(T).Name;
        var id = type?.GetProperty("id");

        var property = type?.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(x => x != type.GetProperty("id"))
            .ToList();

        using(var connetion = new NpgsqlConnection(connectionString))
        {
            connetion.Open();
            using (var command = connetion.CreateCommand())
            {
                var columns = string.Join(", ",typeof(T).GetProperties().Where(x => x != typeof(T).GetProperty("id")).Select(p => p.Name)).Split(",").ToList();
                var values = string.Join(", ", typeof(T).GetProperties().Where(x => x != typeof(T).GetProperty("id")).Select(p => "@" + p.Name)).Split(",").ToList();
                command.CommandText = $"update {tableName} set ";
                for (int t = 0; t < property?.Count; t++)
                    command.CommandText += $"{columns[t]} = {values[t]},";

                command.CommandText = command.CommandText.Remove(command.CommandText.Length - 1);
                command.CommandText += $" where id = {id!.GetValue(entity)}";
                Console.WriteLine(command.CommandText);
                
                foreach (var prop in property)
                {
                    command.Parameters.Add(new NpgsqlParameter("@" + prop.Name, prop.GetValue(entity)));
                }
                
                var number = command.ExecuteNonQuery();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Updated data {0}", number);
                Console.ResetColor();
                return true;
            }
        }
    }

    public bool Delete<T>(int id)
    {
        var type = typeof(T);
        var tableName = type.Name;
        Console.WriteLine(tableName);
        using(var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"delete from {tableName} where id = @Id";
                command.Parameters.Add(new NpgsqlParameter("@Id", id));
                var number = command.ExecuteNonQuery();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Delete {0} object by id = {1}",number,id );
                Console.ResetColor();
                return true;
            }
        }
    }
    public bool Delete<T>(List<object> values)
    {
        var type = typeof(T);
        var tableName = type.Name;
        Console.WriteLine(tableName);

        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                var sqlBuilder = new StringBuilder($"DELETE FROM {tableName} WHERE ");

                for (int i = 0; i < values.Count; i++)
                {
                    var paramName = $"@{i}";
                    sqlBuilder.Append($"{(i == 0 ? "" : " AND ")}{type.GetProperties()[i].Name} = {paramName}");
                    command.Parameters.Add(new NpgsqlParameter(paramName, values[i]));
                }

                command.CommandText = sqlBuilder.ToString();
                var number = command.ExecuteNonQuery();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Удалено {0} строк из таблицы {1}", number, tableName);
                Console.ResetColor();

                return true;
            }
        }
    }
    

    public List<T> Select<T>()
    {
        var tableName = typeof(T).Name.ToLower();
        var result = new List<T>();
        var sql = $"select * from {tableName}";

        using (var connetion = new NpgsqlConnection(connectionString))
        {
            connetion.Open();
            using (var command = new NpgsqlCommand(sql, connetion))
            {
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var obj = Activator.CreateInstance<T>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var columnName = reader.GetName(i);
                        var prop = obj?.GetType().GetProperty(columnName);

                        if (prop != null)
                        {
                            if (!reader.IsDBNull(i))
                                prop.SetValue(obj, reader.GetValue(i));
                            else
                                prop.SetValue(obj,null);
                        }
                        else
                            break;
                    }
                    result.Add(obj);
                }
            }
        }
        return result;
    }

    public  T SelectById<T>(string id)
    {
        var tableName = typeof(T).Name;
        var sql = $"select * from {tableName} where Id = {id}";
        using(var connetion = new NpgsqlConnection(connectionString))
        {
            connetion.Open();
            using (var command = new NpgsqlCommand(sql, connetion))
            {
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var obj = Activator.CreateInstance<T>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var column = reader.GetName(i);
                        var prop = obj?.GetType().GetProperty(column);

                        if (prop != null)
                        {
                            if (!reader.IsDBNull(i))
                            {
                                prop.SetValue(obj, reader.GetValue(i));
                            }
                            else
                            {
                                prop.SetValue(obj,null);
                            }
                        }
                        else
                            break;
                    }

                    return obj;
                }
            }
        }
        return default!;
    } 
}