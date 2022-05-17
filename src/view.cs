using System;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MagRestorer {
public class MGWindow : Form {

private Label lb_backups;
private ListBox lst_backups;
private Button btn_restore;

private List<Magazine> magazines;

public MGWindow() {
magazines = new List<Magazine>();

this.Size = new Size(640, 480);
this.Text = "Przywracanie magazynu KCFirmy";

lb_backups = new Label();
lb_backups.Size = new Size(600, 50);
lb_backups.Location = new Point(20, 20);
lb_backups.Text = "Dostępne kopie";
this.Controls.Add(lb_backups);
lst_backups = new ListBox();
lst_backups.Size = new Size(600, 240);
lst_backups.Location = new Point(20, 100);
this.Controls.Add(lst_backups);

btn_restore = new Button();
btn_restore.Size = new Size(200, 100);
btn_restore.Location = new Point(220, 360);
btn_restore.Text = "Przywróć";
this.Controls.Add(btn_restore);
btn_restore.Click+=(sender, e) => Restore();
}

public void AddMagazine(Magazine magazine) {
int index=magazines.Count;
for(int i=magazines.Count-1; i>=0; --i)
if(magazines[i].CreationDate<magazine.CreationDate) index=i;
else break;
magazines.Insert(index, magazine);
StringBuilder sb = new StringBuilder();
sb.Append(Path.GetFileName(magazine.MagazineFile));
sb.Append(": ");
sb.Append(magazine.CreationDate.ToString("g"));
lst_backups.Items.Insert(index, sb.ToString());
}

private void Restore() {
if(lst_backups.SelectedIndex==-1) return;
Magazine magazine = magazines[lst_backups.SelectedIndex];
StringBuilder sb = new StringBuilder();
sb.Append("Czy na pewno chcesz przywrócić tę kopię?\r\nObecny magazyn zostanie usunięty.\r\n\r\nNazwa magazynu: ");
sb.Append(Path.GetFileName(magazine.MagazineFile));
sb.Append("\r\nData utworzenia: ");
sb.Append(magazine.CreationDate.ToString("g"));
sb.Append("\r\nRozmiar: ");
sb.Append((magazine.Size/1048576).ToString());
sb.Append("MB");
DialogResult result = MessageBox.Show(this, sb.ToString(), "Przywracanie kopii magazynu", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
if(result==DialogResult.Yes) {
var l = new LoadingWindow("Przywracanie...");
l.SetStatus("Przywracanie kopii magazynu...");
l.AllowCancellation(false);
bool completed=false;
Task.Factory.StartNew(async()=> {
while(!completed)
l.SetPercentage(magazine.RestorationPercentage);
await Task.Delay(100);
});
Task.Factory.StartNew(()=> {
magazine.Restore();
completed=true;
l.Close();
});
l.ShowDialog(this);
}
}
}
}