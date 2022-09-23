using System;
using System.Linq;

namespace Kogane
{
    public static class TypeUtils
    {
        // https://stackoverflow.com/questions/1533115/get-generictype-name-in-good-format-using-reflection-on-c-sharp
        public static string GetPrettyTypeName( Type t )
        {
            if ( t.IsArray )
            {
                var arrayCode = GetPrettyTypeName( t.GetElementType() ) + "[]";

                return t.IsNullable()
                        ? $"{arrayCode}?"
                        : arrayCode
                    ;
            }

            if ( t.IsGenericType )
            {
                var genericCode = string.Format
                (
                    "{0}<{1}>",
                    t.Name.Substring( 0, t.Name.LastIndexOf( "`", StringComparison.InvariantCulture ) ),
                    string.Join( ", ", t.GetGenericArguments().Select( GetPrettyTypeName ) )
                );

                return t.IsNullable()
                        ? $"{genericCode}?"
                        : genericCode
                    ;
            }

            var cSharpTypeKeyword = GetCSharpTypeKeyword( t );

            return t.IsNullable()
                    ? $"{cSharpTypeKeyword}?"
                    : cSharpTypeKeyword
                ;
        }

        public static string GetCSharpTypeKeyword( Type type )
        {
            return GetCSharpTypeKeyword( type.Name );
        }

        // https://stackoverflow.com/questions/56352299/gettype-return-int-instead-of-system-int32
        // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/built-in-types
        public static string GetCSharpTypeKeyword( string typeName )
        {
            return typeName switch
            {
                "Boolean" => "bool",
                "Byte"    => "byte",
                "SByte"   => "sbyte",
                "Char"    => "char",
                "Decimal" => "decimal",
                "Double"  => "double",
                "Single"  => "float",
                "Int32"   => "int",
                "UInt32"  => "uint",
                "Int64"   => "long",
                "UInt64"  => "ulong",
                "Int16"   => "short",
                "UInt16"  => "ushort",
                "Object"  => "object",
                "String"  => "string",
                "Void"    => "void",
                _         => typeName,
            };
        }
    }
}