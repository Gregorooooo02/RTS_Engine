# RTS Engine using MonoGame
We are creating a RTS Game Engine using MonoGame framework. It also uses a wrapped version of ImGui for the debug purposes.
For this project to work we need to install .NET 6 SDK and if by any chance something won't work, please install the MonoGame Templates:
```
dotnet new --install MonoGame.Templates.CSharp
```
If anyone needs to understand the code that's in the solution, please refer to [this](https://monogame.net/articles/getting_started/3_understanding_the_code.html) official documentation.

This project also includes the wrapped version of ImGui for the debug purposes. It was simply added as a package from the NuGet repository.

Link to the repository: [ImGui.NET](https://github.com/ImGuiNET/ImGui.NET?tab=readme-ov-file)

## Quick info about the `ContentTypeReader<T>`

In MonoGame, ContentTypeReader<T> is a generic class used for reading and processing content files during the content loading pipeline.

When you're working with content in MonoGame, such as textures, models, or sounds, these assets need to be loaded from files into memory before they can be used in your game. The content pipeline in MonoGame helps manage this process by converting your raw content files into a format that is optimized for runtime use.

ContentTypeReader<T> is a part of this pipeline. It's responsible for deserializing content files into instances of the specified type T. For example, if you have a custom type or class that you want to load from a content file, you would create a custom ContentTypeReader<T> for that type.

Here's a basic outline of how ContentTypeReader<T> works:

Content Pipeline Setup: You define your content types and how they should be processed in the content pipeline. This includes specifying the ContentTypeReader<T> for each type.
Content Compilation: When you build your game, the content pipeline compiles your raw content files (textures, models, etc.) into a format optimized for runtime use. During this process, it uses the specified ContentTypeReader<T> to read and process the content.
Content Loading: At runtime, when your game needs to use a piece of content, you load it using the ContentManager class. The ContentManager uses the appropriate ContentTypeReader<T> to deserialize the content file into an instance of the specified type T, which you can then use in your game.
Overall, ContentTypeReader<T> is an essential component of the MonoGame content pipeline, facilitating the loading and processing of content files into usable objects within your game.