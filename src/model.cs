using System;
using System.IO;
using System.Text;

namespace MagRestorer {
public class Magazine {
public readonly DateTime CreationDate;
public readonly long Size;
public readonly int Offset;
public readonly string BackupFile;
public readonly string MagazineFile;
private static bool IsRunning=false;
private static int TRestorationPercentage=0;

private Magazine(string backupFile, string magazineFile, long size, int offset, DateTime creationDate) {
BackupFile=backupFile;
MagazineFile=magazineFile;
Size=size;
Offset=offset;
CreationDate=creationDate;
}

public static Magazine TryFile(string file) {
long realSize = new System.IO.FileInfo(file).Length;
if(realSize<192) return null;
try {
int size, offset;
string magazinefile;
DateTime date;
using(BinaryReader br = new BinaryReader(new FileStream(file, FileMode.Open))) {
byte[] bsize = new byte[16];
br.Read(bsize, 0, 16);
byte[] boffset = new byte[8];
br.Read(boffset, 0, 8);
br.BaseStream.Seek(96, SeekOrigin.Begin);
byte[] bfile = new byte[160-96];
br.Read(bfile, 0, 160-96);
byte[] bdate = new byte[16];
br.Read(bdate, 0, 16);
size=0;
for(int i=0; i<16; ++i) {
if(bsize[i]<0x30||bsize[i]>0x39) break;
size=size*10+(bsize[i]-0x30);
}
offset=0;
for(int i=0; i<8; ++i) {
if(boffset[i]<0x30||boffset[i]>0x39) break;
offset=offset*10+(boffset[i]-0x30);
}
string strdate = Encoding.ASCII.GetString(bdate, 0, 16);
date = DateTime.ParseExact(strdate, "yyyy-MM-dd HH:mm",null);
magazinefile = Encoding.ASCII.GetString(bfile, 0, 160-96).Trim();
}
if(size!=realSize) return null;
Console.WriteLine(magazinefile);
Console.WriteLine(size);
Console.WriteLine(offset);
Console.WriteLine(date.ToString("g"));
Magazine magazine =  new Magazine(file, magazinefile, size, offset, date);
return magazine;
} catch(Exception) {return null;}
}

public int RestorationPercentage {get{return TRestorationPercentage;}}

public void Restore() {
if(IsRunning) return;
IsRunning=true;
TRestorationPercentage=0;
try {
long totalBytesWritten=0;
using(BinaryReader br = new BinaryReader(new FileStream(BackupFile, FileMode.Open))) {
br.BaseStream.Seek(this.Offset, SeekOrigin.Begin);
using(BinaryWriter bw = new BinaryWriter(new FileStream(MagazineFile, FileMode.Create))) {
const int BUFSIZE=16384;
byte[] buffer = new byte[BUFSIZE];
int bytesRead=0;
do {
bytesRead = br.Read(buffer, 0, BUFSIZE);
bw.Write(buffer, 0, bytesRead);
totalBytesWritten+=bytesRead;
TRestorationPercentage=(int)(100*totalBytesWritten/(Size-Offset));
} while(bytesRead>0);
}
}
} catch(Exception) {}
TRestorationPercentage=100;
IsRunning=false;
}
}
}