using System;
using System.Collections.Generic;
using Npgsql;

public class DatabaseContext
{
    private string _connectionString = "host=localhost;Username=postgres;Password=1520;Database=retail";

    public List<Card> GetAllCards()
    {
        var cards = new List<Card>();
        using (var conn = new NpgsqlConnection(_connectionString))
        {
            conn.Open();
            string sql = "SELECT id, title, category, description FROM cards ORDER BY id";
            using (var cmd = new NpgsqlCommand(sql, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    cards.Add(new Card
                    {
                        Id = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Category = reader.GetString(2),
                        Description = reader.GetString(3)
                    });
                }
            }
        }
        return cards;
    }

    public void AddCard(string title, string category, string description)
    {
        using (var conn = new NpgsqlConnection(_connectionString))
        {
            conn.Open();
            string sql = "INSERT INTO cards (title, category, description) VALUES (@t, @c, @d)";
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("t", title);
                cmd.Parameters.AddWithValue("c", category);
                cmd.Parameters.AddWithValue("d", description);
                cmd.ExecuteNonQuery();
            }
        }
    }

    public List<Card> SearchCards(string query)
    {
        var cards = new List<Card>();
        using (var conn = new NpgsqlConnection(_connectionString))
        {
            conn.Open();
            string sql = "SELECT id, title, category, description FROM cards WHERE LOWER(title) LIKE LOWER(@q)";
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("q", "%" + query + "%");
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cards.Add(new Card
                        {
                            Id = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            Category = reader.GetString(2),
                            Description = reader.GetString(3)
                        });
                    }
                }
            }
        }
        return cards;
    }

    public void DeleteCard(int id)
    {
        using (var conn = new NpgsqlConnection(_connectionString))
        {
            conn.Open();
            string sql = "DELETE FROM cards WHERE id = @id";
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("id", id);
                cmd.ExecuteNonQuery();
            }
        }
    }

    public void UpdateCard(int id, string title, string category, string description)
    {
        using (var conn = new NpgsqlConnection(_connectionString))
        {
            conn.Open();
            string sql = "UPDATE cards SET title = @t, category = @c, description = @d WHERE id = @id";
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("t", title);
                cmd.Parameters.AddWithValue("c", category);
                cmd.Parameters.AddWithValue("d", description);
                cmd.Parameters.AddWithValue("id", id);
                cmd.ExecuteNonQuery();
            }
        }
    }


    public User GetUserByLogin(string login)
    {
        using (var conn = new NpgsqlConnection(_connectionString))
        {
            conn.Open();
            string sql = "SELECT id, login, role FROM users WHERE login = @l";
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("l", login);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new User
                        {
                            Id = reader.GetInt32(0),
                            Login = reader.GetString(1),
                            Role = reader.GetString(2)
                        };
                    }
                }
            }
        }
        return null;
    }


    public List<Category> GetAllCategories()
    {
        var categories = new List<Category>();
        using (var conn = new NpgsqlConnection(_connectionString))
        {
            conn.Open();
            string sql = "SELECT id, name FROM categories ORDER BY id";
            using (var cmd = new NpgsqlCommand(sql, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    categories.Add(new Category
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1)
                    });
                }
            }
        }
        return categories;
    }


    public List<Review> GetReviewsByCardId(int cardId)
    {
        var reviews = new List<Review>();
        using (var conn = new NpgsqlConnection(_connectionString))
        {
            conn.Open();
            string sql = "SELECT id, cardid, authorname, text, rating FROM reviews WHERE cardid = @cid ORDER BY id";
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("cid", cardId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reviews.Add(new Review
                        {
                            Id = reader.GetInt32(0),
                            CardId = reader.GetInt32(1),
                            AuthorName = reader.GetString(2),
                            Text = reader.GetString(3),
                            Rating = reader.GetInt32(4)
                        });
                    }
                }
            }
        }
        return reviews;
    }

    public void AddReview(int cardId, string authorName, string text, int rating)
    {
        using (var conn = new NpgsqlConnection(_connectionString))
        {
            conn.Open();
            string sql = "INSERT INTO reviews (cardid, authorname, text, rating) VALUES (@cid, @a, @t, @r)";
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("cid", cardId);
                cmd.Parameters.AddWithValue("a", authorName);
                cmd.Parameters.AddWithValue("t", text);
                cmd.Parameters.AddWithValue("r", rating);
                cmd.ExecuteNonQuery();
            }
        }
    }
}