# QueryString Helper
A small library to help parse and manipulate ASP.NET QueryString parameters in C#

## Code Examples ##

```csharp
QueryStringHelper qs1 = new QueryStringHelper(Request.QueryString); // initialise from Request.QueryString

string query = "?page=5&username=dan&year=2010&enabled=true&email=dan@example.com&option=apple&option=banana&option=melon&date=2015/07/06";

QueryStringHelper qs = new QueryStringHelper(query); // intialise from string

string username = qs.GetValueByName("username"); // username = "dan"

qs.Add("category", "products"); // adds a new key called "category" with the value "products"

qs.AddOrReplace("year", 1999); // changes the year value from "2010" to "1999"

int year = qs.GetValueByName<int>("year"); // year = 1999

qs.AddOrReplace("page", 6); // changes the value of "page" to "6"

bool enabled = qs.GetValueByName<bool>("enabled"); // enabled = true

qs.RemoveByName("email"); // removes the "email" key

IEnumerable<string> options = qs.GetMultipleValuesByName("option"); // options[0] = "apple", options[1] = "banana", options[2] = "melon"

DateTime date = qs.GetValueByName<DateTime>("date"); // get a DateTime value

var day = qs.GetValueByName<int?>("day", x => Convert.ToInt32(x)); // day = null

var mailAddress = qs.GetValueByName<System.Net.Mail.MailAddress>("email", e => new System.Net.Mail.MailAddress(e)); // use a delegate func

string querystring = qs.ToString(); // qs = "page=6&username=dan&year=1999&enabled=true&option=apple%2cbanana%2cmelon&date=2015%2f07%2f06&category=products";

int count = qs.Count(); // count = 7
```


