using System.Text.Json;
class CodePuller {
    HttpClient client;
    
    
    public CodePuller() {
        client = new HttpClient();
        client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
        client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {File.ReadAllText("ghtoken")}");
        client.DefaultRequestHeaders.Add("User-Agent", "stiglcz");
    }
    
    private void Send<T>(string url, out T? result) {
        result = default(T);
        Task<HttpResponseMessage> messaget = client.GetAsync(url);
        messaget.Wait();
        
        HttpResponseMessage message = messaget.Result;
        if(!message.IsSuccessStatusCode) return;

        Stream stream = message.Content.ReadAsStream();
        T? obj = JsonSerializer.Deserialize<T>(stream);

        if(obj == null) return;
        result = obj;
        return;
    } 
    public string? Pull(Language lang) {
        string langName = "";
        switch(lang) {
            case Language.C:    langName = "c";      break;
            case Language.Cpp:  langName = "cpp";    break;
            case Language.Cs:   langName = "csharp"; break;
            case Language.Java: langName = "java";   break;
            default: langName = "cpp"; break;
        }
        // First request
        Console.WriteLine("[Pulling code... 1 / 4]");
        string url1 = $"https://api.github.com/search/repositories?q=language:{langName}&sort=stars&order=desc";
        Send<RepositoryList>(url1, out RepositoryList? repoList);
        if(repoList == null || repoList.items == null || repoList.items.Length == 0) return null;
        int rand = new Random().Next(repoList.items.Length);
        
        // Second request
        Console.WriteLine("[Pulling code... 2 / 4]");
        string url2 = $"https://api.github.com/search/code?q=%20+language:{langName}+repo:{repoList.items[rand].full_name}";
        Send<CodeList>(url2, out CodeList? codeList);
        if(codeList == null || codeList.items == null || codeList.items.Length == 0) return null;
        int rand2 = new Random().Next(codeList.items.Length);

        // Third request
        Console.WriteLine("[Pulling code... 3 / 4]");
        string url3 = codeList.items[rand2].url;
        Send<FileOverview>(url3, out FileOverview? fileOverview);
        if(fileOverview == null || fileOverview.download_url == "") return null;

        // Final request
        Console.WriteLine("[Pulling code... 4 / 4]");
        string url4 = fileOverview.download_url;
        using (Task<HttpResponseMessage> message = client.GetAsync(url4)) {
            message.Wait();
            HttpResponseMessage msg = message.Result;
            Stream stream = msg.Content.ReadAsStream();
            int len = (int)stream.Length;
            byte[] buffer = new byte[len];
            stream.Read(buffer, 0, len);
            string result = System.Text.Encoding.UTF8.GetString(buffer, 0, len);
            return result;
        }
    }
    class RepositoryList {
        public int total_count { get; set; }
        public bool incomplete_results { get; set; }
        public required Repository[] items { get; set; }
    }

    class CodeList {
        public int total_count { get; set; }
        public bool incomplete_results { get; set; }
        public required Code[] items { get; set; }
    }

    class FileOverview { public required string  download_url { get; set; } }
    class Repository { public required string full_name { get; set; } }
    class Code { public required string url { get; set; } }
}
