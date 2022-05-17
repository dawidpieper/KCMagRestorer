using System;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace MagRestorer {
public static class Program {
[STAThread]
public static void Main() {
Application.EnableVisualStyles();
var l = new LoadingWindow("Wczytywanie...");
l.SetStatus("Wczytywanie listy magazynów...");
MGWindow wnd = new MGWindow();
bool success=false;
CancellationTokenSource cts=new CancellationTokenSource();
CancellationToken ct = cts.Token;
Task.Factory.StartNew(()=> {
try {
string[] finders = Directory.GetFiles(@"C:\kcfirma3\ArchAuto", "*.a00");
int count = finders.Count();
for(int i=0; i<count; ++i) {
string file = finders[i];
l.SetPercentage(100*i/count);
Magazine m = Magazine.TryFile(file);
if(m!=null) wnd.AddMagazine(m);
if(ct.IsCancellationRequested) return;
}
} catch(Exception) {}
success=true;
l.Close();
}, ct);
l.ShowDialog();
if(success) Application.Run(wnd);
else cts.Cancel();
}
}
}