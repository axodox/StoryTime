using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ObjectVersioning
{
  public static class VersionedType
  {
    private const string _assemblyName = "ObjectVersioning.VersionedTypes";

    private const string _namespaceName = _assemblyName;

    private const TypeAttributes _typeAttributes = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass;

    private const MethodAttributes _constructorAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;

    private const MethodAttributes _propertyMethodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Final | MethodAttributes.Virtual;

    private const MethodAttributes _interfacePropertyMethodAttributes = MethodAttributes.Private | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Final | MethodAttributes.Virtual;

    private static readonly ModuleBuilder _moduleBuilder;

    private static readonly object _syncRoot = new object();

    private static readonly ConcurrentDictionary<string, Type> _types = new ConcurrentDictionary<string, Type>();

    private static readonly MethodInfo _equalsMethodInfo = typeof(object).GetRuntimeMethod("Equals", new[] { typeof(object), typeof(object) });

    private static readonly ConstructorInfo _jsonAttributeConstructor = typeof(JsonConstructorAttribute).GetTypeInfo().DeclaredConstructors.FirstOrDefault(p => p.GetParameters().Length == 0);

    private static readonly MethodInfo _recordSetPropertyActionMethodInfo = typeof(VersionedObject).GetTypeInfo().GetDeclaredMethod("RecordSetPropertyAction");

    static VersionedType()
    {
      var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(_assemblyName), AssemblyBuilderAccess.Run);
      _moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
    }

    public static T New<T>()
    {
      return (T)New(typeof(T));
    }

    public static object New(Type type, IHistoryStorage storage = null)
    {
      var implementation = Get(type);
      return Activator.CreateInstance(implementation, storage ?? NullStorage.Instance);
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
            implementation = ImplementType(type);
          }
        }
      }

      return implementation;
    }

    public static T Deserialize<T>(string value)
    {
      return (T)Deserialize(value, typeof(T));
    }

    public static object Deserialize(string value, Type type)
    {
      return JsonConvert.DeserializeObject(value, Get(type));
    }

    public static string Serialize(object value)
    {
      return JsonConvert.SerializeObject(value, Formatting.Indented);
    }

    private static Type ImplementType(Type type)
    {
      var typeInfo = type.GetTypeInfo();
      if (!typeInfo.IsInterface)
      {
        throw new ArgumentException("The supplied type must be an interface!", nameof(type));
      }

      var baseType = typeof(VersionedObject);
      var baseTypeInfo = baseType.GetTypeInfo();

      var typeName = _namespaceName + "." + (type.Name.StartsWith("I") ? type.Name.Substring(1) : type.Name);
      var typeBuilder = _moduleBuilder.DefineType(typeName, _typeAttributes, baseType);
      typeBuilder.AddInterfaceImplementation(type);
      _types[type.FullName] = typeBuilder.AsType();

      foreach (var constructor in baseTypeInfo.DeclaredConstructors)
      {
        DefineRelayConstructor(baseType, typeBuilder, constructor);
      }

      foreach (var propertyInfo in typeInfo.DeclaredProperties)
      {
        DefineProperty(type, typeBuilder, propertyInfo);
      }

      return _types[type.FullName] = typeBuilder.CreateTypeInfo().AsType();
    }

    private static void DefineRelayConstructor(Type baseType, TypeBuilder typeBuilder, ConstructorInfo baseConstructor)
    {
      var parameters = baseConstructor.GetParameters();
      var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();
      var constructorBuilder = typeBuilder.DefineConstructor(_constructorAttributes, CallingConventions.HasThis, parameterTypes);

      if (baseConstructor.GetCustomAttribute<JsonConstructorAttribute>() != null)
      {
        var jsonConstructor = _jsonAttributeConstructor;
        constructorBuilder.SetCustomAttribute(new CustomAttributeBuilder(jsonConstructor, new object[0]));
      }

      for (var index = 0; index < parameters.Length; index++)
      {
        constructorBuilder.DefineParameter(index + 1, parameters[index].Attributes, parameters[index].Name);
      }

      var ilGenerator = constructorBuilder.GetILGenerator();
      ilGenerator.Emit(OpCodes.Ldarg_0);
      for (var index = 0; index < parameters.Length; index++)
      {
        ilGenerator.Emit(OpCodes.Ldarg, (short)(index + 1));
      }
      ilGenerator.Emit(OpCodes.Call, baseConstructor);
      ilGenerator.Emit(OpCodes.Ret);
    }

    private static void DefineProperty(Type type, TypeBuilder typeBuilder, PropertyInfo propertyInfo)
    {
      var propertyName = propertyInfo.Name;
      var propertyType = propertyInfo.PropertyType;
      var isInterface = propertyType.GetTypeInfo().IsInterface;
      if (isInterface)
      {
        propertyType = Get(type);
      }

      var fieldBuilder = typeBuilder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

      var getMethodBuilder = typeBuilder.DefineMethod("get_" + propertyName, _propertyMethodAttributes, propertyType, Type.EmptyTypes);
      var getIlGenerator = getMethodBuilder.GetILGenerator();
      getIlGenerator.Emit(OpCodes.Ldarg_0);
      getIlGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
      getIlGenerator.Emit(OpCodes.Ret);

      var setMethodBuilder = typeBuilder.DefineMethod("set_" + propertyName, _propertyMethodAttributes, null, new[] { propertyType });
      var setIlGenerator = setMethodBuilder.GetILGenerator();
      setIlGenerator.DeclareLocal(typeof(bool));
      var endLabel = setIlGenerator.DefineLabel();
      setIlGenerator.Emit(OpCodes.Ldarg_0);
      setIlGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
      if (!propertyType.IsByRef) setIlGenerator.Emit(OpCodes.Box, propertyType);
      setIlGenerator.Emit(OpCodes.Ldarg_1);
      if (!propertyType.IsByRef) setIlGenerator.Emit(OpCodes.Box, propertyType);
      setIlGenerator.Emit(OpCodes.Call, _equalsMethodInfo);
      setIlGenerator.Emit(OpCodes.Brtrue_S, endLabel);
      setIlGenerator.Emit(OpCodes.Ldarg_0);
      setIlGenerator.Emit(OpCodes.Ldarg_1);
      setIlGenerator.Emit(OpCodes.Stfld, fieldBuilder);
      setIlGenerator.Emit(OpCodes.Ldarg_0);
      setIlGenerator.Emit(OpCodes.Ldstr, propertyName);
      setIlGenerator.Emit(OpCodes.Ldarg_1);
      if (!propertyType.IsByRef) setIlGenerator.Emit(OpCodes.Box, propertyType);
      setIlGenerator.Emit(OpCodes.Call, _recordSetPropertyActionMethodInfo);
      setIlGenerator.MarkLabel(endLabel);
      setIlGenerator.Emit(OpCodes.Ret);

      var propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.None, propertyType, Type.EmptyTypes);
      propertyBuilder.SetGetMethod(getMethodBuilder);
      propertyBuilder.SetSetMethod(setMethodBuilder);

      if (isInterface)
      {
        DefineInterfaceProperty(type, typeBuilder, propertyInfo, propertyBuilder);
      }
    }

    private static void DefineInterfaceProperty(Type type, TypeBuilder typeBuilder, PropertyInfo propertyInfo, PropertyBuilder propertyBuilder)
    {
      var getMethodBuilder = typeBuilder.DefineMethod(type.FullName + ".get_" + propertyInfo.Name, _interfacePropertyMethodAttributes, propertyInfo.PropertyType, Type.EmptyTypes);
      var getIlGenerator = getMethodBuilder.GetILGenerator();
      getIlGenerator.Emit(OpCodes.Ldarg_0);
      getIlGenerator.Emit(OpCodes.Call, propertyBuilder.GetMethod);
      getIlGenerator.Emit(OpCodes.Ret);
      typeBuilder.DefineMethodOverride(getMethodBuilder, propertyInfo.GetMethod);

      var setMethodBuilder = typeBuilder.DefineMethod(type.FullName + ".set_" + propertyInfo.Name, _interfacePropertyMethodAttributes, null, new[] { propertyInfo.PropertyType });
      var setIlGenerator = setMethodBuilder.GetILGenerator();
      setIlGenerator.DeclareLocal(typeof(bool));
      var endLabel = setIlGenerator.DefineLabel();
      setIlGenerator.Emit(OpCodes.Ldarg_0);
      setIlGenerator.Emit(OpCodes.Ldarg_1);
      setIlGenerator.Emit(OpCodes.Isinst, propertyBuilder.PropertyType);
      setIlGenerator.Emit(OpCodes.Call, propertyBuilder.SetMethod);
      setIlGenerator.Emit(OpCodes.Ret);
      typeBuilder.DefineMethodOverride(setMethodBuilder, propertyInfo.SetMethod);

      var interfacePropertyBuilder = typeBuilder.DefineProperty(type.FullName + "." + propertyInfo.Name, PropertyAttributes.None, propertyInfo.PropertyType, Type.EmptyTypes);
      interfacePropertyBuilder.SetGetMethod(getMethodBuilder);
      interfacePropertyBuilder.SetSetMethod(setMethodBuilder);
    }
  }
}
