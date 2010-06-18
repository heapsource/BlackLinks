/*
public static class MultiPartUtil
		{
	
		static List<Chunk> GetChunks(Stream stream,byte[] sequence)
		{
			List<Chunk> chunks = new List<Chunk>();
			stream.Position = 0;
			int currentStreamByte = 0;
			byte currentSequenceByteIndex = 0;
			long sequenceLength = sequence.Length;
			Chunk currentChunk = new Chunk();
			int chunkCount = 0;
			
			while((currentStreamByte = stream.ReadByte()) != -1)
			{
			
				//MainApp.WriteNginxLog(string.Format("Byte={0}, Sequence ={1}",currentStreamByte,sequence[currentSequenceByteIndex]));
			
				if(currentStreamByte == sequence[currentSequenceByteIndex])
				{
					if(currentSequenceByteIndex == sequence.Length -1) //Is last byte of sequence ?
					{
						//...is the end of the chunk.
						if(currentChunk.IsEmpty)
						{
							currentChunk.StartIndex = stream.Position +1;
						}
						else
						{
							currentChunk.EndIndex = stream.Position - sequenceLength;
						
							currentChunk.Number = chunkCount++;
							chunks.Add(currentChunk);
							currentChunk = new Chunk
							{
								StartIndex = stream.Position +1
							};
						}
						//if((stream.Position  +2) < (stream.Length - 1)) //Boundaries always have a CRLF or final boundary have two hypens(--)
						//{
						Console.Error.WriteLine("Skipped 2 positions(hyphens or CRLF)");
							stream.Position +=2;
						//}
						currentSequenceByteIndex = 0;
					}
					else{
						currentSequenceByteIndex++;
					}
				}
				else
				{
					currentSequenceByteIndex = 0;
				}
			}
			if(!currentChunk.IsEmpty)
			{
				currentChunk.EndIndex = stream.Position;
			
			currentChunk.Number = chunkCount++;
				chunks.Add(currentChunk);
			}
			return chunks;
		}
	static string ToByteString(this byte[] data)
	{
		string separatorDataString = "";
				foreach(var b in data)
					separatorDataString+=" " + b.ToString();
		return separatorDataString;
	}
			public static List<FormPart> GetPartsFromBodyStream(Stream stream,string separator)
			{
				//stream.Position = 0;
				var separatorData = Encoding.UTF7.GetBytes("--" + separator);
				var chunks = GetChunks(stream,separatorData);
#if DEBUG		
		MainApp.WriteNginxLog("");
				string separatorDataString = separatorData.ToByteString();
				MainApp.WriteNginxLog(string.Format("Chunks Found={0}, Separator Length = {1}, Separator String length = {2}, Separator Data = {3}",chunks.Count(),separatorData.Length,separator.Length,separatorDataString));
#endif
				List<FormPart> parts = new List<FormPart>();
				for(int i = 1; i < chunks.Count; i++)
				{
					var currentChunk = chunks[i];
			stream.Position = currentChunk.StartIndex;
					byte[] chunkData = new byte[currentChunk.Length];
			stream.Read(chunkData,0,chunkData.Length);
			Console.Error.WriteLine("FOund Chunk IX={0} Data='{1}'",currentChunk.Number,chunkData.ToByteString());
					if(currentChunk.Length == 0) continue;
					MainApp.WriteNginxLog(string.Format("Chunk at {0} and {1}, Length = {2}",currentChunk.StartIndex,currentChunk.EndIndex,currentChunk.Length));
					FormPart part = new FormPart(currentChunk,stream);
					parts.Add(part);
				}
				return parts;
			}
	
		}
public class FormPart
{
	static List<string> GetHeaderLines(Stream stream)
	{
		List<string> list = new List<string>();
		StringBuilder lineBuilder = new StringBuilder();
		int readedByte = 0, lastByte = 0;
		while(true)
				{
				    readedByte = stream.ReadByte();
					Console.Error.Write("HC={0},",readedByte);
					if(readedByte == -1) break;
			
					if(lastByte == 13 && readedByte == 10)
					{
						var createLine = lineBuilder.ToString();
						if(createLine == "--" || createLine.Length == 0) break; //Two hypens at the end of the buffer because multi-parts always use tswo hypens at the final boundary.
						MainApp.WriteNginxLog("Line Found:" + createLine);
						list.Add(createLine.ToString());
						lineBuilder = new StringBuilder();
					}
					else if(readedByte != 13 && readedByte != 10)
					{
						lineBuilder.Append((char)readedByte);
					}
					lastByte = readedByte;
				}
		Console.Error.WriteLine("End of Header");
				return list;
			}
			internal FormPart(Chunk partChunk,Stream stream)
			{
				//stream.Position = partChunk.StartIndex;		
				this.Headers = new FormHeadersCollection();
				var headers = GetHeaderLines(stream);
				MainApp.WriteNginxLog(string.Format("Header Lines '{0}'",headers.Count()));
		foreach(var headerLine in headers)
		{
			MainApp.WriteNginxLog(string.Format("Header Line Found '{0}'",headerLine));
			
			//#error LEER Letra por letra sacando los encabezados y los subvalores del mismo.
						
						
//						MainApp.WriteNginxLog(string.Format("Header Line Found '{0}'",headerLine));
//						var parts = headerLine.Split(new char[]{';'},StringSplitOptions.RemoveEmptyEntries); 
//						NginxMonoHeader mainHeader = null;
//						bool firstPart = true;
//						foreach(var part in parts)
//						{
//							var partArray = part.Split(firstPart?new char[]{':'}:new char[]{'='}, StringSplitOptions.RemoveEmptyEntries);
//							if(firstPart)
//								mainHeader = new NginxMonoHeader(partArray[0].Trim(),partArray.Length > 1 ? partArray[1].Trim() : string.Empty);
//							else
//								mainHeader.AdditionalValues.Add(new NginxMonoHeader(partArray[0].Trim(),partArray.Length > 1 ? partArray[1].Trim() : string.Empty));
//							firstPart = false;
//						}
						
						//this.Headers.Add(mainHeader);
		}
		//partChunk.StartIndex = stream.Position;
		this.Stream = new ChunkStream(stream,partChunk);
		                              
			}
			public Stream Stream{get;private set;}
			public FormHeadersCollection Headers{get;private set;}
			public class FormHeadersCollection : Collection<NginxMonoHeader>
			{
				public NginxMonoHeader this[string name]		
				{
					get
					{
				return (from h in this where h.Key == name select h).FirstOrDefault();
					}
				}
			}
		}
		
		public class HttpFile
		{
			public string ContentType{get;internal set;}
			public string FileName{get;internal set;}
			public Stream Stream{get;internal set;}
		}
		
			class ChunkStream : Stream
	{
		Stream stream;
		Chunk chunk;
		public ChunkStream(Stream stream,Chunk chunk)
		{
			this.stream = stream;
			this.chunk = chunk;
			this.Position = 0;
		}
		public override bool CanRead {
			get {
				return true;
			}
		}
		public override int ReadByte ()
		{
			int @byte = this.stream.ReadByte();
			if(@byte != -1)
			{
				position++;
			}
			return @byte;
		}
		long position = 0;
		public override long Position {
			get {
				return position;
			}
			set {
				this.position = value;
				this.stream.Position = this.chunk.StartIndex + value;
			}
		}
		public override long Length {
			get {
				return this.chunk.Length;
			}
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			int readed =this.stream.Read(buffer,0,count); 
			position+=readed;
			return readed;
		}

 public override void Write (byte[] buffer, int offset, int count)
 {
 	throw new System.NotImplementedException ();
 }
public override void SetLength (long value)
{
	throw new System.NotImplementedException ();
}
		public override bool CanSeek {
			get {
				return false;
			}
			

		}
public override bool CanWrite {
	get {
		return false;
	}
}
		
		public override long Seek (long offset, System.IO.SeekOrigin origin)
			{
				throw new System.NotImplementedException ();
			}
public override void Flush ()
{
	throw new System.NotImplementedException ();
}

	}
	struct Chunk
	{
		public int Number {get;set;}
		public long StartIndex{get;set;}
		public long EndIndex{get;set;}
		public long Length
		{
			get
			{
				return EndIndex - StartIndex;
			}
		}
		public bool IsEmptyContent
		{
			get
			{
				return Length == 0;
			}
		}
		public bool IsEmpty
		{
			get
			{
				return this.EndIndex == 0 && this.StartIndex == 0;
			}
		}
	}
*/
