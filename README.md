# Description 
A scraper for getting down the top `n` hacker news posts. 

# Running 
Make sure you have [dotnet core 3.1 SDK](https://dotnet.microsoft.com/download/dotnet-core/3.1) installed. 

Then in the project root run: 

`dotnet run --project TrueLayer.Scraper/TrueLayer.Scraper.csproj --posts 10`

Where the number after `--posts` is the required number of top posts where `0 < posts <= 100`

## Running Unit and Acceptance Tests 
From the root of the project you can run the following: 

`dotnet test TrueLayer.Scraper.sln -v n`

# Libraries Used 
- **HtmlAgilityPack** to parse the hacker news HTML and access it using XPATH. 
- **Newtonsoft.Json** to serialise the resulting objects as JSON. 
- **Microsoft.Extensions.Http** to add the Http Client and delegating handler. 
- **Microsoft.Extensions.Hosting** to get the default DI/configuration reading/logging from the host builder. 

## Testing Libraries
- **AutoFixture** to create fake, mocked, or subject objects. 
- **AutoFixture.AutoMoq** to mock dependencies. 
- **Moq** for mocking. 
- **RichardSzalay.MockHttp** to mock the http client requests. 
- **WireMock.Net** to create a fake server which the acceptance tests can talk to. 

# Considerations & Assumptions 
## Validation 
- If a post is deemed *invalid* by the requirements it will be skipped from the results. For example, if the top post had no author, it would not be included in the results. 
## Duplication
- If an Id is found to match between pages then it will be skipped from the results on the subsequent page (keeping it's initial rank). The consequence of this is that if you request `50` posts, you might see that the last ranked post is `51`. 
## Being polite 
- There is a hardcoded delay of `250ms` to every request using the http client. 
