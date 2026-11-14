public class Card
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsFirstInCategory { get; set; }
}
public class User
{
    public int Id { get; set; }
    public string Login { get; set; } = "";
    public string Role { get; set; } = "admin";
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class Review
{
    public int Id { get; set; }
    public int CardId { get; set; }
    public string AuthorName { get; set; } = "";
    public string Text { get; set; } = "";
    public int Rating { get; set; }
}