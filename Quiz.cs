using System.Collections.Generic;

public class Test
{
    public int Id;
    public string Name;
    public int Count;
    public string Status;
}

public static class Data
{
    // Тут храним все тесты
    public static List<Test> List = new List<Test>();
    public static int LastId = 0;

    public static void Add(string name, int count)
    {
        LastId++;
        Test t = new Test();
        t.Id = LastId;
        t.Name = name;
        t.Count = count;
        t.Status = "open";
        List.Add(t);
    }
}