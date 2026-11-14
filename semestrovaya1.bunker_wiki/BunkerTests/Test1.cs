using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class Test1
{
    [TestMethod]
    public void Template_ReplacesVariable()
    {
        var renderer = new HtmlTemplateRenderer();
        string template = "Привет, ${Name}!";
        var model = new { Name = "Бункер" };
        string result = renderer.RenderFromString(template, model);
        Assert.AreEqual("Привет, Бункер!", result);
    }

    [TestMethod]
    public void Template_ProcessesForeach()
    {
        var renderer = new HtmlTemplateRenderer();
        var model = new
        {
            Cards = new[]
            {
                new { Title = "Психоз" },
                new { Title = "Угроза" },
                new { Title = "Профессии" }
            }
        };
        string template = "$foreach (card in Cards)<div>${card.Title}</div>$endfor";
        string result = renderer.RenderFromString(template, model);
        string expected = "<div>Психоз</div><div>Угроза</div><div>Профессии</div>";
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void Database_ReturnsAllCards()
    {
        var database = new DatabaseContext();
        var cards = database.GetAllCards();
        Assert.IsNotNull(cards);
        Assert.IsTrue(cards.Count > 0, "В таблице cards нет записей.");
    }
}