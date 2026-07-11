using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SharpDX.IO;
using SharpDX.Multimedia;

namespace SharpDX.Serialization;

public class BinarySerializer : Component
{
	public delegate void SerializerPrimitiveAction<T>(ref T value);

	public delegate void SerializerTypeAction<T>(ref T value, BinarySerializer serializer);

	public delegate object ReadRef(BinarySerializer serializer);

	public delegate void WriteRef(object value, BinarySerializer serializer);

	private class Chunk
	{
		public FourCC Id;

		public long ChunkIndexStart;

		public long ChunkIndexEnd;
	}

	private class Dynamic
	{
		public FourCC Id;

		public Type Type;

		public ReadRef Reader;

		public WriteRef Writer;

		public SerializerAction DynamicSerializer;

		public object DynamicReader<T>(BinarySerializer serializer) where T : new()
		{
			object value = new T();
			DynamicSerializer(ref value, serializer);
			return value;
		}

		public void DynamicWriter(object value, BinarySerializer serializer)
		{
			DynamicSerializer(ref value, serializer);
		}
	}

	private const int LargeByteBufferSize = 1024;

	private int chunkCount;

	private Chunk[] chunks;

	private Chunk currentChunk;

	private readonly Dictionary<FourCC, Dynamic> dynamicMapToType;

	private readonly Dictionary<Type, Dynamic> dynamicMapToFourCC;

	private readonly Dictionary<object, int> objectToPosition;

	private readonly Dictionary<int, object> positionToObject;

	private Dictionary<object, object> mapTag;

	private int allowIdentityReferenceCount;

	private Encoding encoding;

	private Encoder encoder;

	private Decoder decoder;

	private byte[] largeByteBuffer;

	private int maxChars;

	private char[] largeCharBuffer;

	private int maxCharSize;

	private SerializerMode mode;

	private static readonly List<Dynamic> DefaultDynamics = new List<Dynamic>
	{
		new Dynamic
		{
			Id = 0,
			Type = typeof(int),
			Reader = ReaderInt,
			Writer = WriterInt
		},
		new Dynamic
		{
			Id = 1,
			Type = typeof(uint),
			Reader = ReaderUInt,
			Writer = WriterUInt
		},
		new Dynamic
		{
			Id = 2,
			Type = typeof(short),
			Reader = ReaderShort,
			Writer = WriterShort
		},
		new Dynamic
		{
			Id = 3,
			Type = typeof(ushort),
			Reader = ReaderUShort,
			Writer = WriterUShort
		},
		new Dynamic
		{
			Id = 4,
			Type = typeof(long),
			Reader = ReaderLong,
			Writer = WriterLong
		},
		new Dynamic
		{
			Id = 5,
			Type = typeof(ulong),
			Reader = ReaderULong,
			Writer = WriterULong
		},
		new Dynamic
		{
			Id = 6,
			Type = typeof(byte),
			Reader = ReaderByte,
			Writer = WriterByte
		},
		new Dynamic
		{
			Id = 7,
			Type = typeof(sbyte),
			Reader = ReaderSByte,
			Writer = WriterSByte
		},
		new Dynamic
		{
			Id = 8,
			Type = typeof(bool),
			Reader = ReaderBool,
			Writer = WriterBool
		},
		new Dynamic
		{
			Id = 9,
			Type = typeof(float),
			Reader = ReaderFloat,
			Writer = WriterFloat
		},
		new Dynamic
		{
			Id = 10,
			Type = typeof(double),
			Reader = ReaderDouble,
			Writer = WriterDouble
		},
		new Dynamic
		{
			Id = 11,
			Type = typeof(string),
			Reader = ReaderString,
			Writer = WriterString
		},
		new Dynamic
		{
			Id = 12,
			Type = typeof(char),
			Reader = ReaderChar,
			Writer = WriterChar
		},
		new Dynamic
		{
			Id = 13,
			Type = typeof(DateTime),
			Reader = ReaderDateTime,
			Writer = WriterDateTime
		},
		new Dynamic
		{
			Id = 14,
			Type = typeof(Guid),
			Reader = ReaderGuid,
			Writer = WriterGuid
		},
		new Dynamic
		{
			Id = 30,
			Type = typeof(int[]),
			Reader = ReaderIntArray,
			Writer = WriterIntArray
		},
		new Dynamic
		{
			Id = 31,
			Type = typeof(uint[]),
			Reader = ReaderUIntArray,
			Writer = WriterUIntArray
		},
		new Dynamic
		{
			Id = 32,
			Type = typeof(short[]),
			Reader = ReaderShortArray,
			Writer = WriterShortArray
		},
		new Dynamic
		{
			Id = 33,
			Type = typeof(ushort[]),
			Reader = ReaderUShortArray,
			Writer = WriterUShortArray
		},
		new Dynamic
		{
			Id = 34,
			Type = typeof(long[]),
			Reader = ReaderLongArray,
			Writer = WriterLongArray
		},
		new Dynamic
		{
			Id = 35,
			Type = typeof(ulong[]),
			Reader = ReaderULongArray,
			Writer = WriterULongArray
		},
		new Dynamic
		{
			Id = 36,
			Type = typeof(byte[]),
			Reader = ReaderByteArray,
			Writer = WriterByteArray
		},
		new Dynamic
		{
			Id = 37,
			Type = typeof(sbyte[]),
			Reader = ReaderSByteArray,
			Writer = WriterSByteArray
		},
		new Dynamic
		{
			Id = 38,
			Type = typeof(bool[]),
			Reader = ReaderBoolArray,
			Writer = WriterBoolArray
		},
		new Dynamic
		{
			Id = 39,
			Type = typeof(float[]),
			Reader = ReaderFloatArray,
			Writer = WriterFloatArray
		},
		new Dynamic
		{
			Id = 40,
			Type = typeof(double[]),
			Reader = ReaderDoubleArray,
			Writer = WriterDoubleArray
		},
		new Dynamic
		{
			Id = 41,
			Type = typeof(string[]),
			Reader = ReaderStringArray,
			Writer = WriterStringArray
		},
		new Dynamic
		{
			Id = 42,
			Type = typeof(char[]),
			Reader = ReaderCharArray,
			Writer = WriterCharArray
		},
		new Dynamic
		{
			Id = 43,
			Type = typeof(DateTime[]),
			Reader = ReaderDateTimeArray,
			Writer = WriterDateTimeArray
		},
		new Dynamic
		{
			Id = 44,
			Type = typeof(Guid[]),
			Reader = ReaderGuidArray,
			Writer = WriterGuidArray
		},
		new Dynamic
		{
			Id = 45,
			Type = typeof(object[]),
			Reader = ReaderObjectArray,
			Writer = WriterObjectArray
		},
		new Dynamic
		{
			Id = 60,
			Type = typeof(List<int>),
			Reader = ReaderIntList,
			Writer = WriterIntList
		},
		new Dynamic
		{
			Id = 61,
			Type = typeof(List<uint>),
			Reader = ReaderUIntList,
			Writer = WriterUIntList
		},
		new Dynamic
		{
			Id = 62,
			Type = typeof(List<short>),
			Reader = ReaderShortList,
			Writer = WriterShortList
		},
		new Dynamic
		{
			Id = 63,
			Type = typeof(List<ushort>),
			Reader = ReaderUShortList,
			Writer = WriterUShortList
		},
		new Dynamic
		{
			Id = 64,
			Type = typeof(List<long>),
			Reader = ReaderLongList,
			Writer = WriterLongList
		},
		new Dynamic
		{
			Id = 65,
			Type = typeof(List<ulong>),
			Reader = ReaderULongList,
			Writer = WriterULongList
		},
		new Dynamic
		{
			Id = 66,
			Type = typeof(List<byte>),
			Reader = ReaderByteList,
			Writer = WriterByteList
		},
		new Dynamic
		{
			Id = 67,
			Type = typeof(List<sbyte>),
			Reader = ReaderSByteList,
			Writer = WriterSByteList
		},
		new Dynamic
		{
			Id = 68,
			Type = typeof(List<bool>),
			Reader = ReaderBoolList,
			Writer = WriterBoolList
		},
		new Dynamic
		{
			Id = 69,
			Type = typeof(List<float>),
			Reader = ReaderFloatList,
			Writer = WriterFloatList
		},
		new Dynamic
		{
			Id = 70,
			Type = typeof(List<double>),
			Reader = ReaderDoubleList,
			Writer = WriterDoubleList
		},
		new Dynamic
		{
			Id = 71,
			Type = typeof(List<string>),
			Reader = ReaderStringList,
			Writer = WriterStringList
		},
		new Dynamic
		{
			Id = 72,
			Type = typeof(List<char>),
			Reader = ReaderCharList,
			Writer = WriterCharList
		},
		new Dynamic
		{
			Id = 73,
			Type = typeof(List<DateTime>),
			Reader = ReaderDateTimeList,
			Writer = WriterDateTimeList
		},
		new Dynamic
		{
			Id = 74,
			Type = typeof(List<Guid>),
			Reader = ReaderGuidList,
			Writer = WriterGuidList
		},
		new Dynamic
		{
			Id = 75,
			Type = typeof(List<object>),
			Reader = ReaderObjectList,
			Writer = WriterObjectList
		}
	};

