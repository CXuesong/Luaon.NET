# LUAON.NET

Reading / writing LUA table expressions becomes easy. It's LUA Object Notation (or LUA Table Notation).

| Package                                  | Status                                   |
| ---------------------------------------- | ---------------------------------------- |
| [CXuesong.Luaon](https://www.nuget.org/packages/CXuesong.Luaon) | ![NuGet version (CXuesong.Luaon)](https://img.shields.io/nuget/vpre/CXuesong.Luaon.svg?style=flat-square) ![NuGet version (CXuesong.Luaon)](https://img.shields.io/nuget/dt/CXuesong.Luaon.svg?style=flat-square) |
| [CXuesong.Luaon.Json](https://www.nuget.org/packages/CXuesong.Luaon.Json) | ![NuGet version (CXuesong.Luaon.Json)](https://img.shields.io/nuget/vpre/CXuesong.Luaon.Json.svg?style=flat-square) ![NuGet version (CXuesong.Luaon.Json)](https://img.shields.io/nuget/dt/CXuesong.Luaon.Json.svg?style=flat-square) |

[Try out the packages with Xamarin Workbooks!](Workbooks)

## CXuesong.Luaon

This package contains classes for writing Lua tables in a manner similar to `Newtonsoft.Json`, such as

Basic simple-value converter

```c#
using Luaon;
LuaConvert.ToString(12345)
```

```lua
12345
```

```c#
// Custom string delimiter
LuaConvert.ToString("string expression\n\nNew line", "[=====[")
```

```lua
[=====[string expression
New line]=====]
```

A primitive counterpart of `Newtonsoft.Json.Linq`.

```c#
using Luaon.Linq;
var table = new LTable();
table.Add("Item1");
table.Add("Key1", 123);
table.Add("Key with space", 789);
table.Add("Item2");
table.Add(100, "Value100");
table.Add("ChildTable", new LTable(1, 2, 3, 4, 5));
var luaExpr = table.ToString();
var table2 = LToken.Parse(luaExpr);
luaExpr
```

```lua
{
  "Item1",
  Key1 = 123,
  ["Key with space"] = 789,
  "Item2",
  [100] = "Value100",
  ChildTable = {
    1,
    2,
    3,
    4,
    5
  }
}
```

Access table values in a Lua way.

```c#
table[2]
```

```lua
"Item2"
```

And a Lua table writer.

```c#
using System.IO;
using (var sw = new StringWriter())
{
    using (var lw = new LuaTableTextWriter(sw) {
        Formatting = Formatting.Prettified,
        CloseWriter = false
    })
    {
        lw.WriteStartTable();
        lw.WriteLiteral("Records");
        lw.WriteKey("Updated");
        lw.WriteLiteral(DateTime.Now);
        lw.WriteLiteral("Value1");
        lw.WriteLiteral("Value2");
        lw.WriteLiteral("Value3");
        lw.WriteEndTable();
    }
    return sw.ToString();
}
```

```lua
{
  "Records",
  Updated = "2018-01-12T23:13:27.7863687+08:00",
  "Value1",
  "Value2",
  "Value3"
}
```

There is also a reader now.

```c#
using (var sr = new StringReader("{a = 10, ['b'] = {100, 200, k=20} }"))
using (var lr = new LuaTableTextReader(sr))
{
    while (lr.Read() != LuaTableReaderToken.None)
    {
        Console.WriteLine("{0}: {1}, {2}", lr.CurrentPath, lr.CurrentToken, lr.CurrentValue);
    }
}
```

## CXuesong.Luaon.Json

This package contains `JsonLuaWriter`, a `JsonWriter` implementation that writes Lua tables instead of JS objects.

```c#
using Luaon.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

var obj = new {
    Name = "Graystripe",
    Affiliation = new {
        Current = "ThunderClan",
        Past = new [] { "RiverClan", "Kittypet", "Loner" },
    },
    Age = 132   // Months
};

var serializer = new JsonSerializer();
using (var sw = new StringWriter())
{
    using (var jlw = new JsonLuaWriter(sw) {
        CloseOutput = false,
        Formatting = Formatting.Indented})
    {
        serializer.Serialize(jlw, obj);
    }
    return sw.ToString();
}
```

```lua
{
  Name = "Graystripe",
  Affiliation = {
    Current = "ThunderClan",
    Past = {
      "RiverClan",
      "Kittypet",
      "Loner"
    }
  },
  Age = 132
}
```
