#nullable enable

using System.Linq;
using System.Reflection;
using System.Text.Json;
using UnityEditor;
using UnityEngine;

namespace Kogane.Internal
{
    internal static class JsonSerializerWrapperGenerator
    {
        [MenuItem( "Kogane/JsonSerializer ラッパークラス生成" )]
        private static void Hoge()
        {
            var type = typeof( JsonSerializer );

            var methodInfoArray = type.GetMethods( BindingFlags.Static | BindingFlags.Public );

            var result = methodInfoArray
                    .Select( x => CreateMethodCode( x ) )
                    // .Take( 2 )
                    .ConcatWithNewLine()
                ;

            var script = $@"#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

public static class JsonSerializer
{{
{result}
}}
";

            EditorGUIUtility.systemCopyBuffer = script;
            Debug.Log( script );
        }

        private static string CreateMethodCode( MethodInfo methodInfo )
        {
            var methodName       = methodInfo.Name;
            var returnType       = methodInfo.ReturnType;
            var returnTypeString = TypeUtils.GetPrettyTypeName( returnType );
            var hasParameters    = methodInfo.GetParameters().IsNotNullOrEmpty();

            var parameters = methodInfo.GetParameters();

            var parametersString2 = parameters.IsNullOrEmpty()
                    ? ""
                    : $@" {parameters.Select( x => x.ParameterType.IsByRef ? $"ref {x.Name}" : x.Name ).ConcatWith( ", " )} "
                ;

            var template = $@"        public static {returnTypeString} {methodName}{GetGenericArgumentsCode()}{GetParametersCode()}
        {{
            {( returnType == typeof( void ) ? "" : "return " )}System.Text.Json.JsonSerializer.{methodName}{GetGenericArgumentsCode()}({parametersString2});
        }}";

            return template;

            string GetArgumentsCode()
            {
                var isExtensionMethod = methodInfo.IsExtensionMethod();

                return methodInfo
                        .GetParameters()
                        .Select
                        (
                            ( x, index ) =>
                            {
                                if ( x.HasDefaultValue )
                                {
                                    var defaultValue = ToLiteralString( x.DefaultValue );
                                    return $"{( isExtensionMethod && index == 0 ? "this " : "" )}{( x.ParameterType.IsByRef ? "ref " : "" )}{TypeUtils.GetPrettyTypeName( x.ParameterType ).Replace( "&", "" )}{( x.ParameterType.IsNullable() ? "?" : "" )} {x.Name} = {defaultValue}";
                                }

                                return $"{( isExtensionMethod && index == 0 ? "this " : "" )}{( x.ParameterType.IsByRef ? "ref " : "" )}{TypeUtils.GetPrettyTypeName( x.ParameterType ).Replace( "&", "" )}{( x.ParameterType.IsNullable() ? "?" : "" )} {x.Name}";
                            }
                        )
                        .ConcatWith( ", " )
                    ;
            }

            string GetGenericArgumentsCode()
            {
                return methodInfo.IsGenericMethod
                        ? $"<{string.Join( ", ", methodInfo.GetGenericArguments().Select( x => x.Name ) )}>"
                        : ""
                    ;
            }

            string GetParametersCode()
            {
                return hasParameters
                        ? $@"( {GetArgumentsCode()} )"
                        : "()"
                    ;
            }
        }

        /// <summary>
        /// 指定された値をリテラル付きの文字列に変換して返します
        /// </summary>
        private static string ToLiteralString( object value )
        {
            if ( value == default ) return "default";
            if ( value is bool boolValue ) return boolValue ? "true" : "false";
            if ( value is float floatValue ) return $"{floatValue}f";
            if ( value is long longValue ) return $"{longValue}L";
            if ( value is uint uintValue ) return $"{uintValue}u";
            if ( value is ulong ulongValue ) return $"{ulongValue}ul";
            if ( value is decimal decimalValue ) return $"{decimalValue}m";
            if ( value is char charValue ) return $"'{charValue}'";
            if ( value is string stringValue ) return $@"""{stringValue}""";
            return value.ToString();
        }
    }
}