	public Stream Stream { get; private set; }

	public BinaryReader Reader { get; private set; }

	public BinaryWriter Writer { get; private set; }

	public ArrayLengthType ArrayLengthType { get; set; }

	public SerializerMode Mode
	{
		get
		{
			return mode;
		}
		set
		{
			mode = value;
			if (Mode == SerializerMode.Read)
			{
				Reader = Reader ?? new BinaryReader(Stream);
			}
			else
			{
				Writer = Writer ?? new BinaryWriter(Stream);
			}
		}
	}

	public Encoding Encoding
	{
		get
		{
			return encoding;
		}
		set
		{
			if (object.ReferenceEquals(value, null))
			{
				throw new ArgumentNullException("value");
			}
			if (!object.ReferenceEquals(encoding, value))
			{
				encoding = value;
				encoder = encoding.GetEncoder();
				decoder = encoding.GetDecoder();
				maxCharSize = encoding.GetMaxCharCount(1024);
			}
		}
	}

	public bool AllowIdentity
	{
		get
		{
			return allowIdentityReferenceCount > 0;
		}
		set
		{
			allowIdentityReferenceCount += (value ? 1 : (-1));
			if (allowIdentityReferenceCount < 0)
			{
				throw new InvalidOperationException("Invalid call to AllowIdentity. Must match true/false in pair.");
			}
		}
	}

	private Chunk CurrentChunk
	{
		get
		{
			return currentChunk;
		}
		set
		{
			currentChunk = value;
		}
	}

	public BinaryReader ReaderOnly(object context)
	{
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		if (Mode != SerializerMode.Read)
		{
			throw new InvalidOperationException($"[{context.GetType().Name}] is only expecting Read-Only BinarySerializer");
		}
		return Reader;
	}

	public BinarySerializer(Stream stream, SerializerMode mode)
		: this(stream, mode, Encoding.ASCII)
	{
	}

	public BinarySerializer(Stream stream, SerializerMode mode, Encoding encoding)
	{
		Encoding = encoding;
		dynamicMapToType = new Dictionary<FourCC, Dynamic>();
		dynamicMapToFourCC = new Dictionary<Type, Dynamic>();
		objectToPosition = new Dictionary<object, int>(new IdentityEqualityComparer<object>());
		positionToObject = new Dictionary<int, object>();
		chunks = new Chunk[8];
		Stream = stream;
		Mode = mode;
		CurrentChunk = new Chunk
		{
			ChunkIndexStart = 0L
		};
		chunks[chunkCount] = CurrentChunk;
		chunkCount++;
		foreach (Dynamic defaultDynamic in DefaultDynamics)
		{
			RegisterDynamic(defaultDynamic);
		}
		RegisterDynamic<Color4>();
		RegisterDynamic<Color3>();
		RegisterDynamic<Color>();
		RegisterDynamic<Vector4>();
		RegisterDynamic<Vector3>();
		RegisterDynamic<Vector2>();
	}

	public object GetTag(object key)
	{
		if (mapTag == null)
		{
			return null;
		}
		mapTag.TryGetValue(key, out var value);
		return value;
	}

	public bool HasTag(object key)
	{
		if (mapTag == null)
		{
			return false;
		}
		return mapTag.ContainsKey(key);
	}

	public void RemoveTag(object key)
	{
		if (mapTag != null)
		{
			mapTag.Remove(key);
		}
	}

	public void SetTag(object key, object value)
	{
		if (mapTag == null)
		{
			mapTag = new Dictionary<object, object>();
		}
		mapTag.Remove(key);
		mapTag.Add(key, value);
	}

	public void RegisterDynamic<T>() where T : IDataSerializable, new()
	{
		DynamicSerializerAttribute customAttribute = Utilities.GetCustomAttribute<DynamicSerializerAttribute>(typeof(T));
		if (customAttribute == null)
		{
			throw new ArgumentException("Type T doesn't have DynamicSerializerAttribute", "T");
		}
		RegisterDynamic<T>(customAttribute.Id);
	}

	public void RegisterDynamic<T>(FourCC id) where T : IDataSerializable, new()
	{
		RegisterDynamic(GetDynamic<T>(id));
	}

	public void RegisterDynamicArray<T>(FourCC id) where T : IDataSerializable, new()
	{
		RegisterDynamic(GetDynamicArray<T>(id));
	}

	public void RegisterDynamicList<T>(FourCC id) where T : IDataSerializable, new()
	{
		RegisterDynamic(GetDynamicList<T>(id));
	}

	public void RegisterDynamic<T>(FourCC id, SerializerAction serializer) where T : new()
	{
		Dynamic dynamic = new Dynamic();
		dynamic.Id = id;
		dynamic.Type = typeof(T);
		dynamic.DynamicSerializer = serializer;
		Dynamic dynamic2 = dynamic;
		dynamic2.Reader = dynamic2.DynamicReader<T>;
		dynamic2.Writer = dynamic2.DynamicWriter;
		RegisterDynamic(dynamic2);
	}

	public void BeginChunk(FourCC chunkId)
	{
		if (chunks[chunkCount] == null)
		{
			CurrentChunk = new Chunk();
			chunks[chunkCount] = CurrentChunk;
		}
		else
		{
			CurrentChunk = chunks[chunkCount];
		}
		chunkCount++;
		CurrentChunk.Id = chunkId;
		if (Mode == SerializerMode.Write)
		{
			CurrentChunk.ChunkIndexStart = Stream.Position;
		}
		if (chunkCount >= chunks.Length)
		{
			Chunk[] destinationArray = new Chunk[chunks.Length * 2];
			Array.Copy(chunks, destinationArray, chunks.Length);
			chunks = destinationArray;
		}
		if (Mode == SerializerMode.Write)
		{
			Writer.Write((int)chunkId);
			Writer.Write(0);
			return;
		}
		int num = Reader.ReadInt32();
		if (num != chunkId)
		{
			throw new InvalidChunkException(num, chunkId);
		}
		uint num2 = Reader.ReadUInt32();
		CurrentChunk.ChunkIndexEnd = Stream.Position + num2;
	}

	public void EndChunk()
	{
		if (chunkCount <= 1)
		{
			throw new InvalidOperationException("EndChunk() called without BeginChunk()");
		}
		Chunk chunk = CurrentChunk;
		chunkCount--;
		CurrentChunk = chunks[chunkCount - 1];
		if (Mode == SerializerMode.Write)
		{
			long position = Stream.Position;
			Stream.Position = chunk.ChunkIndexStart + 4;
			Writer.Write((uint)(position - Stream.Position - 4));
			Stream.Position = position;
		}
		else if (chunk.ChunkIndexEnd != Stream.Position)
		{
			throw new IOException($"Unexpected size when reading chunk [{CurrentChunk.Id}]");
		}
	}

	public T Load<T>() where T : IDataSerializable, new()
	{
		ResetStoredReference();
		Mode = SerializerMode.Read;
		T value = default(T);
		Serialize(ref value);
		return value;
	}

	public void Save<T>(T value) where T : IDataSerializable, new()
	{
		ResetStoredReference();
		Mode = SerializerMode.Write;
		Serialize(ref value);
		Flush();
	}

	public void Flush()
	{
		Writer.Flush();
	}

	public void SerializeDynamic<T>(ref T value)
	{
		SerializeDynamic(ref value, SerializeFlags.Dynamic | SerializeFlags.Nullable);
	}

	public void SerializeDynamic<T>(ref T value, SerializeFlags serializeFlags)
	{
		if (!SerializeIsNull(ref value, out var _, serializeFlags | SerializeFlags.Dynamic))
		{
			SerializeRawDynamic(ref value);
		}
	}

