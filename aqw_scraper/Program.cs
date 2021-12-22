// @nuget: HtmlAgilityPack

using System;
using System.Net;
using System.Xml;
using System.Linq;
using HtmlAgilityPack;
using System.Collections.Generic;

public class Program
{
	private static string getRawHtml(string url)
	{
		try
		{
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			using (WebClient client = new WebClient())
			{
                //string random = new Random().Next(999, 99999999).ToString();
                //client.Headers.Add($"User-Agent: " + random);
				client.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
				client.UseDefaultCredentials = true;
                client.Proxy.Credentials = CredentialCache.DefaultCredentials;
                client.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
				return client.DownloadString(url);
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
			return null;
		}
	}

	public static void charPageData(string username)
	{
		try
		{
			string cp_uri = @"https://account.aq.com/CharPage?id=" + username;
			var html = getRawHtml(cp_uri);

			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(html);

			var htmlNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='card-body']/div");

            foreach (var node in htmlNodes)
			{
				string[] lines = node.InnerText.Split(new string[] { "\n" }, StringSplitOptions.None);
				foreach (string line in lines)
				{
					string temp = line.Trim();
					if (temp == String.Empty) continue;
					string cleanStr = HtmlEntity.DeEntitize(temp);
					Console.WriteLine(cleanStr);
				}
			}
		}
		catch (Exception ex)
        {
			Console.WriteLine(ex);
        }
	}

