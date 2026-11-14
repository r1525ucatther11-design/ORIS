using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

public class CardsController : IController
{
    public bool CanHandle(string url, string method)
    {
        return method == "GET" && (url == "/cards" || url == "/api/search");
    }

    public void Handle(HttpListenerContext context)
    {
        var req = context.Request;
        var res = context.Response;
        string url = req.Url.AbsolutePath;

        if (url == "/api/search")
        {
            string query = System.Web.HttpUtility.UrlDecode(req.QueryString["q"] ?? "").ToLower();
            var db = new DatabaseContext();
            var results = db.GetAllCards().Where(c => c.Title.ToLower().Contains(query)).ToList();
            
            StringBuilder htmlBuilder = new StringBuilder();
            foreach (var card in results) htmlBuilder.Append(GetCardHtml(card));

            byte[] buffer = Encoding.UTF8.GetBytes(htmlBuilder.ToString());
            res.ContentType = "text/html; charset=utf-8";
            res.OutputStream.Write(buffer, 0, buffer.Length);
            res.OutputStream.Close();
        }
        else
        {
            var db = new DatabaseContext();
            var allCards = db.GetAllCards();
            
            var sortedCards = allCards.OrderBy(c => c.Category == "Профессия" ? 1 :
                                                    c.Category == "Биология" ? 2 :
                                                    c.Category == "Здоровье" ? 3 :
                                                    c.Category == "Хобби" ? 4 :
                                                    c.Category == "Багаж" ? 5 :
                                                    c.Category == "Факты" ? 6 :
                                                    c.Category == "Особые условия" ? 7 :
                                                    c.Category == "Бункер" ? 8 :
                                                    c.Category == "Катастрофа" ? 9 :
                                                    c.Category == "Угроза" ? 10 : 11)
                                         .ThenBy(c => c.Title).ToList();

            StringBuilder htmlBuilder = new StringBuilder();
            string currentCategory = "";

            foreach (var card in sortedCards)
            {
                if (card.Category != currentCategory)
                {
                    if (currentCategory != "") htmlBuilder.Append("</div>");
                    htmlBuilder.Append($"<h2 class='category-header' id='{card.Category}'>{card.Category}</h2>");
                    htmlBuilder.Append("<div class='cards-grid'>");
                    currentCategory = card.Category;
                }
                htmlBuilder.Append(GetCardHtml(card));
            }
            if (currentCategory != "") htmlBuilder.Append("</div>");

            var model = new PageModel { Count = allCards.Count, RenderedCards = htmlBuilder.ToString() };
            var renderer = new HtmlTemplateRenderer();
            string html = renderer.RenderFromFile("Templates/cards.html", model);
            
            byte[] buffer = Encoding.UTF8.GetBytes(html);
            res.ContentType = "text/html; charset=utf-8";
            res.OutputStream.Write(buffer, 0, buffer.Length);
            res.OutputStream.Close();
        }
    }

    private string GetCardHtml(Card card)
    {
        string bgImage = "";
        switch (card.Category)
        {
            case "Профессия": bgImage = "https://i.postimg.cc/bv05DyBK/PROFESSII.png"; break;
            case "Биология": bgImage = "https://i.postimg.cc/mkWhdSwV/BIOLOGIA.png"; break;
            case "Здоровье": bgImage = "https://i.postimg.cc/Xq6X2g8s/ZDOROV_E.png"; break;
            case "Хобби": bgImage = "https://i.postimg.cc/L5k5GZCT/HOBBI.png"; break;
            case "Багаж": bgImage = "https://i.postimg.cc/mkWhdSw8/BAGAZ.png"; break;
            case "Факты": bgImage = "https://i.postimg.cc/6qVqFGjL/FAKTY.png"; break;
            case "Особые условия": bgImage = "https://i.postimg.cc/Bb5bR1Yx/OSOBYE-USLOVIA.png"; break;
            case "Бункер": bgImage = "https://i.postimg.cc/3NQWLZB1/BUNKER1.png"; break;
            case "Катастрофа": bgImage = "https://i.postimg.cc/Hn0nF7Zw/katastrofa.png"; break;
            case "Угроза": bgImage = "https://i.postimg.cc/Vv9v2bG9/UGROZA.png"; break;
            default: bgImage = "https://i.postimg.cc/6qVqFGjL/FAKTY.png"; break;
        }

        return $@"<div class='game-card'>
            <div class='card-bg' style='background-image: url(""{bgImage}"");'></div>
            <div class='card-overlay'></div>
            <div class='card-content'>
                <span class='card-cat'>{card.Category}</span>
                <div class='card-title'>{card.Title}</div>
                <div class='card-desc'>{card.Description}</div>
            </div>
        </div>";
    }
}