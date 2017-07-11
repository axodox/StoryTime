using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;

namespace ObjectVersioning
{
  public static class VersionedType
  {
    private const string _assemblyName = "ObjectVersioning.VersionedTypes";

    private const string _namespaceName = _assemblyName;

    private const TypeAttributes _typeAttributes = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass;

    private const MethodAttributes _constructorAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;

    private const MethodAttributes _propertyMethodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Final | MethodAttributes.Virtual;

    private static readonly ModuleBuilder _moduleBuilder;

    private static readonly object _syncRoot = new object();

    private static readonly ConcurrentDictionary<string, Type> _types = new ConcurrentDictionary<string, Type>();

    static VersionedType()
    {
      var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(_assemblyName), AssemblyBuilderAccess.Run);
      _moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
    }

    public static Type Get<T>()
    {
      return Get(typeof(T));
    }

    public static Type Get(Type type)
    {
      if (!_types.TryGetValue(type.FullName, out var implementation))
      {
        lock (_syncRoot)
        {
          if (!_types.TryGetValue(type.FullName, out implementation))
          {
            implementation = Implement(type);
          }
        }
      }

      return implementation;
    }

    private static Type Implement(Type type)
    {
      var typeInfo = type.GetTypeInfo();
      if (!typeInfo.IsInterface)
      {
        throw new ArgumentException("The supplied type must be an interface", nameof(type));
      }

      var baseType = typeof(VersionedObject);
      var baseTypeInfo = baseType.GetTypeInfo();

      var typeName = _namespaceName + "." + (type.Name.StartsWith("I") ? type.Name.Substring(1) : type.Name);
      var typeBuilder = _moduleBuilder.DefineType(typeName, _typeAttributes, baseType);
      typeBuilder.AddInterfaceImplementation(type);

      foreach (var constructor in baseTypeInfo.DeclaredConstructors)
      {
        DefineRelayConstructor(baseType, typeBuilder, constructor);
      }

      foreach (var propertyInfo in typeInfo.DeclaredProperties)
      {
        DefineProperty(typeBuilder, propertyInfo);
      }

      return typeBuilder.CreateTypeInfo().AsType();
    }

    private static void DefineRelayConstructor(Type baseType, TypeBuilder typeBuilder, ConstructorInfo baseConstructor)
    {
      var parameters = baseConstructor.GetParameters();
      var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();
      var constructorBuilder = typeBuilder.DefineConstructor(_constructorAttributes, CallingConventions.HasThis, parameterTypes);

      for (var index = 0; index < parameters.Length; index++)
      {
        constructorBuilder.DefineParameter(index + 1, parameters[index].Attributes, parameters[index].Name);
      }

      var ilGenerator = constructorBuilder.GetILGenerator();
      ilGenerator.Emit(OpCodes.Ldarg_0);
      ilGenerator.Emit(OpCodes.Ldarg_1);
      ilGenerator.Emit(OpCodes.Call, baseConstructor);
      ilGenerator.Emit(OpCodes.Ret);
    }

    private static void DefineProperty(TypeBuilder typeBuilder, PropertyInfo propertyInfo)
    {
      var fieldBuilder = typeBuilder.DefineField("_" + propertyInfo.Name, propertyInfo.PropertyType, FieldAttributes.Private);

      var getMethodBuilder = typeBuilder.DefineMethod("get_" + propertyInfo.Name, _propertyMethodAttributes, propertyInfo.PropertyType, Type.EmptyTypes);
      var getIlGenerator = getMethodBuilder.GetILGenerator();
      getIlGenerator.Emit(OpCodes.Ldarg_0);
      getIlGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
      getIlGenerator.Emit(OpCodes.Ret);

      var setMethodBuilder = typeBuilder.DefineMethod("set_" + propertyInfo.Name, _propertyMethodAttributes, null, new[] { propertyInfo.PropertyType });
      var setIlGenerator = setMethodBuilder.GetILGenerator();

      setIlGenerator.DeclareLocal(typeof(bool));
      var endLabel = setIlGenerator.DefineLabel();
      setIlGenerator.Emit(OpCodes.Ldarg_0);
      setIlGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
      if (!propertyInfo.PropertyType.IsByRef) setIlGenerator.Emit(OpCodes.Box, propertyInfo.PropertyType);
      setIlGenerator.Emit(OpCodes.Ldarg_1);
      if (!propertyInfo.PropertyType.IsByRef) setIlGenerator.Emit(OpCodes.Box, propertyInfo.PropertyType);
      setIlGenerator.Emit(OpCodes.Call, typeof(object).GetRuntimeMethod("Equals", new[] { typeof(object), typeof(object) }));
      setIlGenerator.Emit(OpCodes.Brtrue_S, endLabel);
      setIlGenerator.Emit(OpCodes.Ldarg_0);
      setIlGenerator.Emit(OpCodes.Ldarg_1);
      setIlGenerator.Emit(OpCodes.Stfld, fieldBuilder);
      setIlGenerator.Emit(OpCodes.Ldarg_0);
      setIlGenerator.Emit(OpCodes.Ldstr, propertyInfo.Name);
      setIlGenerator.Emit(OpCodes.Ldarg_1);
      if (!propertyInfo.PropertyType.IsByRef) setIlGenerator.Emit(OpCodes.Box, propertyInfo.PropertyType);
      setIlGenerator.Emit(OpCodes.Call, typeof(VersionedObject).GetTypeInfo().GetDeclaredMethod("RecordSetPropertyActionAction"));
      setIlGenerator.MarkLabel(endLabel);
      setIlGenerator.Emit(OpCodes.Ret);

      var propertyBuilder = typeBuilder.DefineProperty(propertyInfo.Name, PropertyAttributes.None, propertyInfo.PropertyType, Type.EmptyTypes);
      propertyBuilder.SetGetMethod(getMethodBuilder);
      propertyBuilder.SetSetMethod(setMethodBuilder);
    }
  }
}