	public static string getTotalPage(string wikiUrl)
	{
		string randomNum = new Random().Next(999, 9999999).ToString();
		if (debugDropSearcher)
			Console.WriteLine(wikiUrl);

		var html = getRawHtml(wikiUrl);
		//if (debugDropSearcher)
			//Console.WriteLine(html);

        var htmlDoc = new HtmlDocument();
		htmlDoc.LoadHtml(html);

		var htmlNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='list-pages-item']");
		string pageCount = htmlDoc.DocumentNode.SelectNodes("//div[@class='pager']/span[@class='pager-no']")[0].InnerText;
		string totalPage = pageCount.Split(new string[] { " of " }, StringSplitOptions.None)[1];
		return totalPage;
	}

	public static string getJoinStr(string mapUrl)
    {
		try
        {
			var html = getRawHtml(mapUrl);
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(html);

			var htmlNodes = htmlDoc.DocumentNode.SelectNodes("//div[@id='page-content']");
			foreach (var node in htmlNodes)
			{
				string[] lines = node.InnerText.Trim().Split(new string[] { "\n" }, StringSplitOptions.None);
				foreach (string line in lines)
                {
					if (line == String.Empty)
						continue;
					if (line.StartsWith("/join"))
						return line;
				}
			}
			return null;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex);
			return null;
		}
	}

	public static string[] getRandomItem(string type)
	{
		try
		{
			string typeCode = String.Empty;
			switch(type)
			{
				case "any":
					type = "Any";
					typeCode = "-=";
					break;
				case "armor":
					type = "Armor";
					typeCode = "Armors";
					break;
				case "cape":
					type = "Cape";
					typeCode = "Capes%20&%20Back%20Items";
					break;
				case "helm":
					type = "Helm";
					typeCode = "Helmets%20&%20Hoods";
					break;
				case "pet":
					type = "Pet";
					typeCode = "Pets";
					break;
				case "weapon":
					type = "Weapon";
					string[] wepType = { "Axes", "Bows", "Daggers", "Guns", "Maces", "Polearms", "Staffs", "Swords", "Wands" };
					int randomidx = new Random().Next(0, wepType.Length);
					typeCode = wepType[randomidx];
					break;
				case "axe":
					type = "Axe";
					typeCode = "Axes";
					break;
				case "bow":
					type = "Bow";
					typeCode = "Bows";
					break;
				case "dagger":
					type = "Dagger";
					typeCode = "Daggers";
					break;
				case "gun":
					type = "Gun";
					typeCode = "Guns";
					break;
				case "mace":
					type = "Mace";
					typeCode = "Maces";
					break;
				case "polearm":
					type = "Polearm";
					typeCode = "Polearms";
					break;
				case "staff":
					type = "Staff";
					typeCode = "Staffs";
					break;
				case "sword":
					type = "Sword";
					typeCode = "Swords";
					break;
				case "wand":
					type = "Wand";
					typeCode = "Wands";
					break;
				case "house":
					type = "House";
					typeCode = "Houses";
					break;
				default:
					type = "Any";
					typeCode = "-=";
					break;
			}

			string searchUrl = @"http://aqwwiki.wikidot.com/search-items-by-tag/parent/" + typeCode + @"/tags/+0ac%20+drop%20+freeplayer%20-rare%20-pseudo-rare%20-seasonal%20-specialoffer%20-_index%20-_redirect/perPage/5/order/random";
			
			int totalPage = int.Parse(getTotalPage(searchUrl));
			if (debugDropSearcher)
				Console.WriteLine("totalPage: " + totalPage);

			List<int> possibleNum = Enumerable.Range(1, totalPage).ToList();

			while (true)
            {
				int index = new Random().Next(0, possibleNum.Count);
				string randomPage = possibleNum[index].ToString();
				possibleNum.RemoveAt(index);
				if (debugDropSearcher)
					Console.WriteLine("Goto Page: " + randomPage);

				string newUrl = searchUrl + $"/p/" + randomPage;
				if (debugDropSearcher)
					Console.WriteLine(newUrl);

				var html = getRawHtml(newUrl);
				//Console.WriteLine(html);

				var htmlDoc = new HtmlDocument();
				htmlDoc.LoadHtml(html);

				var htmlNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='list-pages-item']");

				foreach (var node in htmlNodes)
				{
					string raw = node.InnerText;
					if (raw.Contains("Price: N/A (Dropped by") && raw.Contains("Sellback: 0 AC") && !raw.Contains("Type: Item") && !raw.Contains("Type: Quest Item") && !raw.Contains("Type: Resource"))
					{
						if (debugDropSearcher)
							Console.WriteLine(node.InnerHtml);

						string[] lines = node.InnerText.Split(new string[] { "\n" }, StringSplitOptions.None);
						bool isMultiMap = Array.Find(lines, element => element.Trim() == "Locations:") != null ? true : false;
						if (debugDropSearcher)
							Console.WriteLine(isMultiMap);

						string wikiUrl = @"http://aqwwiki.wikidot.com";
						string itemUrl = wikiUrl + node.SelectSingleNode("./p/strong/a").Attributes["href"].Value;
						if (debugDropSearcher)
							Console.WriteLine(itemUrl);

						string mapUrlPath = String.Empty;
						if (isMultiMap)
							mapUrlPath = node.SelectSingleNode("./div[@class='item-content m-content']/ul/li/a")?.Attributes["href"]?.Value;
						else mapUrlPath = node.SelectSingleNode("./div[@class='item-content m-content']/p/a")?.Attributes["href"]?.Value;
						string mapUrl = wikiUrl + mapUrlPath;
						if (debugDropSearcher)
							Console.WriteLine(mapUrl);
						string joinStr = getJoinStr(mapUrl);
						if (debugDropSearcher)
							Console.WriteLine(joinStr);

						//foreach (string line in lines)
						//{
						//	string temp = line.Trim();
						//	if (temp == String.Empty) continue;
						//	string cleanStr = HtmlEntity.DeEntitize(temp);
						//	Console.WriteLine(cleanStr);
						//}

						if (isMultiMap)
						{
							//string location = String.Empty;
							//int idx = Array.FindIndex(lines, element => element.Trim().Contains("Locations:")) + 1;
							//while (idx < lines.Length)
							//{
							//	if (lines[idx].Trim() != String.Empty)
							//	{
							//		location = lines[idx];
							//		break;
							//	}
							//	idx++;
							//}

							string itemName = String.Empty;
							int idx = Array.FindIndex(lines, element => element.Trim().Contains("Locations:")) - 1;
							while (idx >= 0)
							{
								if (lines[idx].Trim() != String.Empty)
								{
									itemName = lines[idx];
									break;
								}
								idx--;
							}

							string droppedByRaw = Array.Find(lines, element => element.Trim().StartsWith("Price: N/A "));
							string droppedBy = droppedByRaw.Split(new string[] { " N/A " }, StringSplitOptions.None)[1];

							Console.WriteLine($"({type}): {itemName} - {joinStr} {droppedBy} - {itemUrl}");
						}
						else
						{
							//int idx = Array.FindIndex(lines, element => element.Trim().StartsWith("Location:"));
							//string location = lines[idx].Split(new string[] { ": " }, StringSplitOptions.None)[1];

							string itemName = String.Empty;
							int idx = Array.FindIndex(lines, element => element.Trim().Contains("Location:")) - 1;
							while (idx >= 0)
							{
								if (lines[idx].Trim() != String.Empty)
								{
									itemName = lines[idx];
									break;
								}
								idx--;
							}

							string droppedByRaw = Array.Find(lines, element => element.Trim().StartsWith("Price: N/A "));
							string droppedBy = droppedByRaw.Split(new string[] { " N/A " }, StringSplitOptions.None)[1];

							Console.WriteLine($"({type}): {itemName} - {joinStr} {droppedBy} - {itemUrl}");
						}

						return lines;
					}
				}
			}
			return null;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex);
			return null;
		}
	}


	static bool debugDropSearcher = false;

	public static void Main()
	{
		while (true)
		{
			Console.WriteLine("AQW Scrapper Tools Menu");
			Console.WriteLine("1. Give you any 0 ACs drop information");
			Console.WriteLine("2. Charpage (show main information about the account)");
			Console.Write("Input (1/2): ");
			
			int inputMenu = int.Parse(Console.ReadLine());

			if (inputMenu == 1)
			{
				Console.WriteLine("\nAvailable drop types: any / armor / cape / helm / pet / weapon / axe / bow / dagger / gun / mace / polearm / staff / sword / wand / house");
				Console.Write("Type of drop: ");
				string dropType = Console.ReadLine();

				//Drops Scrapper
				string[] lines = null;
				while (lines == null)
				{
					Console.WriteLine("----------- Result -----------");
					lines = getRandomItem(type: dropType);
					Console.WriteLine("------------------------------\n");
				}

				//if (debugDropSearcher)
				//	foreach (string line in lines)
				//	{
				//		string temp = line.Trim();
				//		if (temp == String.Empty) continue;
				//		string cleanStr = HtmlEntity.DeEntitize(temp);
				//		//Console.WriteLine(cleanStr + " -- Idx: " + Array.FindIndex(lines, element => element == line));
				//		Console.WriteLine(cleanStr);
				//	}
			}
			else if (inputMenu == 2)
			{
                Console.Write("Account IGN: ");
                string inputIgn = Console.ReadLine();

				//Char Page Scrapper
				Console.WriteLine("----------- Result -----------");
				charPageData(username: inputIgn);
				Console.WriteLine("------------------------------\n");
			}
		}
    }
}