	public void Serialize<T>(ref T value, SerializeFlags serializeFlags = SerializeFlags.Normal) where T : IDataSerializable, new()
	{
		if (!SerializeIsNull(ref value, out var storeObjectReference, serializeFlags))
		{
			if (Mode == SerializerMode.Read)
			{
				value = new T();
			}
			if (storeObjectReference >= 0)
			{
				StoreObjectRef(value, storeObjectReference);
			}
			value.Serialize(this);
		}
	}

	public void SerializeWithNoInstance<T>(ref T value, SerializeFlags serializeFlags = SerializeFlags.Normal) where T : IDataSerializable
	{
		if (!SerializeIsNull(ref value, out var storeObjectReference, serializeFlags))
		{
			if (storeObjectReference >= 0)
			{
				StoreObjectRef(value, storeObjectReference);
			}
			value.Serialize(this);
		}
	}

	public unsafe void SerializeEnum<T>(ref T value) where T : struct, IComparable, IFormattable
	{
		if (!Utilities.IsEnum(typeof(T)))
		{
			throw new ArgumentException("T generic parameter must be a valid enum", "value");
		}
		fixed (T* ptr = &value)
		{
			void* ptr2 = ptr;
			switch (Utilities.SizeOf<T>())
			{
			case 1:
				Serialize(ref *(byte*)ptr2);
				break;
			case 2:
				Serialize(ref *(short*)ptr2);
				break;
			case 4:
				Serialize(ref *(int*)ptr2);
				break;
			case 8:
				Serialize(ref *(long*)ptr2);
				break;
			}
		}
	}

	public void Serialize<T>(ref T[] valueArray, SerializerPrimitiveAction<T> serializer, SerializeFlags serializeFlags = SerializeFlags.Normal)
	{
		if (SerializeIsNull(ref valueArray, out var storeObjectReference, serializeFlags))
		{
			return;
		}
		if (Mode == SerializerMode.Write)
		{
			if (storeObjectReference >= 0)
			{
				StoreObjectRef(valueArray, storeObjectReference);
			}
			WriteArrayLength(valueArray.Length);
			for (int i = 0; i < valueArray.Length; i++)
			{
				serializer(ref valueArray[i]);
			}
			return;
		}
		int num = ReadArrayLength();
		valueArray = new T[num];
		if (storeObjectReference >= 0)
		{
			StoreObjectRef(valueArray, storeObjectReference);
		}
		for (int j = 0; j < num; j++)
		{
			serializer(ref valueArray[j]);
		}
	}

	public void Serialize<T>(ref T[] valueArray, int count, SerializerPrimitiveAction<T> serializer, SerializeFlags serializeFlags = SerializeFlags.Normal)
	{
		if (SerializeIsNull(ref valueArray, out var storeObjectReference, serializeFlags))
		{
			return;
		}
		if (Mode == SerializerMode.Write)
		{
			if (storeObjectReference >= 0)
			{
				StoreObjectRef(valueArray, storeObjectReference);
			}
			for (int i = 0; i < count; i++)
			{
				serializer(ref valueArray[i]);
			}
			return;
		}
		valueArray = new T[count];
		if (storeObjectReference >= 0)
		{
			StoreObjectRef(valueArray, storeObjectReference);
		}
		for (int j = 0; j < count; j++)
		{
			serializer(ref valueArray[j]);
		}
	}

	public void Serialize<T>(ref T[] valueArray, SerializeFlags serializeFlags = SerializeFlags.Normal) where T : IDataSerializable, new()
	{
		if (SerializeIsNull(ref valueArray, out var storeObjectReference, serializeFlags))
		{
			return;
		}
		if (Mode == SerializerMode.Write)
		{
			if (storeObjectReference >= 0)
			{
				StoreObjectRef(valueArray, storeObjectReference);
			}
			WriteArrayLength(valueArray.Length);
			for (int i = 0; i < valueArray.Length; i++)
			{
				Serialize(ref valueArray[i], serializeFlags);
			}
			return;
		}
		int num = ReadArrayLength();
		valueArray = new T[num];
		if (storeObjectReference >= 0)
		{
			StoreObjectRef(valueArray, storeObjectReference);
		}
		for (int j = 0; j < num; j++)
		{
			Serialize(ref valueArray[j], serializeFlags);
		}
	}

	public void SerializeWithNoInstance<T>(ref T[] valueArray, SerializeFlags serializeFlags = SerializeFlags.Normal) where T : IDataSerializable
	{
		if (SerializeIsNull(ref valueArray, out var storeObjectReference, serializeFlags))
		{
			return;
		}
		if (Mode == SerializerMode.Write)
		{
			if (storeObjectReference >= 0)
			{
				StoreObjectRef(valueArray, storeObjectReference);
			}
			WriteArrayLength(valueArray.Length);
			for (int i = 0; i < valueArray.Length; i++)
			{
				SerializeWithNoInstance(ref valueArray[i], serializeFlags);
			}
			return;
		}
		int num = ReadArrayLength();
		valueArray = new T[num];
		if (storeObjectReference >= 0)
		{
			StoreObjectRef(valueArray, storeObjectReference);
		}
		for (int j = 0; j < num; j++)
		{
			SerializeWithNoInstance(ref valueArray[j], serializeFlags);
		}
	}

	public void Serialize<T>(ref T[] valueArray, int count, SerializeFlags serializeFlags = SerializeFlags.Normal) where T : IDataSerializable, new()
	{
		if (SerializeIsNull(ref valueArray, out var storeObjectReference, serializeFlags))
		{
			return;
		}
		if (Mode == SerializerMode.Write)
		{
			if (storeObjectReference >= 0)
			{
				StoreObjectRef(valueArray, storeObjectReference);
			}
			for (int i = 0; i < count; i++)
			{
				Serialize(ref valueArray[i], serializeFlags);
			}
			return;
		}
		valueArray = new T[count];
		if (storeObjectReference >= 0)
		{
			StoreObjectRef(valueArray, storeObjectReference);
		}
		for (int j = 0; j < count; j++)
		{
			Serialize(ref valueArray[j], serializeFlags);
		}
	}

	public void Serialize(ref byte[] valueArray, SerializeFlags serializeFlags = SerializeFlags.Normal)
	{
		if (SerializeIsNull(ref valueArray, out var storeObjectReference, serializeFlags))
		{
			return;
		}
		if (Mode == SerializerMode.Write)
		{
			if (storeObjectReference >= 0)
			{
				StoreObjectRef(valueArray, storeObjectReference);
			}
			WriteArrayLength(valueArray.Length);
			Writer.Write(valueArray);
			return;
		}
		int num = ReadArrayLength();
		valueArray = new byte[num];
		if (storeObjectReference >= 0)
		{
			StoreObjectRef(valueArray, storeObjectReference);
		}
		Reader.Read(valueArray, 0, num);
	}

	public void Serialize(ref byte[] valueArray, int count, SerializeFlags serializeFlags = SerializeFlags.Normal)
	{
		if (SerializeIsNull(ref valueArray, out var storeObjectReference, serializeFlags))
		{
			return;
		}
		if (Mode == SerializerMode.Write)
		{
			if (storeObjectReference >= 0)
			{
				StoreObjectRef(valueArray, storeObjectReference);
			}
			Writer.Write(valueArray, 0, count);
			return;
		}
		valueArray = new byte[count];
		if (storeObjectReference >= 0)
		{
			StoreObjectRef(valueArray, storeObjectReference);
		}
		Reader.Read(valueArray, 0, count);
	}

	public void Serialize<T>(ref List<T> valueList, SerializeFlags serializeFlags = SerializeFlags.Normal) where T : IDataSerializable, new()
	{
		if (SerializeIsNull(ref valueList, out var storeObjectReference, serializeFlags))
		{
			return;
		}
		if (Mode == SerializerMode.Write)
		{
			if (storeObjectReference >= 0)
			{
				StoreObjectRef(valueList, storeObjectReference);
			}
			WriteArrayLength(valueList.Count);
			{
				foreach (T value3 in valueList)
				{
					T value = value3;
					Serialize(ref value);
				}
				return;
			}
		}
		int num = ReadArrayLength();
		valueList = new List<T>(num);
		if (storeObjectReference >= 0)
		{
			StoreObjectRef(valueList, storeObjectReference);
		}
		for (int i = 0; i < num; i++)
		{
			T value2 = default(T);
			Serialize(ref value2);
			valueList.Add(value2);
		}
	}

