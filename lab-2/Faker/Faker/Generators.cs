using System;
using System.Collections;
using System.Collections.Generic;

namespace FakerLib
{
    public class IntGenerator : IValueGenerator
    {
        public object Generate(Type type, GeneratorContext context)
            => context.Random.Next();

        public bool CanGenerate(Type type)
            => type == typeof(int);
    }

    public class DoubleGenerator : IValueGenerator
    {
        public object Generate(Type type, GeneratorContext context)
            => context.Random.NextDouble();

        public bool CanGenerate(Type type)
            => type == typeof(double);
    }

    public class FloatGenerator : IValueGenerator
    {
        public object Generate(Type type, GeneratorContext context)
            => (float)context.Random.NextDouble();

        public bool CanGenerate(Type type)
            => type == typeof(float);
    }

    public class LongGenerator : IValueGenerator
    {
        public object Generate(Type type, GeneratorContext context)
            => (long)(context.Random.NextDouble() * long.MaxValue);

        public bool CanGenerate(Type type)
            => type == typeof(long);
    }

    public class StringGenerator : IValueGenerator
    {
        public object Generate(Type type, GeneratorContext context)
        {
            var len = context.Random.Next(5, 15);
            var chars = "abcdefghijklmnopqrstuvwxyz";
            var result = "";

            for (int i = 0; i < len; i++)
                result += chars[context.Random.Next(chars.Length)];

            return result;
        }

        public bool CanGenerate(Type type)
            => type == typeof(string);
    }

    public class DateTimeGenerator : IValueGenerator
    {
        public object Generate(Type type, GeneratorContext context)
            => DateTime.Now.AddDays(context.Random.Next(-1000, 1000));

        public bool CanGenerate(Type type)
            => type == typeof(DateTime);
    }

    public class ListGenerator : IValueGenerator
    {
        public object Generate(Type type, GeneratorContext context)
        {
            var elementType = type.GetGenericArguments()[0];
            var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));

            var count = context.Random.Next(1, 5);

            for (int i = 0; i < count; i++)
                list.Add(context.Faker.Create(elementType));

            return list;
        }

        public bool CanGenerate(Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(List<>);
        }
    }
}