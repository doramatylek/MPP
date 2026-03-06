using Xunit;
using FakerLib;
using System;
using System.Collections.Generic;

public class User
{
    public string Name { get; set; }
    public int Age { get; set; }
}

public class Address
{
    public string City { get; set; }
    public User Owner { get; set; }
}

public class MultiCtor
{
    public int A;
    public int B;

    public MultiCtor() { }

    public MultiCtor(int a, int b)
    {
        A = a;
        B = b;
    }
}

public class ImmutablePerson
{
    public string Name { get; }

    public ImmutablePerson(string name)
    {
        Name = name;
    }
}

public class A
{
    public B B { get; set; }
}

public class B
{
    public C C { get; set; }
}

public class C
{
    public A A { get; set; }
}

public struct Point
{
    public int X;
    public int Y;

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public class FakerTests
{
    [Fact]
    public void StructConstructorTest()
    {
        var faker = new Faker();

        var point = faker.Create<Point>();

        Assert.NotEqual(0, point.X);
        Assert.NotEqual(0, point.Y);
    }
    [Fact]
    public void GeneratePrimitiveTypes()
    {
        var faker = new Faker();

        int i = faker.Create<int>();
        double d = faker.Create<double>();
        float f = faker.Create<float>();
        long l = faker.Create<long>();

        Assert.IsType<int>(i);
        Assert.IsType<double>(d);
        Assert.IsType<float>(f);
        Assert.IsType<long>(l);
    }

    [Fact]
    public void GenerateString()
    {
        var faker = new Faker();

        string s = faker.Create<string>();

        Assert.NotNull(s);
        Assert.NotEmpty(s);
    }

    [Fact]
    public void GenerateDateTime()
    {
        var faker = new Faker();

        DateTime date = faker.Create<DateTime>();

        Assert.IsType<DateTime>(date);
    }

    [Fact]
    public void GenerateObject()
    {
        var faker = new Faker();

        var user = faker.Create<User>();

        Assert.NotNull(user);
        Assert.NotNull(user.Name);
    }

    [Fact]
    public void GenerateNestedObjects()
    {
        var faker = new Faker();

        var address = faker.Create<Address>();

        Assert.NotNull(address);
        Assert.NotNull(address.Owner);
    }

    [Fact]
    public void GenerateList()
    {
        var faker = new Faker();

        var list = faker.Create<List<int>>();

        Assert.NotNull(list);
        Assert.NotEmpty(list);
    }

    [Fact]
    public void GenerateNestedList()
    {
        var faker = new Faker();

        var list = faker.Create<List<List<User>>>();

        Assert.NotNull(list);
        Assert.NotEmpty(list);
    }

    [Fact]
    public void MultiConstructorTest()
    {
        var faker = new Faker();

        var obj = faker.Create<MultiCtor>();

        Assert.NotNull(obj);
    }

    [Fact]
    public void ImmutableObjectTest()
    {
        var faker = new Faker();

        var person = faker.Create<ImmutablePerson>();

        Assert.NotNull(person);
        Assert.NotNull(person.Name);
    }

    [Fact]
    public void CyclicDependencyTest()
    {
        var faker = new Faker();

        var a = faker.Create<A>();

        Assert.NotNull(a);
    }

    [Fact]
    public void ConfigGeneratorTest()
    {
        var config = new FakerConfig();

        config.Add<User, string, TestNameGenerator>(u => u.Name);

        var faker = new Faker(config);

        var user = faker.Create<User>();

        Assert.Equal("TEST_NAME", user.Name);
    }
}

public class TestNameGenerator : IValueGenerator
{
    public object Generate(Type type, GeneratorContext context)
    {
        return "TEST_NAME";
    }

    public bool CanGenerate(Type type)
    {
        return type == typeof(string);
    }
}