	public void SerializeThis<T>(List<T> valueList, SerializeFlags serializeFlags = SerializeFlags.Normal) where T : IDataSerializable, new()
	{
		if (Mode == SerializerMode.Write)
		{
			WriteArrayLength(valueList.Count);
			{
				foreach (T value3 in valueList)
				{
					T value = value3;
					Serialize(ref value);
				}
				return;
			}
		}
		int num = ReadArrayLength();
		for (int i = 0; i < num; i++)
		{
			T value2 = default(T);
			Serialize(ref value2);
			valueList.Add(value2);
		}
	}

	public void Serialize<T>(ref List<T> valueList, SerializerPrimitiveAction<T> serializerMethod, SerializeFlags serializeFlags = SerializeFlags.Normal)
	{
		if (SerializeIsNull(ref valueList, out var storeObjectReference, serializeFlags))
		{
			return;
		}
		if (Mode == SerializerMode.Write)
		{
			if (storeObjectReference >= 0)
			{
				StoreObjectRef(valueList, storeObjectReference);
			}
			WriteArrayLength(valueList.Count);
			{
				foreach (T value3 in valueList)
				{
					T value = value3;
					serializerMethod(ref value);
				}
				return;
			}
		}
		int num = ReadArrayLength();
		EnsureList(ref valueList, num);
		if (storeObjectReference >= 0)
		{
			StoreObjectRef(valueList, storeObjectReference);
		}
		for (int i = 0; i < num; i++)
		{
			T value2 = default(T);
			serializerMethod(ref value2);
			valueList.Add(value2);
		}
	}

	public void Serialize<T>(ref List<T> valueList, int count, SerializeFlags serializeFlags = SerializeFlags.Normal) where T : IDataSerializable, new()
	{
		if (SerializeIsNull(ref valueList, out var storeObjectReference, serializeFlags))
		{
			return;
		}
		if (Mode == SerializerMode.Write)
		{
			if (storeObjectReference >= 0)
			{
				StoreObjectRef(valueList, storeObjectReference);
			}
			for (int i = 0; i < count; i++)
			{
				T value = valueList[i];
				Serialize(ref value, serializeFlags);
			}
			return;
		}
		EnsureList(ref valueList, count);
		if (storeObjectReference >= 0)
		{
			StoreObjectRef(valueList, storeObjectReference);
		}
		for (int j = 0; j < count; j++)
		{
			T value2 = default(T);
			Serialize(ref value2, serializeFlags);
			valueList.Add(value2);
		}
	}

	public void Serialize<T>(ref List<T> valueList, int count, SerializerPrimitiveAction<T> serializerMethod, SerializeFlags serializeFlags = SerializeFlags.Normal)
	{
		if (SerializeIsNull(ref valueList, out var storeObjectReference, serializeFlags))
		{
			return;
		}
		if (Mode == SerializerMode.Write)
		{
			if (storeObjectReference >= 0)
			{
				StoreObjectRef(valueList, storeObjectReference);
			}
			for (int i = 0; i < count; i++)
			{
				T value = valueList[i];
				serializerMethod(ref value);
			}
			return;
		}
		EnsureList(ref valueList, count);
		if (storeObjectReference >= 0)
		{
			StoreObjectRef(valueList, storeObjectReference);
		}
		for (int j = 0; j < count; j++)
		{
			T value2 = default(T);
			serializerMethod(ref value2);
			valueList.Add(value2);
		}
	}

	private void EnsureList<T>(ref List<T> valueList, int count)
	{
		if (valueList != null)
		{
			valueList.Clear();
			if (valueList.Capacity < count)
			{
				valueList.Capacity = count;
			}
		}
		else
		{
			valueList = new List<T>(count);
		}
	}

	public void Serialize<TKey, TValue>(ref Dictionary<TKey, TValue> dictionary, SerializeFlags serializeFlags = SerializeFlags.Normal) where TKey : IDataSerializable, new() where TValue : IDataSerializable, new()
	{
		if (SerializeIsNull(ref dictionary, out var storeObjectReference, serializeFlags))
		{
			return;
		}
		if (Mode == SerializerMode.Write)
		{
			if (storeObjectReference >= 0)
			{
				StoreObjectRef(dictionary, storeObjectReference);
			}
			WriteArrayLength(dictionary.Count);
			{
				foreach (KeyValuePair<TKey, TValue> item in dictionary)
				{
					TKey key = item.Key;
					TValue value = item.Value;
					key.Serialize(this);
					value.Serialize(this);
				}
				return;
			}
		}
		int num = ReadArrayLength();
		dictionary = new Dictionary<TKey, TValue>(num);
		if (storeObjectReference >= 0)
		{
			StoreObjectRef(dictionary, storeObjectReference);
		}
		for (int i = 0; i < num; i++)
		{
			TKey val = default(TKey);
			TValue val2 = default(TValue);
			val.Serialize(this);
			val2.Serialize(this);
		}
	}

	public void Serialize<TKey, TValue>(ref Dictionary<TKey, TValue> dictionary, SerializerPrimitiveAction<TValue> valueSerializer, SerializeFlags serializeFlags = SerializeFlags.Normal) where TKey : IDataSerializable, new()
	{
		if (SerializeIsNull(ref dictionary, out var storeObjectReference, serializeFlags))
		{
			return;
		}
		if (Mode == SerializerMode.Write)
		{
			if (storeObjectReference >= 0)
			{
				StoreObjectRef(dictionary, storeObjectReference);
			}
			WriteArrayLength(dictionary.Count);
			{
				foreach (KeyValuePair<TKey, TValue> item in dictionary)
				{
					TKey key = item.Key;
					TValue value = item.Value;
					key.Serialize(this);
					valueSerializer(ref value);
				}
				return;
			}
		}
		int num = ReadArrayLength();
		dictionary = new Dictionary<TKey, TValue>(num);
		if (storeObjectReference >= 0)
		{
			StoreObjectRef(dictionary, storeObjectReference);
		}
		for (int i = 0; i < num; i++)
		{
			TKey val = default(TKey);
			TValue value2 = default(TValue);
			val.Serialize(this);
			valueSerializer(ref value2);
		}
	}

	public void Serialize<TKey, TValue>(ref Dictionary<TKey, TValue> dictionary, SerializerPrimitiveAction<TKey> keySerializer, SerializeFlags serializeFlags = SerializeFlags.Normal) where TValue : IDataSerializable, new()
	{
		if (SerializeIsNull(ref dictionary, out var storeObjectReference, serializeFlags))
		{
			return;
		}
		if (Mode == SerializerMode.Write)
		{
			if (storeObjectReference >= 0)
			{
				StoreObjectRef(dictionary, storeObjectReference);
			}
			WriteArrayLength(dictionary.Count);
			{
				foreach (KeyValuePair<TKey, TValue> item in dictionary)
				{
					TKey value = item.Key;
					TValue value2 = item.Value;
					keySerializer(ref value);
					value2.Serialize(this);
				}
				return;
			}
		}
		int num = ReadArrayLength();
		dictionary = new Dictionary<TKey, TValue>(num);
		if (storeObjectReference >= 0)
		{
			StoreObjectRef(dictionary, storeObjectReference);
		}
		for (int i = 0; i < num; i++)
		{
			TKey value3 = default(TKey);
			TValue val = default(TValue);
			keySerializer(ref value3);
			val.Serialize(this);
		}
	}

	public void Serialize<TKey, TValue>(ref Dictionary<TKey, TValue> dictionary, SerializerPrimitiveAction<TKey> keySerializer, SerializerPrimitiveAction<TValue> valueSerializer, SerializeFlags serializeFlags = SerializeFlags.Normal)
	{
		if (SerializeIsNull(ref dictionary, out var storeObjectReference, serializeFlags))
		{
			return;
		}
		if (Mode == SerializerMode.Write)
		{
			if (storeObjectReference >= 0)
			{
				StoreObjectRef(dictionary, storeObjectReference);
			}
			WriteArrayLength(dictionary.Count);
			{
				foreach (KeyValuePair<TKey, TValue> item in dictionary)
				{
					TKey value = item.Key;
					TValue value2 = item.Value;
					keySerializer(ref value);
					valueSerializer(ref value2);
				}
				return;
			}
		}
		int num = ReadArrayLength();
		dictionary = new Dictionary<TKey, TValue>(num);
		if (storeObjectReference >= 0)
		{
			StoreObjectRef(dictionary, storeObjectReference);
		}
		for (int i = 0; i < num; i++)
		{
			TKey value3 = default(TKey);
			TValue value4 = default(TValue);
			keySerializer(ref value3);
			valueSerializer(ref value4);
		}
	}

