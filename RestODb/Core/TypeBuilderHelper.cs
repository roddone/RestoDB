namespace RestODb.Core
{
    public static class TypeBuilderHelper
    {
        public static Type BuildTypeForEntity(string entityName, IEnumerable<ColumnDescription> columnDescriptions)
        {
            var gen = new CustomTypeBuilder.CustomTypeGenerator(name: entityName, @namespace: "RestoDb");
            foreach (var column in columnDescriptions)
            {
                Console.WriteLine(column.Name + " : " + column.Type);
                gen.AddProperty(ToPascalCase(column.Name), TryGetTypeFromString(column.Type, column.NotNull));
            }

            return gen.CompileResultType();
        }
        private static string ToPascalCase(string name)
        {
            return char.ToUpper(name[0]) + name[1..];
        }

        private static Type TryGetTypeFromString(string typeName, bool notnull)
        {
            //for now, we just return object 
            return typeof(object);

            //todo : next time we will try to determine right type from name, something like : 
            /*if (typeName.Contains("text", StringComparison.OrdinalIgnoreCase)
                || typeName.Contains("char", StringComparison.OrdinalIgnoreCase)
                || typeName.Contains("date", StringComparison.OrdinalIgnoreCase)
                || typeName.Contains("time", StringComparison.OrdinalIgnoreCase)
                || typeName.Contains("guid", StringComparison.OrdinalIgnoreCase)
                || typeName.Contains("string", StringComparison.OrdinalIgnoreCase))
                return typeof(string);

            if (typeName.Contains("int", StringComparison.OrdinalIgnoreCase))
                return notnull ? typeof(int) : typeof(int?);

            if(typeName.Contains("double", StringComparison.OrdinalIgnoreCase)
                || typeName.Contains("single", StringComparison.OrdinalIgnoreCase)
                || typeName.Contains("numeric", StringComparison.OrdinalIgnoreCase))
                return notnull ? typeof(double) : typeof(double?);

            if (typeName.Contains("byte", StringComparison.OrdinalIgnoreCase)
                || typeName.Contains("bool", StringComparison.OrdinalIgnoreCase))
                return notnull ? typeof(bool) : typeof(bool?);

            return typeof(object);*/
        }
    }
}
