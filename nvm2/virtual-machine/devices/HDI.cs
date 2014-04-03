using System;
using System.IO;

namespace nvm2
{
	class HDI : StorageDevice
	{
		string mapped_path;
		string working_dir;

		FileStream stream;

		public HDI (string MappedPath)
		{
			if(!Directory.Exists(MappedPath)) {
				Directory.CreateDirectory(MappedPath);
			}
			mapped_path = MappedPath;
			working_dir = "";
		}

		public string GetWorkingDirectory ()
		{
			return Path.Combine (mapped_path, working_dir);
		}

		public string[] ReadFiles ()
		{
			return Directory.GetFiles(GetWorkingDirectory());
		}

		public string[] ReadDirectories ()
		{
			return Directory.GetDirectories(GetWorkingDirectory());
		}

		public void CreateFile (string Name)
		{
			stream = File.Create(Path.Combine(GetWorkingDirectory(),Name));
		}

		public void CreateDirectory (string Name)
		{
			Directory.CreateDirectory(Path.Combine(GetWorkingDirectory(),Name));
		}

		public void ChangeDirectory (string dir)
		{
			if(Directory.Exists(Path.Combine(mapped_path,dir))) {
				working_dir = dir;
			}
		}

		public bool DirectoryExists(string name) {
			return Directory.Exists(Path.Combine(mapped_path,name));
		}

		public bool FileExists(string name) {
			return File.Exists(Path.Combine(mapped_path,name));
		}

		public void OpenFile (string file)
		{
			stream = File.Open(file,FileMode.Open);
		}

		public void CloseFile() {
			stream.Close();
		}

		public byte[] ReadBytes (int length)
		{
			byte[] data = new byte[length];
			if (stream != null) {
				stream.Read (data, 0, length);
			}
			return data;
		}

		public void WriteByte (byte data)
		{
			if (stream != null) {
				stream.WriteByte(data);
			}
		}

		public void WriteBytes (byte[] data)
		{
			if (stream != null) {
				stream.Write(data,0,data.Length);
			}
		}

		public byte[] ReadAllBytes()
		{
			byte[] data = null;
			if(stream != null) {
				stream.Position = 0;
				long length = stream.Length;
				data = new byte[length];
				stream.Read (data, 0, (int)length);
			}
			return data;
		}
	}
}