	public void Serialize(ref string value)
	{
		Serialize(ref value, writeNullTerminatedString: false);
	}

	public void Serialize(ref string value, SerializeFlags serializeFlags)
	{
		Serialize(ref value, writeNullTerminatedString: false, serializeFlags);
	}

	public void Serialize(ref string value, bool writeNullTerminatedString, SerializeFlags serializeFlags = SerializeFlags.Normal)
	{
		int storeObjectReference = -1;
		if (!SerializeIsNull(ref value, out storeObjectReference, serializeFlags))
		{
			if (Mode == SerializerMode.Write)
			{
				WriteString(value, writeNullTerminatedString, -1);
			}
			else
			{
				value = ReadString(writeNullTerminatedString, -1);
			}
			if (storeObjectReference >= 0)
			{
				StoreObjectRef(value, storeObjectReference);
			}
		}
	}

	public void Serialize(ref string value, int len)
	{
		if (Mode == SerializerMode.Write)
		{
			WriteString(value, writeNullTerminated: false, len);
		}
		else
		{
			value = ReadString(readNullTerminatedString: false, len);
		}
	}

	public void Serialize(ref bool value)
	{
		if (Mode == SerializerMode.Write)
		{
			Writer.Write(value);
		}
		else
		{
			value = Reader.ReadBoolean();
		}
	}

	public void Serialize(ref byte value)
	{
		if (Mode == SerializerMode.Write)
		{
			Writer.Write(value);
		}
		else
		{
			value = Reader.ReadByte();
		}
	}

	public void Serialize(ref sbyte value)
	{
		if (Mode == SerializerMode.Write)
		{
			Writer.Write(value);
		}
		else
		{
			value = Reader.ReadSByte();
		}
	}

	public void Serialize(ref short value)
	{
		if (Mode == SerializerMode.Write)
		{
			Writer.Write(value);
		}
		else
		{
			value = Reader.ReadInt16();
		}
	}

	public void Serialize(ref ushort value)
	{
		if (Mode == SerializerMode.Write)
		{
			Writer.Write(value);
		}
		else
		{
			value = Reader.ReadUInt16();
		}
	}

	public void Serialize(ref int value)
	{
		if (Mode == SerializerMode.Write)
		{
			Writer.Write(value);
		}
		else
		{
			value = Reader.ReadInt32();
		}
	}

	public void SerializePackedInt(ref int value)
	{
		if (Mode == SerializerMode.Write)
		{
			Write7BitEncodedInt(value);
		}
		else
		{
			value = Read7BitEncodedInt();
		}
	}

	public void SerializeMemoryRegion(DataPointer dataRegion)
	{
		SerializeMemoryRegion(dataRegion.Pointer, dataRegion.Size);
	}

	public unsafe void SerializeMemoryRegion(IntPtr dataPointer, int sizeInBytes)
	{
		if (Stream is NativeFileStream nativeFileStream)
		{
			if (Mode == SerializerMode.Write)
			{
				nativeFileStream.Write(dataPointer, 0, sizeInBytes);
			}
			else
			{
				nativeFileStream.Read(dataPointer, 0, sizeInBytes);
			}
			return;
		}
		if (Stream is DataStream)
		{
			if (Mode == SerializerMode.Write)
			{
				((DataStream)Stream).Write(dataPointer, 0, sizeInBytes);
			}
			else
			{
				((DataStream)Stream).Read(dataPointer, 0, sizeInBytes);
			}
			return;
		}
		if (largeByteBuffer == null)
		{
			largeByteBuffer = new byte[32768];
		}
		if (largeByteBuffer.Length < 32768)
		{
			largeByteBuffer = new byte[32768];
		}
		if (Mode == SerializerMode.Write)
		{
			int num = sizeInBytes;
			while (num > 0)
			{
				int num2 = ((num < largeByteBuffer.Length) ? num : largeByteBuffer.Length);
				Utilities.Read(dataPointer, largeByteBuffer, 0, num2);
				Stream.Write(largeByteBuffer, 0, num2);
				dataPointer = (IntPtr)((byte*)(void*)dataPointer + num2);
				num -= num2;
			}
			return;
		}
		int num3 = sizeInBytes;
		while (num3 > 0)
		{
			int num4 = ((num3 < largeByteBuffer.Length) ? num3 : largeByteBuffer.Length);
			if (Stream.Read(largeByteBuffer, 0, num4) != num4)
			{
				throw new EndOfStreamException();
			}
			Utilities.Write(dataPointer, largeByteBuffer, 0, num4);
			dataPointer = (IntPtr)((byte*)(void*)dataPointer + num4);
			num3 -= num4;
		}
	}

	public void Serialize(ref uint value)
	{
		if (Mode == SerializerMode.Write)
		{
			Writer.Write(value);
		}
		else
		{
			value = Reader.ReadUInt32();
		}
	}

	public void Serialize(ref long value)
	{
		if (Mode == SerializerMode.Write)
		{
			Writer.Write(value);
		}
		else
		{
			value = Reader.ReadInt64();
		}
	}

	public void Serialize(ref ulong value)
	{
		if (Mode == SerializerMode.Write)
		{
			Writer.Write(value);
		}
		else
		{
			value = Reader.ReadUInt64();
		}
	}

	public void Serialize(ref char value)
	{
		if (Mode == SerializerMode.Write)
		{
			Writer.Write(value);
		}
		else
		{
			value = Reader.ReadChar();
		}
	}

	public void Serialize(ref float value)
	{
		if (Mode == SerializerMode.Write)
		{
			Writer.Write(value);
		}
		else
		{
			value = Reader.ReadSingle();
		}
	}

	public void Serialize(ref double value)
	{
		if (Mode == SerializerMode.Write)
		{
			Writer.Write(value);
		}
		else
		{
			value = Reader.ReadDouble();
		}
	}

	public void Serialize(ref DateTime value)
	{
		if (Mode == SerializerMode.Write)
		{
			Writer.Write(value.ToBinary());
		}
		else
		{
			value = new DateTime(Reader.ReadInt64());
		}
	}

	public void Serialize(ref Guid value)
	{
		if (Mode == SerializerMode.Write)
		{
			Writer.Write(value.ToByteArray());
		}
		else
		{
			value = new Guid(Reader.ReadBytes(16));
		}
	}

	private bool SerializeIsNull<T>(ref T value, out int storeObjectReference, SerializeFlags flags)
	{
		storeObjectReference = -1;
		if (Utilities.IsValueType(typeof(T)))
		{
			return false;
		}
		bool flag = object.ReferenceEquals(value, null);
		if ((flags & SerializeFlags.Nullable) != SerializeFlags.Normal || allowIdentityReferenceCount > 0)
		{
			if (Mode == SerializerMode.Write)
			{
				if (!flag && allowIdentityReferenceCount > 0 && (flags & SerializeFlags.Dynamic) == 0)
				{
					if (objectToPosition.TryGetValue(value, out var value2))
					{
						Writer.Write((byte)2);
						Write7BitEncodedInt(value2);
						return true;
					}
					objectToPosition.Add(value, (int)Stream.Position);
				}
				Writer.Write((!flag) ? ((byte)1) : ((byte)0));
				return flag;
			}
			value = default(T);
			int num = (int)Stream.Position;
			int num2 = Reader.ReadByte();
			switch (num2)
			{
			case 1:
				if (allowIdentityReferenceCount > 0 && (flags & SerializeFlags.Dynamic) == 0)
				{
					storeObjectReference = num;
				}
				break;
			case 2:
			{
				if (allowIdentityReferenceCount == 0)
				{
					throw new InvalidOperationException("Can't read serialized reference when SerializeReference is off");
				}
				num = Read7BitEncodedInt();
				if (!positionToObject.TryGetValue(num, out var value3))
				{
					throw new InvalidOperationException($"Can't find serialized reference at position [{num}]");
				}
				value = (T)value3;
				num2 = 0;
				break;
			}
			}
			return num2 == 0;
		}
		if (flag && Mode == SerializerMode.Write)
		{
			throw new ArgumentNullException("value");
		}
		return false;
	}

