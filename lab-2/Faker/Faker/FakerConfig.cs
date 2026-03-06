using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FakerLib
{
    public class FakerConfig
    {
        private readonly Dictionary<(Type, string), IValueGenerator> _generators = new();

        public void Add<TClass, TProp, TGenerator>(Expression<Func<TClass, TProp>> expr)
            where TGenerator : IValueGenerator, new()
        {
            var body = (MemberExpression)expr.Body;

            var classType = typeof(TClass);
            var name = body.Member.Name;

            _generators[(classType, name)] = new TGenerator();
        }

        public bool TryGetGenerator(Type type, string name, out IValueGenerator generator)
        {
            return _generators.TryGetValue((type, name), out generator);
        }
    }
}