using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace FakerLib
{
    public interface IFaker
    {
        T Create<T>();
        object Create(Type type);
    }

    public class Faker : IFaker
    {
        private readonly Random _random = new();
        private readonly List<IValueGenerator> _generators = new();
        private readonly Dictionary<Type, int> _creationStack = new();
        private readonly FakerConfig _config;

        public Faker(FakerConfig config = null)
        {
            _config = config ?? new FakerConfig();

            _generators.Add(new IntGenerator());
            _generators.Add(new DoubleGenerator());
            _generators.Add(new FloatGenerator());
            _generators.Add(new LongGenerator());
            _generators.Add(new StringGenerator());
            _generators.Add(new DateTimeGenerator());
            _generators.Add(new ListGenerator());
        }

        public T Create<T>()
        {
            return (T)Create(typeof(T));
        }

        public object Create(Type type)
        {
            if (_creationStack.ContainsKey(type))
                return GetDefaultValue(type);

            _creationStack[type] = 1;

            var context = new GeneratorContext(_random, this);

            foreach (var gen in _generators)
                if (gen.CanGenerate(type))
                {
                    _creationStack.Remove(type);
                    return gen.Generate(type, context);
                }

            var result = CreateObject(type);

            _creationStack.Remove(type);
            return result;
        }

        private object CreateObject(Type type)
        {
            var constructors = type.GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length);

            foreach (var ctor in constructors)
            {
                try
                {
                    var parameters = ctor.GetParameters();
                    var args = new object[parameters.Length];

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var param = parameters[i];

                        if (_config.TryGetGenerator(type, param.Name, out var gen))
                            args[i] = gen.Generate(param.ParameterType, new GeneratorContext(_random, this));
                        else
                            args[i] = Create(param.ParameterType);
                    }

                    var obj = ctor.Invoke(args);

                    FillMembers(type, obj);

                    return obj;
                }
                catch { }
            }

            return GetDefaultValue(type);
        }

        private void FillMembers(Type type, object obj)
        {
            var context = new GeneratorContext(_random, this);

            foreach (var prop in type.GetProperties())
            {
                if (!prop.CanWrite)
                    continue;

                if (_config.TryGetGenerator(type, prop.Name, out var gen))
                    prop.SetValue(obj, gen.Generate(prop.PropertyType, context));
                else
                    prop.SetValue(obj, Create(prop.PropertyType));
            }

            foreach (var field in type.GetFields())
            {
                if (_config.TryGetGenerator(type, field.Name, out var gen))
                    field.SetValue(obj, gen.Generate(field.FieldType, context));
                else
                    field.SetValue(obj, Create(field.FieldType));
            }
        }

        private static object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
                return Activator.CreateInstance(t);
            return null;
        }
    }

    public class GeneratorContext
    {
        public Random Random { get; }
        public IFaker Faker { get; }

        public GeneratorContext(Random random, IFaker faker)
        {
            Random = random;
            Faker = faker;
        }
    }

    public interface IValueGenerator
    {
        object Generate(Type type, GeneratorContext context);
        bool CanGenerate(Type type);
    }
}