	private void SerializeRawDynamic<T>(ref T value, bool noDynamic = false)
	{
		if (Mode == SerializerMode.Write)
		{
			Type type = (noDynamic ? typeof(T) : value.GetType());
			if (!dynamicMapToFourCC.TryGetValue(type, out var value2))
			{
				throw new IOException($"Type [{type}] is not registered as dynamic");
			}
			if (!noDynamic)
			{
				Writer.Write((int)value2.Id);
			}
			value2.Writer(value, this);
			return;
		}
		Dynamic value3;
		if (noDynamic)
		{
			Type typeFromHandle = typeof(T);
			if (!dynamicMapToFourCC.TryGetValue(typeFromHandle, out value3))
			{
				throw new IOException($"Type [{typeFromHandle}] is not registered as dynamic");
			}
		}
		else
		{
			FourCC fourCC = Reader.ReadInt32();
			if (!dynamicMapToType.TryGetValue(fourCC, out value3))
			{
				throw new IOException($"Type [{fourCC}] is not registered as dynamic");
			}
		}
		value = (T)value3.Reader(this);
	}

	private void RegisterDynamic(Dynamic dynamic)
	{
		dynamicMapToFourCC.Add(dynamic.Type, dynamic);
		dynamicMapToType.Add(dynamic.Id, dynamic);
	}

	private static object ReaderIntArray(BinarySerializer serializer)
	{
		int[] valueArray = null;
		serializer.Serialize(ref valueArray, serializer.Serialize);
		return valueArray;
	}

	private static object ReaderUIntArray(BinarySerializer serializer)
	{
		uint[] valueArray = null;
		serializer.Serialize(ref valueArray, serializer.Serialize);
		return valueArray;
	}

	private static object ReaderShortArray(BinarySerializer serializer)
	{
		short[] valueArray = null;
		serializer.Serialize(ref valueArray, serializer.Serialize);
		return valueArray;
	}

	private static object ReaderUShortArray(BinarySerializer serializer)
	{
		ushort[] valueArray = null;
		serializer.Serialize(ref valueArray, serializer.Serialize);
		return valueArray;
	}

	private static object ReaderLongArray(BinarySerializer serializer)
	{
		long[] valueArray = null;
		serializer.Serialize(ref valueArray, serializer.Serialize);
		return valueArray;
	}

	private static object ReaderULongArray(BinarySerializer serializer)
	{
		ulong[] valueArray = null;
		serializer.Serialize(ref valueArray, serializer.Serialize);
		return valueArray;
	}

	private static object ReaderBoolArray(BinarySerializer serializer)
	{
		bool[] valueArray = null;
		serializer.Serialize(ref valueArray, serializer.Serialize);
		return valueArray;
	}

	private static object ReaderFloatArray(BinarySerializer serializer)
	{
		float[] valueArray = null;
		serializer.Serialize(ref valueArray, serializer.Serialize);
		return valueArray;
	}

	private static object ReaderDoubleArray(BinarySerializer serializer)
	{
		double[] valueArray = null;
		serializer.Serialize(ref valueArray, serializer.Serialize);
		return valueArray;
	}

	private static object ReaderDateTimeArray(BinarySerializer serializer)
	{
		DateTime[] valueArray = null;
		serializer.Serialize(ref valueArray, serializer.Serialize);
		return valueArray;
	}

	private static object ReaderGuidArray(BinarySerializer serializer)
	{
		Guid[] valueArray = null;
		serializer.Serialize(ref valueArray, serializer.Serialize);
		return valueArray;
	}

	private static object ReaderObjectArray(BinarySerializer serializer)
	{
		object[] valueArray = null;
		serializer.Serialize(ref valueArray, serializer.SerializeDynamic);
		return valueArray;
	}

	private static object ReaderCharArray(BinarySerializer serializer)
	{
		char[] valueArray = null;
		serializer.Serialize(ref valueArray, serializer.Serialize);
		return valueArray;
	}

	private static object ReaderStringArray(BinarySerializer serializer)
	{
		string[] valueArray = null;
		serializer.Serialize(ref valueArray, serializer.Serialize);
		return valueArray;
	}

	private static object ReaderByteArray(BinarySerializer serializer)
	{
		byte[] valueArray = null;
		serializer.Serialize(ref valueArray);
		return valueArray;
	}

	private static object ReaderSByteArray(BinarySerializer serializer)
	{
		sbyte[] valueArray = null;
		serializer.Serialize(ref valueArray, serializer.Serialize);
		return valueArray;
	}

	private static void WriterIntArray(object value, BinarySerializer serializer)
	{
		int[] valueArray = (int[])value;
		serializer.Serialize(ref valueArray, serializer.Serialize);
	}

	private static void WriterUIntArray(object value, BinarySerializer serializer)
	{
		uint[] valueArray = (uint[])value;
		serializer.Serialize(ref valueArray, serializer.Serialize);
	}

	private static void WriterShortArray(object value, BinarySerializer serializer)
	{
		short[] valueArray = (short[])value;
		serializer.Serialize(ref valueArray, serializer.Serialize);
	}

	private static void WriterUShortArray(object value, BinarySerializer serializer)
	{
		ushort[] valueArray = (ushort[])value;
		serializer.Serialize(ref valueArray, serializer.Serialize);
	}

	private static void WriterLongArray(object value, BinarySerializer serializer)
	{
		long[] valueArray = (long[])value;
		serializer.Serialize(ref valueArray, serializer.Serialize);
	}

	private static void WriterULongArray(object value, BinarySerializer serializer)
	{
		ulong[] valueArray = (ulong[])value;
		serializer.Serialize(ref valueArray, serializer.Serialize);
	}

	private static void WriterSByteArray(object value, BinarySerializer serializer)
	{
		sbyte[] valueArray = (sbyte[])value;
		serializer.Serialize(ref valueArray, serializer.Serialize);
	}

	private static void WriterStringArray(object value, BinarySerializer serializer)
	{
		string[] valueArray = (string[])value;
		serializer.Serialize(ref valueArray, serializer.Serialize);
	}

	private static void WriterCharArray(object value, BinarySerializer serializer)
	{
		char[] valueArray = (char[])value;
		serializer.Serialize(ref valueArray, serializer.Serialize);
	}

	private static void WriterBoolArray(object value, BinarySerializer serializer)
	{
		bool[] valueArray = (bool[])value;
		serializer.Serialize(ref valueArray, serializer.Serialize);
	}

	private static void WriterFloatArray(object value, BinarySerializer serializer)
	{
		float[] valueArray = (float[])value;
		serializer.Serialize(ref valueArray, serializer.Serialize);
	}

	private static void WriterDoubleArray(object value, BinarySerializer serializer)
	{
		double[] valueArray = (double[])value;
		serializer.Serialize(ref valueArray, serializer.Serialize);
	}

	private static void WriterDateTimeArray(object value, BinarySerializer serializer)
	{
		DateTime[] valueArray = (DateTime[])value;
		serializer.Serialize(ref valueArray, serializer.Serialize);
	}

	private static void WriterGuidArray(object value, BinarySerializer serializer)
	{
		Guid[] valueArray = (Guid[])value;
		serializer.Serialize(ref valueArray, serializer.Serialize);
	}

	private static void WriterObjectArray(object value, BinarySerializer serializer)
	{
		object[] valueArray = (object[])value;
		serializer.Serialize(ref valueArray, serializer.SerializeDynamic);
	}

	private static void WriterByteArray(object value, BinarySerializer serializer)
	{
		byte[] valueArray = (byte[])value;
		serializer.Serialize(ref valueArray);
	}

	private static object ReaderIntList(BinarySerializer serializer)
	{
		List<int> valueList = null;
		serializer.Serialize(ref valueList, serializer.Serialize);
		return valueList;
	}

	private static object ReaderUIntList(BinarySerializer serializer)
	{
		List<uint> valueList = null;
		serializer.Serialize(ref valueList, serializer.Serialize);
		return valueList;
	}

	private static object ReaderShortList(BinarySerializer serializer)
	{
		List<short> valueList = null;
		serializer.Serialize(ref valueList, serializer.Serialize);
		return valueList;
	}

	private static object ReaderUShortList(BinarySerializer serializer)
	{
		List<ushort> valueList = null;
		serializer.Serialize(ref valueList, serializer.Serialize);
		return valueList;
	}

	private static object ReaderLongList(BinarySerializer serializer)
	{
		List<long> valueList = null;
		serializer.Serialize(ref valueList, serializer.Serialize);
		return valueList;
	}

	private static object ReaderULongList(BinarySerializer serializer)
	{
		List<ulong> valueList = null;
		serializer.Serialize(ref valueList, serializer.Serialize);
		return valueList;
	}

	private static object ReaderBoolList(BinarySerializer serializer)
	{
		List<bool> valueList = null;
		serializer.Serialize(ref valueList, serializer.Serialize);
		return valueList;
	}

