// /*******************
// 文件:Seralizer.cs
// 作者:cocoa
// 时间:0:03
// 描述:通用序列化器
// *******************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace OpWorld.Core.Seralizer
{
    
    public static class Seralizer
    {
        [Serializable]
        [StructLayout(LayoutKind.Explicit, Size = 16)]
        public struct Value
        {
            [SerializeField] [FieldOffset(0)] public byte t; //1byte 类型

            [SerializeField] [FieldOffset(8)] public long v; // 紧挨着byte 这里的FieldOffset是位 即1byte = 8bit

            //7byte
            [NonSerialized] [FieldOffset(8)] private bool boolVal;
            [NonSerialized] [FieldOffset(8)] private char charVal;
            [NonSerialized] [FieldOffset(8)] private sbyte sbyteVal;
            [NonSerialized] [FieldOffset(8)] private byte byteVal;
            [NonSerialized] [FieldOffset(8)] private short shortVal;
            [NonSerialized] [FieldOffset(8)] private ushort ushortVal;
            [NonSerialized] [FieldOffset(8)] private int intVal;
            [NonSerialized] [FieldOffset(8)] private uint uintVal;
            [NonSerialized] [FieldOffset(8)] private long longVal;
            [NonSerialized] [FieldOffset(8)] private ulong ulongVal;
            [NonSerialized] [FieldOffset(8)] private float floatVal;
            [NonSerialized] [FieldOffset(8)] private double doubleVal;

            [NonSerialized] [FieldOffset(8)] private decimal decimalVal;

            //reference t is 255
            public static implicit operator Value(int i)
            {
                return new Value() { t = 255, intVal = i };
            }

            public static implicit operator int(Value v)
            {
                if (v.t != 255)
                    throw new Exception("Value t:" + v.t + " v:" + v.v + " is used like a reference");
                return v.intVal;
            }

            public void Set(object obj, Type type)
            {
                if (type == typeof(int))
                {
                    intVal = (int)obj;
                    t = 1;
                }
                else if (type == typeof(float))
                {
                    floatVal = (float)obj;
                    t = 2;
                }
                else if (type == typeof(bool))
                {
                    boolVal = (bool)obj;
                    t = 3;
                }
                else if (type == typeof(byte))
                {
                    byteVal = (byte)obj;
                    t = 4;
                }
                else if (type == typeof(char))
                {
                    charVal = (char)obj;
                    t = 5;
                }
                else if (type == typeof(uint))
                {
                    uintVal = (uint)obj;
                    t = 6;
                }
                else if (type == typeof(sbyte))
                {
                    sbyteVal = (sbyte)obj;
                    t = 7;
                }
                else if (type == typeof(decimal))
                {
                    decimalVal = (decimal)obj;
                    t = 8;
                }
                else if (type == typeof(double))
                {
                    doubleVal = (double)obj;
                    t = 9;
                }
                else if (type == typeof(long))
                {
                    longVal = (long)obj;
                    t = 10;
                }
                else if (type == typeof(ulong))
                {
                    ulongVal = (ulong)obj;
                    t = 11;
                }
                else if (type == typeof(short))
                {
                    shortVal = (short)obj;
                    t = 12;
                }
                else if (type == typeof(ushort))
                {
                    ushortVal = (ushort)obj;
                    t = 13;
                }

                else throw new Exception("Could not set value for " + type);
            }

            public object Get(Type type)
            {
                if (type == typeof(bool)) return boolVal;
                else if (type == typeof(byte)) return byteVal;
                else if (type == typeof(sbyte)) return sbyteVal;
                else if (type == typeof(char)) return charVal;
                else if (type == typeof(decimal)) return decimalVal;
                else if (type == typeof(double)) return doubleVal;
                else if (type == typeof(float)) return floatVal;
                else if (type == typeof(int)) return intVal;
                else if (type == typeof(uint)) return uintVal;
                else if (type == typeof(long)) return longVal;
                else if (type == typeof(ulong)) return ulongVal;
                else if (type == typeof(short)) return shortVal;
                else if (type == typeof(ushort)) return ushortVal;
                else throw new Exception("Could not get value for " + type);
            }

            public object Get()
            {
                switch (t)
                {
                    case 1: return intVal;
                    case 2: return floatVal;
                    case 3: return boolVal;
                    case 4: return byteVal;
                    case 5: return charVal;
                    case 6: return uintVal;
                    case 7: return sbyteVal;
                    case 8: return decimalVal;
                    case 9: return doubleVal;
                    case 10: return longVal;
                    case 11: return ulongVal;
                    case 12: return shortVal;
                    case 13: return ushortVal;
                    default: throw new Exception("Could not get value for " + t);
                }
            }

            public bool CheckType(Type type)
            {
                if (t == 0) return false;

                switch (t)
                {
                    case 1: return type == typeof(int);
                    case 2: return type == typeof(float);
                    case 3: return type == typeof(bool);
                    case 4: return type == typeof(byte);
                    case 5: return type == typeof(char);
                    case 6: return type == typeof(uint);
                    case 7: return type == typeof(sbyte);
                    case 8: return type == typeof(decimal);
                    case 9: return type == typeof(double);
                    case 10: return type == typeof(long);
                    case 11: return type == typeof(ulong);
                    case 12: return type == typeof(short);
                    case 13: return type == typeof(ushort);
                    case 255: return !type.IsPrimitive;
                    default: throw new Exception("Could not get value for " + t);
                }
            }
        }

        //存放序列化完后的数据
        [Serializable]
        public class SerializedObject
        {
            public int refId = -1;
            public string type = null;
            public string altType = null;
            public string[] fields = null;
            public Value[] values = null;
            public string special = null; // strings, Types, FieldInfos
            public UnityEngine.Object uniObj = null; // Unity Objects
            public static SerializedObject Null => new SerializedObject();
        }

        public static SerializedObject[] Seralize(object obj)
        {
            Dictionary<object, SerializedObject> serializedDict = new();
            SeralizeObject(obj, serializedDict);

            SerializedObject[] serializedObjects = null;
            CopyObjectsToArray(serializedDict, ref serializedObjects);
            return serializedObjects;
        }

        private static SerializedObject SeralizeObject(object obj, Dictionary<object, SerializedObject> serializedDict
            , bool skipNoCopyAttribute = false, Action<object, SerializedObject> onAfterSerialize = null)
        {
            if (obj == null)
                return SerializedObject.Null;

            if (serializedDict.TryGetValue(obj, out SerializedObject serObj))
            {
                return serObj;
            }

            serObj = new SerializedObject();
            serObj.refId = serializedDict.Count;
            serializedDict.Add(obj, serObj);

            Type type = obj.GetType();
            serObj.type = type.AssemblyQualifiedName;
            if (type == typeof(string))
                serObj.special = (string)obj;

            //guid
            else if (type == typeof(Guid))
                serObj.special = ((Guid)obj).ToString();
            //unity object
            //else if (type.IsSubclassOf(typeof(UnityEngine.Object))  &&  !type.IsSubclassOf(typeof(UnityEngine.ScriptableObject)))
            else if (type.IsSubclassOf(typeof(UnityEngine.Object)))
                serObj.uniObj = (UnityEngine.Object)obj;
            else if (type == typeof(SerializedObject[]))
                throw new Exception(
                    "Serializer is trying to serialize serializer objects. This is causing infinite loop.");
            //reflections
            else if (type.IsSubclassOf(typeof(MemberInfo)))
            {
                if (type.IsSubclassOf(typeof(Type))) //could be MonoType
                    serObj.special = ((Type)obj).AssemblyQualifiedName;
                else
                {
                    MemberInfo mi = (MemberInfo)obj;
                    //字段,类型
                    serObj.special = mi.Name + ", " + mi.DeclaringType.AssemblyQualifiedName;
                }
            }
            else if (type.IsPrimitive)
            {
                serObj.values = new Value[1];
                serObj.values[0].Set(obj, type);
            } //animation curve
            else if (type == typeof(AnimationCurve))
            {
                AnimationCurve curve = (AnimationCurve)obj;

                serObj.values = new Value[3];
                serObj.values[0] = SeralizeObject(curve.keys, serializedDict, onAfterSerialize: onAfterSerialize).refId;
                serObj.values[1].Set((int)curve.preWrapMode, typeof(int));
                serObj.values[2].Set((int)curve.postWrapMode, typeof(int));
            }
            else if (type.IsArray)
            {
                Array array = (Array)obj;

                serObj.fields = null;
                serObj.values = new Value[array.Length];

                Type elementType = type.GetElementType();
                bool elementTypeIsPrimitive = elementType.IsPrimitive;

                for (int i = 0; i < array.Length; i++)
                {
                    object val = array.GetValue(i);

                    Value serVal = new Value();
                    if (elementTypeIsPrimitive) serVal.Set(val, elementType);
                    else serVal = SeralizeObject(val, serializedDict, onAfterSerialize: onAfterSerialize).refId;

                    serObj.values[i] = serVal;
                }
            } // Unity Object null check
            else if (type == typeof(UnityEngine.Object))
            {
                FieldInfo ptrField = type.GetField("m_CachedPtr",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                IntPtr ptr = (IntPtr)ptrField.GetValue(obj);
                if (ptr == IntPtr.Zero)
                    return SerializedObject.Null;
            }
            else //class struct
            {
                if (obj is ISerializationCallbackReceiver)
                    ((ISerializationCallbackReceiver)obj).OnBeforeSerialize();

                FieldInfo[] fields =
                    type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (obj is IList || obj is IDictionary)
                    ArrayTools.Append(ref fields,
                        type.BaseType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));


                List<FieldInfo> usedfieldInfos = new List<FieldInfo>();
                for (int f = 0; f < fields.Length; f++)
                {
                    if (fields[f].IsLiteral) continue; //常量跳过
                    if (fields[f].IsNotSerialized) continue;
                    if (fields[f].FieldType.IsPointer) continue;

                    usedfieldInfos.Add(fields[f]);
                }

                int usedFieldCount = usedfieldInfos.Count;
                serObj.fields = new string[usedFieldCount];
                serObj.values = new Value[usedFieldCount];

                for (int i = 0; i < usedFieldCount; i++)
                {
                    FieldInfo field = usedfieldInfos[i];
                    serObj.fields[i] = field.Name;
                    object val = field.GetValue(obj);
                    Value serval = new Value();
                    if (field.FieldType.IsPrimitive)
                    {
                        serval.Set(val, field.FieldType);
                    }
                    else
                    {
                        //class struct
                        serval = SeralizeObject(val, serializedDict).refId;
                    }

                    serObj.values[i] = serval;
                }
            }

            onAfterSerialize?.Invoke(obj, serObj);

            return serObj;
        }

        private static void CopyObjectsToArray(Dictionary<object, SerializedObject> serializedDict,
            ref SerializedObject[] serialized, Action<SerializedObject> onBeforeDeserialize = null)
        {
            if (serialized == null || serialized.Length != serializedDict.Count)
                serialized = new SerializedObject[serializedDict.Count];
            else
            {
                for (int i = 0; i < serialized.Length; i++)
                    serialized[i] = null;
            }

            foreach (var kvp in serializedDict)
            {
                SerializedObject serObj = kvp.Value;
                if (serialized[serObj.refId] != null)
                    throw new Exception("Objects with the same id: " + serObj.refId);
                serialized[serObj.refId] = serObj;
            }
        }


        //反向序列化可以减少调用堆栈 但是要进行一些特殊处理 对非值类型都需要直接CreateInstance
        public static object Deserialize(SerializedObject[] serialized,
            Action<SerializedObject> onBeforeDeserialize = null)
        {
            int length = serialized.Length;
            if (length == 0) return null;
            object[] deserialized = new object[serialized.Length];
            DeserializeObject(0, serialized, deserialized, onBeforeDeserialize: onBeforeDeserialize);
            return deserialized[0];
        }

        private static object DeserializeObject(int refID, SerializedObject[] serializedObjects,
            object[] deserialized = null,
            object[] reuse = null,
            Action<SerializedObject> onBeforeDeserialize = null)
        {
            if (refID < 0)
                return null;
            if (deserialized[refID] != null)
            {
                return deserialized[refID];
            }

            SerializedObject serObj = serializedObjects[refID];
            onBeforeDeserialize?.Invoke(serObj);


            if (serObj.type == null || serObj.refId < 0) //refid could be changed on onBeforeDeserialize
                return null;

            Type type = Type.GetType(serObj.type);

            if (type == null && serObj.altType != null)
                type = Type.GetType(serObj.altType);

            if (type == null && serObj.type.Contains(","))
            {
                string shortName = serObj.type.Substring(0, serObj.type.IndexOf(','));
                type = Type.GetType(shortName);
                if (type == null)
                {
                    foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        type = ass.GetType(shortName);
                        if (type != null)
                            break;
                    }
                }
            }

            //string    
            if (type == typeof(string))
                return serObj.special;
            //guid
            if (type == typeof(Guid) && serObj.special.Length != 0)
                return Guid.Parse(serObj.special);
            //not exist anymore
            if (type == null)
            {
                throw new Exception("Could not find type for: " + serObj.type);
            } //unity object
            //else if (type.IsSubclassOf(typeof(UnityEngine.Object))  &&  !type.IsSubclassOf(typeof(UnityEngine.ScriptableObject)))
            else if (type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                if (serObj.uniObj.GetType() == typeof(UnityEngine.Object))
                    return null;
                return serObj.uniObj;
            } //reflections
            else if (type.IsSubclassOf(typeof(MemberInfo)))
            {
                if (type.IsSubclassOf(typeof(Type))) //could be MonoType
                    return Type.GetType(serObj.special);
                else
                {
                    int d = serObj.special.IndexOf(", ");
                    string mn = serObj.special.Substring(0, d);
                    string tn = serObj.special.Substring(d + 2);

                    Type mt = Type.GetType(tn);
                    return mt.GetField(mn, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                }
            }
            else if (type.IsPrimitive)
                return serObj.values[0].Get();
            //animation curve
            else if (type == typeof(AnimationCurve))
            {
                AnimationCurve curve;
                if (reuse == null || reuse[refID] == null) curve = new AnimationCurve();
                else curve = (AnimationCurve)reuse[refID];

                deserialized[refID] = curve;

                curve.keys = (Keyframe[])DeserializeObject(serObj.values[0], serializedObjects, deserialized, reuse,
                    onBeforeDeserialize);
                curve.preWrapMode = (WrapMode)serObj.values[1].Get();
                curve.postWrapMode = (WrapMode)serObj.values[2].Get();

                return curve;
            } //array
            else if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                bool elementTypeIsPrimitive = elementType.IsPrimitive;

                if (serObj.values.Length > 0 && !serObj.values[0].CheckType(elementType))
                    throw new Exception("Value was saved as a reference, but loading as a primitive (or vice versa)");

                Array array;
                if (reuse == null || reuse[refID] == null)
                    array = (Array)Activator.CreateInstance(type, serObj.values.Length);
                else array = (Array)reuse[refID];

                deserialized[refID] = array;

                for (int i = 0; i < array.Length; i++)
                {
                    Value serVal = serObj.values[i];

                    if (elementTypeIsPrimitive)
                        array.SetValue(serVal.Get(), i);
                    else if (serVal >= 0)
                    {
                        object val = DeserializeObject(serVal, serializedObjects, deserialized, reuse,
                            onBeforeDeserialize);
                        array.SetValue(val, i);
                    }
                }

                return array;
            }
            else //class struct
            {
                object obj;
                if (reuse == null || reuse[refID] == null)
                    obj = Activator.CreateInstance(type); //字典或列表
                else obj = reuse[refID];

                deserialized[refID] = obj;

                if (serObj.values == null)
                    return null;

                for (int v = 0; v < serObj.values.Length; v++)
                {
                    FieldInfo field = type.GetField(serObj.fields[v],
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field == null && (obj is IList || obj is IDictionary))
                        field = type.BaseType.GetField(serObj.fields[v],
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field == null) continue;
                    if (field.IsNotSerialized) continue;

                    Value serVal = serObj.values[v];
                    if (field.FieldType.IsPrimitive)
                    {
                        if (!serVal.CheckType(field.FieldType)) continue;
                        field.SetValue(obj, serVal.Get());
                    }
                    else if (serVal >= 0)
                    {
                        object val = DeserializeObject(serVal, serializedObjects, deserialized, reuse,
                            onBeforeDeserialize);
                        if (val != null && val.GetType() != field.FieldType
                                        && !field.FieldType.IsAssignableFrom(val.GetType()))
                        {
                            Debug.LogWarning(
                                $"Serializer: Could not convert {val.GetType()} to {field.FieldType}. Using default value");
                            continue;
                        }

                        field.SetValue(obj, val);
                    }
                }

                if (obj is ISerializationCallbackReceiver)
                    ((ISerializationCallbackReceiver)obj).OnAfterDeserialize();


                return obj;
            }
        }

        public static object DeepCopy(object obj)
        {
            Dictionary<object, SerializedObject> serializedDict = new Dictionary<object, SerializedObject>();
            SeralizeObject(obj, serializedDict);

            SerializedObject[] serialized = null;
            CopyObjectsToArray(serializedDict, ref serialized);

            object[] deserialized = new object[serialized.Length];
            return DeserializeObject(0, serialized, deserialized);
        }

        public enum ClassMatch
        {
            None,
            ShouldMatch,
            ShouldDiffer
        }

        public static bool CheckMatch(object src, object dst, HashSet<object> chkd,
            ClassMatch checkIfSameReference = ClassMatch.None)
        {

            if (src == null && dst == null) return true;
            if (src == null && dst != null) throw new Exception("Src is null while dst isn't"); //return false;
            if (src != null && dst == null) throw new Exception("Dst is null while src isn't"); //return false;
            if (chkd == null) chkd = new HashSet<object>();

            Type type = src.GetType();

            if (type.IsPrimitive)
            {
                if (!src.Equals(dst)) throw new Exception("Primitives not equal");
            }
            else if (type == typeof(string))
            {
                if (src != dst) throw new Exception("String not equal");
            }
            else if (type.IsSubclassOf(typeof(Type)))
            {
                if (src != dst) throw new Exception("Types not equal");
            }
            else if (type.IsSubclassOf(typeof(FieldInfo)))
            {
                if (src != dst) throw new Exception("Field Infos not equal");
            }
            else if (type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                if (src != dst) throw new Exception("Unity objects not equal");
            }
            else if (type == typeof(AnimationCurve))
            {
                if (checkIfSameReference == ClassMatch.ShouldDiffer && src == dst)
                    throw new Exception("Value references match");

                if (checkIfSameReference == ClassMatch.ShouldMatch && src != dst)
                    throw new Exception("Value references differ");

                AnimationCurve srcCurve = (AnimationCurve)src;
                AnimationCurve dstCurve = (AnimationCurve)dst;
                if (srcCurve.keys.Length != dstCurve.keys.Length)
                    throw new Exception("Anim Curve length differ"); //return false;
                for (int i = 0; i < srcCurve.keys.Length; i++)
                    if (srcCurve.keys[i].time != dstCurve.keys[i].time ||
                        srcCurve.keys[i].value != dstCurve.keys[i].value)
                        return false;

                return true;
            }
            else if (type.IsArray || type.BaseType.IsArray)
            {
                if (checkIfSameReference == ClassMatch.ShouldDiffer && src == dst)
                    throw new Exception("Value references match");

                if (checkIfSameReference == ClassMatch.ShouldMatch && src != dst)
                    throw new Exception("Value references differ");

                Array srcArray = (Array)src;
                Array dstArray = (Array)dst;

                if (chkd.Contains(srcArray)) return true;
                chkd.Add(srcArray);

                if (srcArray.Length != dstArray.Length) return false;

                for (int i = 0; i < dstArray.Length; i++)
                    if (!CheckMatch(srcArray.GetValue(i), dstArray.GetValue(i), chkd, checkIfSameReference))
                        return false;

                return true;
            }
            else
            {
                if (checkIfSameReference == ClassMatch.ShouldDiffer && src == dst)
                    throw new Exception("Value references match");

                if (checkIfSameReference == ClassMatch.ShouldMatch && !src.Equals(dst))
                    throw new Exception("Value references differ");

                if (chkd.Contains(src)) return true;
                chkd.Add(src);

                FieldInfo[] fields =
                    type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                for (int i = 0; i < fields.Length; i++)
                {
                    if (fields[i].IsLiteral) continue; //leaving constant fields blank
                    if (fields[i].FieldType.IsPointer)
                        continue; //skipping pointers (they make unity crash. Maybe require unsafe)
                    if (fields[i].IsNotSerialized) continue;

                    object srcVal = fields[i].GetValue(src);
                    object dstVal = fields[i].GetValue(dst);

                    bool match = CheckMatch(srcVal, dstVal, chkd, checkIfSameReference);
                    if (!match) return false;
                }

                return true;
            }

            return true;
        }
    }
}