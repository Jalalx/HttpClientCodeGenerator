# HttpClientGenerator
HttpClientGenerator is a tool that uses Roslyn code generator feature to write boilerplate HttpClient code for you.

## The Philosophy
> You should not write or generate boilerplate code. Your repository should not host or track auto-generated code.

This leaves you with:
* A much cleaner codebase
* No need for calling generator tool everytime HTTP contracts change
* You do not need to maintain or have a separate tool around out of your repo
* And no dependency on any 3rd-party code at runtime!

### Installing
```sh
dotnet add package HttpClientGenerator
```

### Usage
After installign nuget package, add following code to your class:
```csharp
//using area
using HttpClientGenerator.Shared;
using System.Text.Json.Serialization;
// ...

// Make sure to mark the class and method as partial!
public partial class ToDoHttpService
{
    [HttpGet("todos/{id}")]
    public partial Task<ToDoItem> GetToDoItemByIdAsync(int id);    
}

public class ToDoItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("firstName")]
    public int? UserId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("completed")]
    public bool IsCompleted { get; set; }
}
```
Notice to the `partial` keyword on class and method definition. The library generates the required `GetToDoItemByIdAsync` method for you. All you need to do is to call the method like this:
```csharp
static class Program
{
    static async Task Main(string[] args)
    {
        using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");
            
            // The tool will generate a constructor with HttpClient argument for you
            var todoService = new ToDoHttpService(client);
            
            // Simply call the partial method, the tool has generated the required code for you
            var item = await todoService.GetToDoItemByIdAsync(1);

            Console.WriteLine($"Task {item.Title}: completed: {item.IsCompleted}");
        }

        Console.Read();
    }
}
```

Then build and run!

```
// output
Task delectus aut autem: completed: False
```

It's cool, isn't it?

### Known Issues
* You will currently need to restart Visual Studio 2019 to see IntelliSense and get rid of errors with the early tooling experience.
* If you are using Visual Studio Code (Omnisharp C# extension) you might see some red lines under the partial method.
Until now, there is no full support for this feature on Omnisharp, but dotnet SDK will work without problem.

**Please feel free to open issue for reporting a bug or missing feature.**