	private static object ReaderFloatList(BinarySerializer serializer)
	{
		List<float> valueList = null;
		serializer.Serialize(ref valueList, serializer.Serialize);
		return valueList;
	}

	private static object ReaderDoubleList(BinarySerializer serializer)
	{
		List<double> valueList = null;
		serializer.Serialize(ref valueList, serializer.Serialize);
		return valueList;
	}

	private static object ReaderDateTimeList(BinarySerializer serializer)
	{
		List<DateTime> valueList = null;
		serializer.Serialize(ref valueList, serializer.Serialize);
		return valueList;
	}

	private static object ReaderGuidList(BinarySerializer serializer)
	{
		List<Guid> valueList = null;
		serializer.Serialize(ref valueList, serializer.Serialize);
		return valueList;
	}

	private static object ReaderObjectList(BinarySerializer serializer)
	{
		List<object> valueList = null;
		serializer.Serialize(ref valueList, serializer.SerializeDynamic);
		return valueList;
	}

	private static object ReaderCharList(BinarySerializer serializer)
	{
		List<char> valueList = null;
		serializer.Serialize(ref valueList, serializer.Serialize);
		return valueList;
	}

	private static object ReaderStringList(BinarySerializer serializer)
	{
		List<string> valueList = null;
		serializer.Serialize(ref valueList, serializer.Serialize);
		return valueList;
	}

	private static object ReaderByteList(BinarySerializer serializer)
	{
		List<byte> valueList = null;
		serializer.Serialize(ref valueList, serializer.Serialize);
		return valueList;
	}

	private static object ReaderSByteList(BinarySerializer serializer)
	{
		List<sbyte> valueList = null;
		serializer.Serialize(ref valueList, serializer.Serialize);
		return valueList;
	}

	private static void WriterIntList(object value, BinarySerializer serializer)
	{
		List<int> valueList = (List<int>)value;
		serializer.Serialize(ref valueList, serializer.Serialize);
	}

	private static void WriterUIntList(object value, BinarySerializer serializer)
	{
		List<uint> valueList = (List<uint>)value;
		serializer.Serialize(ref valueList, serializer.Serialize);
	}

	private static void WriterShortList(object value, BinarySerializer serializer)
	{
		List<short> valueList = (List<short>)value;
		serializer.Serialize(ref valueList, serializer.Serialize);
	}

	private static void WriterUShortList(object value, BinarySerializer serializer)
	{
		List<ushort> valueList = (List<ushort>)value;
		serializer.Serialize(ref valueList, serializer.Serialize);
	}

	private static void WriterLongList(object value, BinarySerializer serializer)
	{
		List<long> valueList = (List<long>)value;
		serializer.Serialize(ref valueList, serializer.Serialize);
	}

	private static void WriterULongList(object value, BinarySerializer serializer)
	{
		List<ulong> valueList = (List<ulong>)value;
		serializer.Serialize(ref valueList, serializer.Serialize);
	}

	private static void WriterSByteList(object value, BinarySerializer serializer)
	{
		List<sbyte> valueList = (List<sbyte>)value;
		serializer.Serialize(ref valueList, serializer.Serialize);
	}

	private static void WriterStringList(object value, BinarySerializer serializer)
	{
		List<string> valueList = (List<string>)value;
		serializer.Serialize(ref valueList, serializer.Serialize);
	}

	private static void WriterCharList(object value, BinarySerializer serializer)
	{
		List<char> valueList = (List<char>)value;
		serializer.Serialize(ref valueList, serializer.Serialize);
	}

	private static void WriterBoolList(object value, BinarySerializer serializer)
	{
		List<bool> valueList = (List<bool>)value;
		serializer.Serialize(ref valueList, serializer.Serialize);
	}

	private static void WriterFloatList(object value, BinarySerializer serializer)
	{
		List<float> valueList = (List<float>)value;
		serializer.Serialize(ref valueList, serializer.Serialize);
	}

	private static void WriterDoubleList(object value, BinarySerializer serializer)
	{
		List<double> valueList = (List<double>)value;
		serializer.Serialize(ref valueList, serializer.Serialize);
	}

	private static void WriterDateTimeList(object value, BinarySerializer serializer)
	{
		List<DateTime> valueList = (List<DateTime>)value;
		serializer.Serialize(ref valueList, serializer.Serialize);
	}

	private static void WriterGuidList(object value, BinarySerializer serializer)
	{
		List<Guid> valueList = (List<Guid>)value;
		serializer.Serialize(ref valueList, serializer.Serialize);
	}

	private static void WriterObjectList(object value, BinarySerializer serializer)
	{
		List<object> valueList = (List<object>)value;
		serializer.Serialize(ref valueList, serializer.SerializeDynamic);
	}

	private static void WriterByteList(object value, BinarySerializer serializer)
	{
		List<byte> valueList = (List<byte>)value;
		serializer.Serialize(ref valueList, serializer.Serialize);
	}

	private static object ReaderInt(BinarySerializer serializer)
	{
		return serializer.Reader.ReadInt32();
	}

	private static object ReaderUInt(BinarySerializer serializer)
	{
		return serializer.Reader.ReadUInt32();
	}

	private static object ReaderShort(BinarySerializer serializer)
	{
		return serializer.Reader.ReadInt16();
	}

	private static object ReaderUShort(BinarySerializer serializer)
	{
		return serializer.Reader.ReadUInt16();
	}

	private static object ReaderLong(BinarySerializer serializer)
	{
		return serializer.Reader.ReadInt64();
	}

	private static object ReaderULong(BinarySerializer serializer)
	{
		return serializer.Reader.ReadUInt64();
	}

	private static object ReaderBool(BinarySerializer serializer)
	{
		return serializer.Reader.ReadBoolean();
	}

	private static object ReaderByte(BinarySerializer serializer)
	{
		return serializer.Reader.ReadByte();
	}

	private static object ReaderSByte(BinarySerializer serializer)
	{
		return serializer.Reader.ReadSByte();
	}

	private static object ReaderString(BinarySerializer serializer)
	{
		string value = null;
		serializer.Serialize(ref value);
		return value;
	}

	private static object ReaderFloat(BinarySerializer serializer)
	{
		return serializer.Reader.ReadSingle();
	}

	private static object ReaderDouble(BinarySerializer serializer)
	{
		return serializer.Reader.ReadDouble();
	}

	private static object ReaderChar(BinarySerializer serializer)
	{
		return serializer.Reader.ReadChar();
	}

	private static object ReaderDateTime(BinarySerializer serializer)
	{
		return new DateTime(serializer.Reader.ReadInt64());
	}

	private static object ReaderGuid(BinarySerializer serializer)
	{
		return new Guid(serializer.Reader.ReadBytes(16));
	}

	private static void WriterInt(object value, BinarySerializer serializer)
	{
		serializer.Writer.Write((int)value);
	}

	private static void WriterUInt(object value, BinarySerializer serializer)
	{
		serializer.Writer.Write((uint)value);
	}

	private static void WriterShort(object value, BinarySerializer serializer)
	{
		serializer.Writer.Write((short)value);
	}

	private static void WriterUShort(object value, BinarySerializer serializer)
	{
		serializer.Writer.Write((ushort)value);
	}

	private static void WriterLong(object value, BinarySerializer serializer)
	{
		serializer.Writer.Write((long)value);
	}

	private static void WriterULong(object value, BinarySerializer serializer)
	{
		serializer.Writer.Write((ulong)value);
	}

	private static void WriterByte(object value, BinarySerializer serializer)
	{
		serializer.Writer.Write((byte)value);
	}

	private static void WriterSByte(object value, BinarySerializer serializer)
	{
		serializer.Writer.Write((sbyte)value);
	}

	private static void WriterString(object value, BinarySerializer serializer)
	{
		string value2 = (string)value;
		serializer.Serialize(ref value2);
	}

	private static void WriterChar(object value, BinarySerializer serializer)
	{
		serializer.Writer.Write((char)value);
	}

	private static void WriterBool(object value, BinarySerializer serializer)
	{
		serializer.Writer.Write((bool)value);
	}

	private static void WriterFloat(object value, BinarySerializer serializer)
	{
		serializer.Writer.Write((float)value);
	}

	private static void WriterDouble(object value, BinarySerializer serializer)
	{
		serializer.Writer.Write((double)value);
	}

	private static void WriterDateTime(object value, BinarySerializer serializer)
	{
		serializer.Writer.Write(((DateTime)value).ToBinary());
	}

	private static void WriterGuid(object value, BinarySerializer serializer)
	{
		serializer.Writer.Write(((Guid)value).ToByteArray());
	}

	private static object ReaderDataSerializer<T>(BinarySerializer serializer) where T : IDataSerializable, new()
	{
		T value = default(T);
		serializer.Serialize(ref value);
		return value;
	}

	private static void WriterDataSerializer<T>(object value, BinarySerializer serializer) where T : IDataSerializable, new()
	{
		T value2 = (T)value;
		serializer.Serialize(ref value2);
	}

	private static object ReaderDataSerializerArray<T>(BinarySerializer serializer) where T : IDataSerializable, new()
	{
		T[] valueArray = null;
		serializer.Serialize(ref valueArray);
		return valueArray;
	}

	private static void WriterDataSerializerArray<T>(object value, BinarySerializer serializer) where T : IDataSerializable, new()
	{
		T[] valueArray = (T[])value;
		serializer.Serialize(ref valueArray);
	}

	private static object ReaderDataSerializerList<T>(BinarySerializer serializer) where T : IDataSerializable, new()
	{
		List<T> valueList = null;
		serializer.Serialize(ref valueList);
		return valueList;
	}

	private static void WriterDataSerializerList<T>(object value, BinarySerializer serializer) where T : IDataSerializable, new()
	{
		List<T> valueList = (List<T>)value;
		serializer.Serialize(ref valueList);
	}

	private void StoreObjectRef(object value, int position)
	{
		objectToPosition.Add(value, position);
		positionToObject.Add(position, value);
	}

	private void ResetStoredReference()
	{
		positionToObject.Clear();
		objectToPosition.Clear();
	}

	private string ReadString(bool readNullTerminatedString, int stringLength)
	{
		int num = 0;
		if (largeByteBuffer == null)
		{
			largeByteBuffer = new byte[1024];
		}
		if (largeCharBuffer == null)
		{
			largeCharBuffer = new char[maxCharSize];
		}
		string result = string.Empty;
		if (readNullTerminatedString)
		{
			byte b;
			while ((b = Reader.ReadByte()) != 0)
			{
				if (num == largeByteBuffer.Length)
				{
					byte[] destinationArray = new byte[largeByteBuffer.Length * 2];
					Array.Copy(largeByteBuffer, 0, destinationArray, 0, largeByteBuffer.Length);
					largeByteBuffer = destinationArray;
				}
				largeByteBuffer[num++] = b;
			}
			int maxCharCount = encoding.GetMaxCharCount(num);
			if (maxCharCount > largeCharBuffer.Length)
			{
				largeCharBuffer = new char[maxCharCount];
			}
			int chars = decoder.GetChars(largeByteBuffer, 0, num, largeCharBuffer, 0);
			result = new string(largeCharBuffer, 0, chars);
		}
		else
		{
			if (stringLength < 0)
			{
				stringLength = ReadArrayLength();
				if (stringLength < 0)
				{
					throw new IOException($"Invalid string length ({stringLength})");
				}
			}
			if (stringLength > 0)
			{
				StringBuilder stringBuilder = null;
				do
				{
					int count = ((stringLength - num > 1024) ? 1024 : (stringLength - num));
					int num2 = Stream.Read(largeByteBuffer, 0, count);
					if (num2 == 0)
					{
						throw new EndOfStreamException();
					}
					int chars2 = decoder.GetChars(largeByteBuffer, 0, num2, largeCharBuffer, 0);
					if (num == 0 && num2 == stringLength)
					{
						return new string(largeCharBuffer, 0, chars2);
					}
					if (stringBuilder == null)
					{
						stringBuilder = new StringBuilder(stringLength);
					}
					stringBuilder.Append(largeCharBuffer, 0, chars2);
					num += num2;
				}
				while (num < stringLength);
				result = stringBuilder.ToString();
			}
		}
		return result;
	}

	private unsafe void WriteString(string value, bool writeNullTerminated, int len)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (len < 0)
		{
			len = encoding.GetByteCount(value);
			if (!writeNullTerminated)
			{
				WriteArrayLength(len);
			}
		}
		else
		{
			if (value.Length != len)
			{
				throw new ArgumentException($"length of string to serialized ({value.Length}) != fixed length ({len})");
			}
			if (writeNullTerminated)
			{
				throw new ArgumentException("Cannot use null terminated string and fixed length");
			}
		}
		if (largeByteBuffer == null)
		{
			largeByteBuffer = new byte[1024];
			maxChars = 1024 / encoding.GetMaxByteCount(1);
		}
		if (len <= 1024)
		{
			encoding.GetBytes(value, 0, value.Length, largeByteBuffer, 0);
			Stream.Write(largeByteBuffer, 0, len);
		}
		else
		{
			int num = 0;
			int num2 = value.Length;
			while (num2 > 0)
			{
				int num3 = ((num2 > maxChars) ? maxChars : num2);
				int bytes2;
				fixed (char* ptr = value)
				{
					fixed (byte* bytes = largeByteBuffer)
					{
						bytes2 = encoder.GetBytes(ptr + num, num3, bytes, 1024, num3 == num2);
					}
				}
				Stream.Write(largeByteBuffer, 0, bytes2);
				num += num3;
				num2 -= num3;
			}
		}
		if (writeNullTerminated)
		{
			Stream.WriteByte(0);
		}
	}

	protected int ReadArrayLength()
	{
		return ArrayLengthType switch
		{
			ArrayLengthType.Dynamic => Read7BitEncodedInt(), 
			ArrayLengthType.Byte => Reader.ReadByte(), 
			ArrayLengthType.UShort => Reader.ReadUInt16(), 
			_ => Reader.ReadInt32(), 
		};
	}

	protected void WriteArrayLength(int value)
	{
		switch (ArrayLengthType)
		{
		case ArrayLengthType.Dynamic:
			Write7BitEncodedInt(value);
			break;
		case ArrayLengthType.Byte:
			if (value > 255)
			{
				throw new NotSupportedException($"Cannot serialize array length [{value}], larger then ArrayLengthType [{255}]");
			}
			Writer.Write((byte)value);
			break;
		case ArrayLengthType.UShort:
			if (value > 65535)
			{
				throw new NotSupportedException($"Cannot serialize array length [{value}], larger then ArrayLengthType [{65535}]");
			}
			Writer.Write((ushort)value);
			break;
		case ArrayLengthType.Int:
			if (value < 0)
			{
				throw new NotSupportedException($"Cannot serialize array length [{value}], larger then ArrayLengthType [{134217727}]");
			}
			Writer.Write(value);
			break;
		}
	}

	protected int Read7BitEncodedInt()
	{
		int num = 0;
		int num2 = 0;
		byte b;
		do
		{
			if (num2 == 35)
			{
				throw new FormatException("Bad string length. 7bit Int32 format");
			}
			b = Reader.ReadByte();
			num |= (b & 0x7F) << num2;
			num2 += 7;
		}
		while ((b & 0x80) != 0);
		return num;
	}

	protected void Write7BitEncodedInt(int value)
	{
		uint num;
		for (num = (uint)value; num >= 128; num >>= 7)
		{
			Writer.Write((byte)(num | 0x80));
		}
		Writer.Write((byte)num);
	}

	private static Dynamic GetDynamic<T>(FourCC id) where T : IDataSerializable, new()
	{
		Dynamic dynamic = new Dynamic();
		dynamic.Id = id;
		dynamic.Type = typeof(T);
		dynamic.Reader = ReaderDataSerializer<T>;
		dynamic.Writer = WriterDataSerializer<T>;
		return dynamic;
	}

	private static Dynamic GetDynamicArray<T>(FourCC id) where T : IDataSerializable, new()
	{
		Dynamic dynamic = new Dynamic();
		dynamic.Id = id;
		dynamic.Type = typeof(T[]);
		dynamic.Reader = ReaderDataSerializerArray<T>;
		dynamic.Writer = WriterDataSerializerArray<T>;
		return dynamic;
	}

	private static Dynamic GetDynamicList<T>(FourCC id) where T : IDataSerializable, new()
	{
		Dynamic dynamic = new Dynamic();
		dynamic.Id = id;
		dynamic.Type = typeof(List<T>);
		dynamic.Reader = ReaderDataSerializerList<T>;
		dynamic.Writer = WriterDataSerializerList<T>;
		return dynamic;
	}

	protected override void Dispose(bool disposing)
	{
	}
}
